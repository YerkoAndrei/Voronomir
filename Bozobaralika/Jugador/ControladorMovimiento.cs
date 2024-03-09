using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;

public class ControladorMovimiento : SyncScript
{
    public float sensibilidad;
    public CharacterComponent cuerpo;
    public CameraComponent cámara;

    // Movimiento
    private bool detención;
    private bool caminando;
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
        tiempoAceleración = 20f;

        multiplicadorVelocidad = ObtenerMultiplicadorVelocidad();

        // Debug
        Input.LockMousePosition(true);
        Game.IsMouseVisible = false;
    }

    public override void Update()
    {
        // Correr
        Correr();
        Mirar();

        // Salto
        if (Input.IsKeyPressed(Keys.Space))
            Saltar();

        // Caminar
        if (Input.IsKeyDown(Keys.LeftCtrl) || Input.IsKeyDown(Keys.RightCtrl))
            Caminar();
        if (Input.IsKeyReleased(Keys.LeftCtrl) || Input.IsKeyReleased(Keys.RightCtrl))
            DesactivarCaminar();

        // Debug
        DebugText.Print(cuerpo.LinearVelocity.ToString(), new Int2(x: 20, y: 20));
        DebugText.Print(aceleración.ToString(), new Int2(x: 20, y: 40));
    }

    private void Correr()
    {
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
        if (movimiento == Vector3.Zero || detención || caminando)
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

    private void Caminar()
    {
        if (!cuerpo.IsGrounded)
            return;

        caminando = true;
        multiplicadorVelocidad = ObtenerMultiplicadorVelocidad() * 0.25f;
    }

    private void DesactivarCaminar()
    {
        caminando = false;
        multiplicadorVelocidad = ObtenerMultiplicadorVelocidad();
    }

    private float ObtenerMultiplicadorVelocidad()
    {
        // PENDIENTE: mejoras
        return 1;
    }

    public void DetenerMovimiento()
    {
        detención = true;
    }
}
