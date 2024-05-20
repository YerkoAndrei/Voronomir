using System;
using System.Linq;
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

    private Grid btnJugar;
    private TextBlock txtTiempo;
    private TextBlock txtEnemigos;
    private TextBlock txtSecretos;

    private Grid[] btnsMundo;

    private Escenas escenaElegida;
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

        txtTiempo = página.FindVisualChildOfType<TextBlock>("txtTiempo");
        txtEnemigos = página.FindVisualChildOfType<TextBlock>("txtEnemigos");
        txtSecretos = página.FindVisualChildOfType<TextBlock>("txtSecretos");

        btnJugar = página.FindVisualChildOfType<Grid>("btnEpisodioJugar");

        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnJugar"), EnClicJugar);
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnOpciones"), EnClicOpciones);
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnSalir"), EnClicSalir);

        ConfigurarBotón(btnFácil, () => EnClicDificultad(Dificultades.fácil));
        ConfigurarBotón(btnNormal, () => EnClicDificultad(Dificultades.normal));
        ConfigurarBotón(btnDifícil, () => EnClicDificultad(Dificultades.difícil));

        ConfigurarBotón(btnJugar, EnClicJugarEscena);
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnVolver"), EnClicVolver);

        // Episodios
        btnsMundo = página.FindVisualChildOfType<UniformGrid>("Mundos").FindVisualChildrenOfType<Grid>().Where(o => o.Name.Contains("btn")).ToArray();
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnDemo"), () => ElegirEscena(Escenas.demo));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnE1M1"), () => ElegirEscena(Escenas.E1M1));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnE1M2"), () => ElegirEscena(Escenas.E1M2));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnE1M3"), () => ElegirEscena(Escenas.E1M3));
        ConfigurarBotón(página.FindVisualChildOfType<Grid>("btnE1M4"), () => ElegirEscena(Escenas.E1M4));

        // Predeterminado
        ReiniciarEpisodios();
        EnClicDificultad(SistemaMemoria.Dificultad);
        BloquearBotón(btnJugar, true);

        título.Visibility = Visibility.Visible;
        menú.Visibility = Visibility.Visible;
        episodios.Visibility = Visibility.Hidden;

        SistemaEscenas.BloquearCursor(false);
        Game.UpdateTime.Factor = 1;
    }

    private void ElegirEscena(Escenas escena)
    {
        escenaElegida = escena;
        ReiniciarEpisodios();

        BloquearBotón(btnJugar, false);
        txtTiempo.Text = SistemaMemoria.ObtenerTiempo(escenaElegida);
        txtEnemigos.Text = SistemaMemoria.ObtenerEnemigos(escenaElegida);
        txtSecretos.Text = SistemaMemoria.ObtenerSecretos(escenaElegida);
    }

    private void EnClicJugarEscena()
    {
        animando = true;
        SistemaEscenas.CambiarEscena(escenaElegida);
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
            ReiniciarEpisodios();
        });
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
        SistemaAnimación.AnimarElemento(animOpciones, 0.2f, true, Direcciones.arriba, TipoCurva.rápida, () => animando = false);
    }

    private void ReiniciarEpisodios()
    {
        BloquearBotón(btnJugar, true);

        txtTiempo.Text = string.Empty;
        txtEnemigos.Text = string.Empty;
        txtSecretos.Text = string.Empty;

        foreach(var botón in btnsMundo)
        {
            BloquearBotón(botón, false);
        }

        // PENDIENTE: asignar episodios
        BloquearBotón(Entity.Get<UIComponent>().Page.RootElement.FindVisualChildOfType<Grid>("btnE1M1"), true);
        BloquearBotón(Entity.Get<UIComponent>().Page.RootElement.FindVisualChildOfType<Grid>("btnE1M2"), true);
        BloquearBotón(Entity.Get<UIComponent>().Page.RootElement.FindVisualChildOfType<Grid>("btnE1M3"), true);
        BloquearBotón(Entity.Get<UIComponent>().Page.RootElement.FindVisualChildOfType<Grid>("btnE1M4"), true);
    }

    private void EnClicSalir()
    {
        Environment.Exit(0);
    }
}
