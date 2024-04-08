using System.Collections.Generic;
using System.Threading.Tasks;
using Stride.Engine;

namespace Bozobaralika;
using static Utilidades;

public class ControladorActivador : AsyncScript
{
    public List<Entity> activables = new List<Entity> { };

    private IActivable[] interfaces;
    private PhysicsComponent cuerpo;
    private bool activado;

    public override async Task Execute()
    {
        cuerpo = Entity.Get<PhysicsComponent>();
        interfaces = new IActivable[activables.Count];
        for (int i=0; i < activables.Count; i++)
        {
            interfaces[i] = ObtenerInterfaz<IActivable>(activables[i]);
        }

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
        foreach (var activable in interfaces)
        {
            activable.Activar();
        }
    }
}
