using System.Linq;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;

public class AnimadorObservación : SyncScript
{
    private TransformComponent objetivo;
    private Vector3 dirección;
    private Quaternion rotación;

    public override void Start()
    {
        var jugador = Entity.Scene.Entities.Where(o => o.Get<ControladorJugador>() != null).FirstOrDefault().Get<ControladorJugador>();
        objetivo = jugador.cámara.Entity.Transform;
    }

    public override void Update()
    {
        dirección = Vector3.Normalize(objetivo.WorldMatrix.TranslationVector - Entity.Transform.WorldMatrix.TranslationVector);
        rotación = Quaternion.LookRotation(dirección, Vector3.UnitY);

        // Hijo anula rotación padre
        if(Entity.Transform.Parent != null)
            rotación *= Quaternion.Invert(Entity.Transform.Parent.Rotation);

        Entity.Transform.Rotation = rotación;
    }
}
