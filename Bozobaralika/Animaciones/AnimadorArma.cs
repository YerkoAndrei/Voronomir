using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;
using static Constantes;

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

        ejeIzquierda.Rotation = rotaciónCentro;
        ejeDerecha.Rotation = rotaciónCentro;
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
    public async void AnimarDisparo(float retroceso, float duración, TipoDisparo tipoDisparo)
    {
        float tiempoLerpIzquierda = 0;
        float tiempoLerpDerecha = 0;
        float tiempoIzquierda = 0;
        float tiempoDerecha = 0;

        // Posición rápida
        var posiciónDisparoIzquierda = posiciónInicialIzquierda + new Vector3(0, 0, retroceso);
        var posiciónDisparoDerecha = posiciónInicialDerecha + new Vector3(0, 0, retroceso);

        switch(tipoDisparo)
        {
            case TipoDisparo.espejo:
                ejeIzquierda.Position = posiciónDisparoIzquierda;
                ejeDerecha.Position = posiciónDisparoDerecha;
                break;
            case TipoDisparo.izquierda:
                ejeIzquierda.Position = posiciónDisparoIzquierda;
                break;
            case TipoDisparo.derecha:
                ejeDerecha.Position = posiciónDisparoDerecha;
                break;
        }

        // Evita repeticón de animación
        switch (tipoDisparo)
        {
            case TipoDisparo.espejo:
            case TipoDisparo.izquierda:
                while (tiempoLerpIzquierda < duración)
                {
                    tiempoIzquierda = SistemaAnimación.EvaluarSuave(tiempoLerpIzquierda / duración);

                    switch (tipoDisparo)
                    {
                        case TipoDisparo.espejo:
                            ejeIzquierda.Position = Vector3.Lerp(posiciónDisparoIzquierda, posiciónInicialIzquierda, tiempoIzquierda);
                            ejeDerecha.Position = Vector3.Lerp(posiciónDisparoDerecha, posiciónInicialDerecha, tiempoIzquierda);
                            break;
                        case TipoDisparo.izquierda:
                            ejeIzquierda.Position = Vector3.Lerp(posiciónDisparoIzquierda, posiciónInicialIzquierda, tiempoIzquierda);
                            break;
                    }
                    tiempoLerpIzquierda += (float)Game.UpdateTime.Elapsed.TotalSeconds;
                    await Task.Delay(1);
                }
                break;
            case TipoDisparo.derecha:
                while (tiempoLerpDerecha < duración)
                {
                    tiempoDerecha = SistemaAnimación.EvaluarSuave(tiempoLerpDerecha / duración);

                    ejeDerecha.Position = Vector3.Lerp(posiciónDisparoDerecha, posiciónInicialDerecha, tiempoDerecha);
                    tiempoLerpDerecha += (float)Game.UpdateTime.Elapsed.TotalSeconds;
                    await Task.Delay(1);
                }
                break;
        }
        
        switch (tipoDisparo)
        {
            case TipoDisparo.espejo:
                ejeIzquierda.Position = posiciónInicialIzquierda;
                ejeDerecha.Position = posiciónInicialDerecha;
                break;
            case TipoDisparo.izquierda:
                ejeIzquierda.Position = posiciónInicialIzquierda;
                break;
            case TipoDisparo.derecha:
                ejeDerecha.Position = posiciónInicialDerecha;
                break;
        }
    }

    // Melé
    public async void AnimarAtaque(TipoDisparo tipoDisparo)
    {
        float duración = 0.1f;
        float tiempoLerp = 0;
        float tiempo = 0;

        // Rotación rápida
        var rotaciónAtaqueIzquierda = Quaternion.RotationX(MathUtil.DegreesToRadians(-40));
        var rotaciónAtaqueDerecha = Quaternion.RotationX(MathUtil.DegreesToRadians(-40));

        switch (tipoDisparo)
        {
            case TipoDisparo.izquierda:
                ejeIzquierda.Rotation = rotaciónAtaqueIzquierda;
                break;
            case TipoDisparo.derecha:
                ejeDerecha.Rotation = rotaciónAtaqueDerecha;
                break;
        }

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            switch (tipoDisparo)
            {
                case TipoDisparo.izquierda:
                    ejeIzquierda.Rotation = Quaternion.Lerp(rotaciónAtaqueIzquierda, Quaternion.Identity, tiempo);
                    break;
                case TipoDisparo.derecha:
                    ejeDerecha.Rotation = Quaternion.Lerp(rotaciónAtaqueDerecha, Quaternion.Identity, tiempo);
                    break;
            }

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Task.Delay(1);
        }

        switch (tipoDisparo)
        {
            case TipoDisparo.izquierda:
                ejeIzquierda.Rotation = Quaternion.Identity;
                break;
            case TipoDisparo.derecha:
                ejeDerecha.Rotation = Quaternion.Identity;
                break;
        }
    }
}
