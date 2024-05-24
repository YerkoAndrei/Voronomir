using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Voronomir;
using static Utilidades;
using static Constantes;

public class ElementoDañable : StartupScript
{
    public Entity controlador;
    public float multiplicador;

    [DataMemberIgnore]
    public Enemigos enemigo;

    private IDañable interfaz;

    public override void Start()
    {
        interfaz = ObtenerInterfaz<IDañable>(controlador);

        // Enemigo / Jugador
        if (controlador.Get<ControladorEnemigo>() != null)
        {
            enemigo = controlador.Get<ControladorEnemigo>().enemigo;
            controlador.Get<ControladorEnemigo>().AgregarDañable(this);
        }
        else
            enemigo = Enemigos.nada;
    }

    public void Desactivar()
    {
        Entity.Get<PhysicsComponent>().Enabled = false;
        Entity.Get<PhysicsComponent>().CanSleep = true;
        Entity.Transform.Scale = Vector3.Zero;
    }

    public void RecibirDaño(float daño)
    {
        interfaz.RecibirDaño(daño * multiplicador);
    }

    public void Empujar(Vector3 dirección)
    {
        interfaz.Empujar(dirección);
    }
}
