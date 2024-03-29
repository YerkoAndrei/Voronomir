using Stride.Engine;

namespace Bozobaralika;

public class ElementoDañable : StartupScript
{
    public Entity controlador;

    private IDañable interfaz;

    public override void Start()
    {
        foreach (var componente in controlador.Components)
        {
            if (componente is IDañable)
            {
                interfaz = (IDañable)componente;
                break;
            }
        }
    }

    public void RecibirDaño(float daño)
    {
        interfaz.RecibirDaño(daño);
    }
}
