using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;

public class ElementoExplosión : StartupScript, IImpacto
{
    public float distanciaMínima;
    public float distanciaMáxima;
    public ModelComponent modelo;

    private PhysicsComponent cuerpo;
    private Vector3 escalaInicial;

    public override void Start()
    {
        cuerpo = Entity.Get<PhysicsComponent>();
        escalaInicial = Entity.Transform.Scale;
        modelo.Enabled = false;
        cuerpo.Enabled = false;
    }

    public void Iniciar(Vector3 posición, Vector3 normal, float daño)
    {
        Entity.Transform.Scale = escalaInicial;
        Entity.Transform.Position = posición;

        if (normal != Vector3.Zero)
            Entity.Transform.Rotation = Quaternion.LookRotation(normal, posición);
        else
            Entity.Transform.Rotation = Quaternion.Identity;

        modelo.Enabled = true;
        cuerpo.Enabled = true;

        CalcularDaños(daño);
        Apagar();
    }

    private void CalcularDaños(float daño)
    {
        // Encuentra tocados por exploción

        // Calcula distancia

        // Daña
    }

    private async void Apagar()
    {
        float duración = 0.2f;
        float tiempoLerp = 0;
        float tiempo = 0;

        await Task.Delay(100);
        while (tiempoLerp < duración)
        {
            tiempo = tiempoLerp / duración;
            Entity.Transform.Scale = Vector3.Lerp(escalaInicial, Vector3.Zero, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Task.Delay(1);
        }

        modelo.Enabled = false;
        cuerpo.Enabled = false;
    }
}
