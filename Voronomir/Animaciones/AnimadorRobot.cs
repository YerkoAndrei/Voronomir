using Stride.Core.Mathematics;
using Stride.Particles.Components;
using Stride.Engine;

namespace Voronomir;

public class AnimadorRobot : StartupScript, IAnimador
{
    public ModelComponent modelo;
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

    public void Morir()
    {
        // Siempre explota
    }
}
