using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;

public class ControladorMovimiento : SyncScript
{
    public CharacterComponent cuerpo;
    public CameraComponent cámara;

    private Vector3 movimiento;
    private float multiplicadorVelocidad;
    private float aceleración;

    public override void Start()
    {
        multiplicadorVelocidad = 4;
    }

    // PENDIENTE: Control
    public override void Update()
    {
        // Correr
        if (cuerpo.IsGrounded)
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
        movimiento = new Vector3();

        if (Input.IsKeyDown(Keys.W) || Input.IsKeyDown(Keys.Up))
            movimiento += -Vector3.UnitZ;
        if (Input.IsKeyDown(Keys.A) || Input.IsKeyDown(Keys.Left))
            movimiento += -Vector3.UnitX;
        if (Input.IsKeyDown(Keys.S) || Input.IsKeyDown(Keys.Down))
            movimiento += +Vector3.UnitZ;
        if (Input.IsKeyDown(Keys.D) || Input.IsKeyDown(Keys.Right))
            movimiento += +Vector3.UnitX;

        // Aceleración
        if (movimiento == Vector3.Zero)
            aceleración = 1;
        else
        {
            aceleración += (float)Game.UpdateTime.Elapsed.TotalSeconds * 4;
            aceleración = MathUtil.Clamp(aceleración, 1, 2);
        }

        movimiento.Normalize();
        cuerpo.SetVelocity(movimiento * aceleración * multiplicadorVelocidad);
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
