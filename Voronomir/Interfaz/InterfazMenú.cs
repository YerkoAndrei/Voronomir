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
    private Grid menú;
    private Grid jugar;

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

        menú = página.FindVisualChildOfType<Grid>("Menú");
        jugar = página.FindVisualChildOfType<Grid>("Jugar");

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
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnM1E1"), () => EnClicEscena(Escenas.M1E1));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnM1E2"), () => EnClicEscena(Escenas.M1E2));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnM1E3"), () => EnClicEscena(Escenas.M1E3));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnM1E4"), () => EnClicEscena(Escenas.M1E4));

        // Tiempos máximos
        página.FindVisualChildOfType<TextBlock>("txtTiempoDemo").Text = SistemaMemoria.ObtenerMáximo(Escenas.demo);
        página.FindVisualChildOfType<TextBlock>("txtTiempoM1E1").Text = SistemaMemoria.ObtenerMáximo(Escenas.M1E1);
        página.FindVisualChildOfType<TextBlock>("txtTiempoM1E2").Text = SistemaMemoria.ObtenerMáximo(Escenas.M1E2);
        página.FindVisualChildOfType<TextBlock>("txtTiempoM1E3").Text = SistemaMemoria.ObtenerMáximo(Escenas.M1E3);
        página.FindVisualChildOfType<TextBlock>("txtTiempoM1E4").Text = SistemaMemoria.ObtenerMáximo(Escenas.M1E4);

        // PENDIENTE: desbloquear episodios
        BloquearBotón(página.FindVisualChildOfType<Grid>("btnM1E1"), true);
        BloquearBotón(página.FindVisualChildOfType<Grid>("btnM1E2"), true);
        BloquearBotón(página.FindVisualChildOfType<Grid>("btnM1E3"), true);
        BloquearBotón(página.FindVisualChildOfType<Grid>("btnM1E4"), true);

        // Predeterminado
        EnClicDificultad(SistemaMemoria.Dificultad);

        menú.Visibility = Visibility.Visible;
        jugar.Visibility = Visibility.Hidden;

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
        menú.Visibility = Visibility.Visible;
        jugar.Visibility = Visibility.Visible;

        SistemaAnimación.AnimarElemento(menú, 0.2f, false, Direcciones.derecha, TipoCurva.rápida, null);
        SistemaAnimación.AnimarElemento(jugar, 0.2f, true, Direcciones.izquierda, TipoCurva.rápida, () =>
        {
            animando = false;
            menú.Visibility = Visibility.Hidden;
        });
    }

    private void EnClicVolver()
    {
        if (animando)
            return;

        animando = true;
        menú.Visibility = Visibility.Visible;
        jugar.Visibility = Visibility.Visible;

        SistemaAnimación.AnimarElemento(jugar, 0.2f, false, Direcciones.izquierda, TipoCurva.rápida, null);
        SistemaAnimación.AnimarElemento(menú, 0.2f, true, Direcciones.derecha, TipoCurva.rápida, () =>
        {
            animando = false;
            jugar.Visibility = Visibility.Hidden;
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
