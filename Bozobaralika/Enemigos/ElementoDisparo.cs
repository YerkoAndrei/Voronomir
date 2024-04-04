using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;

public class ElementoDisparo: AsyncScript
{
    private RigidbodyComponent cuerpo;
    private bool activo;
    private float daño;
    private float velocidad;
    private Vector3 dirección;

    public override async Task Execute()
    {
        cuerpo = Entity.Get<RigidbodyComponent>();

        while (Game.IsRunning)
        {
            if (activo)
            {
                var colisión = await cuerpo.NewCollision();

                var dañable = colisión.ColliderA.Entity.Get<ElementoDañable>();
                if (dañable == null)
                    dañable = colisión.ColliderB.Entity.Get<ElementoDañable>();

                if (dañable == null)
                    continue;

                dañable.RecibirDaño(daño);
                activo = false;
            }
            await Script.NextFrame();
        }
    }

    public void Iniciar(float _daño, float _velocidad, Vector3 _dirección, Vector3 inicial)
    {
        Entity.Transform.WorldMatrix.TranslationVector = inicial;

        activo = true;
        daño = _daño;
        velocidad = _velocidad;
        dirección = _dirección;

        Mover();
    }

    private async void Mover()
    {
        while (activo)
        {
            dirección *= (float)Game.UpdateTime.Elapsed.TotalSeconds;
            Entity.Transform.WorldMatrix.TranslationVector = (dirección * velocidad);

            await Task.Delay(1);
        }
    }
}
