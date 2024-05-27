using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Rendering;
using Stride.Engine;

namespace Voronomir;

public class AnimadorCarnicero : StartupScript, IAnimador
{
    public ModelComponent modelo;
    public List<string> brazos = new List<string> { };
    public List<string> piernas = new List<string> { };

    private SkeletonUpdater esqueleto;
    private int[] idBrazos;
    private int[] idPiernas;

    private Quaternion rotaciónInicioBrazoIzq;
    private Quaternion rotaciónInicioBrazoDer;

    private CancellationTokenSource tokenAtaque;

    public void Iniciar()
    {
        esqueleto = modelo.Skeleton;
        tokenAtaque = new CancellationTokenSource();

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

        rotaciónInicioBrazoIzq = esqueleto.NodeTransformations[idBrazos[0]].Transform.Rotation;
        rotaciónInicioBrazoDer = esqueleto.NodeTransformations[idBrazos[1]].Transform.Rotation;
    }

    public void Actualizar()
    {

    }

    public void Activar(bool activar)
    {
        modelo.Enabled = activar;
    }

    public void Caminar(float velocidad)
    {
        esqueleto.NodeTransformations[idPiernas[0]].Transform.Rotation *= Quaternion.RotationY(-velocidad * 10 * (float)Game.UpdateTime.WarpElapsed.TotalSeconds);
        esqueleto.NodeTransformations[idPiernas[1]].Transform.Rotation *= Quaternion.RotationY(velocidad * 10 * (float)Game.UpdateTime.WarpElapsed.TotalSeconds);
    }

    public void Atacar()
    {
        tokenAtaque.Cancel();
        tokenAtaque = new CancellationTokenSource();

        AnimarAtaque(Quaternion.RotationZ(MathUtil.DegreesToRadians(-100)),
                     Quaternion.RotationZ(MathUtil.DegreesToRadians(100)));
    }

    public void Morir()
    {
        tokenAtaque.Cancel();
        tokenAtaque = new CancellationTokenSource();

        AnimarMuerte(modelo.Entity.Transform.Position, modelo.Entity.Transform.Rotation, 
                     new Vector3(0, 0, 1.4f), Quaternion.RotationX(MathUtil.DegreesToRadians(-84)));

        for (int i = 0; i < idBrazos.Length; i++)
        {
            esqueleto.NodeTransformations[idBrazos[i]].Transform.Scale = Vector3.Zero;
        }
        for (int i = 0; i < idPiernas.Length; i++)
        {
            esqueleto.NodeTransformations[idPiernas[i]].Transform.Scale = Vector3.Zero;
        }
    }

    // Animaciones
    private async void AnimarAtaque(Quaternion objetivoIzq, Quaternion objetivoDer)
    {
        float duración = 0.4f;
        float tiempoLerp = 0;
        float tiempo = 0;

        var token = tokenAtaque.Token;
        while (tiempoLerp < duración)
        {
            if (token.IsCancellationRequested)
                break;

            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            esqueleto.NodeTransformations[idBrazos[0]].Transform.Rotation = Quaternion.Lerp(objetivoIzq, rotaciónInicioBrazoIzq, tiempo);
            esqueleto.NodeTransformations[idBrazos[1]].Transform.Rotation = Quaternion.Lerp(objetivoDer, rotaciónInicioBrazoDer, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        // Fin
        esqueleto.NodeTransformations[idBrazos[0]].Transform.Rotation = rotaciónInicioBrazoIzq;
        esqueleto.NodeTransformations[idBrazos[1]].Transform.Rotation = rotaciónInicioBrazoDer;
    }

    private async void AnimarMuerte(Vector3 posiciónInicio, Quaternion rotaciónInicio, Vector3 posiciónObjetivo, Quaternion rotaciónObjetivo)
    {
        float duración = 0.3f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = tiempoLerp / duración;

            modelo.Entity.Transform.Position = Vector3.Lerp(posiciónInicio, posiciónObjetivo, tiempo);
            modelo.Entity.Transform.Rotation = Quaternion.Lerp(rotaciónInicio, rotaciónObjetivo, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        // Fin
        modelo.Entity.Transform.Position = posiciónObjetivo;
        modelo.Entity.Transform.Rotation = rotaciónObjetivo;
    }
}
