using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;
using static Utilidades;

public class ControladorPuerta : AsyncScript, IActivable
{
    public bool instantanea;
    public int activaciones;
    public TransformComponent modelo;
    public Vector3 posiciónAbierta;

    private PhysicsComponent cuerpo;
    private int activadas;

    public override async Task Execute()
    {
        cuerpo = Entity.Get<PhysicsComponent>();

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

        if(instantanea)
            duración = 0.1f;
        else
            await Task.Delay(200);

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            modelo.Position = Vector3.Lerp(inicio, posiciónAbierta, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }
    }
}
