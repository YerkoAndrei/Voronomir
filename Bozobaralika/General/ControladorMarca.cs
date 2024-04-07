using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Particles.Components;

namespace Bozobaralika;
using static Utilidades;
using static Constantes;

public class ControladorMarca : StartupScript
{
    public SpriteComponent textura;
    public ParticleSystemComponent partículasBala;
    public ParticleSystemComponent partículasRifle;
    public ParticleSystemComponent partículasDaño;
    public Texture marcaEspada;
    public Texture marcaBala;

    public override void Start()
    {
        Apagar();
    }

    public void IniciarMarca(Armas arma, Vector3 posición, Vector3 normal, bool soloEfecto)
    {
        Apagar();
        textura.Enabled = !soloEfecto;

        switch (arma)
        {
            case Armas.espada:
                textura.SpriteProvider = ObtenerSprite(marcaEspada);
                textura.Entity.Transform.Scale = Vector3.One * 0.02f;
                textura.Color = new Color(90, 90, 90);

                partículasBala.Entity.Transform.Scale = Vector3.One * 0.2f;
                partículasBala.Enabled = true;
                partículasBala.ParticleSystem.ResetSimulation();
                break;
            case Armas.escopeta:
                textura.SpriteProvider = ObtenerSprite(marcaBala);
                textura.Entity.Transform.Scale = Vector3.One * 0.01f;
                textura.Color = new Color(100, 100, 100);

                partículasBala.Entity.Transform.Scale = Vector3.One * 0.25f;
                partículasBala.Enabled = true;
                partículasBala.ParticleSystem.ResetSimulation();
                break;
            case Armas.metralleta:
                textura.SpriteProvider = ObtenerSprite(marcaBala);
                textura.Entity.Transform.Scale = Vector3.One * 0.01f;
                textura.Color = new Color(100, 100, 100);

                partículasBala.Entity.Transform.Scale = Vector3.One * 0.4f;
                partículasBala.Enabled = true;
                partículasBala.ParticleSystem.ResetSimulation();
                break;
            case Armas.rifle:
                textura.SpriteProvider = ObtenerSprite(marcaBala);
                textura.Entity.Transform.Scale = Vector3.One * 0.03f;
                textura.Color = new Color(90, 90, 90);

                partículasRifle.Entity.Transform.Scale = Vector3.One * 0.5f;
                partículasRifle.Enabled = true;
                partículasRifle.ParticleSystem.ResetSimulation();
                break;
            case Armas.lanzagranadas:
                textura.SpriteProvider = ObtenerSprite(marcaBala);
                textura.Entity.Transform.Scale = Vector3.One * 0.03f;
                textura.Color = new Color(20, 20, 20);

                partículasRifle.Entity.Transform.Scale = Vector3.One * 1f;
                partículasRifle.Enabled = true;
                partículasRifle.ParticleSystem.ResetSimulation();
                break;
        }

        Entity.Transform.Position = posición + (normal * 0.001f);
        Entity.Transform.Rotation = Quaternion.LookRotation(normal, posición);
    }

    public void IniciarDaño(Vector3 posición, Vector3 normal, float multiplicador)
    {
        Apagar();

        partículasDaño.Entity.Transform.Scale = Vector3.One * multiplicador * 0.4f;
        partículasDaño.Enabled = true;
        partículasDaño.ParticleSystem.ResetSimulation();

        Entity.Transform.Position = posición + (normal * 0.001f);
        Entity.Transform.Rotation = Quaternion.LookRotation(normal, posición);
    }

    private void Apagar()
    {
        textura.Enabled = false;
        partículasBala.Enabled = false;
        partículasRifle.Enabled = false;
        partículasDaño.Enabled = false;
    }
}
