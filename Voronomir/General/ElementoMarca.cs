using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;

namespace Voronomir;
using static Utilidades;
using static Constantes;

public class ElementoMarca : StartupScript
{
    public SpriteComponent textura;
    public Texture marcaSuave;
    public Texture marcaFuerte;
    public Texture marcaOctágono;

    public override void Start()
    {
        // PENDIENTE: crear marcas proyectadas desde cero (decals)
        textura.Enabled = false;
    }

    public void IniciarMarcaDisparo(Armas arma, Vector3 posición, Vector3 normal)
    {
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

        textura.Enabled = true;
        Entity.Transform.Position = posición + (normal * 0.001f);
        Entity.Transform.Rotation = Quaternion.LookRotation(normal, posición);
    }

    public void IniciarMarcaMuerte(Enemigos enemigo, Vector3 posición, Vector3 normal)
    {
        switch (enemigo)
        {
            case Enemigos.meléLigero:
                textura.SpriteProvider = ObtenerSprite(marcaOctágono);
                textura.Entity.Transform.Scale = Vector3.One * 0.8f;
                textura.Color = new Color(100, 0, 0, 200);
                break;
            case Enemigos.meléMediano:
                textura.SpriteProvider = ObtenerSprite(marcaOctágono);
                textura.Entity.Transform.Scale = Vector3.One * 0.8f;
                textura.Color = new Color(100, 0, 0, 200);
                break;
            case Enemigos.meléPesado:
                textura.SpriteProvider = ObtenerSprite(marcaFuerte);
                textura.Entity.Transform.Scale = Vector3.One * 0.6f;
                textura.Color = new Color(0, 0, 100, 200);
                break;
            case Enemigos.rangoLigero:
                textura.SpriteProvider = ObtenerSprite(marcaFuerte);
                textura.Entity.Transform.Scale = Vector3.One * 0.2f;
                textura.Color = new Color(0, 0, 100, 200);
                break;
            case Enemigos.rangoMediano:
                textura.SpriteProvider = ObtenerSprite(marcaOctágono);
                textura.Entity.Transform.Scale = Vector3.One * 1.0f;
                textura.Color = new Color(100, 0, 0, 200);
                break;
            case Enemigos.rangoPesado:
                textura.SpriteProvider = ObtenerSprite(marcaOctágono);
                textura.Entity.Transform.Scale = Vector3.One * 1.2f;
                textura.Color = new Color(0, 100, 0, 200);
                break;
        }

        textura.Enabled = true;
        Entity.Transform.Position = posición + (normal * 0.001f);
        Entity.Transform.Rotation = Quaternion.LookRotation(normal, posición);
    }
}
