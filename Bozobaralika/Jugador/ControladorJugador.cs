using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Stride.Core.Mathematics;
using Stride.Physics;
using Stride.Engine;
using Stride.Input;

namespace Bozobaralika;
using static Utilidades;
using static Constantes;

public class ControladorJugador : SyncScript, IDañable
{
    public TransformComponent cabeza;
    public CameraComponent cámara;
    public List<RigidbodyComponent> cuerpos { get; set; }

    private CharacterComponent cuerpo;
    private ControladorMovimiento movimiento;
    private ControladorArmas armas;
    private InterfazJuego interfaz;

    private Vector3 posiciónCabeza;
    private bool curando;
    private float vida;
    private float vidaMax;

    private bool llaveAzul;
    private bool llaveRoja;

    public override void Start()
    {
        vidaMax = 100;
        vida = vidaMax;

        interfaz = Entity.Scene.Entities.Where(o => o.Get<InterfazJuego>() != null).FirstOrDefault().Get<InterfazJuego>();
        interfaz.ActualizarVida(vida / vidaMax);

        cuerpo = Entity.Get<CharacterComponent>();
        movimiento = Entity.Get<ControladorMovimiento>();
        armas = Entity.Get<ControladorArmas>();
        
        movimiento.Iniciar(cuerpo, cabeza, cámara.Entity.Transform);
        armas.Iniciar(this, movimiento, cámara, interfaz);

        posiciónCabeza = cabeza.Position;

        llaveAzul = false;
        llaveRoja = false;
    }

    public override void Update()
    {
        // Updates
        movimiento.ActualizarEntradas();
        armas.ActualizarEntradas();

        // Cura
        if (Input.IsKeyPressed(Keys.F))
            Curar();
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
        float vidaCurada = vida + ObtenerCura();

        float duración = 1f;
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

    public float ObtenerCura()
    {
        // PENDIENTE: mejoras
        return 10;
    }

    public void RecibirDaño(float daño)
    {
        vida -= daño;
        interfaz.ActualizarVida(vida / vidaMax);
        movimiento.DetenerMovimiento();
        VibrarCámara(10, 10);

        if (vida <= 0)
            Morir();
    }

    private void Morir()
    {
        // PENDIENTE: efectos
        interfaz.Morir();
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

    public void VibrarCámara(float fuerza, int iteraciones)
    {
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

            while (tiempoLerp < duración)
            {
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

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            cabeza.Position = Vector3.Lerp(retroceso, posiciónCabeza, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Task.Delay(1);
        }
        cabeza.Position = posiciónCabeza;
    }
}
