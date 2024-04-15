using System.Threading.Tasks;
using Stride.Engine;

namespace Bozobaralika;

public class ControladorFin : AsyncScript
{
    private PhysicsComponent cuerpo;

    public override async Task Execute()
    {
        cuerpo = Entity.Get<PhysicsComponent>();

        while (Game.IsRunning)
        {
            var colisión = await cuerpo.NewCollision();

            var jugador = colisión.ColliderA.Entity.Get<ControladorJugador>();
            if (jugador == null)
                jugador = colisión.ColliderB.Entity.Get<ControladorJugador>();

            if (jugador == null)
                continue;

            Finalizar();
            await Script.NextFrame();
        }
    }

    private void Finalizar()
    {
        ControladorPartida.Finalizar();
    }
}
