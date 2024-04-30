﻿using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Core.Serialization;
using Stride.Engine;
using Stride.Rendering.Compositing;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Panels;

namespace Bozobaralika;
using static Constantes;

public class SistemaEscenas : AsyncScript
{
    public UrlReference<Scene> escenaMenú;

    public GraphicsCompositor compositorBajo;
    public GraphicsCompositor compositorMedio;
    public GraphicsCompositor compositorAlto;

    private static SistemaEscenas instancia;

    private static Escenas siguienteEscena;
    private static Scene escenaActual;
    private static bool abriendo;
    private static bool ocultando;

    private Grid panelOscuro;
    private ImageElement imgCursor;

    public override async Task Execute()
    {
        instancia = this;

        // Pantalla completa
        Game.Window.AllowUserResizing = false;
        Game.Window.Title = SistemaTraducción.ObtenerTraducción("nombreJuego");
        /*
        // Resolución
        var resolución = SistemaMemoria.ObtenerConfiguración(Configuraciones.resolución).Split('x');
        var pantallaCompleta = bool.Parse(SistemaMemoria.ObtenerConfiguración(Configuraciones.pantallaCompleta));

        var ancho = int.Parse(resolución[0]);
        var alto = int.Parse(resolución[1]);
        CambiarPantalla(pantallaCompleta, ancho, alto);
        */

        // Predeterminado
        var página = Entity.Get<UIComponent>().Page.RootElement;
        panelOscuro = página.FindVisualChildOfType<Grid>("PanelOscuro");
        panelOscuro.Opacity = 0;

        escenaActual = Content.Load(escenaMenú);
        Entity.Scene.Children.Add(escenaActual);

        // Traduciones
        SistemaTraducción.ActualizarTextosEscena();

        // Cursor
        //Game.Window.IsMouseVisible = false;
        //imgCursor = página.FindVisualChildOfType<ImageElement>("imgCursor");

        var duraciónOcultar = 0.2f;
        var duraciónAbrir = 0.4f;
        var tiempoLerp = 0f;
        var tiempo = 0f;

        while (Game.IsRunning)
        {
            // Cursor es una imagen
            //PosicionarCursor();
            if (ocultando)
            {
                tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duraciónOcultar);
                panelOscuro.Opacity = MathUtil.Lerp(0f, 1f, tiempo);
                tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;

                // Fin
                if (tiempoLerp >= duraciónOcultar)
                {
                    tiempo = 0;
                    tiempoLerp = 0;
                    CargarEscena();
                }
            }

            if (abriendo)
            {
                tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duraciónAbrir);
                panelOscuro.Opacity = MathUtil.Lerp(1f, 0f, tiempo);
                tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;

                // Fin
                if (tiempoLerp >= duraciónAbrir)
                {
                    panelOscuro.Opacity = 0;
                    panelOscuro.CanBeHitByUser = false;
                    abriendo = false;
                }
            }
            await Script.NextFrame();
        }
    }

    public void PosicionarCursor()
    {
        // Cursor es un ImageElement
        // ¿Optimizar?
        float left = 0;
        float top = 0;
        float right = 0;
        float bottom = 0;

        if (Input.MousePosition.X > 0.5f)
            left = (Input.MousePosition.X - 0.5f) * 2 * 1280;
        else
            right = (0.5f - Input.MousePosition.X) * 2 * 1280;

        if (Input.MousePosition.Y > 0.5f)
            top = (Input.MousePosition.Y - 0.5f) * 2 * 720;
        else
            bottom = (0.5f - Input.MousePosition.Y) * 2 * 720;

        imgCursor.Margin = new Thickness(left, top, right, bottom);
    }

    public static void CambiarPantalla(bool pantallaCompleta, int ancho, int alto)
    {
        instancia.Game.Window.Visible = false;

        instancia.Game.Window.SetSize(new Int2(ancho, alto));
        instancia.Game.Window.PreferredWindowedSize = new Int2(ancho, alto);
        instancia.Game.Window.PreferredFullscreenSize = new Int2(ancho, alto);
        instancia.Game.Window.IsFullscreen = pantallaCompleta;

        instancia.Game.Window.Visible = true;
    }

    public static void RecargarEscena()
    {
        CambiarEscena(ControladorPartida.ObtenerEscena());
    }

    public static void CambiarEscena(Escenas escena)
    {
        if (ocultando || abriendo)
            return;

        instancia.panelOscuro.Opacity = 0;
        instancia.panelOscuro.CanBeHitByUser = true;

        siguienteEscena = escena;
        abriendo = false;
        ocultando = true;
    }

    private async void CargarEscena()
    {
        // Fin Lerp
        ocultando = false;
        panelOscuro.Opacity = 1;

        // Descarga
        Content.Unload(escenaActual);
        Entity.Scene.Children.Remove(escenaActual);

        switch (siguienteEscena)
        {
            case Escenas.menú:
                escenaActual = Content.Load(escenaMenú);
                break;
        }

        // Carga
        Entity.Scene.Children.Add(escenaActual);
        await Task.Delay(200);

        // Traduciones
        SistemaTraducción.ActualizarTextosEscena();

        // Inicio Lerp
        abriendo = true;
    }

    public static GraphicsCompositor ObtenerGráficos(NivelesConfiguración nivel)
    {
        switch (nivel)
        {
            case NivelesConfiguración.bajo:
                return instancia.compositorBajo;
            case NivelesConfiguración.medio:
                return instancia.compositorMedio;
            default:
            case NivelesConfiguración.alto:
                return instancia.compositorAlto;
        }
    }
}
