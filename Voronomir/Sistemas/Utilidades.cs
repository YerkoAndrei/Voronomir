using System;
using System.Linq;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Physics;
using Stride.Rendering.Sprites;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Panels;

namespace Voronomir;

public static class Utilidades
{
    // Color botones
    private static Color colorNormal =      new Color(255, 255, 255, 255);
    private static Color colorEnCursor =    new Color(200, 200, 200, 250);
    private static Color colorEnClic =      new Color(155, 155, 155, 250);
    private static Color colorBloqueado =   new Color(155, 155, 155, 155);

    public static CollisionFilterGroupFlags colisionesMarca = CollisionFilterGroupFlags.StaticFilter | CollisionFilterGroupFlags.SensorTrigger;

    public static float RangoAleatorio(float min, float max)
    {
        var aleatorio = new Random();
        double valor = (aleatorio.NextDouble() * (max - min) + min);
        return (float)valor;
    }

    public static bool TocaEntorno(Collision colisión)
    {
        return (colisión.ColliderA.CollisionGroup == CollisionFilterGroups.StaticFilter ||
                colisión.ColliderB.CollisionGroup == CollisionFilterGroups.StaticFilter ||
                colisión.ColliderA.CollisionGroup == CollisionFilterGroups.SensorTrigger ||
                colisión.ColliderB.CollisionGroup == CollisionFilterGroups.SensorTrigger);
    }

    public static bool TocaEntornoEstáico(Collision colisión)
    {
        return (colisión.ColliderA.CollisionGroup == CollisionFilterGroups.StaticFilter ||
                colisión.ColliderB.CollisionGroup == CollisionFilterGroups.StaticFilter);
    }

    public static bool TocaEnemigo(Collision colisión)
    {
        return (colisión.ColliderA.CollisionGroup == CollisionFilterGroups.KinematicFilter || colisión.ColliderB.CollisionGroup == CollisionFilterGroups.KinematicFilter);
    }

    public static bool TocaJugador(Collision colisión)
    {
        return (colisión.ColliderA.CollisionGroup == CollisionFilterGroups.CharacterFilter || colisión.ColliderB.CollisionGroup == CollisionFilterGroups.CharacterFilter);
    }

    public static ControladorJugador RetornaJugador(Collision colisión)
    {
        var jugador = colisión.ColliderA.Entity.Get<ControladorJugador>();
        if (jugador == null)
            jugador = colisión.ColliderB.Entity.Get<ControladorJugador>();

        return jugador;
    }

    public static T ObtenerInterfaz<T>(Entity entidad)
    {
        foreach (var componente in entidad.Components)
        {
            if (componente is T)
            {
                return (T)(object)componente;
            }
        }
        return default;
    }

    // Interfaz
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
        botón.TouchDown += (s, a) => { SistemaSonidos.SonarBotónEntra(); };
        botón.TouchUp += (s, a) => { SistemaSonidos.SonarBotónSale(); };

        // Cambios color
        var colorBase = imagen.Color;

        // Guarda colores iniciales. Se usa en bloqueo.
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

    public static void ConfigurarBotónOculto(Button botón, Action action)
    {
        botón.Click += (sender, e) => { action.Invoke(); };
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
        //ISO8006;
        var fechaHora = fecha.ToString("yyyy-MM-ddThh:mm:ss");
        return fechaHora;
    }

    public static string FormatearTiempo(float segundos)
    {
        var tiempo = TimeSpan.FromSeconds(segundos);
        var formateado = tiempo.ToString(@"mm\:ss\:ff");

        if(tiempo.TotalHours > 1)
            formateado = tiempo.ToString(@"hh\:mm\:ss\:ff");

        return formateado;
    }
}
