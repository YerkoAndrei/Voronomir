using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Particles.Components;
using Stride.Audio;
using Stride.Engine;
using Stride.Core;

namespace Voronomir;
using static Utilidades;
using static Constantes;

public class ControladorPoder : AsyncScript, ISonidoMundo
{
    public Poderes poder;
    public TransformComponent modelo;
    public ParticleSystemComponent partículas;
    public AudioEmitterComponent emisor;
    private bool activo;

    private PhysicsComponent cuerpo;
    private AudioEmitterSoundController sonido;
    private float velocidadRotación;
    private Vector3 posiciónArriba;
    private Vector3 posiciónAbajo;

    private bool finSonido;
    [DataMemberIgnore] public float distanciaSonido { get; set; }
    [DataMemberIgnore] public float distanciaJugador { get; set; }

    public override async Task Execute()
    {
        cuerpo = Entity.Get<PhysicsComponent>();

        emisor.UseHRTF = bool.Parse(SistemaMemoria.ObtenerConfiguración(Configuraciones.hrtf));
        sonido = emisor["sonido"];

        posiciónArriba = modelo.Position + new Vector3(0, 0.2f, 0);
        posiciónAbajo = modelo.Position;

        activo = true;
        velocidadRotación = 2;
        AnimarMovimiento(posiciónArriba);
        Rotar();

        partículas.Enabled = true;
        partículas.ParticleSystem.ResetSimulation();

        distanciaSonido = 10;
        ActualizarVolumen();
        sonido.IsLooping = true;
        sonido.Play();

        while (Game.IsRunning)
        {
            var colisión = await cuerpo.NewCollision();
            var jugador = RetornaJugador(colisión);

            if (jugador != null && cuerpo.Enabled)
                Obtener(jugador);

            await Script.NextFrame();
        }
    }

    private void Obtener(ControladorJugador controlador)
    {
        cuerpo.Enabled = false;
        velocidadRotación = 50;
        controlador.ActivarPoder(poder);

        AnimarFin();
        SistemaSonidos.SonarPoder(poder);
    }

    private async void Rotar()
    {
        while (activo)
        {
            modelo.Rotation *= Quaternion.RotationY(velocidadRotación * (float)Game.UpdateTime.WarpElapsed.TotalSeconds);
            await Task.Delay(1);
        }
    }

    private async void AnimarMovimiento(Vector3 objetivo)
    {
        var inicio = modelo.Position;
        float duración = 0.5f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración && activo)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            modelo.Position = Vector3.Lerp(inicio, objetivo, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        if (!activo)
            return;

        if (objetivo == posiciónArriba)
            AnimarMovimiento(posiciónAbajo);
        else
            AnimarMovimiento(posiciónArriba);
    }

    private async void AnimarFin()
    {
        finSonido = true;
        partículas.ParticleSystem.StopEmitters();

        var inicio = modelo.Scale;
        float duración = 0.5f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            modelo.Scale = Vector3.Lerp(inicio, Vector3.Zero, tiempo);
            sonido.Volume = MathUtil.Lerp(SistemaSonidos.ObtenerVolumen(Configuraciones.volumenEfectos), 0, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        activo = false;
        sonido.Stop();

        await Task.Delay(2000);
        partículas.Enabled = false;
    }

    public void ActualizarVolumen()
    {
        if (finSonido)
            return;

        distanciaJugador = Vector3.Distance(Entity.Transform.WorldMatrix.TranslationVector, ControladorPartida.ObtenerCabezaJugador());

        if (distanciaJugador > distanciaSonido)
            sonido.Volume = 0;
        else
            sonido.Volume = ((distanciaSonido - distanciaJugador) / distanciaSonido) * SistemaSonidos.ObtenerVolumen(Configuraciones.volumenEfectos);
    }

    public void PausarSonidos(bool pausa)
    {
        if (pausa)
            sonido.Pause();
        else
        {
            ActualizarVolumen();
            emisor.UseHRTF = bool.Parse(SistemaMemoria.ObtenerConfiguración(Configuraciones.hrtf));
            sonido.Play();
        }
    }
}
