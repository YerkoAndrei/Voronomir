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


    private StaticColliderComponent cuerpo;
    private bool animando;
    private bool rotando;
    private float velocidadRotación;
    private Vector3 posiciónArriba;
    private Vector3 posiciónAbajo;

    public override async Task Execute()
    {
        cuerpo = Entity.Get<StaticColliderComponent>();

        posiciónArriba = modelo.Position + new Vector3(0, 0.2f, 0);
        posiciónAbajo = modelo.Position;

        rotando = true;
        animando = true;
        velocidadRotación = 1;
        Animar(posiciónArriba);
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
        animando = false;
        controlador.GuardarLlave(llave);
        Destruir();

        // PENDIENTE: efectos
    }

    private async void Rotar()
    {
        while (rotando)
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

    private async void Animar(Vector3 objetivo)
    {
        var inicio = modelo.Position;
        float duración = 1f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración && animando)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            modelo.Position = Vector3.Lerp(inicio, objetivo, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Script.NextFrame();
        }

        if (!animando)
            return;

        if (objetivo == posiciónArriba)
            Animar(posiciónAbajo);
        else
            Animar(posiciónArriba);
    }

    private async void Destruir()
    {
        var tamañoInicio = modelo.Scale;
        var posiciónInicio = modelo.Position;
        var posiciónObjetivo = new Vector3(modelo.Position.X, -0.2f, modelo.Position.Z);
        float duración = 1f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            modelo.Scale = Vector3.Lerp(tamañoInicio, Vector3.Zero, tiempo);
            modelo.Position = Vector3.Lerp(posiciónInicio, posiciónObjetivo, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Script.NextFrame();
        }

        rotando = false;
    }
}
