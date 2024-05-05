using System.Linq;
using Stride.Engine;
using Stride.Physics;

namespace Voronomir;

public class ControladorArmaMelé : StartupScript
{
    private RigidbodyComponent cuerpo;
    private float daño;

    public void Iniciar(float _daño)
    {
        daño = _daño;
        cuerpo = Entity.Get<RigidbodyComponent>();
    }

    public void Atacar()
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

            dañable.RecibirDaño(daño, false);
        }
    }
}
