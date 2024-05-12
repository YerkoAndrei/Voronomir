using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Audio;
using Stride.Engine;

namespace Voronomir;
using static Utilidades;
using static Constantes;

public class ControladorPuerta : AsyncScript, IActivable
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

    public override async Task Execute()
    {
        cuerpo = Entity.Get<PhysicsComponent>();
        sonidoLento = emisor["lento"];
        sonidoRápido = emisor["rápido"];

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

        if (instantanea)
        {
            duración = 0.1f;
            sonidoRápido.Volume = SistemaSonidos.ObtenerVolumen(Configuraciones.volumenEfectos);
            sonidoRápido.PlayAndForget();
        }
        else
        {
            await Task.Delay(200);
            sonidoLento.Volume = SistemaSonidos.ObtenerVolumen(Configuraciones.volumenEfectos);
            sonidoLento.PlayAndForget();
        }

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            modelo.Position = Vector3.Lerp(inicio, posiciónAbierta, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }
    }

    public void ActualizarVolumen()
    {
        sonidoLento.Volume = SistemaSonidos.ObtenerVolumen(Configuraciones.volumenEfectos);
    }
}
