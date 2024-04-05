using Stride.Core.Mathematics;
using Stride.Engine;
using System.Linq;

namespace Bozobaralika;
using static Constantes;

public class ControladorPartida : SyncScript
{
    private static ControladorPartida instancia;

    private ControladorPersecusionesTrigonométricas[] persecutoresTrigonométricos;
    private TransformComponent jugador;
    private TransformComponent cabeza;

    private bool activo;
    private float tiempo;

    public override void Start()
    {
        instancia = this;

        // Encuentra jugador para los demás
        var entidadJugador = Entity.Scene.Entities.Where(o => o.Get<ControladorJugador>() != null).FirstOrDefault();
        cabeza = entidadJugador.Get<ControladorJugador>().cabeza;
        jugador = entidadJugador.Transform;

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

    public void Perder()
    {
        activo = false;
    }

    public float ObtenerTiempo()
    {
        return tiempo;
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
}
