using Stride.Core.Mathematics;
using Stride.Engine;
using System.Linq;

namespace Bozobaralika;
using static Utilidades;
using static Constantes;

public class ControladorPartida : SyncScript
{
    public Escenas escena;
    public Escenas siguienteEscena;

    private static Escenas _escena;
    private static Escenas _siguienteEscena;

    private static ControladorPersecusionesTrigonométricas[] persecutoresTrigonométricos;
    private static ControladorJugador jugador;
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
        transformJugador = entidadJugador.Transform;
        cabezaJugador = jugador.cabeza;

        // Persecutores
        var entidad = Entity.Scene.Entities.Where(o => o.Get<ControladorPersecusionesTrigonométricas>() != null).FirstOrDefault();
        persecutoresTrigonométricos = entidad.GetAll<ControladorPersecusionesTrigonométricas>().ToArray();

        // Interfaz
        interfaz = Entity.Scene.Entities.Where(o => o.Get<InterfazJuego>() != null).FirstOrDefault().Get<InterfazJuego>();

        // Contadores
        maxEnemigos = Entity.Scene.Entities.Where(o => o.Get<ControladorEnemigo>() != null).Count();
        maxSecretos = Entity.Scene.Entities.Where(o => o.Get<ControladorSecreto>() != null).Count();

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
    }

    public static void SumarSecreto()
    {
        secretos++;

        // PENDIENTE: efectos
        interfaz.MostrarMensaje("Secreto Revelado");
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

    public static float ObtenerAceleración()
    {
        return jugador.ObtenerAceleración();
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
