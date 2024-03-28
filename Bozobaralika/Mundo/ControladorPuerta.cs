using Stride.Core.Mathematics;
using Stride.Engine;
using System.Threading.Tasks;

namespace Bozobaralika;

public class ControladorPuerta : StartupScript
{
    public TransformComponent modelo;
    public Vector3 posiciónAbierta;

    public void Mover()
    {
        AnimarPuerta();
        // PENDIENTE: efectos
    }

    private async void AnimarPuerta()
    {
        var inicio = modelo.Position;
        float duración = 1f;
        float tiempoLerp = 0;
        float tiempo = 0;

        await Task.Delay(200);

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            modelo.Position = Vector3.Lerp(inicio, posiciónAbierta, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Script.NextFrame();
        }
    }
}
