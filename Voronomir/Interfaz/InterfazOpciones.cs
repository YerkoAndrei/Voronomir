using System;
using System.Linq;
using System.Globalization;
using Stride.UI;
using Stride.Engine;
using Stride.UI.Controls;
using Stride.UI.Panels;
using Stride.UI.Events;

namespace Voronomir;
using static Utilidades;
using static Constantes;

public class InterfazOpciones : StartupScript
{
    private UIElement página;
    private Grid Opciones;
    private Grid animOpciones;
    private Grid resoluciones;
    private TextBlock txtResoluciónActual;
    private TextBlock txtSensibilidadActual;
    private TextBlock txtCampoVisiónActual;

    private Grid pestañaGráficos;
    private Grid pestañaSonidos;
    private Grid pestañaJuego;

    private bool animando;

    public override void Start()
    {
        // Botones y deslisadores
        página = Entity.Get<UIComponent>().Page.RootElement;
        Opciones = página.FindVisualChildOfType<Grid>("Opciones");
        animOpciones = página.FindVisualChildOfType<Grid>("animOpciones");

        ConfigurarBotónOculto(página.FindVisualChildOfType<Button>("PanelOscuroOpciones"), EnClicVolver);
        ConfigurarBotónOculto(página.FindVisualChildOfType<Button>("btnPanel"), () => MostrarResoluciones(false));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnVolver"), EnClicVolver);

        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnGráficos"), () => EnClicPestaña("Gráficos"));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnSonidos"), () => EnClicPestaña("Sonidos"));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnJuego"), () => EnClicPestaña("Juego"));

        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnEspañol"), () => EnClicIdioma(Idiomas.español));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnInglés"), () => EnClicIdioma(Idiomas.inglés));

        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnDatosSí"), () => EnClicDatos(true));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnDatosNo"), () => EnClicDatos(false));

        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnGráficosBajos"), () => EnClicGráficos(NivelesConfiguración.bajo));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnGráficosMedios"), () => EnClicGráficos(NivelesConfiguración.medio));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnGráficosAltos"), () => EnClicGráficos(NivelesConfiguración.alto));

        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnSombrasBajas"), () => EnClicSombras(NivelesConfiguración.bajo));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnSombrasMedias"), () => EnClicSombras(NivelesConfiguración.medio));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnSombrasAltas"), () => EnClicSombras(NivelesConfiguración.alto));

        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnVSyncSí"), () => EnClicVSync(true));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnVSyncNo"), () => EnClicVSync(false));

        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnCompleta"), () => EnClicPantallaCompleta(true));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnVentana"), () => EnClicPantallaCompleta(false));

        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnResoluciones"), () => MostrarResoluciones(true));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnR0"), () => EnClicResolución(960, 540, "btnR0"));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnR1"), () => EnClicResolución(1280, 720, "btnR1"));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnR2"), () => EnClicResolución(1366, 768, "btnR2"));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnR3"), () => EnClicResolución(1600, 900, "btnR3"));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnR4"), () => EnClicResolución(1920, 1080, "btnR4"));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnR5"), () => EnClicResolución(2560, 1440, "btnR5"));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnR6"), () => EnClicResolución(3840, 2160, "btnR6"));
        
        pestañaGráficos = página.FindVisualChildOfType<Grid>("Gráficos");
        pestañaSonidos = página.FindVisualChildOfType<Grid>("Sonidos");
        pestañaJuego = página.FindVisualChildOfType<Grid>("Juego");
        EnClicPestaña("Gráficos");

        txtResoluciónActual = página.FindVisualChildOfType<TextBlock>("txtResoluciónActual");
        txtResoluciónActual.Text = SistemaMemoria.ObtenerConfiguración(Configuraciones.resolución).Replace("x", " x ");

        txtSensibilidadActual = página.FindVisualChildOfType<TextBlock>("txtSensibilidadActual");
        txtSensibilidadActual.Text = SistemaMemoria.ObtenerConfiguración(Configuraciones.sensibilidad);

        txtCampoVisiónActual = página.FindVisualChildOfType<TextBlock>("txtCampoVisiónActual");
        txtCampoVisiónActual.Text = SistemaMemoria.ObtenerConfiguración(Configuraciones.campoVisión);
        
        resoluciones = página.FindVisualChildOfType<Grid>("ListaResoluciones");
        resoluciones.Visibility = Visibility.Hidden;

        // Carga
        BloquearIdioma((Idiomas)Enum.Parse(typeof(Idiomas), SistemaMemoria.ObtenerConfiguración(Configuraciones.idioma)));
        BloquearGráficos((NivelesConfiguración)Enum.Parse(typeof(NivelesConfiguración), SistemaMemoria.ObtenerConfiguración(Configuraciones.gráficos)));
        BloquearSombras((NivelesConfiguración)Enum.Parse(typeof(NivelesConfiguración), SistemaMemoria.ObtenerConfiguración(Configuraciones.sombras)));
        BloquearVSync(bool.Parse(SistemaMemoria.ObtenerConfiguración(Configuraciones.vSync)));
        BloquearDatos(bool.Parse(SistemaMemoria.ObtenerConfiguración(Configuraciones.datos)));
        BloquearPantallaCompleta(bool.Parse(SistemaMemoria.ObtenerConfiguración(Configuraciones.pantallaCompleta)));

        DesbloquearBotonesResolución();
        switch (SistemaMemoria.ObtenerConfiguración(Configuraciones.resolución))
        {
            case "960x540":
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnR0"), true);
                break;
            case "1280x720":
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnR1"), true);
                break;
            case "1366x768":
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnR2"), true);
                break;
            case "1600x900":
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnR3"), true);
                break;
            case "1920x1080":
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnR4"), true);
                break;
            case "2560x1440":
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnR5"), true);
                break;
            case "3840x2160":
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnR6"), true);
                break;
        }

        // Slider
        var sliderGeneral = página.FindVisualChildOfType<Slider>("SliderGeneral");
        var sliderMúsica = página.FindVisualChildOfType<Slider>("SliderMúsica");
        var sliderEfectos = página.FindVisualChildOfType<Slider>("SliderEfectos");

        var sliderSensibilidad = página.FindVisualChildOfType<Slider>("SliderSensibilidad");
        var sliderCampoVisión = página.FindVisualChildOfType<Slider>("SliderCampoVisión");
        
        sliderGeneral.Value = float.Parse(SistemaMemoria.ObtenerConfiguración(Configuraciones.volumenGeneral), CultureInfo.InvariantCulture);
        sliderMúsica.Value = float.Parse(SistemaMemoria.ObtenerConfiguración(Configuraciones.volumenMúsica), CultureInfo.InvariantCulture);
        sliderEfectos.Value = float.Parse(SistemaMemoria.ObtenerConfiguración(Configuraciones.volumenEfectos), CultureInfo.InvariantCulture);
        
        sliderSensibilidad.Value = float.Parse(SistemaMemoria.ObtenerConfiguración(Configuraciones.sensibilidad), CultureInfo.InvariantCulture);
        sliderCampoVisión.Value = float.Parse(SistemaMemoria.ObtenerConfiguración(Configuraciones.campoVisión), CultureInfo.InvariantCulture);

        sliderGeneral.ValueChanged += ConfigurarVolumenGeneral;
        sliderMúsica.ValueChanged += ConfigurarVolumenMúsica;
        sliderEfectos.ValueChanged += ConfigurarVolumenEfectos;

        sliderSensibilidad.ValueChanged += ConfigurarSensibilidad;
        sliderCampoVisión.ValueChanged += ConfigurarCampoVisión;

        // Sonidos
        sliderGeneral.TouchDown += (s, a) => { SistemaSonidos.SonarBotónEntra(); };
        sliderGeneral.TouchUp += (s, a) => { SistemaSonidos.SonarBotónSale(); };

        sliderMúsica.TouchDown += (s, a) => { SistemaSonidos.SonarBotónEntra(); };
        sliderMúsica.TouchUp += (s, a) => { SistemaSonidos.SonarBotónSale(); };

        sliderEfectos.TouchDown += (s, a) => { SistemaSonidos.SonarBotónEntra(); };
        sliderEfectos.TouchUp += (s, a) => { SistemaSonidos.SonarBotónSale(); };

        sliderSensibilidad.TouchDown += (s, a) => { SistemaSonidos.SonarBotónEntra(); };
        sliderSensibilidad.TouchUp += (s, a) => { SistemaSonidos.SonarBotónSale(); };

        sliderCampoVisión.TouchDown += (s, a) => { SistemaSonidos.SonarBotónEntra(); };
        sliderCampoVisión.TouchUp += (s, a) => { SistemaSonidos.SonarBotónSale(); };

        // Actualiza gráficos
        //ActualizaGráficos((NivelesConfiguración)Enum.Parse(typeof(NivelesConfiguración), SistemaMemoria.ObtenerConfiguración(Configuraciones.gráficos)));
        //ActualizaSombras();
    }

    private void EnClicPestaña(string pestaña)
    {
        if (animando)
            return;

        switch (pestaña)
        {
            case "Gráficos":
                pestañaGráficos.Visibility = Visibility.Visible;
                pestañaSonidos.Visibility = Visibility.Hidden;
                pestañaJuego.Visibility = Visibility.Hidden;

                BloquearBotón(página.FindVisualChildOfType<Grid>("btnGráficos"), true);
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnSonidos"), false);
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnJuego"), false);
                break;
            case "Sonidos":
                pestañaGráficos.Visibility = Visibility.Hidden;
                pestañaSonidos.Visibility = Visibility.Visible;
                pestañaJuego.Visibility = Visibility.Hidden;

                BloquearBotón(página.FindVisualChildOfType<Grid>("btnGráficos"), false);
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnSonidos"), true);
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnJuego"), false);
                break;
            case "Juego":
                pestañaGráficos.Visibility = Visibility.Hidden;
                pestañaSonidos.Visibility = Visibility.Hidden;
                pestañaJuego.Visibility = Visibility.Visible;

                BloquearBotón(página.FindVisualChildOfType<Grid>("btnGráficos"), false);
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnSonidos"), false);
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnJuego"), true);
                break;
        }
    }

    private void EnClicIdioma(Idiomas idioma)
    {
        if (animando)
            return;

        SistemaTraducción.CambiarIdioma(idioma);
        BloquearIdioma(idioma);
        MostrarResoluciones(false);
    }

    private void ConfigurarVolumenGeneral(object sender, RoutedEventArgs e)
    {
        if (animando)
            return;

        var slider = (Slider)sender;
        SistemaMemoria.GuardarConfiguración(Configuraciones.volumenGeneral, slider.Value.ToString("n2", CultureInfo.InvariantCulture));
        MostrarResoluciones(false);
        SistemaSonidos.ActualizarVolumenMúsica();
    }

    private void ConfigurarVolumenMúsica(object sender, RoutedEventArgs e)
    {
        if (animando)
            return;

        var slider = (Slider)sender;
        SistemaMemoria.GuardarConfiguración(Configuraciones.volumenMúsica, slider.Value.ToString("n2", CultureInfo.InvariantCulture));
        MostrarResoluciones(false);
        SistemaSonidos.ActualizarVolumenMúsica();
    }

    private void ConfigurarVolumenEfectos(object sender, RoutedEventArgs e)
    {
        if (animando)
            return;

        var slider = (Slider)sender;
        SistemaMemoria.GuardarConfiguración(Configuraciones.volumenEfectos, slider.Value.ToString("n2", CultureInfo.InvariantCulture));
        MostrarResoluciones(false);
    }

    private void ConfigurarSensibilidad(object sender, RoutedEventArgs e)
    {
        if (animando)
            return;

        var slider = (Slider)sender;
        SistemaMemoria.GuardarConfiguración(Configuraciones.sensibilidad, slider.Value.ToString("n2", CultureInfo.InvariantCulture));

        var sensibilidadFormateada = float.Parse(SistemaMemoria.ObtenerConfiguración(Configuraciones.sensibilidad), SistemaTraducción.cultura);
        txtSensibilidadActual.Text = sensibilidadFormateada.ToString();
        MostrarResoluciones(false);
    }

    private void ConfigurarCampoVisión(object sender, RoutedEventArgs e)
    {
        if (animando)
            return;

        var slider = (Slider)sender;
        SistemaMemoria.GuardarConfiguración(Configuraciones.campoVisión, slider.Value.ToString("n0", CultureInfo.InvariantCulture));
        txtCampoVisiónActual.Text = SistemaMemoria.ObtenerConfiguración(Configuraciones.campoVisión);
        MostrarResoluciones(false);
    }

    private void EnClicDatos(bool activo)
    {
        if (animando)
            return;

        SistemaMemoria.GuardarConfiguración(Configuraciones.datos, activo.ToString());
        BloquearDatos(activo);
        MostrarResoluciones(false);
    }

    private void EnClicGráficos(NivelesConfiguración nivel)
    {
        if (animando)
            return;

        //SistemaMemoria.GuardarConfiguración(Configuraciones.gráficos, nivel.ToString());
        BloquearGráficos(nivel);
        //ActualizaGráficos(nivel);
        MostrarResoluciones(false);
    }

    private void EnClicSombras(NivelesConfiguración nivel)
    {
        if (animando)
            return;

        SistemaMemoria.GuardarConfiguración(Configuraciones.sombras, nivel.ToString());
        BloquearSombras(nivel);
        ActualizaSombras();
        MostrarResoluciones(false);
    }

    private void EnClicVSync(bool activo)
    {
        if (animando)
            return;

        SistemaMemoria.GuardarConfiguración(Configuraciones.vSync, activo.ToString());
        BloquearVSync(activo);
        MostrarResoluciones(false);
    }

    private void ActualizaGráficos(NivelesConfiguración nivel)
    {
        Services.GetService<SceneSystem>().GraphicsCompositor = SistemaEscenas.ObtenerGráficos(nivel);
    }

    private void ActualizaSombras()
    {
        var controladorLuces = SceneSystem.SceneInstance.RootScene.Children[0].Entities.Where(o => o.Get<ControladorSombras>() != null).FirstOrDefault();
        //controladorLuces.Get<ControladorSombras>().ActualizarSombras();
    }

    private void MostrarResoluciones(bool mostrar)
    {
        if (animando)
            return;

        if (mostrar)
        {
            if (resoluciones.Visibility == Visibility.Visible)
                resoluciones.Visibility = Visibility.Hidden;
            else
                resoluciones.Visibility = Visibility.Visible;
        }
        else
            resoluciones.Visibility = Visibility.Hidden;
    }

    private void EnClicResolución(int ancho, int alto, string botón)
    {
        if (animando)
            return;

        // Resolución
        var guardado = SistemaMemoria.ObtenerConfiguración(Configuraciones.resolución);
        var resolución = ancho.ToString() + "x" + alto.ToString();

        if (resolución == guardado)
        {
            MostrarResoluciones(false);
            return;
        }
        
        txtResoluciónActual.Text = resolución.Replace("x", " x ");
        //SistemaMemoria.GuardarConfiguración(Configuraciones.resolución, resolución);
        ActualizarResolución(ancho, alto);
        MostrarResoluciones(false);

        DesbloquearBotonesResolución();
        BloquearBotón(página.FindVisualChildOfType<Grid>(botón), true);        
    }

    private void ActualizarResolución(int ancho, int alto)
    {
        // Pantalla completa
        var pantallaCompleta = bool.Parse(SistemaMemoria.ObtenerConfiguración(Configuraciones.pantallaCompleta));
       //SistemaEscenas.CambiarPantalla(pantallaCompleta, ancho, alto);
    }

    private void EnClicPantallaCompleta(bool pantallaCompleta)
    {
        if (animando)
            return;

        //var guardado = bool.Parse(SistemaMemoria.ObtenerConfiguración(Configuraciones.pantallaCompleta));
        //if (pantallaCompleta == guardado)
        //    return;

        //SistemaMemoria.GuardarConfiguración(Configuraciones.pantallaCompleta, pantallaCompleta.ToString());
        BloquearPantallaCompleta(pantallaCompleta);
        ActualizaPantalla(pantallaCompleta);
        MostrarResoluciones(false);
    }

    private void ActualizaPantalla(bool pantallaCompleta)
    {
        // Resolución
        var resolución = SistemaMemoria.ObtenerConfiguración(Configuraciones.resolución).Split('x');
        var ancho = int.Parse(resolución[0]);
        var alto = int.Parse(resolución[1]);

        //SistemaEscenas.CambiarPantalla(pantallaCompleta, ancho, alto);
    }

    // Bloqueos
    private void BloquearIdioma(Idiomas idioma)
    {
        switch (idioma)
        {
            case Idiomas.español:
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnEspañol"), true);
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnInglés"), false);
                break;
            case Idiomas.inglés:
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnEspañol"), false);
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnInglés"), true);
                break;
        }
    }

    private void BloquearDatos(bool datos)
    {
        BloquearBotón(página.FindVisualChildOfType<Grid>("btnDatosSí"), datos);
        BloquearBotón(página.FindVisualChildOfType<Grid>("btnDatosNo"), !datos);
    }

    private void BloquearPantallaCompleta(bool pantallaCompleta)
    {
        BloquearBotón(página.FindVisualChildOfType<Grid>("btnCompleta"), pantallaCompleta);
        BloquearBotón(página.FindVisualChildOfType<Grid>("btnVentana"), !pantallaCompleta);
    }

    private void BloquearVSync(bool vSync)
    {
        BloquearBotón(página.FindVisualChildOfType<Grid>("btnVSyncSí"), vSync);
        BloquearBotón(página.FindVisualChildOfType<Grid>("btnVSyncNo"), !vSync);
    }

    private void BloquearGráficos(NivelesConfiguración nivel)
    {
        switch (nivel)
        {
            case NivelesConfiguración.bajo:
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnGráficosBajos"), true);
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnGráficosMedios"), false);
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnGráficosAltos"), false);
                break;
            case NivelesConfiguración.medio:
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnGráficosBajos"), false);
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnGráficosMedios"), true);
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnGráficosAltos"), false);
                break;
            case NivelesConfiguración.alto:
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnGráficosBajos"), false);
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnGráficosMedios"), false);
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnGráficosAltos"), true);
                break;
        }
    }

    private void BloquearSombras(NivelesConfiguración nivel)
    {
        switch (nivel)
        {
            case NivelesConfiguración.bajo:
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnSombrasBajas"), true);
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnSombrasMedias"), false);
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnSombrasAltas"), false);
                break;
            case NivelesConfiguración.medio:
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnSombrasBajas"), false);
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnSombrasMedias"), true);
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnSombrasAltas"), false);
                break;
            case NivelesConfiguración.alto:
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnSombrasBajas"), false);
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnSombrasMedias"), false);
                BloquearBotón(página.FindVisualChildOfType<Grid>("btnSombrasAltas"), true);
                break;
        }
    }

    private void DesbloquearBotonesResolución()
    {
        BloquearBotón(página.FindVisualChildOfType<Grid>("btnR0"), false);
        BloquearBotón(página.FindVisualChildOfType<Grid>("btnR1"), false);
        BloquearBotón(página.FindVisualChildOfType<Grid>("btnR2"), false);
        BloquearBotón(página.FindVisualChildOfType<Grid>("btnR3"), false);
        BloquearBotón(página.FindVisualChildOfType<Grid>("btnR4"), false);
        BloquearBotón(página.FindVisualChildOfType<Grid>("btnR5"), false);
        BloquearBotón(página.FindVisualChildOfType<Grid>("btnR6"), false);
    }

    private void EnClicVolver()
    {
        if (animando)
            return;

        animando = true;
        resoluciones.Visibility = Visibility.Hidden;
        SistemaAnimación.AnimarElemento(animOpciones, 0.2f, false, Direcciones.arriba, TipoCurva.rápida, () =>
        {
            Opciones.Visibility = Visibility.Hidden;
            animando = false;
        });
    }
}
