using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;
using static Utilidades;
using static Constantes;

public class ElementoDañable : StartupScript
{
    public Entity controlador;
    public bool inmunidadExplosión;
    public float multiplicador;

    [DataMemberIgnore]
    public Enemigos enemigo;

    private IDañable interfaz;

    public override void Start()
    {
        interfaz = ObtenerInterfaz<IDañable>(controlador);

        // Enemigo / Jugador
        if (controlador.Get<ControladorEnemigo>() != null)
            enemigo = controlador.Get<ControladorEnemigo>().enemigo;
        else
            enemigo = Enemigos.nada;
    }

    public void RecibirDaño(float daño, bool explosión)
    {
        // Explosiones solo dañan cuerpo principal
        if (explosión && inmunidadExplosión)
            return;

        interfaz.RecibirDaño(daño * multiplicador);
    }

    public void Empujar(Vector3 dirección)
    {
        interfaz.Empujar(dirección);
    }
}
