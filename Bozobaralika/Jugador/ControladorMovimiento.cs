using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;

public class ControladorMovimiento : SyncScript
{
    public CharacterComponent cuerpo;
    public CameraComponent cámara;
    public float sensibilidad;

    // Movimiento
    private bool detención;
    private Vector3 movimiento;
    private float multiplicadorVelocidad;

    // Aceleración
    private float tiempoAceleración;
    private float tempoAceleración;
    private float minAceleración;
    private float maxAceleración;
    private float aceleración;

    // Cursor
    private float rotaciónX;
    private float rotaciónY;

    public override void Start()
    {
        minAceleración = 1f;
        maxAceleración = 1.5f;
        tiempoAceleración = 20;

        multiplicadorVelocidad = ObtenerMultiplicadorVelocidad();

        // Debug
        Input.LockMousePosition(true);
        Game.IsMouseVisible = false;
    }

    // PENDIENTE: Control
    public override void Update()
    {
        if (Input.IsMouseButtonPressed(MouseButton.Left))
            PausarMovimiento();

        // Correr
        Correr();
        Mirar();

        // Salto
        if (Input.IsKeyPressed(Keys.Space))
            Saltar();

        // Deslizamiento
        if (Input.IsKeyPressed(Keys.LeftShift) || Input.IsKeyPressed(Keys.RightShift))
            Deslizar();

        // Caminar
        if (Input.IsKeyPressed(Keys.LeftCtrl) || Input.IsKeyPressed(Keys.RightCtrl))
            Caminar(true);
        if (Input.IsKeyReleased(Keys.LeftCtrl) || Input.IsKeyReleased(Keys.RightCtrl))
            Caminar(false);
    }

    private void Correr()
    {
        // PENDIENTE: controlar cambios bruscos
        movimiento = new Vector3();

        if (Input.IsKeyDown(Keys.W) || Input.IsKeyDown(Keys.Up))
            movimiento -= Vector3.UnitZ;
        if (Input.IsKeyDown(Keys.A) || Input.IsKeyDown(Keys.Left))
            movimiento -= Vector3.UnitX;
        if (Input.IsKeyDown(Keys.S) || Input.IsKeyDown(Keys.Down))
            movimiento += Vector3.UnitZ;
        if (Input.IsKeyDown(Keys.D) || Input.IsKeyDown(Keys.Right))
            movimiento += Vector3.UnitX;

        // Aceleración
        if (movimiento == Vector3.Zero || detención)
        {
            tempoAceleración = 0;
            aceleración = minAceleración;
            detención = false;
        }
        else
        {
            tempoAceleración += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            aceleración = MathUtil.SmoothStep(tempoAceleración / tiempoAceleración);
            aceleración = MathUtil.Clamp((aceleración + minAceleración), minAceleración, maxAceleración);
        }

        // Movimiento
        movimiento = Vector3.Transform(movimiento, cuerpo.Orientation);
        movimiento.Y = 0;

        movimiento.Normalize();
        cuerpo.SetVelocity(movimiento * 10 * multiplicadorVelocidad * aceleración);

        // Rotación
        rotaciónX -= Input.MouseDelta.X * sensibilidad;
        cuerpo.Orientation = Quaternion.RotationYawPitchRoll(rotaciónX, 0, 0);

        // Debug
        DebugText.Print(aceleración.ToString(), new Int2(x: 20, y: 40));
        DebugText.Print(cuerpo.LinearVelocity.ToString(), new Int2(x: 20, y: 20));
    }

    private void Mirar()
    {
        rotaciónY -= Input.MouseDelta.Y * sensibilidad;
        rotaciónY = MathUtil.Clamp(rotaciónY, -MathUtil.PiOverTwo, MathUtil.PiOverTwo);

        cámara.Entity.Transform.Rotation = Quaternion.RotationYawPitchRoll(0, rotaciónY, 0);
    }

    private void Saltar()
    {
        if(cuerpo.IsGrounded)
            cuerpo.Jump();
    }

    private void Deslizar()
    {

    }

    private void Caminar(bool caminar)
    {
        if(caminar)
            multiplicadorVelocidad = ObtenerMultiplicadorVelocidad() / 2f;
        else
            multiplicadorVelocidad = ObtenerMultiplicadorVelocidad();
    }

    private float ObtenerMultiplicadorVelocidad()
    {
        // PENDIENTE: mejoras
        return 1;
    }

    public void PausarMovimiento()
    {
        detención = true;
    }
}
