using System.Collections.Generic;
using System.Threading.Tasks;
using Stride.Engine;

namespace Bozobaralika;

public class ControladorActivador : AsyncScript
{
    public List<Entity> activables = new List<Entity> { };

    private PhysicsComponent cuerpo;
    private bool activado;

    public override async Task Execute()
    {
        cuerpo = Entity.Get<PhysicsComponent>();

        while (Game.IsRunning)
        {
            if (!activado)
            {
                var colisión = await cuerpo.NewCollision();

                var jugador = colisión.ColliderA.Entity.Get<ControladorJugador>();
                if (jugador == null)
                    jugador = colisión.ColliderB.Entity.Get<ControladorJugador>();

                Activar();
            }
            await Script.NextFrame();
        }
    }

    private void Activar()
    {
        activado = true;
        foreach (var activable in activables)
        {
            foreach (var componente in activable.Components)
            {
                if (componente is IActivable)
                {
                    var temp = (IActivable)componente;
                    temp.Activar();
                    break;
                }
            }
        }
    }
}
