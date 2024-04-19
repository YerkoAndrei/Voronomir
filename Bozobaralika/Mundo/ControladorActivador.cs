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
            var colisión = await cuerpo.NewCollision();
            if (TocaJugador(colisión) && cuerpo.Enabled)
                Activar();

            await Script.NextFrame();
        }
    }

    private void Activar()
    {
        cuerpo.Enabled = false;
        foreach (var activable in interfaces)
        {
            activable.Activar();
        }
    }
}
