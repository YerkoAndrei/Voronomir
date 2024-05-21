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
    private Grid título;
    private Grid menú;
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

        título = página.FindVisualChildOfType<Grid>("Título");
        menú = página.FindVisualChildOfType<Grid>("Menú");
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
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnVolver"), EnClicVolver);

        ConfigurarBotón(btnFácil, () => EnClicDificultad(Dificultades.fácil));
        ConfigurarBotón(btnNormal, () => EnClicDificultad(Dificultades.normal));
        ConfigurarBotón(btnDifícil, () => EnClicDificultad(Dificultades.difícil));

        // Episodios
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnDemo"), () => EnClicEscena(Escenas.demo));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnE1M1"), () => EnClicEscena(Escenas.E1M1));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnE1M2"), () => EnClicEscena(Escenas.E1M2));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnE1M3"), () => EnClicEscena(Escenas.E1M3));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnE1M4"), () => EnClicEscena(Escenas.E1M4));

        // Tiempos máximos
        página.FindVisualChildOfType<TextBlock>("txtTiempoDemo").Text = SistemaMemoria.ObtenerMáximo(Escenas.demo);
        página.FindVisualChildOfType<TextBlock>("txtTiempoE1M1").Text = SistemaMemoria.ObtenerMáximo(Escenas.E1M1);
        página.FindVisualChildOfType<TextBlock>("txtTiempoE1M2").Text = SistemaMemoria.ObtenerMáximo(Escenas.E1M2);
        página.FindVisualChildOfType<TextBlock>("txtTiempoE1M3").Text = SistemaMemoria.ObtenerMáximo(Escenas.E1M3);
        página.FindVisualChildOfType<TextBlock>("txtTiempoE1M4").Text = SistemaMemoria.ObtenerMáximo(Escenas.E1M4);

        // PENDIENTE: desbloquear episodios
        BloquearBotón(página.FindVisualChildOfType<Grid>("btnE1M1"), true);
        BloquearBotón(página.FindVisualChildOfType<Grid>("btnE1M2"), true);
        BloquearBotón(página.FindVisualChildOfType<Grid>("btnE1M3"), true);
        BloquearBotón(página.FindVisualChildOfType<Grid>("btnE1M4"), true);

        // Predeterminado
        EnClicDificultad(SistemaMemoria.Dificultad);

        título.Visibility = Visibility.Visible;
        menú.Visibility = Visibility.Visible;
        episodios.Visibility = Visibility.Hidden;

        SistemaEscenas.BloquearCursor(false);
        Game.UpdateTime.Factor = 1;
    }

    private void EnClicEscena(Escenas escena)
    {
        animando = true;
        SistemaEscenas.CambiarEscena(escena);
    }

    private void EnClicJugar()
    {
        if (animando)
            return;

        animando = true;
        título.Visibility = Visibility.Visible;
        menú.Visibility = Visibility.Visible;
        episodios.Visibility = Visibility.Visible;

        SistemaAnimación.AnimarElemento(título, 0.4f, false, Direcciones.arriba, TipoCurva.rápida, null);
        SistemaAnimación.AnimarElemento(menú, 0.2f, false, Direcciones.derecha, TipoCurva.rápida, null);
        SistemaAnimación.AnimarElemento(episodios, 0.2f, true, Direcciones.izquierda, TipoCurva.rápida, () =>
        {
            animando = false;
            título.Visibility = Visibility.Hidden;
            menú.Visibility = Visibility.Hidden;
        });
    }

    private void EnClicVolver()
    {
        if (animando)
            return;

        animando = true;
        título.Visibility = Visibility.Visible;
        menú.Visibility = Visibility.Visible;
        episodios.Visibility = Visibility.Visible;

        SistemaAnimación.AnimarElemento(título, 0.4f, true, Direcciones.arriba, TipoCurva.rápida, null);
        SistemaAnimación.AnimarElemento(episodios, 0.2f, false, Direcciones.izquierda, TipoCurva.rápida, null);
        SistemaAnimación.AnimarElemento(menú, 0.2f, true, Direcciones.derecha, TipoCurva.rápida, () =>
        {
            animando = false;
            episodios.Visibility = Visibility.Hidden;
        });
    }

    private void EnClicDificultad(Dificultades dificultad)
    {
        SistemaMemoria.GuardarConfiguración(Configuraciones.dificultad, dificultad.ToString());
        SistemaMemoria.Dificultad = dificultad;
        Traducir();

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

    public void Traducir()
    {
        txtDescripciónDificultad.Text = SistemaTraducción.ObtenerTraducción("descripción" + Capitalizar(SistemaMemoria.Dificultad.ToString()));
    }

    private void EnClicOpciones()
    {
        if (animando)
            return;

        animando = true;
        opciones.Visibility = Visibility.Visible;
        SistemaAnimación.AnimarElemento(animOpciones, 0.2f, true, Direcciones.arriba, TipoCurva.rápida, () => animando = false);
    }

    private void EnClicSalir()
    {
        Environment.Exit(0);
    }
}
