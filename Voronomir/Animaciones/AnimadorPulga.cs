using System.Collections.Generic;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Rendering;
using Stride.Engine;
using Stride.Physics;

namespace Voronomir;

public class AnimadorPulga : StartupScript, IAnimador
{
    public ModelComponent modelo;
    public string cabeza;
    public string poto;
    public List<string> patasIzq = new List<string> { };
    public List<string> patasDer = new List<string> { };

    private CharacterComponent cuerpo;
    private SkeletonUpdater esqueleto;
    private int idCabeza;
    private int idPoto;
    private int[] idPatasIzq;
    private int[] idPatasDer;

    private Quaternion[] rotacionesInicioPatasIzq;
    private Quaternion[] rotacionesInicioPatasDer;

    private Quaternion rotaciónInicioCabeza;
    private Quaternion rotaciónInicioPoto;

    public void Iniciar()
    {
        cuerpo = Entity.Get<CharacterComponent>();
        esqueleto = modelo.Skeleton;

        idPatasIzq = new int[patasIzq.Count];
        idPatasDer = new int[patasDer.Count];

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
        for (int i = 0; i < idPatasIzq.Length; i++)
        {
            if (cuerpo.IsGrounded)
                esqueleto.NodeTransformations[i].Transform.Rotation = rotacionesInicioPatasIzq[i];
            else
                esqueleto.NodeTransformations[i].Transform.Rotation = rotacionesInicioPatasIzq[i] * Quaternion.RotationX(MathUtil.DegreesToRadians(18));
        }

        for (int i = 0; i < idPatasDer.Length; i++)
        {
            if (cuerpo.IsGrounded)
                esqueleto.NodeTransformations[i].Transform.Rotation = rotacionesInicioPatasDer[i];
            else
                esqueleto.NodeTransformations[i].Transform.Rotation = rotacionesInicioPatasDer[i] * Quaternion.RotationX(MathUtil.DegreesToRadians(-80));
        }
    }

    public void Activar(bool activar)
    {
        modelo.Enabled = activar;
    }

    public void Caminar(float velocidad)
    {
        for (int i = 0; i < idPatasIzq.Length; i++)
        {
            esqueleto.NodeTransformations[idPatasIzq[i]].Transform.Rotation *= Quaternion.RotationY(-velocidad * 10 * (float)Game.UpdateTime.WarpElapsed.TotalSeconds);
        }

        for (int i = 0; i < idPatasDer.Length; i++)
        {
            esqueleto.NodeTransformations[idPatasDer[i]].Transform.Rotation *= Quaternion.RotationY(velocidad * 10 * (float)Game.UpdateTime.WarpElapsed.TotalSeconds);
        }        
    }

    public void Atacar()
    {
        AnimarAtaque();
    }

    public void Morir()
    {
        // Siempre explota
    }

    private async void AnimarAtaque()
    {
        var rotaciónAtaqueCabeza = Quaternion.RotationX(MathUtil.DegreesToRadians(-90));
        var rotaciónAtaquePoto = Quaternion.RotationX(MathUtil.DegreesToRadians(190));
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
