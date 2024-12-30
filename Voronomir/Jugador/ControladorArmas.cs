﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stride.Core.Mathematics;
using Stride.Particles.Components;
using Stride.Physics;
using Stride.Input;
using Stride.Engine;

namespace Voronomir;
using static Utilidades;
using static Constantes;

public class ControladorArmas : StartupScript
{
    public AnimadorArmas animadorEspada;
    public AnimadorArmas animadorEscopeta;
    public AnimadorArmas animadorMetralleta;
    public AnimadorArmas animadorRife;
    public AnimadorArmas animadorLanzagranadas;

    public LightComponent luzDisparo;
    public Color colorMetralletaCaliente;

    public ParticleSystemComponent partículasMetralletaIzquierda;
    public ParticleSystemComponent partículasMetralletaDerecha;

    private ControladorJugador controlador;
    private ControladorMovimiento movimiento;
    private CameraComponent cámara;
    private InterfazJuego interfaz;

    private Vector3[] rayosMelé;
    private CollisionFilterGroupFlags colisionesDisparo;
    private Armas armaActual;
    private Armas armaAnterior;
    private bool bloqueo;
    private float ruedaRatón;
    private float dañoMínimo;
    private float dañoMáximo;

    private float últimoDisparoEspada;
    private float últimoDisparoEscopeta;
    private float últimoDisparoMetralleta;
    private float últimoDisparoRifle;
    private float últimoDisparoLanzagranadas;

    private CancellationTokenSource tokenMira;
    private CancellationTokenSource tokenLuz;
    private bool cambiandoArma;
    private bool usandoMira;

    // Metralleta
    private TipoDisparo turnoMetralleta;
    private float primerDisparoMetralleta;
    private float tempoMetralleta;
    private float tiempoPrecisiónMetralleta;

    // Lanzagranadas
    private TipoDisparo turnoLanzagranadas;

    public void Iniciar(ControladorJugador _controlador, ControladorMovimiento _movimiento, CameraComponent _cámara, InterfazJuego _interfaz)
    {
        controlador = _controlador;
        movimiento = _movimiento;
        cámara = _cámara;
        interfaz = _interfaz;

        dañoMínimo = 1;
        dañoMáximo = 200;

        tiempoPrecisiónMetralleta = 8f;
        tempoMetralleta = tiempoPrecisiónMetralleta;

        // Filtros disparos
        colisionesDisparo = CollisionFilterGroupFlags.StaticFilter |
                            CollisionFilterGroupFlags.KinematicFilter |
                            CollisionFilterGroupFlags.SensorTrigger |
                            CollisionFilterGroupFlags.CustomFilter1 |
                            CollisionFilterGroupFlags.CustomFilter2 |
                            CollisionFilterGroupFlags.CustomFilter3;

        rayosMelé = new Vector3[3]
        {
            new Vector3 (0.3f, 0, 0),
            new Vector3 (0, 0, 0),
            new Vector3 (-0.3f, 0, 0)
        };

        partículasMetralletaIzquierda.Enabled = false;
        partículasMetralletaDerecha.Enabled = false;

        tokenMira = new CancellationTokenSource();
        tokenLuz = new CancellationTokenSource();
        luzDisparo.Enabled = false;

        // Iniciar
        animadorEspada.Iniciar(this);
        animadorEscopeta.Iniciar(this);
        animadorMetralleta.Iniciar(this);
        animadorRife.Iniciar(this);
        animadorLanzagranadas.Iniciar(this);

        // Arma por defecto
        armaActual = Armas.espada;
        armaAnterior = armaActual;
        ApagarArmas();

        interfaz.CambiarMira(armaActual);
        interfaz.CambiarÍcono(armaActual);

        animadorEspada.ActivarArma(true);
        movimiento.CambiarVelocidadMáxima(armaActual);
    }

    public void ActualizarEntradas()
    {
        if (bloqueo)
            return;

        // Disparo general
        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            Disparar();
            primerDisparoMetralleta = ControladorJuego.ObtenerTiempo();
        }

        // Metralleta
        if (Input.IsMouseButtonDown(MouseButton.Left) && armaActual == Armas.metralleta)
        {
            if (tempoMetralleta > 0)
                tempoMetralleta -= (float)Game.UpdateTime.WarpElapsed.TotalSeconds;

            // Permite disparo singular
            if ((primerDisparoMetralleta + 0.12f) < ControladorJuego.ObtenerTiempo())
                Disparar();
        }

        // Metralleta se enfria más rápido de lo que se calienta
        if (!Input.IsMouseButtonDown(MouseButton.Left) && tempoMetralleta < tiempoPrecisiónMetralleta)
        {
            tempoMetralleta += (float)Game.UpdateTime.WarpElapsed.TotalSeconds * 2;
        }

        // Efecto calentamiento metralleta
        if (armaActual == Armas.metralleta)
        {
            partículasMetralletaIzquierda.Color = Color.Lerp(colorMetralletaCaliente, Color.Transparent, (tempoMetralleta / tiempoPrecisiónMetralleta));
            partículasMetralletaDerecha.Color = Color.Lerp(colorMetralletaCaliente, Color.Transparent, (tempoMetralleta / tiempoPrecisiónMetralleta));
        }

        // Espadas
        if (Input.IsMouseButtonPressed(MouseButton.Right) && armaActual == Armas.espada)
            Disparar(false);

        // Rifle
        if (Input.IsMouseButtonPressed(MouseButton.Right) && armaActual == Armas.rifle && !usandoMira)
            AcercarMira(true);

        if (Input.IsMouseButtonReleased(MouseButton.Right) && armaActual == Armas.rifle && usandoMira)
            AcercarMira(false);

        // Cambio armas
        if (Input.IsKeyPressed(Keys.D1) || Input.IsKeyPressed(Keys.NumPad1))
            CambiarArma(Armas.espada);
        if (Input.IsKeyPressed(Keys.D2) || Input.IsKeyPressed(Keys.NumPad2))
            CambiarArma(Armas.escopeta);
        if (Input.IsKeyPressed(Keys.D3) || Input.IsKeyPressed(Keys.NumPad3))
            CambiarArma(Armas.metralleta);
        if (Input.IsKeyPressed(Keys.D4) || Input.IsKeyPressed(Keys.NumPad4))
            CambiarArma(Armas.rifle);
        if (Input.IsKeyPressed(Keys.D5) || Input.IsKeyPressed(Keys.NumPad5))
            CambiarArma(Armas.lanzagranadas);

        if (Input.IsKeyPressed(Keys.Q))
            CambiarArma(armaAnterior);

        // Cambio arma con rueda
        if (Input.MouseWheelDelta != 0)
        {
            ruedaRatón += Input.MouseWheelDelta;
            if (ruedaRatón >= 1)
                CambiarSiguenteArma(true);
            else if (ruedaRatón <= -1)
                CambiarSiguenteArma(false);

        }
    }

    private void Disparar(bool clicIzquierdo = true)
    {
        if (cambiandoArma)
            return;

        // Cadencia
        var tiempoDisparo = 0f;
        switch (armaActual)
        {
            case Armas.espada:
                tiempoDisparo = ObtenerCadencia(armaActual) + últimoDisparoEspada;
                break;
            case Armas.escopeta:
                tiempoDisparo = ObtenerCadencia(armaActual) + últimoDisparoEscopeta;
                break;
            case Armas.metralleta:
                tiempoDisparo = ObtenerCadencia(armaActual) + últimoDisparoMetralleta;
                break;
            case Armas.rifle:
                tiempoDisparo = ObtenerCadencia(armaActual) + últimoDisparoRifle;
                break;
            case Armas.lanzagranadas:
                tiempoDisparo = ObtenerCadencia(armaActual) + últimoDisparoLanzagranadas;
                break;
        }

        if (ControladorJuego.ObtenerTiempo() < tiempoDisparo)
            return;

        // Metralleta por turnos
        if (turnoMetralleta != TipoDisparo.izquierda)
            turnoMetralleta = TipoDisparo.izquierda;
        else
            turnoMetralleta = TipoDisparo.derecha;

        // Lanzagranadas por turnos
        if (turnoLanzagranadas != TipoDisparo.izquierda)
            turnoLanzagranadas = TipoDisparo.izquierda;
        else
            turnoLanzagranadas = TipoDisparo.derecha;

        SistemaSonidos.SonarDisparo(armaActual);
        switch (armaActual)
        {
            case Armas.espada:
                Atacar(clicIzquierdo);
                if (clicIzquierdo)
                    animadorEspada.AnimarAtaque(TipoDisparo.izquierda);
                else
                    animadorEspada.AnimarAtaque(TipoDisparo.derecha);
                últimoDisparoEspada = ControladorJuego.ObtenerTiempo();
                break;
            case Armas.escopeta:
                for (int i = 0; i < 20; i++)
                {
                    CalcularRayo(0.2f, 0.1f);
                }
                controlador.VibrarCámara(16, 10);
                ActivarLuz();
                animadorEscopeta.AnimarDisparo(0.5f, 0.25f, TipoDisparo.espejo);
                últimoDisparoEscopeta = ControladorJuego.ObtenerTiempo();
                break;
            case Armas.metralleta:
                // Metralleta se vuelve impresisa según calentamiento
                CalcularRayo(((tiempoPrecisiónMetralleta - tempoMetralleta) / tiempoPrecisiónMetralleta) * 0.1f,
                             ((tiempoPrecisiónMetralleta - tempoMetralleta) / tiempoPrecisiónMetralleta) * 0.2f);
                ActivarLuz();
                animadorMetralleta.AnimarDisparo(0.1f, 0.1f, turnoMetralleta);
                últimoDisparoMetralleta = ControladorJuego.ObtenerTiempo();
                break;
            case Armas.rifle:
                movimiento.DetenerMovimiento();
                CalcularRayoPenetrante();
                controlador.VibrarCámara(20, 14);
                ActivarLuz();
                animadorRife.AnimarDisparo(2f, 0.4f, TipoDisparo.espejo);
                últimoDisparoRifle = ControladorJuego.ObtenerTiempo();
                break;
            case Armas.lanzagranadas:
                movimiento.DetenerMovimiento();
                DispararGranada(0.08f);
                controlador.VibrarCámara(16, 20);
                ActivarLuz();
                animadorLanzagranadas.AnimarDisparo(0.5f, 0.8f, turnoLanzagranadas);
                últimoDisparoLanzagranadas = ControladorJuego.ObtenerTiempo();
                break;
        }
    }

    private void CalcularRayo(float imprecisiónHorizontal, float imprecisiónVertical)
    {
        var aleatorioX = RangoAleatorio(-(imprecisiónHorizontal), imprecisiónHorizontal);
        var aleatorioY = RangoAleatorio(-(imprecisiónVertical), imprecisiónVertical);
        var aleatorioZ = RangoAleatorio(-(imprecisiónHorizontal), imprecisiónHorizontal);
        var aleatorio = new Vector3(aleatorioX, aleatorioY, aleatorioZ);

        // Distancia máxima de disparo: 500
        var dirección = cámara.Entity.Transform.WorldMatrix.TranslationVector +
                        (cámara.Entity.Transform.WorldMatrix.Forward + aleatorio) * 500;

        var resultado = this.GetSimulation().Raycast(cámara.Entity.Transform.WorldMatrix.TranslationVector,
                                                     dirección,
                                                     CollisionFilterGroups.DefaultFilter,
                                                     colisionesDisparo, true);
        if (!resultado.Succeeded)
            return;

        if (resultado.Collider.CollisionGroup == CollisionFilterGroups.StaticFilter || resultado.Collider.CollisionGroup == CollisionFilterGroups.SensorTrigger)
        {
            // Botón
            var sensor = resultado.Collider.CollisionGroup == CollisionFilterGroups.SensorTrigger;
            if (sensor)
                ActivarBotón(resultado.Collider);

            // Efecto entorno
            ControladorCofres.IniciarEfectoEntorno(armaActual, resultado.Point, resultado.Normal, !sensor);
            return;
        }

        if (resultado.Collider.CollisionGroup == CollisionFilterGroups.CustomFilter3)
        {
            // Efecto lava
            ControladorCofres.IniciarEfectoDañoContinuo(armaActual, resultado.Point, resultado.Normal);
            return;
        }

        var dañable = resultado.Collider.Entity.Get<ElementoDañable>();
        if (dañable == null)
            return;

        // Daño se reduce según distancia
        var distancia = Vector3.Distance(cámara.Entity.Transform.WorldMatrix.TranslationVector, resultado.Point);
        var reducción = 0f;
        if (distancia > ObtenerDistanciaMáxima(armaActual))
            reducción = (distancia - ObtenerDistanciaMáxima(armaActual)) * 0.5f;

        // Daña enemigo
        var dañoFinal = ObtenerDaño(armaActual) - reducción;
        dañoFinal = MathUtil.Clamp(dañoFinal, dañoMínimo, dañoMáximo);
        dañable.RecibirDaño(dañoFinal);

        // Retroalimentación daño
        ControladorCofres.IniciarEfectoDaño(armaActual, dañable.enemigo, dañable.multiplicador, resultado.Point, resultado.Normal);

        if (armaActual == Armas.metralleta)
            controlador.VibrarCámara(0.4f, 4);
    }

    private void CalcularRayoPenetrante()
    {
        // Distancia máxima de disparo: 1000
        var dirección = cámara.Entity.Transform.WorldMatrix.TranslationVector + cámara.Entity.Transform.WorldMatrix.Forward * 1000;

        var resultados = new List<HitResult>();
        this.GetSimulation().RaycastPenetrating(cámara.Entity.Transform.WorldMatrix.TranslationVector,
                                                dirección, resultados,
                                                CollisionFilterGroups.DefaultFilter,
                                                colisionesDisparo, true);
        if (resultados.Count == 0)
            return;

        // Ordena resultados según distancia
        resultados = resultados.Where(o => o.Succeeded).ToList();
        resultados = resultados.GroupBy(o => o.Collider).Select(b => b.First()).ToList();
        resultados = resultados.OrderBy(o => Vector3.Distance(o.Point, cámara.Entity.Transform.WorldMatrix.TranslationVector)).ToList();

        foreach (var resultado in resultados)
        {
            if (resultado.Collider.CollisionGroup == CollisionFilterGroups.StaticFilter || resultado.Collider.CollisionGroup == CollisionFilterGroups.SensorTrigger)
            {
                // Botón
                var sensor = resultado.Collider.CollisionGroup == CollisionFilterGroups.SensorTrigger;
                if (sensor)
                    ActivarBotón(resultado.Collider);

                ControladorCofres.IniciarEfectoEntorno(armaActual, resultado.Point, resultado.Normal, !sensor);
                return;
            }

            if (resultado.Collider.CollisionGroup == CollisionFilterGroups.CustomFilter3)
            {
                // Efecto entorno
                ControladorCofres.IniciarEfectoDañoContinuo(armaActual, resultado.Point, resultado.Normal);
                return;
            }

            var dañable = resultado.Collider.Entity.Get<ElementoDañable>();
            if (dañable == null)
                continue;

            // Daño aumenta según distancia
            var distancia = Vector3.Distance(cámara.Entity.Transform.WorldMatrix.TranslationVector, resultado.Point);
            var aumento = 0f;
            if (distancia > ObtenerDistanciaMáxima(armaActual))
                aumento = (distancia - ObtenerDistanciaMáxima(armaActual)) * 0.5f;

            // Daña enemigo
            var dañoFinal = ObtenerDaño(armaActual) + aumento;
            dañoFinal = MathUtil.Clamp(dañoFinal, dañoMínimo, dañoMáximo);
            dañable.RecibirDaño(dañoFinal);

            // Retroalimentación daño
            ControladorCofres.IniciarEfectoDaño(armaActual, dañable.enemigo, dañable.multiplicador, resultado.Point, resultado.Normal);
        }
    }

    private void DispararGranada(float imprecisión)
    {
        var aleatorioX = RangoAleatorio(-(imprecisión), imprecisión);
        var aleatorioY = RangoAleatorio(-(imprecisión), imprecisión);
        var aleatorioZ = RangoAleatorio(-(imprecisión), imprecisión);
        var aleatorio = new Vector3(aleatorioX, aleatorioY, aleatorioZ);

        // Distancia máxima de disparo: 1000
        var direcciónRayo = cámara.Entity.Transform.WorldMatrix.TranslationVector + (cámara.Entity.Transform.WorldMatrix.Forward + aleatorio) * 1000;
        var resultado = this.GetSimulation().Raycast(cámara.Entity.Transform.WorldMatrix.TranslationVector,
                                                     direcciónRayo,
                                                     CollisionFilterGroups.DefaultFilter,
                                                     colisionesDisparo);

        // Si rayo no toca nada, entonces tira hacia la máxima distancia
        if (!resultado.Succeeded)
            resultado.Point = (cámara.Entity.Transform.WorldMatrix.Forward + aleatorio) * 1000;

        // Granada sigue punto de rayo desde el hombro
        var posición = Vector3.Zero;
        if (turnoLanzagranadas == TipoDisparo.izquierda)
            posición = animadorLanzagranadas.ejeIzquierda.WorldMatrix.TranslationVector + animadorLanzagranadas.ejeIzquierda.WorldMatrix.Forward;
        else
            posición = animadorLanzagranadas.ejeDerecha.WorldMatrix.TranslationVector + animadorLanzagranadas.ejeDerecha.WorldMatrix.Forward;

        var dirección = Vector3.Normalize(posición - resultado.Point);
        var rotación = Quaternion.LookRotation(dirección, Vector3.UnitY);

        ControladorCofres.IniciarGranada(ObtenerDaño(armaActual), 35, posición, rotación);
    }

    private void Atacar(bool desdeIzquierda)
    {
        // Cadencia
        var tiempoDisparo = ObtenerCadencia(Armas.espada) + últimoDisparoEspada;
        if (ControladorJuego.ObtenerTiempo() < tiempoDisparo)
            return;

        var direccionRayos = rayosMelé;
        if (!desdeIzquierda)
            direccionRayos = direccionRayos.Reverse().ToArray();

        // 3 rayos
        // Distancia máxima de disparo: 2
        var entorno = true;
        var posición = Vector3.Zero;
        var normal = Vector3.Zero;
        foreach (var posiciónRayo in direccionRayos)
        {
            var inicioRayo = cámara.Entity.Transform.LocalToWorld(posiciónRayo);
            var dirección = cámara.Entity.Transform.WorldMatrix.TranslationVector + cámara.Entity.Transform.WorldMatrix.Forward * 2;
            var resultado = this.GetSimulation().Raycast(inicioRayo,
                                                         dirección,
                                                         CollisionFilterGroups.DefaultFilter,
                                                         colisionesDisparo, true);

            if (!resultado.Succeeded)
                continue;

            // Dañables
            if (resultado.Collider.CollisionGroup == CollisionFilterGroups.KinematicFilter ||
                resultado.Collider.CollisionGroup == CollisionFilterGroups.CustomFilter2)
            {
                var dañable = resultado.Collider.Entity.Get<ElementoDañable>();
                if (dañable == null)
                    return;

                dañable.RecibirDaño(ObtenerDaño(Armas.espada));
                posición = Vector3.Zero;
                normal = Vector3.Zero;

                ControladorCofres.IniciarEfectoDaño(armaActual, dañable.enemigo, dañable.multiplicador, resultado.Point, resultado.Normal);
            }
            // Proyectiles
            else if (resultado.Collider.CollisionGroup == CollisionFilterGroups.CustomFilter1)
            {
                // Destruye proyectiles simples
                var interfaz = ObtenerInterfaz<IProyectil>(resultado.Collider.Entity);
                interfaz.Destruir();

                var enemigo = resultado.Collider.Entity.Get<ElementoProyectilSimple>().disparador;
                ControladorCofres.IniciarEfectoDaño(armaActual, enemigo, 0.2f, resultado.Point, resultado.Normal);
            }
            // Botones
            else if (resultado.Collider.CollisionGroup == CollisionFilterGroups.SensorTrigger)
            {
                ActivarBotón(resultado.Collider);
            }
            // Entorno
            else if (resultado.Collider.CollisionGroup == CollisionFilterGroups.StaticFilter)
            {
                posición = resultado.Point;
                normal = resultado.Normal;
                entorno = true;
            }
            // Lava
            else if (resultado.Collider.CollisionGroup == CollisionFilterGroups.CustomFilter3)
            {
                posición = resultado.Point;
                normal = resultado.Normal;
                entorno = false;
            }
        }

        if (posición != Vector3.Zero && normal != Vector3.Zero)
        {
            if (entorno)
                ControladorCofres.IniciarEfectoEntorno(armaActual, posición, normal, true);
            else
                ControladorCofres.IniciarEfectoDañoContinuo(armaActual, posición, normal);
        }
    }

    private async void AcercarMira(bool acercar)
    {
        movimiento.CambiarSensiblidad(acercar);
        interfaz.ApagarMiras();

        SistemaSonidos.SonarMira(acercar);
        var inicial = cámara.VerticalFieldOfView;
        var objetivo = inicial;

        var duración = 0f;
        var sombraInicial = 0f;
        var sombraObjetivo = 1f;
        float tiempoLerp = 0;
        float tiempo = 0;

        if (acercar)
        {
            usandoMira = true;
            duración = 0.025f;
            objetivo = controlador.ObtenerCampoVisiónMira();
        }
        else
        {
            sombraInicial = 1f;
            sombraObjetivo = 0f;
            duración = 0.06f;
            objetivo = controlador.ObtenerCampoVisiónMínimo();
            interfaz.MostrarMiraRifle(false);
        }

        // Animación mira
        tokenMira.Cancel();
        tokenMira = new CancellationTokenSource();

        var token = tokenMira.Token;
        while (tiempoLerp < duración)
        {
            if (token.IsCancellationRequested)
                return;

            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            cámara.VerticalFieldOfView = MathUtil.Lerp(inicial, objetivo, tiempo);
            interfaz.ActualizarSombraRifle(MathUtil.Lerp(sombraInicial, sombraObjetivo, tiempo));

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        // Fin
        cámara.VerticalFieldOfView = objetivo;
        interfaz.ActualizarSombraRifle(sombraObjetivo);

        if (acercar)
            interfaz.MostrarMiraRifle(true);
        else
            usandoMira = false;
    }

    private async void ActivarLuz()
    {
        // Cancela luz
        tokenLuz.Cancel();
        tokenLuz = new CancellationTokenSource();

        luzDisparo.Enabled = true;

        var token = tokenLuz.Token;
        if (!token.IsCancellationRequested)
            await Task.Delay(80);

        luzDisparo.Enabled = false;
    }

    private void ActivarBotón(PhysicsComponent elemento)
    {
        var botón = elemento.Entity.Get<ControladorBotón>();
        if (botón != null)
            botón.ActivarPorDisparo();
    }

    private async void CambiarArma(Armas nuevaArma)
    {
        if (cambiandoArma || nuevaArma == armaActual || usandoMira)
            return;

        AnimarSalida();
        SistemaSonidos.SonarCambioArma();

        cambiandoArma = true;
        armaAnterior = armaActual;
        armaActual = nuevaArma;
        movimiento.DetenerMovimiento();

        interfaz.ApagarMiras();
        interfaz.CambiarÍcono(armaActual);
        movimiento.CambiarVelocidadMáxima(armaActual);

        partículasMetralletaIzquierda.Enabled = false;
        partículasMetralletaDerecha.Enabled = false;

        await AnimarEntrada();

        cambiandoArma = false;
        interfaz.CambiarMira(armaActual);
    }

    private void CambiarSiguenteArma(bool adelante)
    {
        ruedaRatón = 0;
        var nuevaArma = (int)armaActual;

        if (adelante)
            nuevaArma += 1;
        else
            nuevaArma -= 1;

        if (nuevaArma >= 5)
            nuevaArma = 0;
        else if (nuevaArma < 0)
            nuevaArma = 4;

        CambiarArma((Armas)nuevaArma);
    }

    public float[] ObtenerParametrosAnimación()
    {
        var duraciónAnimación = 1.9f - movimiento.ObtenerAceleración();
        var fuerzaAnimación = 1f;

        switch (armaActual)
        {
            case Armas.espada:
                duraciónAnimación += 0.2f;
                fuerzaAnimación = movimiento.ObtenerAceleración() * 0.5f;
                break;
            case Armas.escopeta:
                fuerzaAnimación = movimiento.ObtenerAceleración() * 1f;
                break;
            case Armas.metralleta:
                fuerzaAnimación = movimiento.ObtenerAceleración() * 1f;
                break;
            case Armas.rifle:
                fuerzaAnimación = movimiento.ObtenerAceleración() * 2f;
                break;
            case Armas.lanzagranadas:
                fuerzaAnimación = movimiento.ObtenerAceleración() * 4f;
                break;
        }

        // Ajuste
        duraciónAnimación = MathUtil.Clamp(duraciónAnimación, 0.2f, 2);
        fuerzaAnimación = MathUtil.Clamp(fuerzaAnimación, 0.5f, 3);

        return new float[] { duraciónAnimación, fuerzaAnimación };
    }

    public void GuardarArma()
    {
        switch (armaActual)
        {
            case Armas.espada:
                animadorEspada.AnimarSalidaArma();
                break;
            case Armas.escopeta:
                animadorEscopeta.AnimarSalidaArma();
                break;
            case Armas.metralleta:
                animadorMetralleta.AnimarSalidaArma();
                break;
            case Armas.rifle:
                animadorRife.AnimarSalidaArma();
                break;
            case Armas.lanzagranadas:
                animadorLanzagranadas.AnimarSalidaArma();
                break;
        }
    }

    private void ApagarArmas()
    {
        animadorEspada.ActivarArma(false);
        animadorEscopeta.ActivarArma(false);
        animadorMetralleta.ActivarArma(false);
        animadorRife.ActivarArma(false);
        animadorLanzagranadas.ActivarArma(false);
    }

    private float ObtenerDaño(Armas arma)
    {
        var daño = 0f;
        switch (arma)
        {
            case Armas.espada:
                daño = 25;  // * 3
                break;
            case Armas.escopeta:
                daño = 5;   // * 20
                break;
            case Armas.metralleta:
                daño = 10;
                break;
            case Armas.rifle:
                daño = 100;
                break;
            case Armas.lanzagranadas:
                daño = 80;
                break;
        }

        // Dificultad
        if (SistemaMemoria.Dificultad == Dificultades.fácil)
            daño *= 1.25f;

        if (controlador.ObtenerPoder(Poderes.daño))
            daño *= 3;

        return daño;
    }

    private float ObtenerDistanciaMáxima(Armas arma)
    {
        switch (arma)
        {
            default:
            case Armas.espada:
                return 0;
            case Armas.escopeta:
                return 5;
            case Armas.metralleta:
                return 10;
            case Armas.rifle:
                return 10;
            case Armas.lanzagranadas:
                return 0;
        }
    }

    private float ObtenerCadencia(Armas arma)
    {
        switch (arma)
        {
            case Armas.espada:
                return 0.2f;
            case Armas.escopeta:
                return 0.6f;
            case Armas.metralleta:
                return 0.06f;
            case Armas.rifle:
                return 1.2f;
            case Armas.lanzagranadas:
                return 1.4f;
            default:
                return 0;
        }
    }

    public void AnimarSalida()
    {
        switch (armaActual)
        {
            case Armas.espada:
                animadorEspada.AnimarSalidaArma();
                break;
            case Armas.escopeta:
                animadorEscopeta.AnimarSalidaArma();
                break;
            case Armas.metralleta:
                animadorMetralleta.AnimarSalidaArma();
                break;
            case Armas.rifle:
                animadorRife.AnimarSalidaArma();
                break;
            case Armas.lanzagranadas:
                animadorLanzagranadas.AnimarSalidaArma();
                break;
        }
    }

    public async Task AnimarEntrada()
    {
        switch (armaActual)
        {
            case Armas.espada:
                await animadorEspada.AnimarEntradaArma();
                break;
            case Armas.escopeta:
                await animadorEscopeta.AnimarEntradaArma();
                break;
            case Armas.metralleta:
                partículasMetralletaIzquierda.Enabled = true;
                partículasMetralletaDerecha.Enabled = true;
                await animadorMetralleta.AnimarEntradaArma();
                break;
            case Armas.rifle:
                await animadorRife.AnimarEntradaArma();
                break;
            case Armas.lanzagranadas:
                await animadorLanzagranadas.AnimarEntradaArma();
                break;
        }
    }

    public float ObtenerCalentamientoMetralleta()
    {
        return tempoMetralleta;
    }

    public bool ObtenerAnimando()
    {
        return cambiandoArma;
    }

    public bool ObtenerUsoMira()
    {
        return usandoMira;
    }

    public void Bloquear(bool bloquear)
    {
        bloqueo = bloquear;
    }
}
