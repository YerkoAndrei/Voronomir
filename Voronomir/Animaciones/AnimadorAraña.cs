using System.Collections.Generic;
using Stride.Core.Mathematics;
using Stride.Particles.Components;
using Stride.Rendering;
using Stride.Physics;
using Stride.Engine;
using System.Threading.Tasks;

namespace Voronomir;

public class AnimadorAraña : StartupScript, IAnimador
{
    public ModelComponent modelo;
    public RigidbodyComponent veneno;
    public ParticleSystemComponent partículas;

    public string cabeza;
    public string poto;
    public List<string> patasIzq = new List<string> { };
    public List<string> patasDer = new List<string> { };
    public List<string> patasCompletas = new List<string> { };

    private SkeletonUpdater esqueleto;
    private int idCabeza;
    private int idPoto;
    private int[] idPatasIzq;
    private int[] idPatasDer;
    private int[] idPatasCompletas;

    private Quaternion[] rotacionesInicioPatasIzq;
    private Quaternion[] rotacionesInicioPatasDer;

    private Quaternion rotaciónInicioCabeza;
    private Quaternion rotaciónInicioPoto;

    public void Iniciar()
    {
        esqueleto = modelo.Skeleton;

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
                    rotacionesInicioPatasIzq[ii] = esqueleto.NodeTransformations[ii].Transform.Rotation;
                }
            }

            for (int ii = 0; ii < patasDer.Count; ii++)
            {
                if (esqueleto.Nodes[i].Name == patasDer[ii])
                {
                    idPatasDer[ii] = i;
                    rotacionesInicioPatasDer[ii] = esqueleto.NodeTransformations[ii].Transform.Rotation;
                }
            }

            for (int ii = 0; ii < patasCompletas.Count; ii++)
            {
                if (esqueleto.Nodes[i].Name == patasCompletas[ii])
                    idPatasCompletas[ii] = i;
            }

            if (esqueleto.Nodes[i].Name == cabeza)
                idCabeza = i;

            if (esqueleto.Nodes[i].Name == poto)
                idPoto = i;
        }

        rotaciónInicioCabeza = esqueleto.NodeTransformations[idCabeza].Transform.Rotation;
        rotaciónInicioPoto = esqueleto.NodeTransformations[idPoto].Transform.Rotation;
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
            esqueleto.NodeTransformations[idPatasIzq[i]].Transform.Rotation *= Quaternion.RotationZ(-velocidad * 10 * (float)Game.UpdateTime.WarpElapsed.TotalSeconds);
        }

        for (int i = 0; i < idPatasDer.Length; i++)
        {
            esqueleto.NodeTransformations[idPatasDer[i]].Transform.Rotation *= Quaternion.RotationZ(velocidad * 10 * (float)Game.UpdateTime.WarpElapsed.TotalSeconds);
        }
    }

    public void Atacar()
    {
        AnimarAtaque();
    }

    public void Morir()
    {
        modelo.Entity.Transform.Position = new Vector3(0, -0.8f, 0);
        partículas.Entity.Transform.Position = new Vector3(0, 0.2f, 0);

        veneno.Entity.Transform.Position = new Vector3(0, 0, 0);
        veneno.Entity.Transform.Scale = new Vector3(1.5f, 0, 1.5f);

        for (int i = 0; i < idPatasCompletas.Length; i++)
        {
            esqueleto.NodeTransformations[idPatasCompletas[i]].Transform.Position = Vector3.Zero;
        }
    }

    private async void AnimarAtaque()
    {
        var rotaciónAtaqueCabeza = Quaternion.RotationX(MathUtil.DegreesToRadians(60));
        var rotaciónAtaquePoto = Quaternion.RotationX(MathUtil.DegreesToRadians(-100));
        float duración = 0.2f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            esqueleto.NodeTransformations[idCabeza].Transform.Rotation = Quaternion.Lerp(rotaciónAtaqueCabeza, rotaciónInicioCabeza, tiempo);
            esqueleto.NodeTransformations[idPoto].Transform.Rotation = Quaternion.Lerp(rotaciónAtaquePoto, rotaciónInicioPoto, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        // Fin
        esqueleto.NodeTransformations[idCabeza].Transform.Rotation = rotaciónInicioCabeza;
        esqueleto.NodeTransformations[idPoto].Transform.Rotation = rotaciónInicioPoto;
    }
}
