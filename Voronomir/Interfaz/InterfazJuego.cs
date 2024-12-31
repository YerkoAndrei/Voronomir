using System.Threading.Tasks;
using System.Threading;
using Stride.Core.Mathematics;
using Stride.UI;
using Stride.Engine;
using Stride.UI.Controls;
using Stride.UI.Panels;
using Stride.Input;

namespace Voronomir;
using static Utilidades;
using static Constantes;

public class InterfazJuego : SyncScript
{
    public Color vidaCompleta;
    public Color vidaVacía;
    public Color armaNormal;
    public Color armaSeleccionada;

    private ImageElement imgVida;
    private ImageElement imgFinal;

    private ImageElement miraEspada;
    private ImageElement miraEscopeta;
    private ImageElement miraMetralleta;
    private ImageElement miraRifle;
    private ImageElement sombraRifle;
    private ImageElement miraLanzagranadas;

    private ImageElement imgEspada;
    private ImageElement imgEscopeta;
    private ImageElement imgMetralleta;
    private ImageElement imgRifle;
    private ImageElement imgLanzagranadas;

    private Grid imgLlaveAzul;
    private Grid imgLlaveRoja;
    private Grid imgLlaveAmarilla;

    private Grid imgDaño;
    private Grid imgInvulnerabilidad;
    private Grid imgVelocidad;

    private UniformGrid panelDepuración;
    private TextBlock txtCuadrosPorSegundo;
    private TextBlock txtTiempo;
    private TextBlock txtPosición;
    private TextBlock txtVelocidad;
    private TextBlock txtAceleración;
    private TextBlock txtCampoVisión;
    private TextBlock txtMetralleta;
    private TextBlock txtPrueba;

    private TextBlock txtMensaje;
    private TextBlock txtMuerteTiempo;

    private TextBlock txtFinalNivel;
    private TextBlock txtDificultad;
    private TextBlock txtFinalTiempo;
    private TextBlock txtEnemigos;
    private TextBlock txtSecretos;

    private ImageElement imgDecoE;
    private TextBlock txtFinalE;

    private Grid btnContinuar;
    private Grid btnFinalSalir;

    private UniformGrid panelDatosFinales;
    private Grid panelPausa;
    private Grid panelMuerte;
    private Grid panelFinal;

    private Grid opciones;
    private Grid animOpciones;

    private bool animando;
    private float tamañoVida;
    private CancellationTokenSource tokenMensaje;

    public override void Start()
    {
        var página = Entity.Get<UIComponent>().Page.RootElement;

        opciones = página.FindVisualChildOfType<Grid>("Opciones");
        animOpciones = página.FindVisualChildOfType<Grid>("animOpciones");
        opciones.Visibility = Visibility.Hidden;

        // Interfaz
        imgVida = página.FindVisualChildOfType<ImageElement>("imgVida");
        imgFinal = página.FindVisualChildOfType<ImageElement>("imgFinal");

        miraEspada = página.FindVisualChildOfType<ImageElement>("miraEspada");
        miraEscopeta = página.FindVisualChildOfType<ImageElement>("miraEscopeta");
        miraMetralleta = página.FindVisualChildOfType<ImageElement>("miraMetralleta");
        miraRifle = página.FindVisualChildOfType<ImageElement>("miraRifle");
        sombraRifle = página.FindVisualChildOfType<ImageElement>("sombraRifle");
        miraLanzagranadas = página.FindVisualChildOfType<ImageElement>("miraLanzagranadas");

        imgEspada = página.FindVisualChildOfType<ImageElement>("imgEspada");
        imgEscopeta = página.FindVisualChildOfType<ImageElement>("imgEscopeta");
        imgMetralleta = página.FindVisualChildOfType<ImageElement>("imgMetralleta");
        imgRifle = página.FindVisualChildOfType<ImageElement>("imgRifle");
        imgLanzagranadas = página.FindVisualChildOfType<ImageElement>("imgLanzagranadas");

        imgLlaveAzul = página.FindVisualChildOfType<Grid>("imgLlaveAzul");
        imgLlaveRoja = página.FindVisualChildOfType<Grid>("imgLlaveRoja");
        imgLlaveAmarilla = página.FindVisualChildOfType<Grid>("imgLlaveAmarilla");

        imgDaño = página.FindVisualChildOfType<Grid>("imgDaño");
        imgInvulnerabilidad = página.FindVisualChildOfType<Grid>("imgInvulnerabilidad");
        imgVelocidad = página.FindVisualChildOfType<Grid>("imgVelocidad");

        panelDepuración = página.FindVisualChildOfType<UniformGrid>("Depuración");
        txtMensaje = página.FindVisualChildOfType<TextBlock>("txtMensaje");

        // Datos depuración
        txtCuadrosPorSegundo = página.FindVisualChildOfType<TextBlock>("txtCuadrosPorSegundo");
        txtTiempo = página.FindVisualChildOfType<TextBlock>("txtTiempo");
        txtPosición = página.FindVisualChildOfType<TextBlock>("txtPosición");
        txtVelocidad = página.FindVisualChildOfType<TextBlock>("txtVelocidad");
        txtAceleración = página.FindVisualChildOfType<TextBlock>("txtAceleración");
        txtCampoVisión = página.FindVisualChildOfType<TextBlock>("txtCampoVisión");
        txtMetralleta = página.FindVisualChildOfType<TextBlock>("txtMetralleta");
        txtPrueba = página.FindVisualChildOfType<TextBlock>("txtPrueba");

        // Datos finales
        txtMuerteTiempo = página.FindVisualChildOfType<TextBlock>("txtMuerteTiempo");
        txtFinalNivel = página.FindVisualChildOfType<TextBlock>("txtFinalNivel");
        txtDificultad = página.FindVisualChildOfType<TextBlock>("txtDificultad");
        txtFinalTiempo = página.FindVisualChildOfType<TextBlock>("txtFinalTiempo");
        txtEnemigos = página.FindVisualChildOfType<TextBlock>("txtEnemigos");
        txtSecretos = página.FindVisualChildOfType<TextBlock>("txtSecretos");

        imgDecoE = página.FindVisualChildOfType<ImageElement>("imgDecoE");
        txtFinalE = página.FindVisualChildOfType<TextBlock>("txtFinalE");

        // Paneles
        panelDatosFinales = página.FindVisualChildOfType<UniformGrid>("DatosFinales");
        panelPausa = página.FindVisualChildOfType<Grid>("PanelPausa");
        panelMuerte = página.FindVisualChildOfType<Grid>("PanelMuerte");
        panelFinal = página.FindVisualChildOfType<Grid>("PanelFinal");

        // Botónes
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnReanudar"), Pausar);
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnReiniciar"), EnClicReiniciar);
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnReintentar"), EnClicReiniciar);
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnPausaSalir"), EnClicSalir);
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnMuerteSalir"), EnClicSalir);
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnOpciones"), EnClicOpciones);

        btnContinuar = página.FindVisualChildOfType<Grid>("btnContinuar");
        btnFinalSalir = página.FindVisualChildOfType<Grid>("btnFinalSalir");

        ConfigurarBotón(btnContinuar, EnClicContinuar);
        ConfigurarBotón(btnFinalSalir, EnClicSalir);
        BloquearBotón(btnContinuar, true);
        BloquearBotón(btnFinalSalir, true);

        // Predeterminado
        tamañoVida = imgVida.Width;
        txtMensaje.Text = string.Empty;
        txtMensaje.Opacity = 0;
        tokenMensaje = new CancellationTokenSource();

        panelDatosFinales.Visibility = Visibility.Hidden;
        panelPausa.Visibility = Visibility.Hidden;
        panelMuerte.Visibility = Visibility.Hidden;
        panelFinal.Visibility = Visibility.Hidden;

        imgDecoE.Visibility = Visibility.Hidden;
        txtFinalE.Visibility = Visibility.Hidden;

        // Campo Visión

        // Mira
        var colorMira = SistemaMemoria.ObtenerConfiguración(Configuraciones.colorMira).Split(',');
        var r = float.Parse(colorMira[0]) / 255;
        var g = float.Parse(colorMira[1]) / 255;
        var b = float.Parse(colorMira[2]) / 255;
        ConfigurarMira(new Color(r, g, b));

        // Depuración
        panelDepuración.Visibility = Visibility.Hidden;
        if (bool.Parse(SistemaMemoria.ObtenerConfiguración(Configuraciones.depuración)))
            panelDepuración.Visibility = Visibility.Visible;

        ApagarMiras();
        ApagarLlaves();
        ApagarPoderes();

        // Bloqueo
        BloquearInterfaz(true);
    }

    public override void Update()
    {
        if (Input.IsKeyPressed(Keys.Escape))
            Pausar();

        if (Input.IsKeyPressed(Keys.E))
        {
            if (panelPausa.Visibility == Visibility.Visible)
                Pausar();
            else if (panelMuerte.Visibility == Visibility.Visible)
                EnClicReiniciar();
            else if (panelDatosFinales.Visibility == Visibility.Visible)
                EnClicContinuar();
        }

        if (ControladorJuego.ObtenerActivo() && panelDepuración.Visibility == Visibility.Visible)
            ActualizarDepuración();
    }

    private void Pausar()
    {
        if (SistemaEscenas.ObtenerAnimando())
            return;

        ControladorJuego.Pausar(!ControladorJuego.ObtenerActivo());
        SistemaSonidos.PausarSonidosMundo(!ControladorJuego.ObtenerActivo());
        opciones.Visibility = Visibility.Hidden;

        if (ControladorJuego.ObtenerActivo())
        {
            BloquearInterfaz(true);
            panelPausa.Visibility = Visibility.Hidden;
        }
        else
        {
            BloquearInterfaz(false);
            panelPausa.Visibility = Visibility.Visible;
        }
    }

    private void BloquearInterfaz(bool bloquear)
    {
        SistemaEscenas.BloquearCursor(bloquear);

        if (bloquear)
            Game.UpdateTime.Factor = 1;
        else
            Game.UpdateTime.Factor = 0;
    }

    private void EnClicReiniciar()
    {
        ControladorJuego.ApagarFísicas();
        SistemaEscenas.CambiarEscena(ControladorJuego.ObtenerEscena());
    }

    private void EnClicContinuar()
    {
        ControladorJuego.ApagarFísicas();
        SistemaEscenas.CambiarEscena(ControladorJuego.ObtenerSiguienteEscena());
    }

    private void EnClicOpciones()
    {
        if (animando)
            return;

        animando = true;
        opciones.Visibility = Visibility.Visible;
        SistemaAnimación.AnimarElemento(animOpciones, 0.2f, true, Direcciones.arriba, TipoCurva.rápida, () =>
        {
            animando = false;
        });
    }

    public void ConfigurarMira(Color color)
    {
        miraEspada.Color = color;
        miraEscopeta.Color = color;
        miraMetralleta.Color = color;
        miraRifle.Color = color;
        miraLanzagranadas.Color = color;
    }

    public void ConfigurarDepuración(bool activo)
    {
        if (activo)
            panelDepuración.Visibility = Visibility.Visible;
        else
            panelDepuración.Visibility = Visibility.Hidden;
    }

    private void EnClicSalir()
    {
        ControladorCofres.ApagarFísicas();
        SistemaEscenas.CambiarEscena(Escenas.menú);
    }

    public void ActualizarVida(float porcentaje)
    {
        imgVida.Opacity = MathUtil.Clamp((1 - porcentaje), 0.6f, 0.9f);
        imgVida.Color = Color.Lerp(vidaVacía, vidaCompleta, porcentaje);
        imgVida.Width = tamañoVida * porcentaje;
    }

    public void CambiarÍcono(Armas arma)
    {
        ApagarÍconos();
        switch (arma)
        {
            case Armas.espada:
                imgEspada.Color = armaSeleccionada;
                break;
            case Armas.escopeta:
                imgEscopeta.Color = armaSeleccionada;
                break;
            case Armas.metralleta:
                imgMetralleta.Color = armaSeleccionada;
                break;
            case Armas.rifle:
                imgRifle.Color = armaSeleccionada;
                break;
            case Armas.lanzagranadas:
                imgLanzagranadas.Color = armaSeleccionada;
                break;
        }
    }

    public void CambiarMira(Armas arma)
    {
        switch (arma)
        {
            case Armas.espada:
                miraEspada.Visibility = Visibility.Visible;
                break;
            case Armas.escopeta:
                miraEscopeta.Visibility = Visibility.Visible;
                break;
            case Armas.metralleta:
                miraMetralleta.Visibility = Visibility.Visible;
                break;
            case Armas.rifle:
                break;
            case Armas.lanzagranadas:
                miraLanzagranadas.Visibility = Visibility.Visible;
                break;
        }
    }

    public void MostrarMiraRifle(bool mostrar)
    {
        if (mostrar)
        {
            miraRifle.Visibility = Visibility.Visible;
            ActualizarSombraRifle(1);
        }
        else
            miraRifle.Visibility = Visibility.Hidden;
    }

    public void ActualizarSombraRifle(float opacidad)
    {
        if(opacidad > 0)
            sombraRifle.Visibility = Visibility.Visible;

        sombraRifle.Opacity = opacidad;
    }

    public void ApagarMiras()
    {
        miraEspada.Visibility = Visibility.Hidden;
        miraEscopeta.Visibility = Visibility.Hidden;
        miraMetralleta.Visibility = Visibility.Hidden;
        miraRifle.Visibility = Visibility.Hidden;
        sombraRifle.Visibility = Visibility.Hidden;
        miraLanzagranadas.Visibility = Visibility.Hidden;
    }

    public void ApagarÍconos()
    {
        imgEspada.Color = armaNormal;
        imgEscopeta.Color = armaNormal;
        imgMetralleta.Color = armaNormal;
        imgRifle.Color = armaNormal;
        imgLanzagranadas.Color = armaNormal;
    }

    private void ApagarLlaves()
    {
        imgLlaveAzul.Visibility = Visibility.Hidden;
        imgLlaveRoja.Visibility = Visibility.Hidden;
        imgLlaveAmarilla.Visibility = Visibility.Hidden;
    }

    public void ActivarLlave(Llaves llave)
    {
        switch(llave)
        {
            case Llaves.azul:
                imgLlaveAzul.Visibility = Visibility.Visible;
                break;
            case Llaves.roja:
                imgLlaveRoja.Visibility = Visibility.Visible;
                break;
            case Llaves.amarilla:
                imgLlaveAmarilla.Visibility = Visibility.Visible;
                break;
        }
    }

    public void ActivarPoder(Poderes poder, bool activar)
    {
        switch (poder)
        {
            case Poderes.daño:
                if(activar)
                    imgDaño.Visibility = Visibility.Visible;
                else
                    imgDaño.Visibility = Visibility.Hidden;
                break;
            case Poderes.invulnerabilidad:
                if (activar)
                    imgInvulnerabilidad.Visibility = Visibility.Visible;
                else
                    imgInvulnerabilidad.Visibility = Visibility.Hidden;
                break;
            case Poderes.velocidad:
                if (activar)
                    imgVelocidad.Visibility = Visibility.Visible;
                else
                    imgVelocidad.Visibility = Visibility.Hidden;
                break;
        }
    }

    private void ApagarPoderes()
    {
        imgDaño.Visibility = Visibility.Hidden;
        imgInvulnerabilidad.Visibility = Visibility.Hidden;
        imgVelocidad.Visibility = Visibility.Hidden;
    }

    public async void MostrarMensaje(string mensaje)
    {
        // Cancela mensaje actual para iniciar nuevo
        tokenMensaje.Cancel();
        tokenMensaje = new CancellationTokenSource();

        txtMensaje.Text = mensaje;
        txtMensaje.Opacity = 1;

        float duración = 1f;
        float tiempoLerp = 0;
        float tiempo = 0;

        var token = tokenMensaje.Token;
        if (!token.IsCancellationRequested)
            await Task.Delay(500);

        while (tiempoLerp < duración)
        {
            if (token.IsCancellationRequested)
                return;

            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            txtMensaje.Opacity = MathUtil.Lerp(1f, 0f, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        // Fin
        txtMensaje.Text = string.Empty;
        txtMensaje.Opacity = 0;
    }

    public void Morir()
    {
        imgVida.Width = 0;

        ApagarMiras();
        ApagarLlaves();
        ApagarPoderes();

        ControladorJuego.Pausar(false);
        BloquearInterfaz(false);

        txtMuerteTiempo.Text = ControladorJuego.ObtenerTextoDuración();
        panelMuerte.Visibility = Visibility.Visible;
    }

    public void Finalizar()
    {
        ControladorJuego.Pausar(false);
        BloquearInterfaz(false);

        if (panelDepuración.Visibility == Visibility.Visible)
            ActualizarDepuración();

        // Datos final
        txtFinalNivel.Text = ControladorJuego.ObtenerEscena().ToString() + ": " + SistemaTraducción.ObtenerTraducción(ControladorJuego.ObtenerEscena().ToString());
        txtDificultad.Text = SistemaTraducción.ObtenerTraducción(SistemaMemoria.Dificultad.ToString());
        txtFinalTiempo.Text = ControladorJuego.ObtenerTextoDuración();
        txtEnemigos.Text = ControladorJuego.ObtenerTextoEnemigos();
        txtSecretos.Text = ControladorJuego.ObtenerTextoSecretos();

        txtFinalNivel.Visibility = Visibility.Hidden;
        txtDificultad.Visibility = Visibility.Hidden;
        panelFinal.Visibility = Visibility.Visible;

        SistemaSonidos.SonarPuerta();
        MostrarFinal();
    }

    public async void MostrarFinal()
    {
        float duración = 0.4f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            imgFinal.Opacity = MathUtil.Lerp(0f, 1f, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Task.Delay(1);
        }

        // Fin
        imgFinal.Opacity = 1;        
        panelDatosFinales.Visibility = Visibility.Visible;
        txtFinalNivel.Visibility = Visibility.Visible;
        txtDificultad.Visibility = Visibility.Visible;
        imgDecoE.Visibility = Visibility.Visible;
        txtFinalE.Visibility = Visibility.Visible;

        BloquearBotón(btnContinuar, false);
        BloquearBotón(btnFinalSalir, false);
        SistemaSonidos.SonarFinalizar();
    }

    private void ActualizarDepuración()
    {
        txtCuadrosPorSegundo.Text = Game.UpdateTime.FramePerSecond.ToString("000");
        txtTiempo.Text = FormatearTiempo(ControladorJuego.ObtenerTiempo());
        txtPosición.Text = ControladorJuego.ObtenerPosiciónJugador().ToString("n3");
        txtVelocidad.Text = (ControladorJuego.ObtenerVelocidad() * 10).ToString("n3");
        txtAceleración.Text = ControladorJuego.ObtenerAceleración().ToString("n3");
        txtCampoVisión.Text = ControladorJuego.ObtenerCampoVisión().ToString("000");
        txtMetralleta.Text = ControladorJuego.ObtenerCalentamientoMetralleta().ToString("n3");
        txtPrueba.Text = ControladorJuego.ObtenerValorPrueba().ToString("n3");
    }
}
