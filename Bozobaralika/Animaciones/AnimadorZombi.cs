using System.Collections.Generic;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Rendering;
using Stride.Engine;

namespace Bozobaralika;
using static Utilidades;

public class AnimadorZombi : StartupScript, IAnimador
{
    public ModelComponent modelo;
    public List<string> brazos = new List<string> { };
    public List<string> piernas = new List<string> { };

    private SkeletonUpdater esqueleto;
    private int[] idBrazos;
    private int[] idPiernas;

    private Quaternion rotaciónInicio0;
    private Quaternion rotaciónInicio1;

    public void Iniciar()
    {
        esqueleto = modelo.Skeleton;
        idBrazos = new int[brazos.Count];
        idPiernas = new int[piernas.Count];

        // Tamaño aleatorio entre 160cm a 200cm
        var aleatorio = RangoAleatorio(0.8f, 1f);
        Entity.Transform.Scale *= aleatorio;

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
        rotaciónInicio0 = esqueleto.NodeTransformations[idBrazos[0]].Transform.Rotation;
        rotaciónInicio1 = esqueleto.NodeTransformations[idBrazos[1]].Transform.Rotation;
    }

    public void Actualizar()
    {

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

            esqueleto.NodeTransformations[idBrazos[0]].Transform.Rotation = Quaternion.Lerp(rotaciónAtaque0, rotaciónInicio0, tiempo);
            esqueleto.NodeTransformations[idBrazos[1]].Transform.Rotation = Quaternion.Lerp(rotaciónAtaque1, rotaciónInicio1, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        // Fin
        esqueleto.NodeTransformations[idBrazos[0]].Transform.Rotation = rotaciónInicio0;
        esqueleto.NodeTransformations[idBrazos[1]].Transform.Rotation = rotaciónInicio1;
    }
}
