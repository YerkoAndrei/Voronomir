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
    private static TransformComponent jugador;
    private static TransformComponent cabeza;
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
        cabeza = entidadJugador.Get<ControladorJugador>().cabeza;
        jugador = entidadJugador.Transform;

        // Persecutores
        var entidad = Entity.Scene.Entities.Where(o => o.Get<ControladorPersecusionesTrigonométricas>() != null).FirstOrDefault();
        persecutoresTrigonométricos = entidad.GetAll<ControladorPersecusionesTrigonométricas>().ToArray();

        // Interfaz
        interfaz = Entity.Scene.Entities.Where(o => o.Get<InterfazJuego>() != null).FirstOrDefault().Get<InterfazJuego>();

        // Contadores
        maxEnemigos = Entity.Scene.Entities.Where(o => o.Get<ControladorEnemigo>() != null).Count();
        maxSecretos = Entity.Scene.Entities.Where(o => o.Get<ControladorSecreto>() != null).Count();

        activo = true;
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
    }

    // Jugador
    public static Vector3 ObtenerPosiciónJugador()
    {
        return jugador.WorldMatrix.TranslationVector;
    }

    public static Vector3 ObtenerCabezaJugador()
    {
        return cabeza.WorldMatrix.TranslationVector;
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
