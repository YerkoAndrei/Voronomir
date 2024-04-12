using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;
using Stride.Physics;
using System.Linq;

namespace Bozobaralika;
using static Utilidades;
using static Constantes;

public class ControladorMovimiento : StartupScript
{
    private ControladorJugador controlador;
    private CharacterComponent cuerpo;
    private TransformComponent cabeza;
    private TransformComponent cámara;

    // Movimiento
    private Vector3 entradas;
    private Vector3 movimiento;
    private float multiplicadorVelocidad;
    private bool caminando;
    private bool detención;
    private bool bloqueo;

    // Aceleración
    private float aceleraciónInicial;
    private float tiempoRotación;

    private float tiempoIniciación;
    private float tempoIniciación;

    private float tiempoAceleración;
    private float tempoAceleración;
    
    private float minVelocidad;
    private float maxVelocidad;
    private float aceleración;

    // Cursor
    private float sensibilidad;
    private float rotaciónX;
    private float rotaciónY;
    private float rotaciónZ;

    public void Iniciar(ControladorJugador _controlador, CharacterComponent _cuerpo, TransformComponent _cabeza, TransformComponent _cámara)
    {
        controlador = _controlador;
        cuerpo = _cuerpo;
        cabeza = _cabeza;
        cámara = _cámara;

        minVelocidad = 1f;
        aceleraciónInicial = 0.1f;
        tiempoIniciación = 0.2f;
        tiempoAceleración = 20f;
        maxVelocidad = 1.5f;

        CambiarSensiblidad(false);
        multiplicadorVelocidad = ObtenerMultiplicadorVelocidad();
    }

    public void ActualizarEntradas()
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
    }

    private void Correr()
    {
        entradas = new Vector3();

        if (Input.IsKeyDown(Keys.W) || Input.IsKeyDown(Keys.Up))
            entradas -= Vector3.UnitZ;
        if (Input.IsKeyDown(Keys.A) || Input.IsKeyDown(Keys.Left))
            entradas -= Vector3.UnitX;
        if (Input.IsKeyDown(Keys.S) || Input.IsKeyDown(Keys.Down))
            entradas += Vector3.UnitZ;
        if (Input.IsKeyDown(Keys.D) || Input.IsKeyDown(Keys.Right))
            entradas += Vector3.UnitX;

        // Aceleración
        if (entradas == Vector3.Zero || detención || caminando || RevisarColisiones())
        {
            tempoAceleración = 0;
            tempoIniciación = 0;
            aceleración = minVelocidad;
            detención = false;
        }
        else
        {
            if (tempoIniciación < tiempoIniciación)
            {
                // Aceleración inicial
                tempoIniciación += (float)Game.UpdateTime.Elapsed.TotalSeconds;
                aceleración = MathUtil.SmoothStep(tempoIniciación / tiempoIniciación);
                aceleración = MathUtil.Clamp((aceleración + aceleraciónInicial), aceleraciónInicial, minVelocidad);
            }
            else
            {
                // Aceleración máxima
                tempoAceleración += (float)Game.UpdateTime.Elapsed.TotalSeconds;
                aceleración = MathUtil.SmoothStep(tempoAceleración / tiempoAceleración);
                aceleración = MathUtil.Clamp((aceleración + minVelocidad), minVelocidad, maxVelocidad);
            }
        }

        // Movimiento
        if(!bloqueo)
        {
            if (controlador.ObtenerPoder(Poderes.rapidez))
                aceleración = maxVelocidad;

            movimiento = Vector3.Transform(entradas, cuerpo.Orientation);
            movimiento.Y = 0;
            movimiento.Normalize();

            cuerpo.SetVelocity(movimiento * 10 * multiplicadorVelocidad * aceleración);
        }

        // Rotación
        rotaciónY -= Input.MouseDelta.X * sensibilidad;
        cuerpo.Orientation = Quaternion.RotationY(rotaciónY);
    }

    private bool RevisarColisiones()
    {
        // 1 enemigo reduce velocidad
        if (cuerpo.Collisions.Where(TocaEnemigo).Count() > 0)
            return true;

        // 2 pisos o paredes reducen velocidad
        if (cuerpo.Collisions.Where(o => o.ColliderA.CollisionGroup == CollisionFilterGroups.StaticFilter ||
                                         o.ColliderB.CollisionGroup == CollisionFilterGroups.StaticFilter).Count() > 1)
            return true;

        return false;
    }

    private void Mirar()
    {
        rotaciónX -= Input.MouseDelta.Y * sensibilidad;
        rotaciónX = MathUtil.Clamp(rotaciónX, -MathUtil.PiOverTwo, MathUtil.PiOverTwo);

        rotaciónZ = (entradas.X * aceleración) * -0.025f;
        tiempoRotación = (float)Game.UpdateTime.Elapsed.TotalSeconds * 10;

        cabeza.Entity.Transform.Rotation = Quaternion.RotationX(rotaciónX);
        cámara.Entity.Transform.Rotation = Quaternion.Lerp(cámara.Entity.Transform.Rotation, Quaternion.RotationZ(rotaciónZ), tiempoRotación);
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

    public float ObtenerAceleración()
    {
        return aceleración;
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

    public void CambiarVelocidadMáxima(Armas arma)
    {
        switch (arma)
        {
            case Armas.espada:
                maxVelocidad = 2.0f;
                break;
            case Armas.escopeta:
            case Armas.metralleta:
                maxVelocidad = 1.5f;
                break;
            case Armas.rifle:
                maxVelocidad = 1.4f;
                break;
            case Armas.lanzagranadas:
                maxVelocidad = 1.2f;
                break;
        }
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
