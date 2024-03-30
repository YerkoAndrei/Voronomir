using System.Linq;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;

public class ControladorSaltador : AsyncScript
{
    public float fuerza;
    private Vector3 dirección;

    public override async Task Execute()
    {
        var cuerpo = Entity.Get<StaticColliderComponent>();
        dirección = new Vector3(0, fuerza, 0);

        while (Game.IsRunning)
        {
            await cuerpo.NewCollision();
            Saltar(cuerpo.Collisions.ToArray());

            await Script.NextFrame();
        }
    }

    private void Saltar(Collision[] colisiones)
    {
        foreach (var colisión in colisiones)
        {
            var cuerpo = colisión.ColliderA.Entity.Get<CharacterComponent>();
            if (cuerpo == null)
                cuerpo = colisión.ColliderB.Entity.Get<CharacterComponent>();

            if (cuerpo == null)
                continue;

            cuerpo.Jump(dirección);
        }
    }
}
