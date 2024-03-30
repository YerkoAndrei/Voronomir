using System.Linq;
using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;

public class ControladorArmaMelé : StartupScript
{
    private CollisionFilterGroupFlags colisiones;
    private RigidbodyComponent cuerpo;

    public void Iniciar(bool _jugador)
    {
        cuerpo = Entity.Get<RigidbodyComponent>();

        if (_jugador)
            colisiones = CollisionFilterGroupFlags.KinematicFilter;
        else
            colisiones = CollisionFilterGroupFlags.CharacterFilter;

        cuerpo.CanCollideWith = colisiones;
    }

    public void Atacar(float daño)
    {
        EnviarDaño(ObtenerColisiones(), daño);
    }

    public Collision[] ObtenerColisiones()
    {
        return cuerpo.Collisions.ToArray();
    }

    private void EnviarDaño(Collision[] colisiones, float daño)
    {
        // Melé envia daño por todos los elementos
        foreach (var colisión in colisiones)
        {
            var dañable = colisión.ColliderA.Entity.Get<ElementoDañable>();
            if (dañable == null)
                dañable = colisión.ColliderB.Entity.Get<ElementoDañable>();

            if (dañable == null)
                continue;

            dañable.RecibirDaño(daño);
        }
    }
}
