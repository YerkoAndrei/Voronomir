using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Rendering;

namespace Bozobaralika;

public class AnimadorProcedural : SyncScript
{
    private ModelComponent modelo;
    private SkeletonUpdater esqueleto;

    private Vector3 objetivoPrueba0 = new Vector3(5, 0, 4);
    private Vector3 objetivoPrueba1 = new Vector3(5, 0, 0);

    public override void Start()
    {
        modelo = Entity.Get<ModelComponent>();
        esqueleto = modelo.Skeleton;

        ProbarCaminata(objetivoPrueba0);
    }

    public override void Update()
    {

    }

    private async void ProbarCaminata(Vector3 objetivo)
    {
        float duración = 2f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);

            if (objetivo == objetivoPrueba0)
                Entity.Transform.Position = Vector3.Lerp(objetivoPrueba1, objetivoPrueba0, tiempo);
            else
                Entity.Transform.Position = Vector3.Lerp(objetivoPrueba0, objetivoPrueba1, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Task.Delay(1);
        }

        await Task.Delay(200);

        if(objetivo == objetivoPrueba0)
            ProbarCaminata(objetivoPrueba1);
        else
            ProbarCaminata(objetivoPrueba0);
    }
}
