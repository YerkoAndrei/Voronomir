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

    private Vector3 movimiento;
    private float multiplicadorVelocidad;
    private float aceleración;

    private float rotaciónX;
    private float rotaciónY;

    public override void Start()
    {
        multiplicadorVelocidad = 4;

        // interfaz
        Input.LockMousePosition(true);
        Game.IsMouseVisible = false;
    }

    // PENDIENTE: Control
    public override void Update()
    {
        // interfaz
        if (Input.IsKeyPressed(Keys.Space))
        {
            Input.LockMousePosition(false);
            Game.IsMouseVisible = true;
        }

        // Correr
        Mirar();
        Correr();

        // Salto
        if (Input.IsKeyPressed(Keys.Space))
            Saltar();

        // Deslizamiento
        if (Input.IsKeyPressed(Keys.LeftShift) || Input.IsKeyPressed(Keys.RightShift))
            Deslizar();

        // Caminar
        if (Input.IsKeyDown(Keys.LeftCtrl) || Input.IsKeyDown(Keys.RightCtrl))
            Caminar();
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
        if (movimiento == Vector3.Zero)
            aceleración = 1;
        else
        {
            aceleración += (float)Game.UpdateTime.Elapsed.TotalSeconds * 4;
            aceleración = MathUtil.Clamp(aceleración, 1, 2);
        }

        // Movimiento
        movimiento = Vector3.Transform(movimiento, cámara.Entity.Transform.Rotation);
        movimiento.Y = 0;

        movimiento.Normalize();
        cuerpo.SetVelocity(movimiento * aceleración * multiplicadorVelocidad);
    }

    private void Mirar()
    {
        rotaciónX -= Input.MouseDelta.X * sensibilidad;
        rotaciónY -= Input.MouseDelta.Y * sensibilidad;
        rotaciónY = MathUtil.Clamp(rotaciónY, -MathUtil.PiOverTwo, MathUtil.PiOverTwo);

        cámara.Entity.Transform.Rotation = Quaternion.RotationYawPitchRoll(rotaciónX, rotaciónY, 0);
    }

    private void Saltar()
    {
        if(cuerpo.IsGrounded)
            cuerpo.Jump();
    }

    private void Deslizar()
    {

    }

    private void Caminar()
    {
        multiplicadorVelocidad = 0.2f;
    }
}
