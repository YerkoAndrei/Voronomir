﻿using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;

public class AnimadorArma : StartupScript
{
    public TransformComponent ejeIzquierda;
    public TransformComponent ejeDerecha;

    public ModelComponent modeloIzquierda;
    public ModelComponent modeloDerecha;

    private Vector3 posiciónInicialIzquierda;
    private Vector3 posiciónInicialDerecha;

    public void Iniciar()
    {
        posiciónInicialIzquierda = ejeIzquierda.Position;
        posiciónInicialDerecha = ejeDerecha.Position;
    }

    public void ApagarArma()
    {
        modeloIzquierda.Entity.Get<ModelComponent>().Enabled = false;
        modeloDerecha.Entity.Get<ModelComponent>().Enabled = false;
    }

    public async Task AnimarEntradaArma()
    {
        var rotaciónCentro = Quaternion.Identity;
        var rotaciónEntraIzquierda = Quaternion.RotationY(MathUtil.DegreesToRadians(90));
        var rotaciónEntraDerecha = Quaternion.RotationY(MathUtil.DegreesToRadians(-90));

        float duración = 0.1f;
        float tiempoLerp = 0;
        float tiempo = 0;

        modeloIzquierda.Entity.Get<ModelComponent>().Enabled = true;
        modeloDerecha.Entity.Get<ModelComponent>().Enabled = true;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            ejeIzquierda.Rotation = Quaternion.Lerp(rotaciónEntraIzquierda, rotaciónCentro, tiempo);
            ejeDerecha.Rotation = Quaternion.Lerp(rotaciónEntraDerecha, rotaciónCentro, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Task.Delay(1);
        }
    }

    public async void AnimarSalidaArma()
    {
        var rotaciónCentro = Quaternion.Identity;
        var rotaciónSale = Quaternion.RotationX(MathUtil.DegreesToRadians(-90));
        var inicioIzquierda = posiciónInicialIzquierda;
        var inicioDerecha = posiciónInicialDerecha;
        var pocisiónSalida = Vector3.UnitY * 0.5f;

        float duración = 0.1f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            ejeIzquierda.Position = Vector3.Lerp(inicioIzquierda, (inicioIzquierda - pocisiónSalida), tiempo);
            ejeDerecha.Position = Vector3.Lerp(inicioDerecha, (inicioDerecha - pocisiónSalida), tiempo);

            ejeIzquierda.Rotation = Quaternion.Lerp(rotaciónCentro, rotaciónSale, tiempo);
            ejeDerecha.Rotation = Quaternion.Lerp(rotaciónCentro, rotaciónSale, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Task.Delay(1);
        }

        modeloIzquierda.Entity.Get<ModelComponent>().Enabled = false;
        modeloDerecha.Entity.Get<ModelComponent>().Enabled = false;

        ejeIzquierda.Position = posiciónInicialIzquierda;
        ejeDerecha.Position = posiciónInicialDerecha;
    }

    // Rango
    public async void AnimarDisparo(float retroceso, float duración)
    {
        float tiempoLerp = 0;
        float tiempo = 0;

        // Posición rápida
        var posiciónDisparoIzquierda = posiciónInicialIzquierda + new Vector3(0, 0, retroceso);
        var posiciónDisparoDerecha = posiciónInicialDerecha + new Vector3(0, 0, retroceso);
        ejeIzquierda.Position = posiciónDisparoIzquierda;
        ejeDerecha.Position = posiciónDisparoDerecha;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            ejeIzquierda.Position = Vector3.Lerp(posiciónDisparoIzquierda, posiciónInicialIzquierda, tiempo);
            ejeDerecha.Position = Vector3.Lerp(posiciónDisparoDerecha, posiciónInicialDerecha, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Task.Delay(1);
        }

        ejeIzquierda.Position = posiciónInicialIzquierda;
        ejeDerecha.Position = posiciónInicialDerecha;
    }

    // Melé
    public async void AnimarAtaque()
    {
        float duración = 0.1f;
        float tiempoLerp = 0;
        float tiempo = 0;

        // Rotación rápida
        var rotaciónAtaqueIzquierda = Quaternion.RotationX(MathUtil.DegreesToRadians(-40));
        var rotaciónAtaqueDerecha = Quaternion.RotationX(MathUtil.DegreesToRadians(-40));
        ejeIzquierda.Rotation = rotaciónAtaqueIzquierda;
        ejeDerecha.Rotation = rotaciónAtaqueDerecha;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            ejeIzquierda.Rotation = Quaternion.Lerp(rotaciónAtaqueIzquierda, Quaternion.Identity, tiempo);
            ejeDerecha.Rotation = Quaternion.Lerp(rotaciónAtaqueDerecha, Quaternion.Identity, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Task.Delay(1);
        }

        ejeIzquierda.Rotation = Quaternion.Identity;
        ejeDerecha.Rotation = Quaternion.Identity;
    }
}
