using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Particles.Components;
using Stride.Engine;

namespace Bozobaralika;
using static Utilidades;
using static Constantes;

public class AnimadorArmas : AsyncScript
{
    public TransformComponent ejeIzquierda;
    public TransformComponent ejeDerecha;

    public ModelComponent modeloIzquierda;
    public ModelComponent modeloDerecha;

    public SpriteComponent fuegoIzquierda;
    public SpriteComponent fuegoDerecha;

    public ParticleSystemComponent partículasIzquierda;
    public ParticleSystemComponent partículasDerecha;

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

    private float tiempoLerp;
    private float tiempo;

    private Vector3 tamañoFuegoIzquierda;
    private Vector3 tamañoFuegoDerecha;

    private ControladorArmas controlador;

    public void Iniciar(ControladorArmas _controlador)
    {
        controlador = _controlador;

        // Entrada / salida
        posiciónInicialIzquierda = ejeIzquierda.Position;
        posiciónInicialDerecha = ejeDerecha.Position;

        // Animación movimiento
        bajando = true;
        activa = false;

        centroIzquierda = modeloIzquierda.Entity.Transform.Rotation;
        centroDerecha = modeloDerecha.Entity.Transform.Rotation;
        abajoIzquierda = centroIzquierda * RotaciónÁngulos(1, -2, 2);
        abajoDerecha = centroDerecha * RotaciónÁngulos(-1, -2, -2);
        ReiniciarAnimación();
        ReiniciarParametros();

        // Efectos
        if (fuegoIzquierda != null && fuegoDerecha != null)
        {
            tamañoFuegoIzquierda = fuegoIzquierda.Entity.Transform.Scale;
            tamañoFuegoDerecha = fuegoDerecha.Entity.Transform.Scale;
            fuegoIzquierda.Enabled = false;
            fuegoDerecha.Enabled = false;
        }

        if(partículasIzquierda != null && partículasDerecha != null)
        {
            partículasIzquierda.Enabled = false;
            partículasDerecha.Enabled = false;
        }
    }

    public override async Task Execute()
    {
        // Armas siempre están en animación
        while (Game.IsRunning)
        {
            if (activa)
            {
                tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duraciónAnimación);

                modeloIzquierda.Entity.Transform.Rotation = Quaternion.Lerp(inicialIzquierda * fuerzaAnimación, objetivoIzquierda * fuerzaAnimación, tiempo);
                modeloDerecha.Entity.Transform.Rotation = Quaternion.Lerp(inicialDerecha * fuerzaAnimación, objetivoDerecha * fuerzaAnimación, tiempo);

                tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;

                // Vuelta
                if (tiempoLerp >= duraciónAnimación)
                    CambiarAnimación();
            }
            await Script.NextFrame();
        }
    }

    private void CambiarAnimación()
    {
        if (bajando)
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

        bajando = !bajando;
        ReiniciarParametros();
    }

    private void ReiniciarAnimación()
    {
        modeloIzquierda.Entity.Transform.Rotation = centroIzquierda;
        modeloDerecha.Entity.Transform.Rotation = centroDerecha;

        inicialIzquierda = centroIzquierda;
        inicialDerecha = centroDerecha;
        objetivoIzquierda = abajoIzquierda;
        objetivoDerecha = abajoDerecha;

        bajando = true;
    }

    private void ReiniciarParametros()
    {
        tiempoLerp = 0;
        tiempo = 0;

        var parametros = controlador.ObtenerParametrosAnimación();
        duraciónAnimación = parametros[0];
        fuerzaAnimación = parametros[1];
    }

    public void ActivarArma(bool activo)
    {
        modeloIzquierda.Entity.Get<ModelComponent>().Enabled = activo;
        modeloDerecha.Entity.Get<ModelComponent>().Enabled = activo;

        // Inicial
        if (activo)
            activa = true;
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

        ReiniciarAnimación();
        ReiniciarParametros();
        activa = true;
    }

    public async void AnimarSalidaArma()
    {
        activa = false;
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

            ejeIzquierda.Rotation = Quaternion.Lerp(Quaternion.Identity, rotaciónSale, tiempo);
            ejeDerecha.Rotation = Quaternion.Lerp(Quaternion.Identity, rotaciónSale, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Task.Delay(1);
        }

        modeloIzquierda.Entity.Get<ModelComponent>().Enabled = false;
        modeloDerecha.Entity.Get<ModelComponent>().Enabled = false;

        ejeIzquierda.Position = posiciónInicialIzquierda;
        ejeDerecha.Position = posiciónInicialDerecha;

        ReiniciarAnimación();
    }

    // Rango
    public async void AnimarDisparo(float retroceso, float duración, TipoDisparo tipoDisparo)
    {
        float tiempoLerpIzquierda = 0;
        float tiempoLerpDerecha = 0;
        float tiempoIzquierda = 0;
        float tiempoDerecha = 0;

        if (fuegoIzquierda != null && fuegoDerecha != null)
            MostrarFuego(tipoDisparo);

        if (partículasIzquierda != null && partículasDerecha != null)
            MostrarPartículas(tipoDisparo);

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
                    tiempoLerpIzquierda += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
                    await Task.Delay(1);
                }
                break;
            case TipoDisparo.derecha:
                while (tiempoLerpDerecha < duración)
                {
                    tiempoDerecha = SistemaAnimación.EvaluarSuave(tiempoLerpDerecha / duración);

                    ejeDerecha.Position = Vector3.Lerp(posiciónDisparoDerecha, posiciónInicialDerecha, tiempoDerecha);
                    tiempoLerpDerecha += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
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

    private async void MostrarFuego(TipoDisparo tipoDisparo)
    {
        switch (tipoDisparo)
        {
            case TipoDisparo.espejo:
                fuegoIzquierda.Enabled = true;
                fuegoDerecha.Enabled = true;

                // Aleatorización
                var colorFuego = ColorFuegoAleatorio();
                fuegoIzquierda.Color = colorFuego;
                fuegoDerecha.Color = colorFuego;

                fuegoIzquierda.Entity.Transform.Scale = tamañoFuegoIzquierda;
                fuegoDerecha.Entity.Transform.Scale = tamañoFuegoDerecha;
                fuegoIzquierda.Entity.Transform.Rotation = Quaternion.RotationZ(MathUtil.DegreesToRadians(RangoAleatorio(0, 360)));
                fuegoDerecha.Entity.Transform.Rotation = Quaternion.RotationZ(MathUtil.DegreesToRadians(RangoAleatorio(0, 360)));

                partículasIzquierda.Enabled = true;
                partículasDerecha.Enabled = true;
                partículasIzquierda.ParticleSystem.ResetSimulation();
                partículasDerecha.ParticleSystem.ResetSimulation();
                await Task.Delay(100);
                break;
            case TipoDisparo.izquierda:
                fuegoIzquierda.Enabled = true;

                // Aleatorización
                var colorFuegoIzquierdo = ColorFuegoAleatorio();
                fuegoIzquierda.Color = colorFuegoIzquierdo;

                fuegoIzquierda.Entity.Transform.Scale = tamañoFuegoIzquierda * RangoAleatorio(1f, 1.2f);
                fuegoIzquierda.Entity.Transform.Rotation = Quaternion.RotationZ(MathUtil.DegreesToRadians(RangoAleatorio(0, 360)));

                partículasIzquierda.Enabled = true;
                partículasIzquierda.ParticleSystem.ResetSimulation();
                await Task.Delay(70);
                break;
            case TipoDisparo.derecha:
                fuegoDerecha.Enabled = true;

                // Aleatorización
                var colorFuegoDerecho = ColorFuegoAleatorio();
                fuegoDerecha.Color = colorFuegoDerecho;

                fuegoDerecha.Entity.Transform.Scale = tamañoFuegoDerecha * RangoAleatorio(1f, 1.2f);
                fuegoDerecha.Entity.Transform.Rotation = Quaternion.RotationZ(MathUtil.DegreesToRadians(RangoAleatorio(0, 360)));

                partículasDerecha.Enabled = true;
                partículasDerecha.ParticleSystem.ResetSimulation();
                await Task.Delay(70);
                break;
        }

        switch (tipoDisparo)
        {
            case TipoDisparo.espejo:
                fuegoIzquierda.Enabled = false;
                fuegoDerecha.Enabled = false;
                break;
            case TipoDisparo.izquierda:
                fuegoIzquierda.Enabled = false;
                break;
            case TipoDisparo.derecha:
                fuegoDerecha.Enabled = false;
                break;
        }
    }

    private void MostrarPartículas(TipoDisparo tipoDisparo)
    {
        switch (tipoDisparo)
        {
            case TipoDisparo.espejo:
                partículasIzquierda.Enabled = true;
                partículasDerecha.Enabled = true;

                partículasIzquierda.ParticleSystem.ResetSimulation();
                partículasDerecha.ParticleSystem.ResetSimulation();
                break;
            case TipoDisparo.izquierda:
                partículasIzquierda.Enabled = true;
                partículasIzquierda.ParticleSystem.ResetSimulation();
                break;
            case TipoDisparo.derecha:
                partículasDerecha.Enabled = true;
                partículasDerecha.ParticleSystem.ResetSimulation();
                break;
        }
    }

    private Color ColorFuegoAleatorio()
    {
        return new Color((byte)RangoAleatorio(200, 255), 200, 0, 240);
    }

    private Quaternion RotaciónÁngulos(float x, float y, float z)
    {
        return Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(x), MathUtil.DegreesToRadians(y), MathUtil.DegreesToRadians(z));
    }

    // Melé
    public async void AnimarAtaque(TipoDisparo tipoDisparo)
    {
        var rotaciónAtaqueIzquierda = RotaciónÁngulos(-40, -90, 60);
        var rotaciónAtaqueDerecha = RotaciónÁngulos(40, -90, -60);

        partículasIzquierda.ParticleSystem.ResetSimulation();
        partículasDerecha.ParticleSystem.ResetSimulation();
        partículasIzquierda.Enabled = true;
        partículasDerecha.Enabled = true;

        switch (tipoDisparo)
        {
            case TipoDisparo.izquierda:
                AnimarEspada(ejeIzquierda, rotaciónAtaqueIzquierda, 0.09f);
                await Task.Delay(60);
                AnimarEspada(ejeDerecha, rotaciónAtaqueDerecha, 0.1f);
                break;
            case TipoDisparo.derecha:
                AnimarEspada(ejeDerecha, rotaciónAtaqueDerecha, 0.09f);
                await Task.Delay(60);
                AnimarEspada(ejeIzquierda, rotaciónAtaqueIzquierda, 0.1f);
                break;
        }

        await Task.Delay(140);
        partículasIzquierda.Enabled = false;
        partículasDerecha.Enabled = false;
    }

    private async void AnimarEspada(TransformComponent espada, Quaternion rotaciónAtaque, float duración)
    {
        espada.Rotation = rotaciónAtaque;

        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            espada.Rotation = Quaternion.Lerp(rotaciónAtaque, Quaternion.Identity, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }
        espada.Rotation = Quaternion.Identity;
    }
}
