using Stride.Engine;
using Stride.Particles.Components;
using Stride.Physics;

namespace Voronomir;

public class AnimadorAraña : StartupScript, IAnimador
{
    public ModelComponent modelo;
    public RigidbodyComponent veneno;
    public ParticleSystemComponent partículas;

    public void Iniciar()
    {

    }

    public void Actualizar()
    {

    }

    public void Activar(bool activar)
    {
        modelo.Enabled = activar;
        veneno.Enabled = activar;
        partículas.Enabled = activar;

        if (activar)
            partículas.ParticleSystem.ResetSimulation();
    }

    public void Caminar(float velocidad)
    {

    }

    public void Atacar()
    {

    }
}
