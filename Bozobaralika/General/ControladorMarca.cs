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
    public ParticleSystemComponent partículasEspada;
    public ParticleSystemComponent partículasBalas;
    public ParticleSystemComponent partículasRifle;
    public Texture marcaEspada;
    public Texture marcaBala;
    public Texture marcaRifle;

    public override void Start()
    {
        textura.Enabled = false;
        partículasEspada.Enabled = false;
        partículasBalas.Enabled = false;
        partículasRifle.Enabled = false;
    }

    public void Iniciar(Armas arma, Vector3 posición, Vector3 normal, bool soloEfecto)
    {
        textura.Enabled = !soloEfecto;
        partículasEspada.Enabled = false;
        partículasBalas.Enabled = false;
        partículasRifle.Enabled = false;

        switch (arma)
        {
            case Armas.espada:
                textura.SpriteProvider = ObtenerSprite(marcaEspada);
                textura.Entity.Transform.Scale = Vector3.One * 0.2f;
                partículasEspada.Enabled = true;
                partículasEspada.ParticleSystem.ResetSimulation();
                break;
            case Armas.escopeta:
            case Armas.metralleta:
                textura.SpriteProvider = ObtenerSprite(marcaBala);
                textura.Entity.Transform.Scale = Vector3.One * 0.01f;
                partículasBalas.Enabled = true;
                partículasBalas.ParticleSystem.ResetSimulation();
                break;
            case Armas.rifle:
                textura.SpriteProvider = ObtenerSprite(marcaRifle);
                textura.Entity.Transform.Scale = Vector3.One * 0.6f;
                partículasRifle.Enabled = true;
                partículasRifle.ParticleSystem.ResetSimulation();
                break;
        }

        Entity.Transform.Position = posición + (normal * 0.001f);
        Entity.Transform.Rotation = Quaternion.LookRotation(normal, Vector3.UnitY);
    }
}
