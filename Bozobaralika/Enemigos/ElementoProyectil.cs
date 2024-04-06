using System.Linq;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;

public class ElementoProyectil: AsyncScript
{
    public ModelComponent modelo;
    public Prefab prefabEfecto;

    private PhysicsComponent[] disparador;
    private RigidbodyComponent cuerpo;
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

            if (colisión.ColliderA.CollisionGroup == CollisionFilterGroups.StaticFilter ||
                colisión.ColliderB.CollisionGroup == CollisionFilterGroups.StaticFilter ||
                colisión.ColliderA.CollisionGroup == CollisionFilterGroups.SensorTrigger ||
                colisión.ColliderB.CollisionGroup == CollisionFilterGroups.SensorTrigger)
            {
                // Explosiones o veneno
                if (prefabEfecto != null)
                {
                    var efecto = prefabEfecto.Instantiate()[0];
                    efecto.Transform.Position = colisión.Contacts.ToArray()[0].PositionOnB;
                    Entity.Scene.Entities.Add(efecto);
                }

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

                // Daña enemigo un 25%
                if (!tocaDisparador)
                {
                    dañable.RecibirDaño(daño * 0.25f);
                    Apagar();
                }
            }
            await Script.NextFrame();
        }
    }

    private void Apagar()
    {
        cuerpo.LinearVelocity = Vector3.Zero;
        modelo.Enabled = false;
        cuerpo.IsKinematic = true;
        cuerpo.Enabled = false;
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
