using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Particles.Components;

namespace Voronomir;

public class AnimadorDron : StartupScript, IAnimador
{
    public TransformComponent transform;
    public ModelComponent modelo;
    public ParticleSystemComponent partículas;

    private Vector3 dirección;

	public void Iniciar()
    {

	}

    public void Actualizar()
    {
        dirección = Vector3.Normalize(ControladorPartida.ObtenerCabezaJugador() - transform.WorldMatrix.TranslationVector);
        transform.Rotation = Quaternion.Lerp(transform.Rotation, Quaternion.LookRotation(dirección, Vector3.UnitY), 10 * (float)Game.UpdateTime.WarpElapsed.TotalSeconds);
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
