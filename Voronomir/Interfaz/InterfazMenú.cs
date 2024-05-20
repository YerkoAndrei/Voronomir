using System;
using Stride.Engine;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Panels;

namespace Voronomir;
using static Utilidades;
using static Constantes;

public class InterfazMenú : StartupScript
{
    private UniformGrid menú;
    private Grid episodios;

    private Grid opciones;
    private Grid animOpciones;

    private TextBlock txtDescripciónDificultad;
    private Grid btnFácil;
    private Grid btnNormal;
    private Grid btnDifícil;

    private bool animando;

    public override void Start()
    {
        var página = Entity.Get<UIComponent>().Page.RootElement;

        menú = página.FindVisualChildOfType<UniformGrid>("Menú");
        episodios = página.FindVisualChildOfType<Grid>("Episodios");

        opciones = página.FindVisualChildOfType<Grid>("Opciones");
        animOpciones = página.FindVisualChildOfType<Grid>("animOpciones");
        opciones.Visibility = Visibility.Hidden;

        txtDescripciónDificultad = página.FindVisualChildOfType<TextBlock>("txtDescripciónDificultad");
        btnFácil = página.FindVisualChildOfType<Grid>("btnFácil");
        btnNormal = página.FindVisualChildOfType<Grid>("btnNormal");
        btnDifícil = página.FindVisualChildOfType<Grid>("btnDifícil");

        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnJugar"), EnClicJugar);
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnOpciones"), EnClicOpciones);
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnSalir"), EnClicSalir);

        ConfigurarBotón(btnFácil, () => EnClicDificultad(Dificultades.fácil));
        ConfigurarBotón(btnNormal, () => EnClicDificultad(Dificultades.normal));
        ConfigurarBotón(btnDifícil, () => EnClicDificultad(Dificultades.difícil));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnVolver"), EnClicVolver);

        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnDemo"), EnClicDemo);
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnEpisodio1"), null);

        BloquearBotón(página.FindVisualChildOfType<Grid>("btnEpisodio1"), true);

        EnClicDificultad(SistemaMemoria.Dificultad);

        menú.Visibility = Visibility.Visible;
        episodios.Visibility = Visibility.Hidden;

        SistemaEscenas.BloquearCursor(false);
        Game.UpdateTime.Factor = 1;
    }

    private void EnClicDemo()
    {
        SistemaEscenas.CambiarEscena(Escenas.demo);
    }

    private void EnClicJugar()
    {
        // PENDIENTE: animar
        menú.Visibility = Visibility.Hidden;
        episodios.Visibility = Visibility.Visible;
    }

    private void EnClicVolver()
    {
        menú.Visibility = Visibility.Visible;
        episodios.Visibility = Visibility.Hidden;
    }

    private void EnClicDificultad(Dificultades dificultad)
    {
        txtDescripciónDificultad.Text = SistemaTraducción.ObtenerTraducción("descripción" + Capitalizar(dificultad.ToString()));
        SistemaMemoria.Dificultad = dificultad;

        BloquearBotón(btnFácil, false);
        BloquearBotón(btnNormal, false);
        BloquearBotón(btnDifícil, false);

        switch (dificultad)
        {
            case Dificultades.fácil:
                BloquearBotón(btnFácil, true);
                break;
            case Dificultades.normal:
                BloquearBotón(btnNormal, true);
                break;
            case Dificultades.difícil:
                BloquearBotón(btnDifícil, true);
                break;
        }
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

    private void EnClicSalir()
    {
        Environment.Exit(0);
    }
}
