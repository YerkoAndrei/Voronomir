using Stride.Engine;

namespace Bozobaralika;
using static Utilidades;

public class ElementoDañable : StartupScript
{
    public Entity controlador;
    public float multiplicador;

    private IDañable interfaz;

    public override void Start()
    {
        interfaz = ObtenerInterfaz<IDañable>(controlador);
    }

    public void RecibirDaño(float daño)
    {
        interfaz.RecibirDaño(daño * multiplicador);
    }
}
