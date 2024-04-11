using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Particles.Components;

namespace Bozobaralika;
using static Utilidades;
using static Constantes;

public class ElementoMarca : StartupScript
{
    public SpriteComponent textura;
    public ParticleSystemComponent partículasBala;
    public ParticleSystemComponent partículasExplosión;
    public ParticleSystemComponent partículasDaño;
    public Texture marcaBala;
    public Texture marcaExplosión;

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
                textura.SpriteProvider = ObtenerSprite(marcaExplosión);
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
                textura.Entity.Transform.Scale = Vector3.One * 0.012f;
                textura.Color = new Color(100, 100, 100);

                partículasBala.Entity.Transform.Scale = Vector3.One * 0.4f;
                partículasBala.Enabled = true;
                partículasBala.ParticleSystem.ResetSimulation();
                break;
            case Armas.rifle:
                textura.SpriteProvider = ObtenerSprite(marcaBala);
                textura.Entity.Transform.Scale = Vector3.One * 0.04f;
                textura.Color = new Color(90, 90, 90);

                partículasExplosión.Entity.Transform.Scale = Vector3.One * 0.5f;
                partículasExplosión.Enabled = true;
                partículasExplosión.ParticleSystem.ResetSimulation();
                break;
            case Armas.lanzagranadas:
                textura.SpriteProvider = ObtenerSprite(marcaExplosión);
                textura.Entity.Transform.Scale = Vector3.One * 0.05f;
                textura.Color = new Color(80, 80, 80);

                partículasExplosión.Entity.Transform.Scale = Vector3.One * 0.6f;
                partículasExplosión.Enabled = true;
                partículasExplosión.ParticleSystem.ResetSimulation();
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
        partículasExplosión.Enabled = false;
        partículasDaño.Enabled = false;
    }
}
