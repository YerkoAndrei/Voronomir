using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using Stride.Core.Mathematics;
using Stride.Physics;
using Stride.Engine;
using Stride.Input;

namespace Voronomir;
using static Utilidades;
using static Constantes;

public class ControladorJugador : SyncScript, IDañable
{
    public TransformComponent cabeza;
    public CameraComponent cámara;

    private CharacterComponent cuerpo;
    private ControladorMovimiento movimiento;
    private ControladorArmas armas;
    private InterfazJuego interfaz;

    private CancellationTokenSource tokenVibración;
    private Vector3 posiciónCabeza;
    private bool curando;
    private float vida;
    private float vidaMax;

    // Laves
    private bool llaveAzul;
    private bool llaveRoja;

    // Poderes
    private float tiempoDaño;
    private float tiempoInvulnerabilidad;
    private float tiempoVelocidad;

    public override void Start()
    {
        vidaMax = 100;
        vida = vidaMax;

        interfaz = ControladorPartida.ObtenerInterfaz();
        interfaz.ActualizarVida(vida / vidaMax);

        cuerpo = Entity.Get<CharacterComponent>();
        movimiento = Entity.Get<ControladorMovimiento>();
        armas = Entity.Get<ControladorArmas>();
        
        movimiento.Iniciar(this, cuerpo, cabeza, cámara.Entity.Transform);
        armas.Iniciar(this, movimiento, cámara, interfaz);

        tokenVibración = new CancellationTokenSource();
        posiciónCabeza = cabeza.Position;

        llaveAzul = false;
        llaveRoja = false;
    }

    public override void Update()
    {
        if (!ControladorPartida.ObtenerActivo())
            return;

        // Updates
        movimiento.ActualizarEntradas();
        armas.ActualizarEntradas();

        // Cura
        if (Input.IsKeyPressed(Keys.F))
            Curar();

        // Poderes        
        if (tiempoDaño > 0)
            tiempoDaño -= (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
        if (tiempoInvulnerabilidad > 0)
            tiempoInvulnerabilidad -= (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
        if (tiempoVelocidad > 0)
            tiempoVelocidad -= (float)Game.UpdateTime.WarpElapsed.TotalSeconds;

        if(tiempoDaño <= 0)
            ApagarPoder(Poderes.daño);
        if (tiempoInvulnerabilidad <= 0)
            ApagarPoder(Poderes.invulnerabilidad);
        if (tiempoVelocidad <= 0)
            ApagarPoder(Poderes.velocidad);
    }

    private async void Curar()
    {
        if (curando || vida >= vidaMax || !movimiento.ObtenerEnSuelo())
            return;

        curando = true;
        movimiento.Bloquear(true);
        armas.Bloquear(true);
        await Task.Delay(200);

        float vidaActual = vida;
        float vidaCurada = vida + 10;

        float duración = 1f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            vida = MathUtil.Lerp(vidaActual, vidaCurada, tiempo);
            vida = MathUtil.Clamp(vida, 0, vidaMax);
            interfaz.ActualizarVida(vida / vidaMax);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        await Task.Delay(200);
        movimiento.Bloquear(false);
        armas.Bloquear(false);
        curando = false;
    }

    public void RecibirDaño(float daño)
    {
        if (!ControladorPartida.ObtenerActivo())
            return;

        SistemaSonidos.SonarDaño();
        movimiento.DetenerMovimiento();
        VibrarCámara(10, 10);

        if (ObtenerPoder(Poderes.invulnerabilidad))
            return;

        vida -= daño;
        interfaz.ActualizarVida(vida / vidaMax);

        if (vida <= 0)
            Morir();
    }

    public void Empujar(Vector3 dirección)
    {
        cuerpo.Jump(dirección * (cuerpo.JumpSpeed * 0.8f));
    }

    private void Morir()
    {
        armas.GuardarArma();
        AnimarMuerte();
        ControladorPartida.Morir();
        SistemaSonidos.SonarMorir();
    }

    public float ObtenerVelocidad()
    {
        return cuerpo.LinearVelocity.Length();
    }

    public float ObtenerAceleración()
    {
        return movimiento.ObtenerAceleración();
    }

    public void GuardarLlave(Llaves llave)
    {
        interfaz.ActivarLlave(llave);

        switch (llave)
        {
            case Llaves.azul:
                llaveAzul = true;
                break;
            case Llaves.roja:
                llaveRoja= true;
                break;
        }
    }

    public bool ObtenerLlave(Llaves llave)
    {
        switch (llave)
        {
            case Llaves.nada:
                return true;
            case Llaves.azul:
                return llaveAzul;
            case Llaves.roja:
                return llaveRoja;
            default:
                return false;
        }
    }

    public void ActivarPoder(Poderes poder)
    {
        interfaz.ActivarPoder(poder, true);
        switch (poder)
        {
            case Poderes.daño:
                tiempoDaño = 30;
                ControladorPartida.MostrarMensaje(SistemaTraducción.ObtenerTraducción("poderDaño"));
                break;
            case Poderes.invulnerabilidad:
                vida = vidaMax;
                interfaz.ActualizarVida(vida / vidaMax);
                tiempoInvulnerabilidad = 30;
                ControladorPartida.MostrarMensaje(SistemaTraducción.ObtenerTraducción("poderInvulnerabilidad"));
                break;
            case Poderes.velocidad:
                tiempoVelocidad = 30;
                ControladorPartida.MostrarMensaje(SistemaTraducción.ObtenerTraducción("poderVelocidad"));
                break;
        }
    }

    private void ApagarPoder(Poderes poder)
    {
        interfaz.ActivarPoder(poder, false);
        switch (poder)
        {
            case Poderes.daño:
                tiempoDaño = 0;
                break;
            case Poderes.invulnerabilidad:
                tiempoInvulnerabilidad = 0;
                break;
            case Poderes.velocidad:
                tiempoVelocidad = 0;
                break;
        }
    }

    public bool ObtenerPoder(Poderes poder)
    {
        switch (poder)
        {
            case Poderes.daño:
                return tiempoDaño > 0;
            case Poderes.invulnerabilidad:
                return tiempoInvulnerabilidad > 0;
            case Poderes.velocidad:
                return tiempoVelocidad > 0;
             default:
                return false;
        }
    }

    public void VibrarCámara(float fuerza, int iteraciones)
    {
        // Cancela vibración actual para iniciar nueva
        tokenVibración.Cancel();
        tokenVibración = new CancellationTokenSource();

        var duración = 0.01f;
        RotarCámara(duración, fuerza, iteraciones);

        var duraciónMovimiento = duración * iteraciones;
        MoverCámara(duraciónMovimiento, fuerza);
    }

    private async void RotarCámara(float duración, float fuerza, int iteraciones)
    {
        // Vibración se suaviza al final
        var aleatorios = new List<Vector2>();
        for (int i = 0; i < iteraciones; i++)
        {
            aleatorios.Add(new Vector2(RangoAleatorio(-0.05f, 0.05f), RangoAleatorio(-0.05f, 0.05f)));
        }
        aleatorios = aleatorios.OrderByDescending(o => Vector2.Distance(o, Vector2.Zero)).ToList();

        // Vectores ordenados a cuaterniones
        var rotaciónCabeza = cámara.Entity.Transform.Rotation;
        var rotaciones = new List<Quaternion>();
        for (int i = 0; i < iteraciones; i++)
        {
            var rotación = rotaciónCabeza;
            rotación *= Quaternion.RotationX(aleatorios[i].X);
            rotación *= Quaternion.RotationY(aleatorios[i].Y);
            rotaciones.Add(rotación * fuerza);
        }

        // Iteraciones de rotación
        for (int i = 0; i < iteraciones; i++)
        {
            var inicial = cámara.Entity.Transform.Rotation;
            var objetivo = Quaternion.Identity;

            // Última iteración vuelve
            if (i < (iteraciones - 1))
                objetivo = rotaciones[i];

            float tiempoLerp = 0;
            float tiempo = 0;

            var token = tokenVibración.Token;
            while (tiempoLerp < duración)
            {
                if (token.IsCancellationRequested)
                    return;

                tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
                cámara.Entity.Transform.Rotation = Quaternion.Lerp(inicial, objetivo, tiempo);

                tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
                await Task.Delay(1);
            }
        }
        cámara.Entity.Transform.Rotation = rotaciónCabeza;
    }

    private async void MoverCámara(float duración, float fuerza)
    {
        var retroceso = posiciónCabeza + (new Vector3(0, 0, 0.012f) * fuerza);
        float tiempoLerp = 0;
        float tiempo = 0;

        var token = tokenVibración.Token;
        while (tiempoLerp < duración)
        {
            if (token.IsCancellationRequested)
                return;

            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            cabeza.Position = Vector3.Lerp(retroceso, posiciónCabeza, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Task.Delay(1);
        }
        cabeza.Position = posiciónCabeza;
    }

    private async void AnimarMuerte()
    {
        var inicio = posiciónCabeza;
        var objetivo = posiciónCabeza + (Vector3.UnitY * -1.3f);
        float duración = 0.25f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            cabeza.Position = Vector3.Lerp(inicio, objetivo, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Task.Delay(1);
        }
        cabeza.Position = objetivo;
    }
}
