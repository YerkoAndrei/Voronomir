﻿using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;
using Stride.Physics;
using System.Linq;

namespace Bozobaralika;

public class ControladorMovimiento : SyncScript
{
    private CharacterComponent cuerpo;
    private TransformComponent cabeza;

    // Movimiento
    private Vector3 movimiento;
    private float multiplicadorVelocidad;
    private bool caminando;
    private bool detención;
    private bool bloqueo;

    // Aceleración
    private float aceleraciónInicial;
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

    public void Iniciar(CharacterComponent _cuerpo, TransformComponent _cabeza)
    {
        cuerpo = _cuerpo;
        cabeza = _cabeza;

        minVelocidad = 1f;
        aceleraciónInicial = 0.1f;
        tiempoIniciación = 0.2f;
        tiempoAceleración = 20f;
        CambiarVelocidadMáxima(false);

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
        if (movimiento == Vector3.Zero || detención || caminando || RevisarColisiones())
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
            movimiento = Vector3.Transform(movimiento, cuerpo.Orientation);
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
        if (cuerpo.Collisions.Where(o => o.ColliderA.CollisionGroup == CollisionFilterGroups.KinematicFilter ||
                                         o.ColliderB.CollisionGroup == CollisionFilterGroups.KinematicFilter).Count() > 0)
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

        cabeza.Entity.Transform.Rotation = Quaternion.RotationX(rotaciónX);
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

    public void CambiarVelocidadMáxima(bool melé)
    {
        if (melé)
            maxVelocidad = 2.0f;
        else
            maxVelocidad = 1.5f;
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
