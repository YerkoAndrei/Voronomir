using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;

namespace Bozobaralika;
using static Utilidades;
using static Constantes;

public class ControladorMarca : StartupScript
{
    public SpriteComponent textura;
    public Texture marcaEspada;
    public Texture marcaBala;
    public Texture marcaRifle;

    public void Iniciar(Armas arma, Vector3 posición, Vector3 normal, bool soloEfecto)
    {
        // PENDIENTE: efectos
        switch (arma)
        {
            case Armas.espada:
                textura.SpriteProvider = ObtenerSprite(marcaEspada);
                textura.Entity.Transform.Scale = Vector3.One * 0.2f;
                break;
            case Armas.escopeta:
            case Armas.metralleta:
                textura.SpriteProvider = ObtenerSprite(marcaBala);
                textura.Entity.Transform.Scale = Vector3.One * 0.01f;
                break;
            case Armas.rifle:
                textura.SpriteProvider = ObtenerSprite(marcaRifle);
                textura.Entity.Transform.Scale = Vector3.One * 0.6f;
                break;
        }

        if (soloEfecto)
            textura.Entity.Transform.Scale = Vector3.Zero;

        Entity.Transform.Position = posición + (normal * 0.001f);
        Entity.Transform.Rotation = Quaternion.LookRotation(normal, Vector3.UnitY);
    }
}
