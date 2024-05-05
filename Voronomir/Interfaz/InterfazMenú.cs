using Stride.Engine;
using Stride.UI;
using Stride.UI.Panels;
using System;

namespace Voronomir;
using static Utilidades;
using static Constantes;

public class InterfazMenú : StartupScript
{
    public override void Start()
    {
        var página = Entity.Get<UIComponent>().Page.RootElement;

        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnJugar"), EnClicJugar);
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnDemo"), EnClicDemo);
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnOpciones"), EnClicOpciones);
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnSalir"), EnClicSalir);

        BloquearBotón(página.FindVisualChildOfType<Grid>("btnJugar"), true);
    }

    private void EnClicJugar()
    {
        
    }

    private void EnClicDemo()
    {
        SistemaEscenas.CambiarEscena(Escenas.demo);
    }

    private void EnClicOpciones()
    {

    }

    private void EnClicSalir()
    {
        Environment.Exit(0);
    }
}
