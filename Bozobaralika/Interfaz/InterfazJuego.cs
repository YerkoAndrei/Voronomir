using Stride.Core.Mathematics;
using Stride.UI;
using Stride.Engine;
using Stride.UI.Controls;
using Stride.UI.Panels;
using Stride.Input;

namespace Bozobaralika;
using static Utilidades;
using static Constantes;

public class InterfazJuego : SyncScript
{
    public Color vidaCompleta;
    public Color vidaVacía;
    public Color armaNormal;
    public Color armaSeleccionada;

    private ImageElement imgVida;
    private float tamañoVida;

    private ImageElement miraEspada;
    private ImageElement miraEscopeta;
    private ImageElement miraMetralleta;
    private ImageElement miraRifle;
    private ImageElement miraLanzagranadas;

    private ImageElement imgEspada;
    private ImageElement imgEscopeta;
    private ImageElement imgMetralleta;
    private ImageElement imgRifle;
    private ImageElement imgLanzagranadas;

    private ImageElement imgLlaveAzul;
    private ImageElement imgLlaveRoja;

    private ImageElement imgDaño;
    private ImageElement imgInvencibilidad;
    private ImageElement imgRapidez;

    private UniformGrid panelDatos;
    private TextBlock txtTiempo;
    private TextBlock txtAceleración;

    private TextBlock txtNivel;
    private TextBlock txtTiempoFinal;
    private TextBlock txtEnemigos;
    private TextBlock txtSecretos;

    private UniformGrid panelDatosFinales;
    private Grid panelPausa;
    private Grid panelMuerte;
    private Grid panelFinal;

    public override void Start()
    {
        var página = Entity.Get<UIComponent>().Page.RootElement;

        // Interfaz
        imgVida = página.FindVisualChildOfType<ImageElement>("imgVida");

        miraEspada = página.FindVisualChildOfType<ImageElement>("miraEspada");
        miraEscopeta = página.FindVisualChildOfType<ImageElement>("miraEscopeta");
        miraMetralleta = página.FindVisualChildOfType<ImageElement>("miraMetralleta");
        miraRifle = página.FindVisualChildOfType<ImageElement>("miraRifle");
        miraLanzagranadas = página.FindVisualChildOfType<ImageElement>("miraLanzagranadas");

        imgEspada = página.FindVisualChildOfType<ImageElement>("imgEspada");
        imgEscopeta = página.FindVisualChildOfType<ImageElement>("imgEscopeta");
        imgMetralleta = página.FindVisualChildOfType<ImageElement>("imgMetralleta");
        imgRifle = página.FindVisualChildOfType<ImageElement>("imgRifle");
        imgLanzagranadas = página.FindVisualChildOfType<ImageElement>("imgLanzagranadas");

        imgLlaveAzul = página.FindVisualChildOfType<ImageElement>("imgLlaveAzul");
        imgLlaveRoja = página.FindVisualChildOfType<ImageElement>("imgLlaveRoja");

        imgDaño = página.FindVisualChildOfType<ImageElement>("imgDaño");
        imgInvencibilidad = página.FindVisualChildOfType<ImageElement>("imgInvencibilidad");
        imgRapidez = página.FindVisualChildOfType<ImageElement>("imgRapidez");

        panelDatos = página.FindVisualChildOfType<UniformGrid>("Datos");
        txtTiempo = página.FindVisualChildOfType<TextBlock>("txtTiempo");
        txtAceleración = página.FindVisualChildOfType<TextBlock>("txtAceleración");

        // Datos finales
        txtNivel = página.FindVisualChildOfType<TextBlock>("txtNivel");
        txtTiempoFinal = página.FindVisualChildOfType<TextBlock>("txtTiempoFinal");
        txtEnemigos = página.FindVisualChildOfType<TextBlock>("txtEnemigos");
        txtSecretos = página.FindVisualChildOfType<TextBlock>("txtSecretos");

        // Paneles
        panelDatosFinales = página.FindVisualChildOfType<UniformGrid>("DatosFinales");
        panelPausa = página.FindVisualChildOfType<Grid>("PanelPausa");
        panelMuerte = página.FindVisualChildOfType<Grid>("PanelMuerte");
        panelFinal = página.FindVisualChildOfType<Grid>("PanelFinal");

        // Botónes
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnReanudar"), Pausar);
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnReiniciar"), EnClicReiniciar);
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnReintentar"), EnClicReiniciar);
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnContinuar"), EnClicContinuar);
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnPausaSalir"), EnClicSalir);
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnMuerteSalir"), EnClicSalir);
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnFinalSalir"), EnClicSalir);

        // Predeterminado
        tamañoVida = imgVida.Width;

        panelDatosFinales.Visibility = Visibility.Hidden;
        panelPausa.Visibility = Visibility.Hidden;
        panelMuerte.Visibility = Visibility.Hidden;
        panelFinal.Visibility = Visibility.Hidden;

        // PENDIENTE: configuración
        panelDatos.Visibility = Visibility.Visible;

        ApagarMiras();
        ApagarLlaves();
        ApagarPoderes();

        // Bloqueo
        BloquearInterfaz(false);
    }

    public override void Update()
    {
        if (Input.IsKeyPressed(Keys.Escape))
            Pausar();

        if(ControladorPartida.ObtenerActivo())
        {
            txtTiempo.Text = FormatearTiempo(ControladorPartida.ObtenerTiempo());
            txtAceleración.Text = ControladorPartida.ObtenerAceleración().ToString("n2");
        }
    }

    private void Pausar()
    {
        ControladorPartida.Pausar(!ControladorPartida.ObtenerActivo());

        if(ControladorPartida.ObtenerActivo())
        {
            BloquearInterfaz(false);
            panelPausa.Visibility = Visibility.Hidden;
        }
        else
        {
            BloquearInterfaz(true);
            panelPausa.Visibility = Visibility.Visible;
        }
    }

    private void BloquearInterfaz(bool bloquear)
    {
        if (bloquear)
        {
            Input.UnlockMousePosition();
            Game.IsMouseVisible = true;
            Game.UpdateTime.Factor = 0;
        }
        else
        {
            Input.LockMousePosition(true);
            Game.IsMouseVisible = false;
            Game.UpdateTime.Factor = 1;
        }
    }

    private void EnClicReiniciar()
    {
        SistemaEscenas.CambiarEscena(ControladorPartida.ObtenerEscena());
    }

    private void EnClicContinuar()
    {
        SistemaEscenas.CambiarEscena(ControladorPartida.ObtenerSiguienteEscena());
    }

    private void EnClicSalir()
    {
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
        ApagarMiras();

        if(mostrar)
            miraRifle.Visibility = Visibility.Visible;
        else
            miraRifle.Visibility = Visibility.Hidden;
    }

    public void ApagarMiras()
    {
        miraEspada.Visibility = Visibility.Hidden;
        miraEscopeta.Visibility = Visibility.Hidden;
        miraMetralleta.Visibility = Visibility.Hidden;
        miraRifle.Visibility = Visibility.Hidden;
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
            case Poderes.invencibilidad:
                if (activar)
                    imgInvencibilidad.Visibility = Visibility.Visible;
                else
                    imgInvencibilidad.Visibility = Visibility.Hidden;
                break;
            case Poderes.rapidez:
                if (activar)
                    imgRapidez.Visibility = Visibility.Visible;
                else
                    imgRapidez.Visibility = Visibility.Hidden;
                break;
        }
    }

    private void ApagarPoderes()
    {
        imgDaño.Visibility = Visibility.Hidden;
        imgInvencibilidad.Visibility = Visibility.Hidden;
        imgRapidez.Visibility = Visibility.Hidden;
    }

    public void Morir()
    {
        imgVida.Color = vidaVacía;
        imgVida.Width = tamañoVida;

        ControladorPartida.Pausar(false);
        BloquearInterfaz(true);

        // Datos
        //txtNivel.Text = ControladorPartida.ObtenerEscena().ToString() + ": " + SistemaTraducción.ObtenerTraducción(ControladorPartida.ObtenerEscena().ToString());
        txtTiempoFinal.Text = ControladorPartida.ObtenerTextoDuración();
        txtEnemigos.Text = ControladorPartida.ObtenerTextoEnemigos();
        txtSecretos.Text = ControladorPartida.ObtenerTextoSecretos();

        panelDatosFinales.Visibility = Visibility.Visible;
        panelMuerte.Visibility = Visibility.Visible;
    }

    public void Finalizar()
    {
        ControladorPartida.Pausar(false);
        BloquearInterfaz(true);

        // Datos
        //txtNivel.Text = ControladorPartida.ObtenerEscena().ToString() + ": " + SistemaTraducción.ObtenerTraducción(ControladorPartida.ObtenerEscena().ToString());
        txtTiempoFinal.Text = ControladorPartida.ObtenerTextoDuración();
        txtEnemigos.Text = ControladorPartida.ObtenerTextoEnemigos();
        txtSecretos.Text = ControladorPartida.ObtenerTextoSecretos();

        panelDatosFinales.Visibility = Visibility.Visible;
        panelFinal.Visibility = Visibility.Visible;
    }
}
