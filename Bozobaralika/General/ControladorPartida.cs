using Stride.Core.Mathematics;
using Stride.Engine;
using System.Linq;

namespace Bozobaralika;
using static Constantes;

public class ControladorPartida : SyncScript
{
    public Escenas escena;
    public Escenas siguienteEscena;

    // Singleton por escena
    private static ControladorPartida instancia;

    private ControladorPersecusionesTrigonométricas[] persecutoresTrigonométricos;
    private TransformComponent jugador;
    private TransformComponent cabeza;
    private InterfazJuego interfaz;

    private bool activo;
    private float tiempo;

    public override void Start()
    {
        instancia = this;

        // Encuentra jugador para los demás
        var entidadJugador = Entity.Scene.Entities.Where(o => o.Get<ControladorJugador>() != null).FirstOrDefault();
        cabeza = entidadJugador.Get<ControladorJugador>().cabeza;
        jugador = entidadJugador.Transform;

        // Persecutores
        var entidad = Entity.Scene.Entities.Where(o => o.Get<ControladorPersecusionesTrigonométricas>() != null).FirstOrDefault();
        persecutoresTrigonométricos = entidad.GetAll<ControladorPersecusionesTrigonométricas>().ToArray();

        // Interfaz
        interfaz = Entity.Scene.Entities.Where(o => o.Get<InterfazJuego>() != null).FirstOrDefault().Get<InterfazJuego>();

        activo = true;
        tiempo = 0;
    }

    public override void Update()
    {
        if (!activo)
            return;

        tiempo += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
    }

    public static void Pausar(bool activo)
    {
        instancia.activo = activo;
    }

    public static bool ObtenerActivo()
    {
        return instancia.activo;
    }

    public static void Morir()
    {
        instancia.activo = false;
        instancia.interfaz.Morir();
    }

    public static void Finalizar()
    {
        instancia.activo = false;
        instancia.interfaz.Finalizar();
    }

    public static float ObtenerTiempo()
    {
        return instancia.tiempo;
    }

    public static Escenas ObtenerEscena()
    {
        return instancia.escena;
    }

    public static Escenas ObtenerSiguienteEscena()
    {
        return instancia.siguienteEscena;
    }

    // Jugador
    public static Vector3 ObtenerPosiciónJugador()
    {
        return instancia.jugador.WorldMatrix.TranslationVector;
    }

    public static Vector3 ObtenerCabezaJugador()
    {
        return instancia.cabeza.WorldMatrix.TranslationVector;
    }

    // Persecutores
    public static ControladorPersecusionesTrigonométricas ObtenerPersecutor(Enemigos enemigo)
    {
        return instancia.persecutoresTrigonométricos.Where(o => o.enemigo == enemigo).FirstOrDefault();
    }

    // Interfaz
    public static InterfazJuego ObtenerInterfaz()
    {
        return instancia.interfaz;
    }
}
