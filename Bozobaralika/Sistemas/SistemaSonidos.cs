using Stride.Engine;

namespace Bozobaralika;

public class SistemaSonidos : StartupScript
{
    private static SistemaSonidos instancia;

    public override void Start()
    {
        instancia = this;
    }
}
