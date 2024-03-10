using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;
using System.Linq;

namespace Bozobaralika;

public class ControladorJugador : SyncScript
{
    public CharacterComponent cuerpo;
    public TransformComponent cabeza;
    public CameraComponent cámara;

    public ControladorMovimiento movimiento;
    public ControladorArmas armas;

    private InterfazJuego interfaz;
    private float vida;
    private float vidaMax;

    public override void Start()
    {
        vidaMax = 100;
        vida = vidaMax;

        interfaz = Entity.Scene.Entities.Where(o => o.Get<InterfazJuego>() != null).FirstOrDefault().Get<InterfazJuego>();

        movimiento.Iniciar(cuerpo, cabeza);
        armas.Iniciar(movimiento, cabeza, cámara, interfaz);

        // Debug
        Input.LockMousePosition(true);
        Game.IsMouseVisible = false;
    }

    public override void Update()
    {
        // Debug
        DebugText.Print(vida + "/" + vidaMax, new Int2(x: 20, y: 140));
    }

    public void RecibirDaño(float daño)
    {
        vida -= daño;

        if (vida <= 0)
            Morir();
    }

    private void Morir()
    {
        // interfaz
    }
}
