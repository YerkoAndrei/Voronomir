using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;
using static Utilidades;
using static Constantes;

public class ControladorBotón : AsyncScript
{
    public Llaves llave;
    public ControladorPuerta puerta;
    public TransformComponent modelo;

    private PhysicsComponent cuerpo;
    private Vector3 posiciónActivado;

    public override async Task Execute()
    {
        cuerpo = Entity.Get<PhysicsComponent>();
        posiciónActivado = modelo.Position + new Vector3(0, -0.5f, 0);

        while (Game.IsRunning)
        {
            var colisión = await cuerpo.NewCollision();
            var jugador = RetornaJugador(colisión);

            if (jugador != null)
                Activar(jugador);

            await Script.NextFrame();            
        }
    }

    private void Activar(ControladorJugador controlador)
    {
        if (controlador == null)
            return;

        // Llave necesaria
        if (!controlador.ObtenerLlave(llave))
            return;

        cuerpo.Enabled = false;
        puerta.Activar();
        AnimarActivación();

        // PENDIENTE: efectos
    }

    private async void AnimarActivación()
    {
        var inicio = modelo.Position;
        float duración = 0.5f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            modelo.Position = Vector3.Lerp(inicio, posiciónActivado, tiempo);

            tiempoLerp += SistemaAnimación.TiempoTranscurrido();
            await Task.Delay(1);
        }
    }
}
