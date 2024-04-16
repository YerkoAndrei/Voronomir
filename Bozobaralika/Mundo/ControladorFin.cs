using System.Threading.Tasks;
using Stride.Engine;

namespace Bozobaralika;
using static Utilidades;

public class ControladorFin : AsyncScript
{
    private PhysicsComponent cuerpo;

    public override async Task Execute()
    {
        cuerpo = Entity.Get<PhysicsComponent>();

        while (Game.IsRunning)
        {
            var colisión = await cuerpo.NewCollision();
            var jugador = RetornaJugador(colisión);

            if (jugador != null)
                Finalizar();

            await Script.NextFrame();
        }
    }

    private void Finalizar()
    {
        ControladorPartida.Finalizar();
    }
}
