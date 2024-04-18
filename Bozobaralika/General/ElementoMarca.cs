using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;

namespace Bozobaralika;
using static Utilidades;
using static Constantes;

public class ElementoMarca : StartupScript
{
    public SpriteComponent textura;
    public Texture marcaSuave;
    public Texture marcaFuerte;

    public override void Start()
    {
        textura.Enabled = false;
    }

    public void IniciarMarca(Armas arma, Vector3 posición, Vector3 normal)
    {
        textura.Enabled = true;

        switch (arma)
        {
            case Armas.espada:
                textura.SpriteProvider = ObtenerSprite(marcaFuerte);
                textura.Entity.Transform.Scale = Vector3.One * 0.08f;
                textura.Color = new Color(90, 90, 90);
                break;
            case Armas.escopeta:
                textura.SpriteProvider = ObtenerSprite(marcaSuave);
                textura.Entity.Transform.Scale = Vector3.One * 0.04f;
                textura.Color = new Color(100, 100, 100);
                break;
            case Armas.metralleta:
                textura.SpriteProvider = ObtenerSprite(marcaSuave);
                textura.Entity.Transform.Scale = Vector3.One * 0.05f;
                textura.Color = new Color(100, 100, 100);
                break;
            case Armas.rifle:
                textura.SpriteProvider = ObtenerSprite(marcaSuave);
                textura.Entity.Transform.Scale = Vector3.One * 0.15f;
                textura.Color = new Color(80, 80, 80);
                break;
            case Armas.lanzagranadas:
                textura.SpriteProvider = ObtenerSprite(marcaFuerte);
                textura.Entity.Transform.Scale = Vector3.One * 0.2f;
                textura.Color = new Color(60, 60, 60);
                break;
        }

        Entity.Transform.Position = posición + (normal * 0.001f);
        Entity.Transform.Rotation = Quaternion.LookRotation(normal, posición);
    }
}
