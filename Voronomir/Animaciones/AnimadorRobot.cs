using Stride.Core.Mathematics;
using Stride.Particles.Components;
using Stride.Engine;

namespace Voronomir;
using static Utilidades;

public class AnimadorRobot : StartupScript, IAnimador
{
    public ModelComponent modelo;
    public TransformComponent cuerpo;
    public ParticleSystemComponent partículas;

    private float velocidadRotación;

    public void Iniciar()
    {
        if (BoolAleatorio())
            velocidadRotación = RangoAleatorio(400, 450);
        else
            velocidadRotación = RangoAleatorio(-400, -450);
    }

    public void Actualizar()
    {
        cuerpo.Rotation *= Quaternion.RotationY(MathUtil.DegreesToRadians(velocidadRotación * (float)Game.UpdateTime.WarpElapsed.TotalSeconds));
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
