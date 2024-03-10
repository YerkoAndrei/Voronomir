using Stride.Core.Mathematics;
using Stride.UI;
using Stride.Engine;
using Stride.UI.Controls;

namespace Bozobaralika;
using static Constantes;

public class InterfazJuego : SyncScript
{
    public Color vidaCompleta;
    public Color vidaVacía;
    public Color armaNormal;
    public Color armaSeleccionada;

    private ImageElement imgVida;
    private float tamañoVida;

    private ImageElement miraPistola;
    private ImageElement miraEscopeta;
    private ImageElement miraMetralleta;
    private ImageElement miraRifle;

    private ImageElement imgPistola;
    private ImageElement imgEscopeta;
    private ImageElement imgMetralleta;
    private ImageElement imgRifle;


    public override void Start()
    {
        var página = Entity.Get<UIComponent>().Page.RootElement;

        imgVida = página.FindVisualChildOfType<ImageElement>("imgVida");
        tamañoVida = imgVida.Width;

        miraPistola = página.FindVisualChildOfType<ImageElement>("miraPistola");
        miraEscopeta = página.FindVisualChildOfType<ImageElement>("miraEscopeta");
        miraMetralleta = página.FindVisualChildOfType<ImageElement>("miraMetralleta");
        miraRifle = página.FindVisualChildOfType<ImageElement>("miraRifle");

        imgPistola = página.FindVisualChildOfType<ImageElement>("imgPistola");
        imgEscopeta = página.FindVisualChildOfType<ImageElement>("imgEscopeta");
        imgMetralleta = página.FindVisualChildOfType<ImageElement>("imgMetralleta");
        imgRifle = página.FindVisualChildOfType<ImageElement>("imgRifle");

        ApagarMiras();
    }

    public override void Update()
    {

    }

    public void ActualizarVida(float porcentaje)
    {
        imgVida.Color = Color.Lerp(vidaVacía, vidaCompleta, porcentaje);
        imgVida.Width = tamañoVida * porcentaje;
    }

    public void CambiarÍcono(Armas arma)
    {
        ApagarÍconos();
        switch (arma)
        {
            case Armas.pistola:
                imgPistola.Color = armaSeleccionada;
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
        }
    }

    public void CambiarMira(Armas arma)
    {
        switch (arma)
        {
            case Armas.pistola:
                miraPistola.Visibility = Visibility.Visible;
                break;
            case Armas.escopeta:
                miraEscopeta.Visibility = Visibility.Visible;
                break;
            case Armas.metralleta:
                miraMetralleta.Visibility = Visibility.Visible;
                break;
            case Armas.rifle:
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
        miraPistola.Visibility = Visibility.Hidden;
        miraEscopeta.Visibility = Visibility.Hidden;
        miraMetralleta.Visibility = Visibility.Hidden;
        miraRifle.Visibility = Visibility.Hidden;
    }

    public void ApagarÍconos()
    {
        imgPistola.Color = armaNormal;
        imgEscopeta.Color = armaNormal;
        imgMetralleta.Color = armaNormal;
        imgRifle.Color = armaNormal;
    }

    public void Morir()
    {
        imgVida.Color = vidaVacía;
        imgVida.Width = tamañoVida;

        // Reiniciar
    }
}
