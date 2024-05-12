using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Audio;
using Stride.Engine;

namespace Voronomir;
using static Utilidades;
using static Constantes;

public class ControladorPuerta : AsyncScript, IActivable, ISonidoMundo
{
    public bool instantanea;
    public int activaciones;
    public TransformComponent modelo;
    public AudioEmitterComponent emisor;
    public Vector3 posiciónAbierta;

    private PhysicsComponent cuerpo;
    private AudioEmitterSoundController sonidoLento;
    private AudioEmitterSoundController sonidoRápido;
    private int activadas;

    private bool finSonido;
    public float distanciaSonido { get; set; }
    public float distanciaJugador { get; set; }

    public override async Task Execute()
    {
        cuerpo = Entity.Get<PhysicsComponent>();
        sonidoLento = emisor["lento"];
        sonidoRápido = emisor["rápido"];
        distanciaSonido = 20;

        while (Game.IsRunning)
        {
            var colisión = await cuerpo.NewCollision();
            if (TocaJugador(colisión) && cuerpo.Enabled)
                TocarJugador();

            await Script.NextFrame();
        }
    }

    public void Activar()
    {
        activadas++;

        if (activadas >= activaciones)
        {
            AnimarPuerta();
            cuerpo.Enabled = false;
            ControladorPartida.MostrarMensaje(SistemaTraducción.ObtenerTraducción("puertaAbierta"));
            SistemaSonidos.SonarPuerta();
        }
        else
        {
            ControladorPartida.MostrarMensaje((activaciones - activadas) + " " + SistemaTraducción.ObtenerTraducción("activaciones"));
            SistemaSonidos.SonarCerrado();
        } 
    }

    private void TocarJugador()
    {
        ControladorPartida.MostrarMensaje(SistemaTraducción.ObtenerTraducción("puertaCerrada"));
        SistemaSonidos.SonarCerrado();
    }

    private async void AnimarPuerta()
    {
        var inicio = modelo.Position;
        float duración = 1f;
        float tiempoLerp = 0;
        float tiempo = 0;

        ActualizarVolumen();

        if (instantanea)
        {
            duración = 0.1f;
            sonidoRápido.PlayAndForget();
        }
        else
        {
            await Task.Delay(200);
            sonidoLento.PlayAndForget();
        }

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            modelo.Position = Vector3.Lerp(inicio, posiciónAbierta, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        finSonido = true;
    }

    public void ActualizarVolumen()
    {
        if (finSonido)
            return;

        distanciaJugador = Vector3.Distance(Entity.Transform.WorldMatrix.TranslationVector, ControladorPartida.ObtenerCabezaJugador());

        if (distanciaJugador > distanciaSonido)
        {
            sonidoRápido.Volume = 0;
            sonidoLento.Volume = 0;
        }
        else
        {
            sonidoRápido.Volume = ((distanciaSonido - distanciaJugador) / distanciaSonido) * SistemaSonidos.ObtenerVolumen(Configuraciones.volumenEfectos);
            sonidoLento.Volume = ((distanciaSonido - distanciaJugador) / distanciaSonido) * SistemaSonidos.ObtenerVolumen(Configuraciones.volumenEfectos);
        }
    }

    public void PausarSonidos(bool pausa)
    {
        if (pausa)
        {
            sonidoRápido.Pause();
            sonidoLento.Pause();
        }
        else
        {
            sonidoRápido.Play();
            sonidoLento.Play();
        }
    }
}
