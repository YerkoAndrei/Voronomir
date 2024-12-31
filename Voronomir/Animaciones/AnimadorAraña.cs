using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Particles.Components;
using Stride.Rendering;
using Stride.Engine;
using Stride.Physics;

namespace Voronomir;

public class AnimadorAraña : StartupScript, IAnimador
{
    public ModelComponent modelo;
    public RigidbodyComponent veneno;
    public ParticleSystemComponent partículas;

    public string cabeza;
    public string cola;
    public List<string> patasIzq = new List<string> { };
    public List<string> patasDer = new List<string> { };
    public List<string> patasCompletas = new List<string> { };

    private SkeletonUpdater esqueleto;
    private int idCabeza;
    private int idCola;
    private int[] idPatasIzq;
    private int[] idPatasDer;
    private int[] idPatasCompletas;

    private Quaternion[] rotacionesInicioPatasIzq;
    private Quaternion[] rotacionesInicioPatasDer;

    private Quaternion rotaciónInicioCabeza;
    private Quaternion rotaciónInicioCola;

    private CancellationTokenSource tokenAtaque;

    public void Iniciar()
    {
        esqueleto = modelo.Skeleton;
        tokenAtaque = new CancellationTokenSource();

        idPatasIzq = new int[patasIzq.Count];
        idPatasDer = new int[patasDer.Count];
        idPatasCompletas = new int[patasCompletas.Count];

        rotacionesInicioPatasIzq = new Quaternion[patasIzq.Count];
        rotacionesInicioPatasDer = new Quaternion[patasDer.Count];

        // Encuentra huesos por nombre
        for (int i = 0; i < esqueleto.Nodes.Length; i++)
        {
            for (int ii = 0; ii < patasIzq.Count; ii++)
            {
                if (esqueleto.Nodes[i].Name == patasIzq[ii])
                {
                    idPatasIzq[ii] = i;
                    rotacionesInicioPatasIzq[ii] = esqueleto.NodeTransformations[i].Transform.Rotation;
                }
            }

            for (int ii = 0; ii < patasDer.Count; ii++)
            {
                if (esqueleto.Nodes[i].Name == patasDer[ii])
                {
                    idPatasDer[ii] = i;
                    rotacionesInicioPatasDer[ii] = esqueleto.NodeTransformations[i].Transform.Rotation;
                }
            }

            for (int ii = 0; ii < patasCompletas.Count; ii++)
            {
                if (esqueleto.Nodes[i].Name == patasCompletas[ii])
                    idPatasCompletas[ii] = i;
            }

            if (esqueleto.Nodes[i].Name == cabeza)
                idCabeza = i;

            if (esqueleto.Nodes[i].Name == cola)
                idCola = i;
        }

        rotaciónInicioCabeza = esqueleto.NodeTransformations[idCabeza].Transform.Rotation;
        rotaciónInicioCola = esqueleto.NodeTransformations[idCola].Transform.Rotation;
    }

    public void Actualizar()
    {

    }

    public void Activar(bool activar)
    {
        modelo.Enabled = activar;
        veneno.Enabled = activar;
        partículas.Enabled = activar;

        if (activar)
            partículas.ParticleSystem.ResetSimulation();
    }

    public void Caminar(float velocidad)
    {
        for (int i = 0; i < idPatasIzq.Length; i++)
        {
            esqueleto.NodeTransformations[idPatasIzq[i]].Transform.Rotation *= Quaternion.RotationZ(-velocidad * 20 * (float)Game.UpdateTime.WarpElapsed.TotalSeconds);
        }
        for (int i = 0; i < idPatasDer.Length; i++)
        {
            esqueleto.NodeTransformations[idPatasDer[i]].Transform.Rotation *= Quaternion.RotationZ(velocidad * 20 * (float)Game.UpdateTime.WarpElapsed.TotalSeconds);
        }
    }

    public void Atacar()
    {
        tokenAtaque.Cancel();
        tokenAtaque = new CancellationTokenSource();

        for (int i = 0; i < idPatasIzq.Length; i++)
        {
            esqueleto.NodeTransformations[idPatasIzq[i]].Transform.Rotation = rotacionesInicioPatasIzq[i];
        }
        for (int i = 0; i < idPatasDer.Length; i++)
        {
            esqueleto.NodeTransformations[idPatasDer[i]].Transform.Rotation = rotacionesInicioPatasDer[i];
        }

        AnimarAtaque(rotaciónInicioCabeza * Quaternion.RotationX(MathUtil.DegreesToRadians(-40)),
                     rotaciónInicioCola * Quaternion.RotationX(MathUtil.DegreesToRadians(-80)));
    }

    public void Morir()
    {
        tokenAtaque.Cancel();
        tokenAtaque = new CancellationTokenSource();

        AnimarMuerte(modelo.Entity.Transform.Position,
                     new Vector3(0, -0.8f, -0.4f));

        veneno.Entity.Transform.Position = new Vector3(0, 0, -1);
        veneno.Entity.Transform.Scale = new Vector3(1.5f, 1, 1.5f);
        partículas.Entity.Transform.Position = new Vector3(0, 0.2f, 0);

        // Apagar extremidades
        for (int i = 0; i < idPatasCompletas.Length; i++)
        {
            esqueleto.NodeTransformations[idPatasCompletas[i]].Transform.Scale = Vector3.Zero;
            esqueleto.NodeTransformations[idPatasCompletas[i]].Transform.Position -= Vector3.UnitY * 0.5f;
        }
    }

    // Animaciones
    private async void AnimarAtaque(Quaternion objetivoCabeza, Quaternion objetivoPoto)
    {
        float duración = 0.2f;
        float tiempoLerp = 0;
        float tiempo = 0;

        var token = tokenAtaque.Token;
        while (tiempoLerp < duración)
        {
            if (token.IsCancellationRequested)
                break;

            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            esqueleto.NodeTransformations[idCabeza].Transform.Rotation = Quaternion.Lerp(objetivoCabeza, rotaciónInicioCabeza, tiempo);
            esqueleto.NodeTransformations[idCola].Transform.Rotation = Quaternion.Lerp(objetivoPoto, rotaciónInicioCola, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        // Fin
        esqueleto.NodeTransformations[idCabeza].Transform.Rotation = rotaciónInicioCabeza;
        esqueleto.NodeTransformations[idCola].Transform.Rotation = rotaciónInicioCola;
    }

    private async void AnimarMuerte(Vector3 posiciónInicio, Vector3 posiciónObjetivo)
    {
        float duración = 0.25f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = tiempoLerp / duración;

            modelo.Entity.Transform.Position = Vector3.Lerp(posiciónInicio, posiciónObjetivo, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        // Fin
        modelo.Entity.Transform.Position = posiciónObjetivo;
    }
}
