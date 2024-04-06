using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;

public class ElementoVeneno: StartupScript
{
    public float tiempoVida;
    public RigidbodyComponent cuerpo;

    public override void Start()
    {
        cuerpo = Entity.Get<RigidbodyComponent>();
        cuerpo.Enabled = true;
        ContarVida();
    }

    private async void ContarVida()
    {
        float tiempoLerp = 0;
        float tiempo = 0;

        await Task.Delay(400);
        while (tiempoLerp < tiempoVida)
        {
            tiempo = tiempoLerp / tiempoVida;
            Entity.Transform.Scale = Vector3.Lerp(Entity.Transform.Scale, Vector3.Zero, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Task.Delay(1);
        }

        cuerpo.Enabled = false;
        Entity.Scene.Entities.Remove(Entity);
    }
}
