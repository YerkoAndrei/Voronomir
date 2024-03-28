using System.Linq;
using System.Threading.Tasks;
using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;

public class ControladorDañoContinuo : AsyncScript
{
    private StaticColliderComponent cuerpo;
    private float daño;

    public override async Task Execute()
    {
        cuerpo = Entity.Get<StaticColliderComponent>();
        daño = 10f;

        while (Game.IsRunning)
        {
            await cuerpo.NewCollision();
            IntentarDañar();

            await Script.NextFrame();
        }
    }

    private async void IntentarDañar()
    {
        while(cuerpo.Collisions.Count > 0)
        {
            Dañar(cuerpo.Collisions.ToArray());
            await Task.Delay(500);
        }
    }

    private void Dañar(Collision[] colisiones)
    {
        foreach (var colisión in colisiones)
        {
            var jugador = colisión.ColliderA.Entity.Get<ControladorJugador>();
            if (jugador == null)
                jugador = colisión.ColliderB.Entity.Get<ControladorJugador>();

            if (jugador == null)
                continue;

            // PENDIENTE: efectos
            jugador.RecibirDaño(daño);
        }
    }
}
