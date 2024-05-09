using Stride.Core.Mathematics;
using Stride.Engine;

namespace Voronomir;

public class AnimadorPulga : StartupScript, IAnimador
{
    public ModelComponent modelo;

	public void Iniciar()
    {

	}

    public void Actualizar()
    {

    }

    public void Activar(bool activar)
    {
        modelo.Enabled = activar;
    }

    public void Caminar(float velocidad)
    {

	}

    public void Atacar()
    {

    }

}
