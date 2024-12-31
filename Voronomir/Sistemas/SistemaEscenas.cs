using System.Globalization;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Core.Serialization;
using Stride.Rendering.Compositing;
using Stride.Engine;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Panels;

namespace Voronomir;
using static Constantes;

public class SistemaEscenas : AsyncScript
{
    public UrlReference<Scene> escenaPruebas;
    public UrlReference<Scene> escenaMenú;
    public UrlReference<Scene> escenaDemo;

    public GraphicsCompositor compositorBajo;
    public GraphicsCompositor compositorMedio;
    public GraphicsCompositor compositorAlto;

    private static SistemaEscenas instancia;

    private static Escenas siguienteEscena;
    private static Scene escenaActual;
    private static bool abriendo;
    private static bool ocultando;

    private Grid panelOscuro;
    //private ImageElement imgCursor;

    private float duraciónOcultar;
    private float duraciónAbrir;
    private float tiempoLerp;
    private float tiempo;
    private bool juego;

    public override async Task Execute()
    {
        instancia = this;

        // Pantalla completa
        Game.Window.AllowUserResizing = false;
        Game.Window.Title = SistemaTraducción.ObtenerTraducción("nombreJuego");

        // Resolución
        var resolución = SistemaMemoria.ObtenerConfiguración(Configuraciones.resolución).Split('x');
        var pantallaCompleta = bool.Parse(SistemaMemoria.ObtenerConfiguración(Configuraciones.pantallaCompleta));

        var ancho = int.Parse(resolución[0], CultureInfo.InvariantCulture);
        var alto = int.Parse(resolución[1], CultureInfo.InvariantCulture);
        CambiarPantalla(pantallaCompleta, ancho, alto);

        // Predeterminado
        var página = Entity.Get<UIComponent>().Page.RootElement;
        panelOscuro = página.FindVisualChildOfType<Grid>("PanelOscuro");
        panelOscuro.Opacity = 0;
        duraciónOcultar = 0.2f;
        duraciónAbrir = 0.4f;

        escenaActual = Content.Load(escenaMenú);
        Entity.Scene.Children.Add(escenaActual);

        // Traduciones
        SistemaTraducción.ActualizarTextosEscena();

        // Cursor
        //Game.Window.IsMouseVisible = false;
        //imgCursor = página.FindVisualChildOfType<ImageElement>("imgCursor");

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
                    CargarEscena();
            }

            if (abriendo)
            {
                tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duraciónAbrir);
                panelOscuro.Opacity = MathUtil.Lerp(1f, 0f, tiempo);
                tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;

                // Fin
                if (tiempoLerp >= duraciónAbrir)
                    TerminarCarga();
            }
            await Script.NextFrame();
        }
    }
    /*
    private void PosicionarCursor()
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
    }*/

    public static void BloquearCursor(bool bloquear)
    {
        if (bloquear)
        {
            instancia.Game.IsMouseVisible = false;
            instancia.Input.LockMousePosition(true);
        }
        else
        {
            instancia.Input.UnlockMousePosition();
            instancia.Game.IsMouseVisible = true;
        }
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

    public static void CambiarEscena(Escenas escena)
    {
        if (ocultando || abriendo)
            return;

        BloquearCursor(true);
        SistemaSonidos.AsignarSonidosMundo(null);

        instancia.panelOscuro.Opacity = 0;
        instancia.panelOscuro.CanBeHitByUser = true;

        siguienteEscena = escena;
        abriendo = false;
        ocultando = true;
    }

    private async void CargarEscena()
    {
        // Fin Lerp
        tiempo = 0;
        tiempoLerp = 0;
        juego = false;
        ocultando = false;
        panelOscuro.Opacity = 1;
        await Task.Delay(10);

        // Descarga
        Content.Unload(escenaActual);
        Entity.Scene.Children.Remove(escenaActual);

        switch (siguienteEscena)
        {
            case Escenas.pruebas:
                escenaActual = Content.Load(escenaPruebas);
                juego = true;
                break;
            case Escenas.menú:
                escenaActual = Content.Load(escenaMenú);
                break;
            case Escenas.demo:
                escenaActual = Content.Load(escenaDemo);
                juego = true;
                break;
        }

        // Carga
        Entity.Scene.Children.Add(escenaActual);
        await Task.Delay(10);

        SistemaTraducción.ActualizarTextosEscena();

        // Retraso predeterminado
        await Task.Delay(200);
        abriendo = true;
    }

    private void TerminarCarga()
    {
        tiempo = 0;
        tiempoLerp = 0;
        abriendo = false;
        panelOscuro.Opacity = 0;
        panelOscuro.CanBeHitByUser = false;

        // Iniciar cuanto termine pantalla suave
        if (juego)
            ControladorJuego.Pausar(true);
    }

    public static bool ObtenerAnimando()
    {
        return abriendo || ocultando;
    }

    public static GraphicsCompositor ObtenerGráficos(Calidades nivel)
    {
        switch (nivel)
        {
            case Calidades.bajo:
                return instancia.compositorBajo;
            case Calidades.medio:
                return instancia.compositorMedio;
            default:
            case Calidades.alto:
                return instancia.compositorAlto;
        }
    }
}
