using System.Linq;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace Voronomir;
using static Utilidades;

public class ElementoExplosión : StartupScript, IImpacto
{
    public ModelComponent modelo;

    public static CollisionFilterGroupFlags colisionesExplosión;
    private RigidbodyComponent cuerpo;
    private Vector3 escalaInicial;
    private float daño;

    public override void Start()
    {
        colisionesExplosión = CollisionFilterGroupFlags.StaticFilter |
                              CollisionFilterGroupFlags.SensorTrigger |
                              CollisionFilterGroupFlags.KinematicFilter |
                              CollisionFilterGroupFlags.CharacterFilter;

        cuerpo = Entity.Get<RigidbodyComponent>();
        escalaInicial = modelo.Entity.Transform.Scale;
        modelo.Enabled = false;
        cuerpo.Enabled = false;
    }

    public void Iniciar(Vector3 posición, Vector3 normal, float _daño)
    {
        daño = _daño;
        Entity.Transform.Position = posición;
        Entity.Transform.UpdateWorldMatrix();

        modelo.Entity.Transform.Scale = escalaInicial;
        modelo.Enabled = true;

        cuerpo.Enabled = true;
        cuerpo.UpdatePhysicsTransformation();

        EsperarCuadro();
    }

    private async void EsperarCuadro()
    {
        await EsperarCuadroFísica();
        CalcularDaños();
        Apagar();
    }

    private void CalcularDaños()
    {
        // Encuentra tocados por explosión
        var colisiones = cuerpo.Collisions.ToArray();
        cuerpo.Enabled = false;

        foreach (var colisión in colisiones)
        {
            // Encuentra dañables
            var dañable = colisión.ColliderA.Entity.Get<ElementoDañable>();
            if (dañable == null)
                dañable = colisión.ColliderB.Entity.Get<ElementoDañable>();

            if (dañable == null)
                continue;

            dañable.RecibirDaño(daño, true);

            // Empujar
            var dirección = dañable.Entity.Transform.WorldMatrix.TranslationVector - Entity.Transform.WorldMatrix.TranslationVector;
            dañable.Empujar(dirección);
        }
    }

    private async void Apagar()
    {
        float duración = 0.2f;
        float tiempoLerp = 0;
        float tiempo = 0;

        await Task.Delay(100);
        while (tiempoLerp < duración)
        {
            tiempo = tiempoLerp / duración;
            modelo.Entity.Transform.Scale = Vector3.Lerp(escalaInicial, Vector3.Zero, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Task.Delay(1);
        }
        modelo.Enabled = false;
    }
}
