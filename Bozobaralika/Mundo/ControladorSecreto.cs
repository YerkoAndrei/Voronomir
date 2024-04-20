using System.Threading.Tasks;
using Stride.Engine;

namespace Bozobaralika;

public class ControladorSecreto : AsyncScript
{
    private PhysicsComponent cuerpo;

    public override async Task Execute()
    {
        cuerpo = Entity.Get<PhysicsComponent>();

        while (Game.IsRunning)
        {
            await cuerpo.NewCollision();
            if (cuerpo.Enabled)
                Activar();

            await Script.NextFrame();
        }
    }

    private void Activar()
    {
        cuerpo.Enabled = false;
        ControladorPartida.SumarSecreto();
    }
}
