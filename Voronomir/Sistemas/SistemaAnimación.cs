using System;
using System.Threading.Tasks;
using Stride.Engine;
using Stride.Animations;
using Stride.Core.Mathematics;
using Stride.UI.Panels;
using Stride.UI;

namespace Voronomir;
using static Constantes;

public class SistemaAnimación : AsyncScript
{
    public ComputeCurveSampler<float> curvaRápida;

    private static SistemaAnimación instancia;

    // Animación
    private static Grid elemento;
    private static bool animando;
    private static float duración;
    private static float tiempoDelta;
    private static float tiempo;
    private static TipoCurva tipoCurva;
    private static Thickness margenInicio;
    private static Thickness margenObjetivo;
    private static Action enFin;

    public override async Task Execute()
    {
        instancia = this;

        while (Game.IsRunning)
        {
            if (animando)
                Animar();

            await Script.NextFrame();
        }
    }

    public static float EvaluarSuave(float tiempo)
    {
        return MathUtil.SmoothStep(tiempo);
    }

    public static float EvaluarRápido(float tiempo)
    {
        if (tiempo > 1)
            tiempo = 1;

        instancia.curvaRápida.UpdateChanges();
        return instancia.curvaRápida.Evaluate(tiempo);
    }

    public static void AnimarElemento(Grid _elemento, float _duración, bool entrando, Direcciones dirección, TipoCurva _tipoCurva, Action _enFin)
    {
        // Crea dirección exterior
        var fuera = new Thickness();
        switch (dirección)
        {
            case Direcciones.arriba:
                fuera = new Thickness(0, 0, 0, 1500);
                break;
            case Direcciones.abajo:
                fuera = new Thickness(0, 1500, 0, 0);
                break;
            case Direcciones.izquierda:
                fuera = new Thickness(0, 0, 3000, 0);
                break;
            case Direcciones.derecha:
                fuera = new Thickness(3000, 0, 0, 0);
                break;
        }

        if (entrando)
        {
            margenInicio = fuera;
            margenObjetivo = new Thickness();
        }
        else
        {
            margenInicio = new Thickness();
            margenObjetivo = fuera;
        }

        // Predeterminados
        elemento = _elemento;
        duración = _duración;
        tipoCurva = _tipoCurva;
        enFin = _enFin;

        elemento.Margin = margenInicio;

        tiempoDelta = 0;
        tiempo = 0;
        animando = true;
    }

    private void Animar()
    {
        tiempoDelta += (float)Game.UpdateTime.Elapsed.TotalSeconds;
        switch (tipoCurva)
        {
            case TipoCurva.nada:
                tiempo = tiempoDelta / duración;
                break;
            case TipoCurva.suave:
                tiempo = EvaluarSuave(tiempoDelta / duración);
                break;
            case TipoCurva.rápida:
                tiempo = EvaluarRápido(tiempoDelta / duración);
                break;
        }

        elemento.Margin = new Thickness(MathUtil.Lerp(margenInicio.Left, margenObjetivo.Left, tiempo),
                                        MathUtil.Lerp(margenInicio.Top, margenObjetivo.Top, tiempo),
                                        MathUtil.Lerp(margenInicio.Right, margenObjetivo.Right, tiempo),
                                        MathUtil.Lerp(margenInicio.Bottom, margenObjetivo.Bottom, tiempo));
        // Fin
        if (tiempoDelta >= duración)
        {
            elemento.Margin = margenObjetivo;
            TerminarLerp();
        }
    }

    private void TerminarLerp()
    {
        animando = false;
        tiempoDelta = 0;
        tiempo = 0;

        // Llamado
        if (enFin != null)
        {
            enFin.Invoke();
            enFin = null;
        }
    }
}
