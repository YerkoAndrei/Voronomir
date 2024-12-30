using System.Threading.Tasks;
using Stride.Engine;

namespace Voronomir;
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

            if (jugador != null && cuerpo.Enabled)
                Finalizar();

            await Script.NextFrame();
        }
    }

    private void Finalizar()
    {
        cuerpo.Enabled = false;
        ControladorJuego.Finalizar();
    }
}
