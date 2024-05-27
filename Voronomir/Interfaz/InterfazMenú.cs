using System;
using System.Collections.Generic;
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

    private Dictionary<Escenas, TextBlock> txtTiempos;
    private Dictionary<Escenas, Grid> btnEtapas;

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

        // Tiempos
        txtTiempos = new Dictionary<Escenas, TextBlock>
        {
            { Escenas.demo, página.FindVisualChildOfType<TextBlock>("txtTiempoDemo") },
            { Escenas.M1E1, página.FindVisualChildOfType<TextBlock>("txtTiempoM1E1") },
            { Escenas.M1E2, página.FindVisualChildOfType<TextBlock>("txtTiempoM1E2") },
            { Escenas.M1E3, página.FindVisualChildOfType<TextBlock>("txtTiempoM1E3") },
            { Escenas.M1E4, página.FindVisualChildOfType<TextBlock>("txtTiempoM1E4") }
        };

        // Mundos / Etapas
        btnEtapas = new Dictionary<Escenas, Grid>
        {
            { Escenas.demo, página.FindVisualChildOfType<Grid>("btnDemo") },
            { Escenas.M1E1, página.FindVisualChildOfType<Grid>("btnM1E1") },
            { Escenas.M1E2, página.FindVisualChildOfType<Grid>("btnM1E2") },
            { Escenas.M1E3, página.FindVisualChildOfType<Grid>("btnM1E3") },
            { Escenas.M1E4, página.FindVisualChildOfType<Grid>("btnM1E4") }
        };

        foreach (var botón in btnEtapas)
        {
            ConfigurarBotón(botón.Value, () => EnClicEscena(botón.Key));
        }

        // Predeterminado
        EnClicDificultad(SistemaMemoria.Dificultad);
        ActualizarTiempos();
        DesbloquearEtapas();

        menú.Visibility = Visibility.Visible;
        jugar.Visibility = Visibility.Hidden;

        SistemaEscenas.BloquearCursor(false);
        Game.UpdateTime.Factor = 1;

        //SistemaEscenas.CambiarEscena(Escenas.demo);
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
        ActualizarTiempos();
        DesbloquearEtapas();
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

    public void ActualizarTiempos()
    {
        foreach(var tiempo in txtTiempos)
        {
            tiempo.Value.Text = SistemaMemoria.ObtenerTiempo(tiempo.Key);
        }
    }

    public void DesbloquearEtapas()
    {
        // Desbloquear episodios según tiempo anterior
        foreach (var botón in btnEtapas)
        {
            //var etapaAnterior = (Escenas)((int)botón.Key - 1);
            //BloquearBotón(botón.Value, (SistemaMemoria.ObtenerTiempo(etapaAnterior) == string.Empty));
            BloquearBotón(botón.Value, true);
        }

        // Demo siempre disponible
        BloquearBotón(btnEtapas[Escenas.demo], false);
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
