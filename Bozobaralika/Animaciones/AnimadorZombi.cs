using System.Collections.Generic;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Rendering;
using Stride.Engine;

namespace Bozobaralika;

public class AnimadorZombi : StartupScript, IAnimador
{
    public ModelComponent modelo;
    public List<string> brazos = new List<string> { };
    public List<string> piernas = new List<string> { };

    private SkeletonUpdater esqueleto;
    private int[] idBrazos;
    private int[] idPiernas;

    private Quaternion rotaciónInicio;

    public void Iniciar()
    {
        esqueleto = modelo.Skeleton;
        idBrazos = new int[brazos.Count];
        idPiernas = new int[piernas.Count];

        // Encuentra huesos por nombre
        for (int i = 0; i < esqueleto.Nodes.Length; i++)
        {
            for (int ii = 0; ii < brazos.Count; ii++)
            {
                if (esqueleto.Nodes[i].Name == brazos[ii])
                    idBrazos[ii] = i;
            }
            for (int ii = 0; ii < piernas.Count; ii++)
            {
                if (esqueleto.Nodes[i].Name == piernas[ii])
                    idPiernas[ii] = i;
            }
        }
        rotaciónInicio = esqueleto.NodeTransformations[idBrazos[0]].Transform.Rotation;
    }

    public void Caminar(float velocidad)
    {
        esqueleto.NodeTransformations[idPiernas[0]].Transform.Rotation *= Quaternion.RotationY(-0.2f * velocidad);
        esqueleto.NodeTransformations[idPiernas[1]].Transform.Rotation *= Quaternion.RotationY(0.2f * velocidad);
    }

    public void Atacar()
    {
        AnimarAtaque();
    }

    private async void AnimarAtaque()
    {
        var rotaciónAtaque0 = Quaternion.RotationZ(-90);
        var rotaciónAtaque1 = Quaternion.RotationZ(90);
        float duración = 0.25f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);

            esqueleto.NodeTransformations[idBrazos[0]].Transform.Rotation = Quaternion.Lerp(rotaciónAtaque0, rotaciónInicio, tiempo);
            esqueleto.NodeTransformations[idBrazos[1]].Transform.Rotation = Quaternion.Lerp(rotaciónAtaque1, rotaciónInicio, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Task.Delay(1);
        }

        // Fin
        for (int i = 0; i < idBrazos.Length; i++)
        {
            esqueleto.NodeTransformations[idBrazos[i]].Transform.Rotation = rotaciónInicio;
        }
    }
}
