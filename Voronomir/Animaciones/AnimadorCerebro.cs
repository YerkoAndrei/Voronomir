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
    public List<RigidbodyComponent> cuerpos = new List<RigidbodyComponent> { };

    private CollisionFilterGroups[] colisionesCuerpos;

    public void Iniciar()
    {
        // Cerebro tiene colisiones extras
        colisionesCuerpos = new CollisionFilterGroups[cuerpos.Count];
        for (int i = 0; i < cuerpos.Count; i++)
        {
            colisionesCuerpos[i] = cuerpos[i].CollisionGroup;
            cuerpos[i].CollisionGroup = CollisionFilterGroups.StaticFilter;
            cuerpos[i].Enabled = false;
        }
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
        for (int i = 0; i < cuerpos.Count; i++)
        {
            cuerpos[i].CollisionGroup = colisionesCuerpos[i];
            cuerpos[i].Enabled = true;
        }

        partículas.Enabled = activar;
        saltador.Enabled = activar;

        if (activar)
            partículas.ParticleSystem.ResetSimulation();
    }

    public void Caminar(float velocidad)
    {
        rueda.Rotation *= Quaternion.RotationY(velocidad * (float)Game.UpdateTime.WarpElapsed.TotalSeconds);
    }

    public void Atacar()
    {

    }

}
