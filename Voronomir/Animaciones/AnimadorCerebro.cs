using System.Collections.Generic;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Particles.Components;
using Stride.Physics;

namespace Voronomir;

public class AnimadorCerebro : StartupScript, IAnimador
{
    public TransformComponent rueda;
    public ParticleSystemComponent partículas;
    public RigidbodyComponent saltador;
    public List<ModelComponent> modelos = new List<ModelComponent> { };

    public void Iniciar()
    {

    }

    public void Actualizar()
    {

    }

    public void Activar(bool activar)
    {
        foreach (var modelo in modelos)
        {
            modelo.Enabled = activar;
        }

        partículas.Enabled = activar;
        saltador.Enabled = activar;

        if (activar)
            partículas.ParticleSystem.ResetSimulation();
    }

    public void Caminar(float velocidad)
    {
        rueda.Rotation *= Quaternion.RotationY(velocidad * 20 *(float)Game.UpdateTime.WarpElapsed.TotalSeconds);
    }

    public void Atacar()
    {

    }

}
