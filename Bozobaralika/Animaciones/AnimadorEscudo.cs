using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;

public class AnimadorEscudo : StartupScript, IAnimador
{
    public TransformComponent rueda;

    public void Iniciar()
    {

    }

    public void Caminar(float velocidad)
    {
        rueda.Rotation *= Quaternion.RotationY(0.2f * velocidad);
    }

    public void Atacar()
    {

    }

}
