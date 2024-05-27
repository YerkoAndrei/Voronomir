using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Voronomir;
using static Constantes;

public class ElementoDañable : StartupScript
{
    public float multiplicador;

    [DataMemberIgnore]
    public Enemigos enemigo;

    private IDañable interfaz;
    private bool activo;

    public void Iniciar(IDañable _interfaz, Enemigos _enemigo = Enemigos.nada)
    {
        interfaz = _interfaz;
        enemigo = _enemigo;
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
