using System;
using Stride.Engine;
using Stride.UI;
using Stride.UI.Panels;

namespace Voronomir;
using static Utilidades;
using static Constantes;

public class InterfazMenú : StartupScript
{
    private Grid Opciones;
    private Grid animOpciones;

    private bool animando;

    public override void Start()
    {
        var página = Entity.Get<UIComponent>().Page.RootElement;

        Opciones = página.FindVisualChildOfType<Grid>("Opciones");
        animOpciones = página.FindVisualChildOfType<Grid>("animOpciones");
        Opciones.Visibility = Visibility.Hidden;

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
        if (animando)
            return;

        animando = true;
        Opciones.Visibility = Visibility.Visible;
        SistemaAnimación.AnimarElemento(animOpciones, 0.2f, true, Direcciones.arriba, TipoCurva.rápida, () =>
        {
            animando = false;
        });
    }

    private void EnClicSalir()
    {
        Environment.Exit(0);
    }
}
