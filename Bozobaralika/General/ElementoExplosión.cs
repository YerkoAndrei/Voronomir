using System.Linq;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;

public class ElementoExplosión : StartupScript, IImpacto
{
    public float distanciaMáxima;
    public ModelComponent modelo;

    public static CollisionFilterGroupFlags colisionesExplosión;
    private RigidbodyComponent cuerpo;
    private Vector3 escalaInicial;

    public override void Start()
    {
        colisionesExplosión = CollisionFilterGroupFlags.StaticFilter |
                              CollisionFilterGroupFlags.SensorTrigger |
                              CollisionFilterGroupFlags.KinematicFilter |
                              CollisionFilterGroupFlags.CharacterFilter;

        cuerpo = Entity.Get<RigidbodyComponent>();
        escalaInicial = Entity.Transform.Scale;
        modelo.Enabled = false;
        cuerpo.Enabled = false;
    }

    public void Iniciar(Vector3 posición, Vector3 normal, float daño)
    {
        Entity.Transform.Position = posición;
        Entity.Transform.Rotation = Quaternion.Identity;
        Entity.Transform.Scale = escalaInicial;

        Entity.Transform.UpdateWorldMatrix();
        cuerpo.UpdatePhysicsTransformation();
        modelo.Enabled = true;
        cuerpo.Enabled = true;

        CalcularDaños(daño);
        Apagar();
    }

    private void CalcularDaños(float daño)
    {
        // Encuentra tocados por explosión
        var colisiones = cuerpo.Collisions.ToArray();

        foreach (var colisión in colisiones)
        {
            // Encuentra dañables
            var dañable = colisión.ColliderA.Entity.Get<ElementoDañable>();
            if (dañable == null)
                dañable = colisión.ColliderB.Entity.Get<ElementoDañable>();

            if (dañable == null)
                continue;

            // Calcula distancia con rayo para evitar obstáculos
            // Desde explosión hasta cintura (1m)
            var dirección = (Entity.Transform.WorldMatrix.TranslationVector + (Vector3.UnitY * 0.1f)) + (dañable.Entity.Transform.WorldMatrix.TranslationVector + Vector3.UnitY) * distanciaMáxima;
            var resultado = this.GetSimulation().Raycast(Entity.Transform.WorldMatrix.TranslationVector,
                                                         dirección,
                                                         CollisionFilterGroups.DefaultFilter,
                                                         colisionesExplosión);

            if (!resultado.Succeeded || resultado.Collider.CollisionGroup == CollisionFilterGroups.StaticFilter || resultado.Collider.CollisionGroup == CollisionFilterGroups.SensorTrigger)
                continue;

            // Daña según distancia
            var distancia = Vector3.Distance(Entity.Transform.WorldMatrix.TranslationVector, resultado.Point);
            var multiplicador = 1f - (distancia / distanciaMáxima);
            dañable.RecibirDaño(daño * multiplicador);
        }
    }

    private async void Apagar()
    {
        cuerpo.Enabled = false;

        float duración = 0.2f;
        float tiempoLerp = 0;
        float tiempo = 0;

        await Task.Delay(100);
        while (tiempoLerp < duración)
        {
            tiempo = tiempoLerp / duración;
            Entity.Transform.Scale = Vector3.Lerp(escalaInicial, Vector3.Zero, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Task.Delay(1);
        }

        modelo.Enabled = false;
    }
}
