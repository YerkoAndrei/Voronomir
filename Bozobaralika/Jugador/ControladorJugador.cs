using System.Linq;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;
using Stride.Physics;

namespace Bozobaralika;
using static Sistema;

public class ControladorJugador : SyncScript
{
    public TransformComponent cabeza;
    public CameraComponent cámara;

    private CharacterComponent cuerpo;
    private ControladorMovimiento movimiento;
    private ControladorArmas armas;
    private InterfazJuego interfaz;

    private Vector3 posiciónCabeza;
    private bool curando;
    private float vida;
    private float vidaMax;

    public override void Start()
    {
        vidaMax = 100;
        vida = vidaMax;

        interfaz = Entity.Scene.Entities.Where(o => o.Get<InterfazJuego>() != null).FirstOrDefault().Get<InterfazJuego>();
        interfaz.ActualizarVida(vida / vidaMax);

        cuerpo = Entity.Get<CharacterComponent>();
        movimiento = Entity.Get<ControladorMovimiento>();
        armas = Entity.Get<ControladorArmas>();
        
        movimiento.Iniciar(cuerpo, cabeza);
        armas.Iniciar(this, movimiento, cámara, interfaz);

        posiciónCabeza = cabeza.Position;

        // Debug
        Input.LockMousePosition(true);
        Game.IsMouseVisible = false;
    }

    public override void Update()
    {
        // Cura
        if (Input.IsKeyPressed(Keys.F))
            Curar();

        // Debug
        DebugText.Print(vida + "/" + vidaMax, new Int2(x: 20, y: 140));
        if (Input.IsKeyPressed(Keys.K))
            RecibirDaño(10);
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
        float vidaCurada = vida + ObtnerCura();

        float duración = 0.8f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            vida = MathUtil.Lerp(vidaActual, vidaCurada, tiempo);
            vida = MathUtil.Clamp(vida, 0, vidaMax);
            interfaz.ActualizarVida(vida / vidaMax);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Task.Delay(1);
        }

        await Task.Delay(200);
        movimiento.Bloquear(false);
        armas.Bloquear(false);
        curando = false;
    }

    public float ObtnerCura()
    {
        // PENDIENTE: mejoras
        return 20;
    }

    public void RecibirDaño(float daño)
    {
        vida -= daño;
        interfaz.ActualizarVida(vida / vidaMax);

        if (vida <= 0)
            Morir();
    }

    private void Morir()
    {
        // PENDIENTE: efectos
        interfaz.Morir();
    }

    public async void VibrarCámara(float fuerza)
    {
        float duración = 0.01f;
        int iteraciones = 6;
        int iteración = 0;

        while (iteración < iteraciones)
        {
            var inicial = cabeza.Position;
            var objetivo = posiciónCabeza;

            // Aleatorio
            if (iteración < (iteraciones - 1))
            {
                var aletorioX = RangoAleatorio(-0.02f, 0.02f);
                var aletorioY = RangoAleatorio(-0.02f, 0.02f);
                var aletorioZ = RangoAleatorio(-0.01f, 0.01f);
                objetivo = posiciónCabeza + new Vector3(aletorioX, aletorioY, aletorioZ) * fuerza;
            }

            float tiempoLerp = 0;
            float tiempo = 0;

            while (tiempoLerp < duración)
            {
                tiempo = tiempoLerp / duración;
                cabeza.Position = Vector3.Lerp(inicial, objetivo, tiempo);

                tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
                await Task.Delay(1);
            }
            iteración++;
        }

        cabeza.Position = posiciónCabeza;
    }
}
