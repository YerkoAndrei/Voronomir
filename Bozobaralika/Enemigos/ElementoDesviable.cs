using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;

public class ElementoDesviable : StartupScript
{
    public Entity controlador;

    private IProyectil interfaz;

    public override void Start()
    {
        foreach (var componente in controlador.Components)
        {
            if (componente is IProyectil)
            {
                interfaz = (IProyectil)componente;
                break;
            }
        }
    }

    public void Desviar(Vector3 dirección)
    {
        interfaz.Desviar(dirección);
    }
}
