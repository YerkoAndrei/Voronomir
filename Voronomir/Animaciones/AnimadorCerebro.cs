using System.Collections.Generic;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Voronomir;

public class AnimadorCerebro : StartupScript, IAnimador
{
    public TransformComponent rueda;
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
    }

    public void Caminar(float velocidad)
    {
        rueda.Rotation *= Quaternion.RotationY(velocidad * (float)Game.UpdateTime.WarpElapsed.TotalSeconds);
    }

    public void Atacar()
    {

    }

}
