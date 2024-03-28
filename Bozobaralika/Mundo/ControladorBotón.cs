using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;

public class ControladorBotón : AsyncScript
{
    public ControladorPuerta puerta;
    public TransformComponent modelo;

    private StaticColliderComponent cuerpo;
    private Vector3 posiciónActivado;

    public override async Task Execute()
    {
        cuerpo = Entity.Get<StaticColliderComponent>();
        posiciónActivado = modelo.Position + new Vector3(0, -0.5f, 0);

        while (Game.IsRunning)
        {
            var colisión = await cuerpo.NewCollision();

            var jugador = colisión.ColliderA.Entity.Get<ControladorJugador>();
            if (jugador == null)
                jugador = colisión.ColliderB.Entity.Get<ControladorJugador>();

            Activar(jugador);

            await Script.NextFrame();            
        }
    }

    private void Activar(ControladorJugador controlador)
    {
        if (controlador == null)
            return;

        cuerpo.Enabled = false;
        puerta.Mover();
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

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Script.NextFrame();
        }
    }
}
