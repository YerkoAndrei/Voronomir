using System;
using System.Linq;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;

public class ElementoProyectilSimple : AsyncScript, IProyectil
{
    public ModelComponent modelo;
    public TransformComponent cola;

    private PhysicsComponent[] disparador;
    private RigidbodyComponent cuerpo;
    private CollisionFilterGroupFlags colisionesMarca;
    private Action<Vector3, Vector3, bool> iniciarImpacto;
    private bool desviado;
    private float velocidad;
    private float tempo;
    private float daño;

    public override async Task Execute()
    {
        cuerpo = Entity.Get<RigidbodyComponent>();
        colisionesMarca = CollisionFilterGroupFlags.StaticFilter | CollisionFilterGroupFlags.SensorTrigger;
        Apagar();

        while (Game.IsRunning)
        {
            var colisión = await cuerpo.NewCollision();
            if (!cuerpo.Enabled)
                continue;

            if (colisión.ColliderA.CollisionGroup == CollisionFilterGroups.StaticFilter ||
                colisión.ColliderB.CollisionGroup == CollisionFilterGroups.StaticFilter ||
                colisión.ColliderA.CollisionGroup == CollisionFilterGroups.SensorTrigger ||
                colisión.ColliderB.CollisionGroup == CollisionFilterGroups.SensorTrigger)
            {
                // Impacto
                if (iniciarImpacto == null)
                {
                    Apagar();
                    continue;
                }

                // Crea pequeño rayo para crear Impacto
                if (cola != null)
                {
                    var dirección = cola.WorldMatrix.TranslationVector + cola.Entity.Transform.WorldMatrix.Forward;
                    var resultado = this.GetSimulation().Raycast(cola.WorldMatrix.TranslationVector,
                                                                 dirección,
                                                                 CollisionFilterGroups.DefaultFilter,
                                                                 colisionesMarca);
                    if (resultado.Succeeded)
                        iniciarImpacto.Invoke(resultado.Point, resultado.Normal, false);
                    else
                        iniciarImpacto.Invoke(colisión.Contacts.ToArray()[0].PositionOnB, Vector3.Zero, false);
                }
                else
                    iniciarImpacto.Invoke(colisión.Contacts.ToArray()[0].PositionOnA, Vector3.Zero, false);

                // PENDIENTE: efecto disparo enemigo
                Apagar();
                continue;
            }

            var dañable = colisión.ColliderA.Entity.Get<ElementoDañable>();
            if (dañable == null)
                dañable = colisión.ColliderB.Entity.Get<ElementoDañable>();

            if (dañable == null)
                continue;

            if (colisión.ColliderA.CollisionGroup == CollisionFilterGroups.CharacterFilter ||
                colisión.ColliderB.CollisionGroup == CollisionFilterGroups.CharacterFilter )
            {
                // Daña jugador
                dañable.RecibirDaño(daño);
                Apagar();
            }
            else if (colisión.ColliderA.CollisionGroup == CollisionFilterGroups.KinematicFilter ||
                     colisión.ColliderB.CollisionGroup == CollisionFilterGroups.KinematicFilter)
            {
                // No se daña a sí mismo
                var tocaDisparador = false;
                for(int i=0; i< disparador.Length; i++)
                {
                    if (colisión.ColliderA == disparador[i] || colisión.ColliderB == disparador[i])
                        tocaDisparador = true;
                }

                if (tocaDisparador && desviado)
                {
                    // Daña 400% si se lo devuelven
                    dañable.RecibirDaño(daño * 4f);
                    Apagar();
                }
                else if (!tocaDisparador && desviado)
                {
                    // Daña enemigo un 200% si va desviado
                    dañable.RecibirDaño(daño * 2f);
                    Apagar();
                }
                else if (!tocaDisparador && !desviado)
                {
                    // Daña enemigo un 100%
                    dañable.RecibirDaño(daño);
                    Apagar();
                }
            }
            await Script.NextFrame();
        }
    }

    private void Apagar()
    {
        desviado = false;
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

    public void Desviar(Vector3 dirección)
    {
        cuerpo.LinearVelocity = Vector3.Zero;
        cuerpo.Enabled = false;
        cuerpo.IsKinematic = true;

        var nuevaDirección = Vector3.Normalize(Entity.Transform.WorldMatrix.TranslationVector - dirección);
        var rotación = Quaternion.LookRotation(nuevaDirección, Vector3.UnitY);

        desviado = true;
        Iniciar(daño, velocidad, rotación, Entity.Transform.Position, disparador);
    }

    public void Iniciar(float _daño, float _velocidad, Quaternion _rotación, Vector3 _posición, PhysicsComponent[] _disparador)
    {
        Apagar();

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
            tempo -= (float)Game.UpdateTime.Elapsed.TotalSeconds;
            if (tempo <= 0)
                Apagar();

            await Task.Delay(1);
        }
    }
}
