using Stride.UI;
using Stride.Engine;
using Stride.UI.Controls;

namespace Bozobaralika;
using static Constantes;

public class InterfazJuego : SyncScript
{
    private ImageElement miraPistola;
    private ImageElement miraEscopeta;
    private ImageElement miraMetralleta;
    private ImageElement miraRifle;

    public override void Start()
    {
        var página = Entity.Get<UIComponent>().Page.RootElement;

        miraPistola = página.FindVisualChildOfType<ImageElement>("miraPistola");
        miraEscopeta = página.FindVisualChildOfType<ImageElement>("miraEscopeta");
        miraMetralleta = página.FindVisualChildOfType<ImageElement>("miraMetralleta");
        miraRifle = página.FindVisualChildOfType<ImageElement>("miraRifle");

        ApagarMiras();
    }

    public override void Update()
    {

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
}
