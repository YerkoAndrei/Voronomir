using Stride.Core.Mathematics;
using Stride.Engine;
using System.Linq;

namespace Bozobaralika;

public class AnimadorDron : StartupScript, IAnimador
{
    public TransformComponent modelo;

    private TransformComponent objetivo;
	private Vector3 dirección;
	private Quaternion rotación;

	public void Iniciar()
    {
		var jugador = Entity.Scene.Entities.Where(o => o.Get<ControladorJugador>() != null).FirstOrDefault().Get<ControladorJugador>();
		objetivo = jugador.cámara.Entity.Transform;
	}

    public void Actualizar()
    {
        dirección = Vector3.Normalize(objetivo.WorldMatrix.TranslationVector - modelo.WorldMatrix.TranslationVector);
        rotación = Quaternion.LookRotation(dirección, Vector3.UnitY);

        // Hijo anula rotación padre
        if (Entity.Transform.Parent != null)
            rotación *= Quaternion.Invert(Entity.Transform.Parent.Rotation);

        modelo.Rotation = rotación;
    }

    public void Caminar(float velocidad)
    {

	}

    public void Atacar()
    {

    }

}
