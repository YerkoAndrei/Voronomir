using System;
using System.Linq;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering.Sprites;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Panels;

namespace Bozobaralika;
using static Constantes;

public static  class Sistema
{
    private const string ISO8006 = "yyyy-MM-ddThh:mm:ss";

    public static float RangoAleatorio(float min, float max)
    {
        var aleatorio = new Random();
        double valor = (aleatorio.NextDouble() * (max - min) + min);
        return (float)valor;
    }

    public static ISpriteProvider ObtenerSprite(Texture textura)
    {
        var sprite = new SpriteFromTexture
        {
            Texture = textura
        };

        return sprite;
    }

    public static void ConfigurarBotón(Grid grid, Action action)
    {
        // Busca contenido dentro del "grid botón"
        var imagen = grid.FindVisualChildOfType<ImageElement>("img");
        var botón = grid.FindVisualChildOfType<Button>("btn");

        // Texto
        var texto = ObtenerTexto(grid);
        var colorTexto = new Color(25, 20, 20);
        if (texto != null)
            colorTexto = texto.TextColor;

        // En clic
        botón.Click += (s, a) => { action.Invoke(); };

        // Sonidos
        //botón.TouchDown += (s, a) => { SistemaSonido.SonarBotónEntra(); };
        //botón.TouchUp += (s, a) => { SistemaSonido.SonarBotónSale(); };

        // Cambios color
        var colorBase = imagen.Color;

        // Guarda colores iniciales. Se usa en bloqueo. Podría mejorar.
        grid.Children.Add(new Grid()
        {
            Name = "colorInicialBotón",
            BackgroundColor = colorBase,
            Opacity = 0
        });
        grid.Children.Add(new Grid()
        {
            Name = "colorInicialTexto",
            BackgroundColor = colorTexto,
            Opacity = 0
        });

        botón.MouseOverStateChanged += (s, a) =>
        {
            if (VerificarBloqueo(botón, imagen, texto, colorBase, colorTexto))
                return;
            switch (a.NewValue)
            {
                case MouseOverState.MouseOverElement:
                    imagen.Color = colorBase * colorEnCursor;
                    break;
                case MouseOverState.MouseOverNone:
                    imagen.Color = colorBase * colorNormal;
                    break;
            }
        };
        botón.TouchDown += (s, a) =>
        {
            if (VerificarBloqueo(botón, imagen, texto, colorBase, colorTexto))
                return;
            imagen.Color = colorBase * colorEnClic;
        };
        botón.TouchUp += (s, a) =>
        {
            if (VerificarBloqueo(botón, imagen, texto, colorBase, colorTexto))
                return;
            imagen.Color = colorBase * colorEnCursor;
        };
    }

    public static TextBlock ObtenerTexto(Grid grid)
    {
        // Encuentra primer texto del grid
        var textos = grid.FindVisualChildrenOfType<TextBlock>().ToArray();
        if (textos.Length > 0)
            return textos[0];
        else
            return null;
    }

    private static bool VerificarBloqueo(Button botón, ImageElement imagen, TextBlock texto, Color colorBase, Color colorTexto)
    {
        if (botón.CanBeHitByUser)
            return false;

        // Bloqueo
        imagen.Color = colorBase * colorBloqueado;
        if (texto != null)
            texto.TextColor = colorTexto * colorBloqueado;

        return true;
    }

    public static void BloquearBotón(Grid grid, bool bloquear)
    {
        // Busca contenido dentro del "grid botón"
        var imagen = grid.FindVisualChildOfType<ImageElement>("img");
        var botón = grid.FindVisualChildOfType<Button>("btn");
        var ícono = grid.FindVisualChildOfType<ImageElement>("imgÍcono");
        var texto = ObtenerTexto(grid);

        botón.CanBeHitByUser = !bloquear;

        // Colores
        if (bloquear)
        {
            imagen.Color = grid.FindVisualChildOfType<Grid>("colorInicialBotón").BackgroundColor * colorBloqueado;
            if (texto != null)
                texto.TextColor = grid.FindVisualChildOfType<Grid>("colorInicialTexto").BackgroundColor * colorBloqueado;
            if (ícono != null)
                ícono.Color = grid.FindVisualChildOfType<Grid>("colorInicialTexto").BackgroundColor * colorBloqueado;
        }
        else
        {
            imagen.Color = grid.FindVisualChildOfType<Grid>("colorInicialBotón").BackgroundColor * colorNormal;
            if (texto != null)
                texto.TextColor = grid.FindVisualChildOfType<Grid>("colorInicialTexto").BackgroundColor * colorNormal;
            if (ícono != null)
                ícono.Color = grid.FindVisualChildOfType<Grid>("colorInicialTexto").BackgroundColor * colorNormal;
        }
    }

    public static string FormatearFechaEstándar(DateTime fecha)
    {
        var fechaHora = fecha.ToString(ISO8006);
        return fechaHora;
    }
}
