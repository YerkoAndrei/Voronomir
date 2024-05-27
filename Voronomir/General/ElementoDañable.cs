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
    private bool activo;

    public override void Start()
    {
        interfaz = ObtenerInterfaz<IDañable>(controlador);

        // Enemigo / Jugador
        if (controlador.Get<ControladorEnemigo>() != null)
        {
            enemigo = controlador.Get<ControladorEnemigo>().enemigo;
            controlador.Get<ControladorEnemigo>().AgregarDañable(this);
        }
        else if (controlador.Get<ControladorBarril>() != null)
            controlador.Get<ControladorBarril>().AgregarDañable(this);
        else
            enemigo = Enemigos.nada;

        activo = true;
    }

    public void Desactivar()
    {
        Entity.Get<PhysicsComponent>().Enabled = false;
        Entity.Get<PhysicsComponent>().CanSleep = true;
        Entity.Transform.Scale = Vector3.Zero;
        activo = false;
    }

    public void RecibirDaño(float daño)
    {
        if (!activo)
            return;

        interfaz.RecibirDaño(daño * multiplicador);
    }

    public void Empujar(Vector3 dirección)
    {
        if (!activo)
            return;

        interfaz.Empujar(dirección);
    }
}
