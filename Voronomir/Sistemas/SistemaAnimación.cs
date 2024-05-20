using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stride.Core.Mathematics;
using Stride.Animations;
using Stride.Engine;
using Stride.UI.Panels;
using Stride.UI;

namespace Voronomir;
using static Constantes;

public class SistemaAnimación : AsyncScript
{
    public ComputeCurveSampler<float> curvaRápida;

    private static SistemaAnimación instancia;
    private static List<AnimaciónInterfaz> animaciones;

    public override async Task Execute()
    {
        instancia = this;
        animaciones = new List<AnimaciónInterfaz>();

        while (Game.IsRunning)
        {
            if (animaciones.Count > 0)
            {
                // ToArray() evita usar lista cambiante
                foreach (var animación in animaciones.ToArray())
                {
                    Animar(animación, (float)Game.UpdateTime.Elapsed.TotalSeconds);
                }
            }
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
                fuera = new Thickness(0, 0, 0, 1620);
                break;
            case Direcciones.abajo:
                fuera = new Thickness(0, 1620, 0, 0);
                break;
            case Direcciones.izquierda:
                fuera = new Thickness(0, 0, 2880, 0);
                break;
            case Direcciones.derecha:
                fuera = new Thickness(2880, 0, 0, 0);
                break;
        }

        var margenInicio = new Thickness();
        var margenObjetivo = new Thickness();

        if (entrando)
            margenInicio = fuera;
        else
            margenObjetivo = fuera;

        // Crea animación
        var nuevaAnimación = new AnimaciónInterfaz
        {
            Elemento = _elemento,
            Duración = _duración,
            TiempoDelta = 0,
            Tiempo = 0,
            TipoCurva = _tipoCurva,
            MargenInicio = margenInicio,
            MargenObjetivo = margenObjetivo,
            EnFin = _enFin,
        };

        nuevaAnimación.Elemento.Margin = nuevaAnimación.MargenInicio;
        animaciones.Add(nuevaAnimación);
    }

    private static void Animar(AnimaciónInterfaz animación, float tiempo)
    {
        animación.TiempoDelta += tiempo;
        switch (animación.TipoCurva)
        {
            case TipoCurva.nada:
                animación.Tiempo = animación.TiempoDelta / animación.Duración;
                break;
            case TipoCurva.suave:
                animación.Tiempo = EvaluarSuave(animación.TiempoDelta / animación.Duración);
                break;
            case TipoCurva.rápida:
                animación.Tiempo = EvaluarRápido(animación.TiempoDelta / animación.Duración);
                break;
        }

        animación.Elemento.Margin = new Thickness(MathUtil.Lerp(animación.MargenInicio.Left, animación.MargenObjetivo.Left, animación.Tiempo),
                                        MathUtil.Lerp(animación.MargenInicio.Top, animación.MargenObjetivo.Top, animación.Tiempo),
                                        MathUtil.Lerp(animación.MargenInicio.Right, animación.MargenObjetivo.Right, animación.Tiempo),
                                        MathUtil.Lerp(animación.MargenInicio.Bottom, animación.MargenObjetivo.Bottom, animación.Tiempo));
        // Fin
        if (animación.TiempoDelta >= animación.Duración)
        {
            // Llamado
            if (animación.EnFin != null)
            {
                animación.EnFin.Invoke();
                animación.EnFin = null;
            }

            animación.Elemento.Margin = animación.MargenObjetivo;
            animaciones.Remove(animación);
        }
    }
}

public class AnimaciónInterfaz()
{
     public Grid Elemento { get; set; }
     public float Duración { get; set; }
     public float TiempoDelta { get; set; }
     public float Tiempo { get; set; }
     public TipoCurva TipoCurva { get; set; }
     public Thickness MargenInicio { get; set; }
     public Thickness MargenObjetivo { get; set; }
     public Action EnFin { get; set; }
}
