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
    public List<ElementoDañable> dañables { get; set; }

    private CharacterComponent cuerpo;
    private ControladorMovimiento movimiento;
    private ControladorArmas armas;
    private InterfazJuego interfaz;

    private CancellationTokenSource tokenVibración;
    private CancellationTokenSource tokenCura;

    private Vector3 posiciónCabeza;
    private bool curando;
    private float vida;
    private float vidaMax;
    private float campoVisiónMin;
    private float campoVisiónMax;

    // Laves
    private bool llaveAzul;
    private bool llaveRoja;
    private bool llaveAmarilla;

    // Poderes
    private bool dañoActivo;
    private bool invulnerabilidadActiva;
    private bool velocidadActiva;
    private float tiempoDaño;
    private float tiempoInvulnerabilidad;
    private float tiempoVelocidad;

    public override void Start()
    {
        vidaMax = 100;
        vida = vidaMax;

        campoVisiónMin = 90;
        campoVisiónMax = 120;

        interfaz = ControladorJuego.ObtenerInterfaz();
        interfaz.ActualizarVida(vida / vidaMax);

        cuerpo = Entity.Get<CharacterComponent>();
        movimiento = Entity.Get<ControladorMovimiento>();
        armas = Entity.Get<ControladorArmas>();
        foreach (var dañable in dañables)
        {
            dañable.Iniciar(this);
        }

        movimiento.Iniciar(this, cuerpo, cabeza, cámara.Entity.Transform);
        armas.Iniciar(this, movimiento, cámara, interfaz);

        tokenVibración = new CancellationTokenSource();
        tokenCura = new CancellationTokenSource();
        posiciónCabeza = cabeza.Position;

        llaveAzul = false;
        llaveRoja = false;

        // Campo visión
        ConfigurarCampoVisión();
    }

    public override void Update()
    {
        if (!ControladorJuego.ObtenerActivo())
            return;

        // Updates
        movimiento.ActualizarEntradas();
        armas.ActualizarEntradas();
        ConfigurarCampoVisión();

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

        if (dañoActivo && tiempoDaño <= 0)
            ApagarPoder(Poderes.daño);
        if (invulnerabilidadActiva && tiempoInvulnerabilidad <= 0)
            ApagarPoder(Poderes.invulnerabilidad);
        if (velocidadActiva && tiempoVelocidad <= 0)
            ApagarPoder(Poderes.velocidad);
    }

    private async void Curar()
    {
        if (curando || vida >= vidaMax || !movimiento.ObtenerEnSuelo() || armas.ObtenerAnimando())
            return;

        // Dificultad
        if (SistemaMemoria.Dificultad == Dificultades.difícil)
            return;

        curando = true;
        movimiento.Bloquear(true);
        armas.Bloquear(true);
        armas.AnimarSalida();
        await Task.Delay(200);

        var vidaActual = vida;
        var vidaCurada = vida;

        // Dificultad
        switch (SistemaMemoria.Dificultad)
        {
            case Dificultades.fácil:
                vidaCurada = MathUtil.Clamp(vida + 45, 0, vidaMax);
                break;
            case Dificultades.normal:
                vidaCurada = MathUtil.Clamp(vida + 15, 0, vidaMax);
                break;
        }

        float duración = 1f;
        float tiempoLerp = 0;
        float tiempo = 0;

        var token = tokenCura.Token;
        while (tiempoLerp < duración)
        {
            // Recibir daño cancela cura
            if (token.IsCancellationRequested)
                break;

            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            vida = MathUtil.Lerp(vidaActual, vidaCurada, tiempo);
            interfaz.ActualizarVida(vida / vidaMax);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        await armas.AnimarEntrada();
        await Task.Delay(100);

        // Control errores en cambio de escena
        if (!cuerpo.Enabled)
            return;

        movimiento.Bloquear(false);
        armas.Bloquear(false);
        curando = false;
    }

    public void RecibirDaño(float daño)
    {
        if (!ControladorJuego.ObtenerActivo())
            return;

        tokenCura.Cancel();
        tokenCura = new CancellationTokenSource();

        SistemaSonidos.SonarDaño();
        movimiento.DetenerMovimiento();
        VibrarCámara(10, 10);

        if (ObtenerPoder(Poderes.invulnerabilidad))
            return;

        // Dificultad
        if (SistemaMemoria.Dificultad == Dificultades.fácil)
            daño *= 0.75f;

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
        ControladorJuego.Morir();
        SistemaSonidos.SonarMorir();
    }

    public void ApagarFísicas()
    {
        cuerpo.SetVelocity(Vector3.Zero);
        cuerpo.Enabled = false;
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
                llaveRoja = true;
                break;
            case Llaves.amarilla:
                llaveAmarilla = true;
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
            case Llaves.amarilla:
                return llaveAmarilla;
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
                dañoActivo = true;
                ControladorJuego.MostrarMensaje(SistemaTraducción.ObtenerTraducción("poderDaño"));
                break;
            case Poderes.invulnerabilidad:
                vida = vidaMax;
                interfaz.ActualizarVida(vida / vidaMax);
                tiempoInvulnerabilidad = 30;
                invulnerabilidadActiva = true;
                ControladorJuego.MostrarMensaje(SistemaTraducción.ObtenerTraducción("poderInvulnerabilidad"));
                break;
            case Poderes.velocidad:
                tiempoVelocidad = 30;
                velocidadActiva = true;
                movimiento.ReiniciarPorcentajeVelocidad(true);
                ControladorJuego.MostrarMensaje(SistemaTraducción.ObtenerTraducción("poderVelocidad"));
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
                dañoActivo = false;
                break;
            case Poderes.invulnerabilidad:
                tiempoInvulnerabilidad = 0;
                invulnerabilidadActiva = false;
                break;
            case Poderes.velocidad:
                tiempoVelocidad = 0;
                velocidadActiva = false;
                movimiento.ReiniciarPorcentajeVelocidad(false);
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

    public void ConfigurarCampoVisión()
    {
        if (armas.ObtenerUsoMira())
            return;

        var campoVisión = MathUtil.Lerp(campoVisiónMin, campoVisiónMax, movimiento.ObtenerPorcentajeAceleración());
        cámara.VerticalFieldOfView = campoVisión;
    }

    public float ObtenerCampoVisión()
    {
        return cámara.VerticalFieldOfView;
    }

    public float ObtenerCampoVisiónMínimo()
    {
        return campoVisiónMin;
    }

    public float ObtenerCampoVisiónMira()
    {
        return 20;
    }

    public void VibrarCámara(float fuerza, int iteraciones)
    {
        // Cancela vibración actual para iniciar nueva
        tokenVibración.Cancel();
        tokenVibración = new CancellationTokenSource();

        var duración = 0.014f;
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
        var rotaciones = new List<Quaternion>();
        for (int i = 0; i < iteraciones; i++)
        {
            var rotación = Quaternion.Identity;
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
            cámara.Entity.Transform.Rotation = objetivo;
        }
        cámara.Entity.Transform.Rotation = Quaternion.Identity;
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
