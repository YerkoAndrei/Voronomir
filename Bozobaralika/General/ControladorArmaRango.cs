using Stride.Core.Mathematics;
using Stride.Engine;
using System.Linq;

namespace Bozobaralika;

public class ControladorArmaRango: StartupScript
{
    public Prefab prefabDisparo;

    private TransformComponent jugador;
    private ElementoDisparo[] disparos;
    private int disparoActual;
    private int maxDisparos;

    public void Iniciar()
    {
        jugador = Entity.Scene.Entities.Where(o => o.Get<ControladorJugador>() != null).FirstOrDefault().Transform;

        maxDisparos = 8;
        disparos = new ElementoDisparo[maxDisparos];
        for (int i = 0; i < maxDisparos; i++)
        {
            var marca = prefabDisparo.Instantiate()[0];
            disparos[i] = marca.Get<ElementoDisparo>();
            Entity.Scene.Entities.Add(marca);
        }
    }

    public void Disparar(float daño, float velocidad)
    {
        // PENDIENTE: según enemigo
        var objetivo = (jugador.WorldMatrix.TranslationVector + new Vector3(0, 1, 0));

        var dirección = Vector3.Normalize(Entity.Transform.WorldMatrix.TranslationVector - objetivo);
        var rotación = Quaternion.LookRotation(dirección, Vector3.UnitY);

        disparos[disparoActual].Iniciar(daño, velocidad, rotación, Entity.Transform.WorldMatrix.TranslationVector);
        disparoActual++;

        if (disparoActual >= maxDisparos)
            disparoActual = 0;
    }
}
