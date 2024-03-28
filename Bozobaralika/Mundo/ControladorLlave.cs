using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;
using static Constantes;

public class ControladorLlave : AsyncScript
{
    public Llaves llave;
    public TransformComponent modelo;
    private bool activo;

    private StaticColliderComponent cuerpo;
    private float velocidadRotación;
    private Vector3 posiciónArriba;
    private Vector3 posiciónAbajo;

    public override async Task Execute()
    {
        cuerpo = Entity.Get<StaticColliderComponent>();

        posiciónArriba = modelo.Position + new Vector3(0, 0.2f, 0);
        posiciónAbajo = modelo.Position;

        activo = true;
        velocidadRotación = 1;
        AnimarMovimiento(posiciónArriba);
        Rotar();

        while (Game.IsRunning)
        {
            var colisión = await cuerpo.NewCollision();

            var jugador = colisión.ColliderA.Entity.Get<ControladorJugador>();
            if (jugador == null)
                jugador = colisión.ColliderB.Entity.Get<ControladorJugador>();

            Obtener(jugador);
            await Script.NextFrame();
        }
    }

    private void Obtener(ControladorJugador controlador)
    {
        if (controlador == null)
            return;

        velocidadRotación = 10;
        cuerpo.Enabled = false;
        controlador.GuardarLlave(llave);
        AnimarFin();

        // PENDIENTE: efectos
    }

    private async void Rotar()
    {
        while (activo)
        {
            switch (llave)
            {
                case Llaves.azul:
                    modelo.Rotation *= Quaternion.RotationY(0.01f * velocidadRotación);
                    break;
                case Llaves.roja:
                    modelo.Rotation *= Quaternion.RotationY(-0.01f * velocidadRotación);
                    break;
            }
            await Script.NextFrame();
        }
    }

    private async void AnimarMovimiento(Vector3 objetivo)
    {
        var inicio = modelo.Position;
        float duración = 1f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración && activo)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            modelo.Position = Vector3.Lerp(inicio, objetivo, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Script.NextFrame();
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
        var inicio = modelo.Scale;
        float duración = 1f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            modelo.Scale = Vector3.Lerp(inicio, Vector3.Zero, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Script.NextFrame();
        }

        activo = false;
    }
}
