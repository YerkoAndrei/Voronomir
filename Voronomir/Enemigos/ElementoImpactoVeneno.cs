using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Particles.Components;
using Stride.Engine;

namespace Voronomir;

public class ElementoImpactoVeneno : StartupScript, IImpacto
{
    public float tiempoVida;
    public ModelComponent modelo;
    public ParticleSystemComponent partículas;

    private PhysicsComponent cuerpo;

    public override void Start()
    {
        cuerpo = Entity.Get<PhysicsComponent>();
        modelo.Enabled = false;
        cuerpo.Enabled = false;
        partículas.Enabled = false;
    }

    public void Iniciar(Vector3 posición, Vector3 normal, float daño)
    {
        Entity.Transform.Position = posición;
        Entity.Transform.Scale = Vector3.One;

        // Solo rota modelo
        if (normal != Vector3.UnitY)
            modelo.Entity.Transform.Rotation = Quaternion.LookRotation(normal, posición) * Quaternion.RotationY(MathUtil.DegreesToRadians(90));
        else
            modelo.Entity.Transform.Rotation = Quaternion.Identity;

        modelo.Enabled = true;
        cuerpo.Enabled = true;
        partículas.Enabled = true;
        partículas.ParticleSystem.ResetSimulation();

        ContarVida();
    }

    private async void ContarVida()
    {
        float tiempoLerp = 0;
        float tiempo = 0;

        await Task.Delay(400);
        while (tiempoLerp < tiempoVida)
        {
            tiempo = tiempoLerp / tiempoVida;
            Entity.Transform.Scale = Vector3.Lerp(Vector3.One, Vector3.Zero, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        modelo.Enabled = false;
        cuerpo.Enabled = false;
        partículas.Enabled = false;
    }
}
