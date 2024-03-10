using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;

public class ControladorMovimiento : SyncScript
{
    private CharacterComponent cuerpo;
    private TransformComponent cabeza;

    // Movimiento
    private bool detención;
    private bool bloqueo;
    private bool caminando;
    private float multiplicadorVelocidad;
    private Vector3 movimiento;

    // Aceleración
    private float tiempoAceleración;
    private float tempoAceleración;
    private float minAceleración;
    private float maxAceleración;
    private float aceleración;

    // Cursor
    private float sensibilidad;
    private float rotaciónX;
    private float rotaciónY;

    public void Iniciar(CharacterComponent _cuerpo, TransformComponent _cabeza)
    {
        cuerpo = _cuerpo;
        cabeza = _cabeza;

        minAceleración = 1f;
        maxAceleración = 1.5f;
        tiempoAceleración = 20f;

        CambiarSensiblidad(false);
        multiplicadorVelocidad = ObtenerMultiplicadorVelocidad();
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
        if (Input.IsKeyDown(Keys.LeftShift) || Input.IsKeyDown(Keys.RightShift))
            Caminar();
        if (Input.IsKeyReleased(Keys.LeftShift) || Input.IsKeyReleased(Keys.RightShift))
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
        if (movimiento == Vector3.Zero || detención || caminando || cuerpo.Collisions.Count > 1)
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
        if(!bloqueo)
        {
            movimiento = Vector3.Transform(movimiento, cuerpo.Orientation);
            movimiento.Y = 0;

            movimiento.Normalize();
            cuerpo.SetVelocity(movimiento * 10 * multiplicadorVelocidad * aceleración);
        }

        // Rotación
        rotaciónX -= Input.MouseDelta.X * sensibilidad;
        cuerpo.Orientation = Quaternion.RotationYawPitchRoll(rotaciónX, 0, 0);
    }

    private void Mirar()
    {
        rotaciónY -= Input.MouseDelta.Y * sensibilidad;
        rotaciónY = MathUtil.Clamp(rotaciónY, -MathUtil.PiOverTwo, MathUtil.PiOverTwo);

        cabeza.Entity.Transform.Rotation = Quaternion.RotationYawPitchRoll(0, rotaciónY, 0);
    }

    private void Saltar()
    {
        if(cuerpo.IsGrounded && !bloqueo)
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

    public bool ObtenerEnSuelo()
    {
        return cuerpo.IsGrounded;
    }

    public void DetenerMovimiento()
    {
        detención = true;
    }

    public void Bloquear(bool bloquear)
    {
        detención = true;
        bloqueo = bloquear;
        cuerpo.SetVelocity(Vector3.Zero);
    }

    public void CambiarSensiblidad(bool reducir)
    {
        // PENDIENTE: ajustes
        if (reducir)
            sensibilidad = 0.4f;
        else
            sensibilidad = 1f;
    }
}
