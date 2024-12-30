using System.Linq;
using Stride.Core.Mathematics;
using Stride.Navigation;
using Stride.Engine;

namespace Voronomir;
using static Utilidades;
using static Constantes;

public class ControladorJuego : SyncScript
{
    public Escenas escena;
    public Escenas siguienteEscena;

    public NavigationMesh navegación;

    private static Escenas _escena;
    private static Escenas _siguienteEscena;

    private static ControladorPersecusionesTrigonométricas[] persecutoresTrigonométricos;
    private static ControladorActivadorMuerte[] activadoresMuerte;
    private static ControladorJugador jugador;
    private static ControladorArmas armas;
    private static TransformComponent transformJugador;
    private static TransformComponent cabezaJugador;
    private static InterfazJuego interfaz;

    private static bool activo;
    private static float tiempo;

    private static int enemigos;
    private static int maxEnemigos;

    private static int secretos;
    private static int maxSecretos;

    public override void Start()
    {
        _escena = escena;
        _siguienteEscena = siguienteEscena;

        // Encuentra jugador para los demás
        var entidadJugador = Entity.Scene.Entities.Where(o => o.Get<ControladorJugador>() != null).FirstOrDefault();
        jugador = entidadJugador.Get<ControladorJugador>();
        armas = entidadJugador.Get<ControladorArmas>();
        transformJugador = entidadJugador.Transform;
        cabezaJugador = jugador.cabeza;

        // Interfaz
        interfaz = Entity.Scene.Entities.Where(o => o.Get<InterfazJuego>() != null).FirstOrDefault().Get<InterfazJuego>();

        // Persecutores
        var entidad = Entity.Scene.Entities.Where(o => o.Get<ControladorPersecusionesTrigonométricas>() != null).FirstOrDefault();
        persecutoresTrigonométricos = entidad.GetAll<ControladorPersecusionesTrigonométricas>().ToArray();

        // Activadores
        activadoresMuerte = Entity.Scene.Entities.Where(o => o.Get<ControladorActivadorMuerte>() != null)
                                                 .Select(o => o.Get<ControladorActivadorMuerte>()).ToArray();

        // Navegables
        var navegables = Entity.Scene.Entities.Where(o => o.Get<NavigationComponent>() != null).ToArray();
        foreach (var navegable in navegables)
        {
            navegable.Get<NavigationComponent>().NavigationMesh = navegación;
        }

        // Sonidos
        var entidadesSonido = Entity.Scene.Entities.Where(o => o.Get<ElementoEfecto>() != null || 
                                                               o.Get<ControladorPoder>() != null ||
                                                               o.Get<ControladorLlave>() != null ||
                                                               o.Get<ControladorPuerta>() != null ||
                                                               o.Get<ControladorEnemigo>() != null).ToArray();

        var sonidosMundo = new ISonidoMundo[entidadesSonido.Count()];
        for (int i = 0; i < sonidosMundo.Length; i++)
        {
            sonidosMundo[i] = ObtenerInterfaz<ISonidoMundo>(entidadesSonido[i]);
        }
        SistemaSonidos.AsignarSonidosMundo(sonidosMundo);

        // Contadores
        maxEnemigos = Entity.Scene.Entities.Where(o => o.Get<ControladorEnemigo>() != null).Count();
        maxSecretos = Entity.Scene.Entities.Where(o => o.Get<ControladorSecreto>() != null).Count();
        enemigos = 0;
        secretos = 0;

        activo = false;
        tiempo = 0;
    }

    public override void Update()
    {
        if (!activo)
            return;

        tiempo += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
    }

    public static void Pausar(bool _activo)
    {
        activo = _activo;
    }

    public static bool ObtenerActivo()
    {
        return activo;
    }

    public static void Morir()
    {
        activo = false;
        interfaz.Morir();
    }

    public static void Finalizar()
    {
        activo = false;
        interfaz.Finalizar();

        SistemaMemoria.GuardarTiempo(_escena, tiempo);
    }

    public static float ObtenerTiempo()
    {
        return tiempo;
    }

    public static Escenas ObtenerEscena()
    {
        return _escena;
    }

    public static Escenas ObtenerSiguienteEscena()
    {
        return _siguienteEscena;
    }

    public static void SumarEnemigo()
    {
        enemigos++;

        // Intenta activar por cada enemigo
        foreach (var activador in activadoresMuerte)
        {
            activador.Activar();
        }
    }

    public static void SumarSecreto()
    {
        secretos++;
        MostrarMensaje(SistemaTraducción.ObtenerTraducción("secreto"));
        SistemaSonidos.SonarSecreto();
    }

    public static void ApagarFísicas()
    {
        ControladorCofres.ApagarFísicas();
        jugador.ApagarFísicas();
    }

    // Jugador
    public static Vector3 ObtenerPosiciónJugador()
    {
        return transformJugador.WorldMatrix.TranslationVector;
    }

    public static Vector3 ObtenerCabezaJugador()
    {
        return cabezaJugador.WorldMatrix.TranslationVector;
    }

    public static float ObtenerVelocidad()
    {
        return jugador.ObtenerVelocidad();
    }

    public static float ObtenerAceleración()
    {
        return jugador.ObtenerAceleración();
    }

    public static float ObtenerCampoVisión()
    {
        return jugador.ObtenerCampoVisión();
    }

    public static float ObtenerCalentamientoMetralleta()
    {
        return armas.ObtenerCalentamientoMetralleta();
    }

    // Persecutores
    public static ControladorPersecusionesTrigonométricas ObtenerPersecutor(Enemigos enemigo)
    {
        return persecutoresTrigonométricos.Where(o => o.enemigo == enemigo).FirstOrDefault();
    }

    // Interfaz
    public static InterfazJuego ObtenerInterfaz()
    {
        return interfaz;
    }

    public static void MostrarMensaje(string mensaje)
    {
        interfaz.MostrarMensaje(mensaje);
    }

    public static string ObtenerTextoDuración()
    {
        return FormatearTiempo(tiempo);
    }

    public static string ObtenerTextoEnemigos()
    {
        return enemigos + " / " + maxEnemigos;
    }

    public static string ObtenerTextoSecretos()
    {
        return secretos + " / " + maxSecretos;
    }
}
