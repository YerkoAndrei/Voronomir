using System;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;
using static Utilidades;
using static Constantes;

public class ElementoProyectilSimple : AsyncScript, IProyectil
{
    public ModelComponent modelo;
    public TransformComponent cola;

    private Enemigos disparador;
    private RigidbodyComponent cuerpo;
    private Action<Vector3, Vector3, bool> iniciarImpacto;
    private float velocidad;
    private float tempo;
    private float daño;

    public override async Task Execute()
    {
        cuerpo = Entity.Get<RigidbodyComponent>();
        Apagar();

        while (Game.IsRunning)
        {
            var colisión = await cuerpo.NewCollision();
            if (!cuerpo.Enabled)
                continue;

            if (TocaEntorno(colisión))
            {
                // Impacto
                if (iniciarImpacto != null)
                {
                    if (cola != null)
                        CrearImpacto();
                    else
                        iniciarImpacto.Invoke(Entity.Transform.WorldMatrix.TranslationVector, Vector3.Zero, false);
                }

                Destruir();
                continue;
            }

            var dañable = colisión.ColliderA.Entity.Get<ElementoDañable>();
            if (dañable == null)
                dañable = colisión.ColliderB.Entity.Get<ElementoDañable>();

            if (dañable == null)
                continue;

            if (TocaJugador(colisión))
            {
                // Daña jugador
                dañable.RecibirDaño(daño);
                Destruir();
            }
            else if (TocaEnemigo(colisión))
            {
                // No daña a su mismo tipo
                if (disparador == dañable.controlador.Get<ControladorEnemigo>().enemigo)
                    continue;

                // Daña enemigos un 50%
                dañable.RecibirDaño(daño * 0.5f);
                Destruir();
            }
            await Script.NextFrame();
        }
    }

    public void Destruir()
    {
        // PENDIENTE: efecto disparo enemigo
        Apagar();
    }

    private void Apagar()
    {
        cuerpo.LinearVelocity = Vector3.Zero;
        modelo.Enabled = false;
        cuerpo.IsKinematic = true;
        cuerpo.Enabled = false;
    }

    public void AsignarImpacto(Action<Vector3, Vector3, bool> _iniciarImpacto)
    {
        iniciarImpacto = _iniciarImpacto;
    }

    public void IniciarPersecutor(float _velocidadRotación, Vector3 _altura)
    {

    }

    public void Iniciar(float _daño, float _velocidad, Quaternion _rotación, Vector3 _posición, Enemigos _disparador)
    {
        Destruir();

        Entity.Transform.Position = _posición;
        Entity.Transform.Rotation = _rotación;

        daño = _daño;
        disparador = _disparador;
        velocidad = _velocidad;

        // Dirección
        cuerpo.IsKinematic = false;
        cuerpo.UpdatePhysicsTransformation();
        cuerpo.LinearVelocity = Entity.Transform.WorldMatrix.Forward * velocidad;

        modelo.Enabled = true;
        cuerpo.Enabled = true;

        // Proyectiles simples duran 10 segundos
        tempo = 10f;
        ContarVida();
    }

    private async void ContarVida()
    {
        while (cuerpo.Enabled)
        {
            tempo -= SistemaAnimación.TiempoTranscurrido();
            if (tempo <= 0)
                Destruir();

            await Task.Delay(1);
        }
    }

    private void CrearImpacto()
    {
        // Rayo para crear marca
        var dirección = cola.WorldMatrix.TranslationVector + cola.WorldMatrix.Forward * 2;
        var resultado = this.GetSimulation().Raycast(cola.WorldMatrix.TranslationVector,
                                                     dirección,
                                                     CollisionFilterGroups.DefaultFilter,
                                                     colisionesMarca);
        if (resultado.Succeeded)
            iniciarImpacto.Invoke(resultado.Point, resultado.Normal, false);
        else
            iniciarImpacto.Invoke(Entity.Transform.WorldMatrix.TranslationVector, Vector3.Zero, false);
    }
}
