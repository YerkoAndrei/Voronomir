using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;

public class ElementoDisparo: AsyncScript
{
    public ModelComponent modelo;
    private RigidbodyComponent cuerpo;
    private float daño;

    public override async Task Execute()
    {
        cuerpo = Entity.Get<RigidbodyComponent>();
        Apagar();

        while (Game.IsRunning)
        {
            if (cuerpo.Enabled)
            {
                var colisión = await cuerpo.NewCollision();

                var dañable = colisión.ColliderA.Entity.Get<ElementoDañable>();
                if (dañable == null)
                    dañable = colisión.ColliderB.Entity.Get<ElementoDañable>();

                if (dañable == null)
                    continue;

                // PENDIENTE: Bajar daño a enemigos no dañar disparador
                //dañable.RecibirDaño(daño);
                //Apagar();
            }
            await Script.NextFrame();
        }
    }

    public void Apagar()
    {
        cuerpo.LinearVelocity = Vector3.Zero;
        modelo.Enabled = false;
        cuerpo.Enabled = false;
    }

    public void Iniciar(float _daño, float _velocidad, Quaternion _rotación, Vector3 _posición)
    {
        Apagar();

        Entity.Transform.Position = _posición;
        Entity.Transform.Rotation = _rotación;

        daño = _daño;
        modelo.Enabled = true;
        cuerpo.Enabled = true;

        // Dirección
        cuerpo.UpdatePhysicsTransformation();
        cuerpo.LinearVelocity = Entity.Transform.WorldMatrix.Forward * _velocidad;

        // PENDIENTE: Duración máxima
        ContarVida(2);
    }

    private async void ContarVida(float tiempo)
    {
        await Task.Delay((int)(tiempo * 1000));

        if (cuerpo.Enabled)
            Apagar();
    }
}
