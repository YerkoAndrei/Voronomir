using Stride.Core.Mathematics;
using Stride.Engine;
using System.Linq;

namespace Bozobaralika;
using static Constantes;

public class ControladorPartida : SyncScript
{
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

        // Debug
        Input.LockMousePosition(true);
        Game.IsMouseVisible = false;
    }

    public override void Update()
    {
        if (!activo)
            return;

        tiempo += (float)Game.UpdateTime.Elapsed.TotalSeconds;
    }

    public static bool ObtenerActivo()
    {
        return instancia.activo;
    }

    public static void Perder()
    {
        instancia.activo = false;
        instancia.interfaz.Morir();
    }

    public static float ObtenerTiempo()
    {
        return instancia.tiempo;
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
