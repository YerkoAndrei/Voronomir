using System.Linq;
using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;

public class ControladorArmaMelé : StartupScript
{
    private CollisionFilterGroupFlags colisiones;
    private RigidbodyComponent cuerpo;
    private bool jugador;

    public void Iniciar(bool _jugador)
    {
        cuerpo = Entity.Get<RigidbodyComponent>();
        colisiones = new CollisionFilterGroupFlags();
        jugador = _jugador;

        if (jugador)
            colisiones |= CollisionFilterGroupFlags.KinematicFilter;
        else
            colisiones |= CollisionFilterGroupFlags.CharacterFilter;

        cuerpo.CanCollideWith = colisiones;
    }

    public void Atacar(float daño)
    {
        if (jugador)
            DañarEnemigos(cuerpo.Collisions.ToArray(), daño);
        else
            DañarJugador(cuerpo.Collisions.ToArray(), daño);
    }

    private void DañarEnemigos(Collision[] colisiones, float daño)
    {
        foreach (var colisión in colisiones)
        {
            var enemigo = colisión.ColliderA.Entity.Get<ControladorEnemigo>();
            if (enemigo == null)
                enemigo = colisión.ColliderB.Entity.Get<ControladorEnemigo>();

            if (enemigo == null)
                continue;

            // Daña enemigo
            enemigo.RecibirDaño(daño);
        }
    }

    private void DañarJugador(Collision[] colisiones, float daño)
    {
        foreach (var colisión in colisiones)
        {
            var jugador = colisión.ColliderA.Entity.Get<ControladorJugador>();
            if (jugador == null)
                jugador = colisión.ColliderB.Entity.Get<ControladorJugador>();

            if (jugador == null)
                continue;

            // Daña jugador
            jugador.RecibirDaño(daño);
        }
    }
}
