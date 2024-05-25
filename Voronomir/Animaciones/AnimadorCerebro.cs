using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Particles.Components;
using Stride.Physics;

namespace Voronomir;

public class AnimadorCerebro : StartupScript, IAnimador
{
    public TransformComponent rueda;
    public ModelComponent cerebro;
    public ModelComponent modeloEscudo;
    public RigidbodyComponent cuerpoEscudo;
    public Entity escudoMuerte;
    public ParticleSystemComponent partículas;
    public RigidbodyComponent saltador;

    public void Iniciar()
    {
        escudoMuerte.Get<ModelComponent>().Enabled = false;
        escudoMuerte.Get<RigidbodyComponent>().Enabled = false;
        escudoMuerte.Transform.Position = Vector3.One * -2;
        escudoMuerte.Transform.Scale = Vector3.Zero;
    }

    public void Actualizar()
    {

    }

    public void Activar(bool activar)
    {
        partículas.Enabled = activar;
        saltador.Enabled = activar;

        if (activar)
            partículas.ParticleSystem.ResetSimulation();
    }

    public void Caminar(float velocidad)
    {
        rueda.Rotation *= Quaternion.RotationX(velocidad * 20 *(float)Game.UpdateTime.WarpElapsed.TotalSeconds);
    }

    public void Atacar()
    {

    }

    public void Morir()
    {
        Activar(false);

        cerebro.Enabled = false;
        modeloEscudo.Enabled = false;
        cuerpoEscudo.Enabled = false;

        escudoMuerte.Get<ModelComponent>().Enabled = true;
        escudoMuerte.Get<RigidbodyComponent>().Enabled = true;
        escudoMuerte.Transform.Position = Vector3.UnitY * -0.6f;
        escudoMuerte.Transform.Scale = Vector3.One;

        rueda.Position = Vector3.UnitY * 0.05f;
        rueda.Rotation = Quaternion.RotationZ(MathUtil.DegreesToRadians(90));
    }
}
