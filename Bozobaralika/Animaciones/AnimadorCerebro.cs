using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;

public class AnimadorCerebro : StartupScript, IAnimador
{
    public TransformComponent rueda;

    public void Iniciar()
    {

    }

    public void Actualizar()
    {

    }

    public void Caminar(float velocidad)
    {
        rueda.Rotation *= Quaternion.RotationY(velocidad * (float)Game.UpdateTime.WarpElapsed.TotalSeconds);
    }

    public void Atacar()
    {

    }

}
