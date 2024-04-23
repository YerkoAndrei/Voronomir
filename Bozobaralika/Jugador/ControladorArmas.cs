using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Physics;
using Stride.Input;
using Stride.Engine;
using Stride.Particles.Components;

namespace Bozobaralika;
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
    private float dañoMínimo;
    private float dañoMáximo;

    private float últimoDisparoEspada;
    private float últimoDisparoEscopeta;
    private float últimoDisparoMetralleta;
    private float últimoDisparoRifle;
    private float últimoDisparoLanzagranadas;

    private bool cambiandoArma;
    private bool usandoMira;

    // Metralleta
    private TipoDisparo turnoMetralleta;
    private bool metralletaAtascada;
    private float primerDisparoMetralleta;
    private float tempoMetralleta;
    private float tiempoMaxMetralleta;
    private float tiempoAtascamientoMetralleta;

    // Lanzagranadas
    private TipoDisparo turnoLanzagranadas;

    public async void Iniciar(ControladorJugador _controlador, ControladorMovimiento _movimiento, CameraComponent _cámara, InterfazJuego _interfaz)
    {
        controlador = _controlador;
        movimiento = _movimiento;
        cámara = _cámara;
        interfaz = _interfaz;

        dañoMínimo = 1f;
        dañoMáximo = 200f;

        tiempoMaxMetralleta = 6f;
        tiempoAtascamientoMetralleta = 3f;
        tempoMetralleta = tiempoAtascamientoMetralleta;

        // Filtros disparos
        colisionesDisparo = CollisionFilterGroupFlags.StaticFilter | 
                            CollisionFilterGroupFlags.KinematicFilter | 
                            CollisionFilterGroupFlags.SensorTrigger |
                            CollisionFilterGroupFlags.CustomFilter1 |
                            CollisionFilterGroupFlags.CustomFilter2;

        rayosMelé = new Vector3[3]
        {
            new Vector3 (0.3f, 0, 0),
            new Vector3 (0, 0, 0),
            new Vector3 (-0.3f, 0, 0)
        };

        animadorEspada.Iniciar();
        animadorEscopeta.Iniciar();
        animadorMetralleta.Iniciar();
        animadorRife.Iniciar();
        animadorLanzagranadas.Iniciar();

        // Arma por defecto
        ApagarArmas();
        armaActual = Armas.espada;
        armaAnterior = armaActual;

        interfaz.CambiarMira(armaActual);
        interfaz.CambiarÍcono(armaActual);

        luzDisparo.Enabled = false;
        partículasMetralletaIzquierda.Enabled = false;
        partículasMetralletaDerecha.Enabled = false;

        cambiandoArma = true;
        await animadorEspada.AnimarEntradaArma();
        cambiandoArma = false;
        movimiento.CambiarVelocidadMáxima(armaActual);
    }

    public void ActualizarEntradas()
    {
        if (bloqueo)
            return;

        // Movimiento correr
        AnimarMovimientoArma();

        // Disparo general
        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            Disparar();

            primerDisparoMetralleta = ControladorPartida.ObtenerTiempo();
            if (tempoMetralleta >= tiempoMaxMetralleta)
                metralletaAtascada = false;
        }

        // Metralleta
        if (Input.IsMouseButtonDown(MouseButton.Left) && armaActual == Armas.metralleta && !metralletaAtascada)
        {
            tempoMetralleta -= (float)Game.UpdateTime.WarpElapsed.TotalSeconds;

            // Permite disparo singular
            if ((primerDisparoMetralleta + 0.12f) < ControladorPartida.ObtenerTiempo())
            {
                if (tempoMetralleta > 0)
                    Disparar();
                else
                    AtascarMetralleta();
            }
        }

        if (!Input.IsMouseButtonDown(MouseButton.Left))
            EnfriarMetralleta();

        if (armaActual == Armas.metralleta)
        {
            if (metralletaAtascada)
            {
                partículasMetralletaIzquierda.Color = Color.Transparent;
                partículasMetralletaDerecha.Color = Color.Transparent;
            }
            else
            {
                partículasMetralletaIzquierda.Color = Color.Lerp(colorMetralletaCaliente, Color.Transparent, (tempoMetralleta / tiempoMaxMetralleta));
                partículasMetralletaDerecha.Color = Color.Lerp(colorMetralletaCaliente, Color.Transparent, (tempoMetralleta / tiempoMaxMetralleta));
            }
        }

        // Espadas
        if (Input.IsMouseButtonPressed(MouseButton.Right) && armaActual == Armas.espada)
            Disparar(false);

        // Rifle
        if (Input.IsMouseButtonPressed(MouseButton.Right) && armaActual == Armas.rifle)
            AcercarMira(true);

        if (Input.IsMouseButtonReleased(MouseButton.Right) && armaActual == Armas.rifle)
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
    }

    private void Disparar(bool clicIzquierdo = true)
    {
        if (cambiandoArma)
            return;

        // Metralleta
        if (armaActual == Armas.metralleta && metralletaAtascada)
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

        if (ControladorPartida.ObtenerTiempo() < tiempoDisparo)
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

        switch (armaActual)
        {
            case Armas.espada:
                Atacar(clicIzquierdo);
                if(clicIzquierdo)
                    animadorEspada.AnimarAtaque(TipoDisparo.izquierda);
                else
                    animadorEspada.AnimarAtaque(TipoDisparo.derecha);
                últimoDisparoEspada = ControladorPartida.ObtenerTiempo();
                break;
            case Armas.escopeta:
                for (int i = 0; i < 20; i++)
                {
                    CalcularRayo(0.1f);
                }
                controlador.VibrarCámara(16, 10);
                ActivarLuz();
                animadorEscopeta.AnimarDisparo(0.5f, 0.2f, TipoDisparo.espejo);
                últimoDisparoEscopeta = ControladorPartida.ObtenerTiempo();
                break;
            case Armas.metralleta:
                // Metralleta se vuelve impresisa según calentamiento
                CalcularRayo(((tiempoMaxMetralleta - tempoMetralleta) / tiempoMaxMetralleta) * 0.1f);
                ActivarLuz();
                animadorMetralleta.AnimarDisparo(0.1f, 0.09f, turnoMetralleta);
                últimoDisparoMetralleta = ControladorPartida.ObtenerTiempo();
                break;
            case Armas.rifle:
                movimiento.DetenerMovimiento();
                CalcularRayoPenetrante();
                controlador.VibrarCámara(20, 12);
                ActivarLuz();
                animadorRife.AnimarDisparo(2f, 0.4f, TipoDisparo.espejo);
                últimoDisparoRifle = ControladorPartida.ObtenerTiempo();
                break;
            case Armas.lanzagranadas:
                movimiento.DetenerMovimiento();
                DispararGranada(0.08f);
                controlador.VibrarCámara(16, 20);
                ActivarLuz();
                animadorLanzagranadas.AnimarDisparo(0.5f, 0.8f, turnoLanzagranadas);
                últimoDisparoLanzagranadas = ControladorPartida.ObtenerTiempo();
                break;
        }
    }

    private void CalcularRayo(float imprecisión)
    {
        var aleatorioX = RangoAleatorio(-(imprecisión), imprecisión);
        var aleatorioY = RangoAleatorio(-(imprecisión), imprecisión);
        var aleatorioZ = RangoAleatorio(-(imprecisión), imprecisión);
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
            var sensor = resultado.Collider.CollisionGroup == CollisionFilterGroups.SensorTrigger;
            if (sensor)
                ActivarBotón(resultado.Collider);

            ControladorCofres.IniciarEfectoEntorno(armaActual, resultado.Point, resultado.Normal, !sensor);
            return;
        }

        var dañable = resultado.Collider.Entity.Get<ElementoDañable>();
        if (dañable == null)
            return;

        // PENDIENTE: efecto
        // Daño segun distancia
        var distancia = Vector3.Distance(cámara.Entity.Transform.WorldMatrix.TranslationVector, resultado.Point);
        var reducción = 0f;
        if(distancia > ObtenerDistanciaMáxima(armaActual))
            reducción = (distancia - ObtenerDistanciaMáxima(armaActual)) * 0.5f;

        // Daña enemigo
        var dañoFinal = ObtenerDaño(armaActual) - reducción;
        dañoFinal = MathUtil.Clamp(dañoFinal, dañoMínimo, dañoMáximo);
        dañable.RecibirDaño(dañoFinal, false);

        // Retroalimentación daño
        ControladorCofres.IniciarEfectoDaño(armaActual, dañable.enemigo, dañable.multiplicador, resultado.Point, resultado.Normal);

        if (armaActual == Armas.metralleta)
            controlador.VibrarCámara(0.6f, 4);
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
                var sensor = resultado.Collider.CollisionGroup == CollisionFilterGroups.SensorTrigger;
                if (sensor)
                    ActivarBotón(resultado.Collider);

                ControladorCofres.IniciarEfectoEntorno(armaActual, resultado.Point, resultado.Normal, !sensor);
                return;
            }

            var dañable = resultado.Collider.Entity.Get<ElementoDañable>();
            if (dañable == null)
                continue;

            // Retroalimentación daño
            controlador.VibrarCámara(4f, 4);

            // PENDIENTE: efecto
            // Daño segun distancia
            var distancia = Vector3.Distance(cámara.Entity.Transform.WorldMatrix.TranslationVector, resultado.Point);
            var aumento = 0f;
            if (distancia > ObtenerDistanciaMáxima(armaActual))
                aumento = (distancia - ObtenerDistanciaMáxima(armaActual)) * 0.5f;

            // Daña enemigo
            var dañoFinal = ObtenerDaño(armaActual) + aumento;
            dañoFinal = MathUtil.Clamp(dañoFinal, dañoMínimo, dañoMáximo);
            dañable.RecibirDaño(dañoFinal, false);

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
        if (ControladorPartida.ObtenerTiempo() < tiempoDisparo)
            return;

        var direccionRayos = rayosMelé;
        if (!desdeIzquierda)
            direccionRayos = direccionRayos.Reverse().ToArray();

        // 3 rayos
        // Distancia máxima de disparo: 2
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

                dañable.RecibirDaño(ObtenerDaño(Armas.espada), false);
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
                ControladorCofres.IniciarEfectoDaño(armaActual, enemigo, 0.5f, resultado.Point, resultado.Normal);
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
            }
        }

        if (posición != Vector3.Zero && normal != Vector3.Zero)
            ControladorCofres.IniciarEfectoEntorno(armaActual, posición, normal, true);
    }

    private void EnfriarMetralleta()
    {
        if (tempoMetralleta < tiempoMaxMetralleta)
            tempoMetralleta += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
    }

    private async void AtascarMetralleta()
    {
        metralletaAtascada = true;
        await Task.Delay((int)(tiempoAtascamientoMetralleta * 1000));
        tempoMetralleta = tiempoMaxMetralleta;
    }

    private void AcercarMira(bool acercar)
    {
        // PENDIENTE: elegir FOV de opciones
        // PENDIENTE: animacioón
        usandoMira = acercar;
        if (usandoMira)
            cámara.VerticalFieldOfView = 20;
        else
            cámara.VerticalFieldOfView = 90;

        movimiento.CambiarSensiblidad(acercar);
        interfaz.MostrarMiraRifle(acercar);
    }

    private async void ActivarLuz()
    {
        luzDisparo.Enabled = true;
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

        cambiandoArma = true;
        armaAnterior = armaActual;
        armaActual = nuevaArma;
        movimiento.DetenerMovimiento();

        interfaz.ApagarMiras();
        interfaz.CambiarÍcono(armaActual);
        partículasMetralletaIzquierda.Enabled = false;
        partículasMetralletaDerecha.Enabled = false;

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

        cambiandoArma = false;

        movimiento.CambiarVelocidadMáxima(armaActual);
        interfaz.CambiarMira(armaActual);
    }

    private void AnimarMovimientoArma()
    {
        switch (armaActual)
        {
            case Armas.espada:
                animadorEspada.AnimarCorrerArma(0.5f, movimiento.ObtenerAceleración() - 0.1f, movimiento.ObtenerEnSuelo());
                break;
            case Armas.escopeta:
                animadorEscopeta.AnimarCorrerArma(1, movimiento.ObtenerAceleración(), movimiento.ObtenerEnSuelo());
                break;
            case Armas.metralleta:
                animadorMetralleta.AnimarCorrerArma(1, movimiento.ObtenerAceleración(), movimiento.ObtenerEnSuelo());
                break;
            case Armas.rifle:
                animadorRife.AnimarCorrerArma(2, movimiento.ObtenerAceleración(), movimiento.ObtenerEnSuelo());
                break;
            case Armas.lanzagranadas:
                animadorLanzagranadas.AnimarCorrerArma(2, movimiento.ObtenerAceleración(), movimiento.ObtenerEnSuelo());
                break;
        }
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
        animadorEspada.ApagarArma();
        animadorEscopeta.ApagarArma();
        animadorMetralleta.ApagarArma();
        animadorRife.ApagarArma();
        animadorLanzagranadas.ApagarArma();
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
                return 10;
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

    public void Bloquear(bool bloquear)
    {
        bloqueo = bloquear;
    }
}
