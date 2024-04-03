using System.Threading.Tasks;
using System.Windows;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;
using static Constantes;

public class AnimadorArma : AsyncScript
{
    public TransformComponent ejeIzquierda;
    public TransformComponent ejeDerecha;

    public ModelComponent modeloIzquierda;
    public ModelComponent modeloDerecha;

    private Vector3 posiciónInicialIzquierda;
    private Vector3 posiciónInicialDerecha;

    // Animación correr
    private bool activa;
    private bool bajando;
    private float duraciónAnimación;
    private float fuerzaAnimación;
    private Quaternion centroIzquierda;
    private Quaternion centroDerecha;
    private Quaternion abajoIzquierda;
    private Quaternion abajoDerecha;

    private Quaternion inicialIzquierda;
    private Quaternion inicialDerecha;
    private Quaternion objetivoIzquierda;
    private Quaternion objetivoDerecha;

    public void Iniciar()
    {
        posiciónInicialIzquierda = ejeIzquierda.Position;
        posiciónInicialDerecha = ejeDerecha.Position;

        bajando = true;
        duraciónAnimación = 2;
        fuerzaAnimación = 0.5f;

        centroIzquierda = modeloIzquierda.Entity.Transform.Rotation;
        centroDerecha = modeloDerecha.Entity.Transform.Rotation;
        abajoIzquierda = centroIzquierda * Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(1f), MathUtil.DegreesToRadians(-2f), MathUtil.DegreesToRadians(2));
        abajoDerecha = centroDerecha * Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(-1f), MathUtil.DegreesToRadians(-2f), MathUtil.DegreesToRadians(-2));

        inicialIzquierda = centroIzquierda;
        inicialDerecha = centroDerecha;
        objetivoIzquierda = abajoIzquierda;
        objetivoDerecha = abajoDerecha;
    }

    public override async Task Execute()
    {
        float tiempoLerp = 0;
        float tiempo = 0;

        // Armas siempre están en animación
        while (Game.IsRunning)
        {
            if (activa)
            {
                tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duraciónAnimación);

                modeloIzquierda.Entity.Transform.Rotation = Quaternion.Lerp(inicialIzquierda * fuerzaAnimación, objetivoIzquierda * fuerzaAnimación, tiempo);
                modeloDerecha.Entity.Transform.Rotation = Quaternion.Lerp(inicialDerecha * fuerzaAnimación, objetivoDerecha * fuerzaAnimación, tiempo);

                tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;

                // Vuelta
                if(tiempoLerp >= duraciónAnimación)
                {
                    if(bajando)
                    {
                        inicialIzquierda = abajoIzquierda;
                        inicialDerecha = abajoDerecha;
                        objetivoIzquierda = centroIzquierda;
                        objetivoDerecha = centroDerecha;
                    }
                    else
                    {
                        inicialIzquierda = centroIzquierda;
                        inicialDerecha = centroDerecha;
                        objetivoIzquierda = abajoIzquierda;
                        objetivoDerecha = abajoDerecha;
                    }

                    tiempoLerp = 0;
                    tiempo = 0;
                    bajando = !bajando;
                }

                // Debug
                DebugText.Print(duraciónAnimación.ToString(), new Int2(x: 20, y: 140));
                DebugText.Print(fuerzaAnimación.ToString(), new Int2(x: 20, y: 160));
            }
            await Script.NextFrame();
        }
    }

    public void AnimarCorrerArma(float fuerza, float velocidad, bool enSuelo)
    {
        // Reposo
        if (velocidad <= 1)
        {
            duraciónAnimación = 2;
            fuerzaAnimación = 0.5f;
            return;
        }

        duraciónAnimación = ((1 - velocidad) + 1);
        fuerzaAnimación = velocidad * fuerza;

        // En aire se mueve más lento y más 
        if (!enSuelo)
        {
            duraciónAnimación = (((1 - velocidad) + 1)) * 2;
            fuerzaAnimación = (duraciónAnimación * fuerza) * 4;
        }
    }

    public void ApagarArma()
    {
        modeloIzquierda.Entity.Get<ModelComponent>().Enabled = false;
        modeloDerecha.Entity.Get<ModelComponent>().Enabled = false;
    }

    public async Task AnimarEntradaArma()
    {
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
            ejeIzquierda.Rotation = Quaternion.Lerp(rotaciónEntraIzquierda, Quaternion.Identity, tiempo);
            ejeDerecha.Rotation = Quaternion.Lerp(rotaciónEntraDerecha, Quaternion.Identity, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Task.Delay(1);
        }

        ejeIzquierda.Rotation = Quaternion.Identity;
        ejeDerecha.Rotation = Quaternion.Identity;
        activa = true;
    }

    public async void AnimarSalidaArma()
    {
        activa = false;
        var rotaciónCentro = Quaternion.Identity;
        var rotaciónSale = Quaternion.RotationX(MathUtil.DegreesToRadians(-60));
        var pocisiónSalida = Vector3.UnitZ * -0.1f;

        float duración = 0.1f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            ejeIzquierda.Position = Vector3.Lerp(posiciónInicialIzquierda, (posiciónInicialIzquierda - pocisiónSalida), tiempo);
            ejeDerecha.Position = Vector3.Lerp(posiciónInicialDerecha, (posiciónInicialDerecha - pocisiónSalida), tiempo);

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
