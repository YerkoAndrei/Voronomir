using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Physics;
using Stride.Input;
using Stride.Engine;

namespace Bozobaralika;
using static Utilidades;
using static Constantes;

public class ControladorArmas : StartupScript
{
    public Prefab prefabMarca;
    public Prefab prefabGranada;
    public Prefab prefabExplosión;

    public AnimadorArmas animadorEspada;
    public AnimadorArmas animadorEscopeta;
    public AnimadorArmas animadorMetralleta;
    public AnimadorArmas animadorRife;
    public AnimadorArmas animadorLanzagranadas;

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
    private IProyectil[] granadas;
    private IImpacto[] explosiones;
    private int granadaActual;
    private int explosiónActual;
    private int maxGranadas;
    private int maxExplosiones;

    private ElementoMarca[] marcas;
    private int marcaActual;
    private int maxMarcas;

    public async void Iniciar(ControladorJugador _controlador, ControladorMovimiento _movimiento, CameraComponent _cámara, InterfazJuego _interfaz)
    {
        controlador = _controlador;
        movimiento = _movimiento;
        cámara = _cámara;
        interfaz = _interfaz;

        dañoMínimo = 1f;
        dañoMáximo = 200f;

        tiempoMaxMetralleta = 4f;
        tiempoAtascamientoMetralleta = 2f;
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

        // Cofre granadas
        maxGranadas = 4;
        granadas = new IProyectil[maxGranadas];
        for (int i = 0; i < maxGranadas; i++)
        {
            var granada = prefabGranada.Instantiate()[0];
            granadas[i] = ObtenerInterfaz<IProyectil>(granada);
            Entity.Scene.Entities.Add(granada);

            // Impactos son explosiones o veneno
            if (prefabExplosión != null)
                granadas[i].AsignarImpacto(IniciarExplosión);
        }

        // Cofre explosiones
        maxExplosiones = 4;
        explosiones = new IImpacto[maxExplosiones];
        for (int i = 0; i < maxExplosiones; i++)
        {
            var explosión = prefabExplosión.Instantiate()[0];
            explosiones[i] = ObtenerInterfaz<IImpacto>(explosión);
            Entity.Scene.Entities.Add(explosión);
        }

        // Cofre marcas
        maxMarcas = 100;
        marcas = new ElementoMarca[maxMarcas];
        for (int i = 0; i < maxMarcas; i++)
        {
            var marca = prefabMarca.Instantiate()[0];
            marcas[i] = marca.Get<ElementoMarca>();
            Entity.Scene.Entities.Add(marca);
        }

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

            primerDisparoMetralleta = (float)Game.UpdateTime.Total.TotalSeconds;
            if (tempoMetralleta >= tiempoMaxMetralleta)
                metralletaAtascada = false;
        }

        // Metralleta
        if (Input.IsMouseButtonDown(MouseButton.Left) && armaActual == Armas.metralleta && !metralletaAtascada)
        {
            tempoMetralleta -= (float)Game.UpdateTime.Elapsed.TotalSeconds;

            // Permite disparo singular
            if ((primerDisparoMetralleta + 0.12f) < (float)Game.UpdateTime.Total.TotalSeconds)
            {
                if (tempoMetralleta > 0)
                    Disparar();
                else
                    AtascarMetralleta();
            }
        }

        if (!Input.IsMouseButtonDown(MouseButton.Left))
            EnfriarMetralleta();

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

        if ((float)Game.UpdateTime.Total.TotalSeconds < tiempoDisparo)
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
                últimoDisparoEspada = (float)Game.UpdateTime.Total.TotalSeconds;
                break;
            case Armas.escopeta:
                movimiento.DetenerMovimiento();
                for (int i = 0; i < ObtenerCantidadPerdigones(); i++)
                {
                    CalcularRayo(0.12f);
                }
                controlador.VibrarCámara(16, 10);
                animadorEscopeta.AnimarDisparo(0.5f, 0.2f, TipoDisparo.espejo);
                últimoDisparoEscopeta = (float)Game.UpdateTime.Total.TotalSeconds;
                break;
            case Armas.metralleta:
                // Metralleta se vuelve impresisa según calentamiento
                CalcularRayo(((tiempoMaxMetralleta - tempoMetralleta) / tiempoMaxMetralleta) * 0.1f);
                animadorMetralleta.AnimarDisparo(0.1f, 0.09f, turnoMetralleta);
                últimoDisparoMetralleta = (float)Game.UpdateTime.Total.TotalSeconds;
                break;
            case Armas.rifle:
                movimiento.DetenerMovimiento();
                CalcularRayoPenetrante();
                controlador.VibrarCámara(20, 12);
                animadorRife.AnimarDisparo(2f, 0.4f, TipoDisparo.espejo);
                últimoDisparoRifle = (float)Game.UpdateTime.Total.TotalSeconds;
                break;
            case Armas.lanzagranadas:
                movimiento.DetenerMovimiento();
                DispararGranada(0.08f);
                controlador.VibrarCámara(16, 20);
                animadorLanzagranadas.AnimarDisparo(0.5f, 0.8f, turnoLanzagranadas);
                últimoDisparoLanzagranadas = (float)Game.UpdateTime.Total.TotalSeconds;
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
                                                     colisionesDisparo);
        if (!resultado.Succeeded)
            return;

        if (resultado.Collider.CollisionGroup == CollisionFilterGroups.StaticFilter || resultado.Collider.CollisionGroup == CollisionFilterGroups.SensorTrigger)
        {
            CrearMarca(resultado.Point, resultado.Normal, resultado.Collider.CollisionGroup == CollisionFilterGroups.SensorTrigger);
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
        dañable.RecibirDaño(dañoFinal);

        // Retroalimentación daño
        CrearMarcaDaño(resultado.Point, resultado.Normal, dañable.multiplicador);
        if (armaActual == Armas.metralleta)
            controlador.VibrarCámara(1f, 4);
    }

    private void CalcularRayoPenetrante()
    {
        // Distancia máxima de disparo: 1000
        var dirección = cámara.Entity.Transform.WorldMatrix.TranslationVector + cámara.Entity.Transform.WorldMatrix.Forward * 1000;

        var resultados = new List<HitResult>();
        this.GetSimulation().RaycastPenetrating(cámara.Entity.Transform.WorldMatrix.TranslationVector,
                                                dirección, resultados,
                                                CollisionFilterGroups.DefaultFilter,
                                                colisionesDisparo);
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
                CrearMarca(resultado.Point, resultado.Normal, resultado.Collider.CollisionGroup == CollisionFilterGroups.SensorTrigger);
                break;
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
            dañable.RecibirDaño(dañoFinal);

            // Retroalimentación daño
            CrearMarcaDaño(resultado.Point, resultado.Normal, dañable.multiplicador);
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

        granadas[granadaActual].Iniciar(0, 35, rotación, posición, Enemigos.nada);

        granadaActual++;
        if (granadaActual >= maxGranadas)
            granadaActual = 0;
    }

    private void IniciarExplosión(Vector3 posición, Vector3 normal, bool soloEfecto)
    {
        explosiones[explosiónActual].Iniciar(posición, Quaternion.Identity, ObtenerDaño(armaActual));

        if (normal != Vector3.Zero)
            CrearMarca(posición, normal, soloEfecto);

        explosiónActual++;
        if (explosiónActual >= maxExplosiones)
            explosiónActual = 0;
    }

    private void Atacar(bool desdeIzquierda)
    {
        // Cadencia
        var tiempoDisparo = ObtenerCadencia(Armas.espada) + últimoDisparoEspada;
        if ((float)Game.UpdateTime.Total.TotalSeconds < tiempoDisparo)
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
            var posiciónRayoGlobal = cámara.Entity.Transform.LocalToWorld(posiciónRayo);
            var dirección = cámara.Entity.Transform.WorldMatrix.TranslationVector + cámara.Entity.Transform.WorldMatrix.Forward * 2;
            var resultado = this.GetSimulation().Raycast(posiciónRayoGlobal,
                                                         dirección,
                                                         CollisionFilterGroups.DefaultFilter,
                                                         colisionesDisparo);

            if (!resultado.Succeeded)
                continue;

            if (resultado.Collider.CollisionGroup == CollisionFilterGroups.KinematicFilter ||
                resultado.Collider.CollisionGroup == CollisionFilterGroups.CustomFilter2)
            {
                var dañable = resultado.Collider.Entity.Get<ElementoDañable>();
                if (dañable == null)
                    return;

                dañable.RecibirDaño(ObtenerDaño(Armas.espada));
                posición = Vector3.Zero;
                normal = Vector3.Zero;

                CrearMarcaDaño(resultado.Point, resultado.Normal, dañable.multiplicador);
            }
            else if (resultado.Collider.CollisionGroup == CollisionFilterGroups.CustomFilter1)
            {
                // Destruye proyectiles simples
                var interfaz = ObtenerInterfaz<IProyectil>(resultado.Collider.Entity);
                interfaz.Destruir();
                CrearMarcaDaño(resultado.Point, resultado.Normal, 1);
            }
            else
            {
                posición = resultado.Point;
                normal = resultado.Normal;
            }
        }

        if (posición != Vector3.Zero && normal != Vector3.Zero)
            CrearMarca(posición, normal, false);
    }

    private void CrearMarca(Vector3 posición, Vector3 normal, bool soloEfecto)
    {
        marcas[marcaActual].IniciarMarca(armaActual, posición, normal, soloEfecto);
        marcaActual++;

        if (marcaActual >= maxMarcas)
            marcaActual = 0;
    }

    private void CrearMarcaDaño(Vector3 posición, Vector3 normal, float multiplicador)
    {
        switch (armaActual)
        {
            case Armas.espada:
                multiplicador *= 0.5f;
                break;
            case Armas.escopeta:
                multiplicador *= 0.4f;
                break;
            case Armas.metralleta:
                multiplicador *= 1f;
                break;
            case Armas.rifle:
                multiplicador *= 2f;
                break;
            case Armas.lanzagranadas:
                multiplicador *= 2f;
                break;
        }
        marcas[marcaActual].IniciarDaño(posición, normal, multiplicador);
        marcaActual++;

        if (marcaActual >= maxMarcas)
            marcaActual = 0;
    }

    private int ObtenerCantidadPerdigones()
    {
        // PENDIENTE: mejoras
        return 20;
    }

    private void EnfriarMetralleta()
    {
        if (tempoMetralleta < tiempoMaxMetralleta)
            tempoMetralleta += (float)Game.UpdateTime.Elapsed.TotalSeconds;
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

        switch (armaActual)
        {
            case Armas.espada:
                await animadorEspada.AnimarEntradaArma();
                break;
            case Armas.escopeta:
                await animadorEscopeta.AnimarEntradaArma();
                break;
            case Armas.metralleta:
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
        switch (arma)
        {
            case Armas.espada:
                return 25; // * 3
            case Armas.escopeta:
                return 6;
            case Armas.metralleta:
                return 8;
            case Armas.rifle:
                return 100;
            case Armas.lanzagranadas:
                return 80;
            default:
                return 0;
        }
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
                return 0.8f;
            case Armas.metralleta:
                return 0.05f;
            case Armas.rifle:
                return 1.4f;
            case Armas.lanzagranadas:
                return 1f;
            default:
                return 0;
        }
    }

    public void Bloquear(bool bloquear)
    {
        bloqueo = bloquear;
    }
}
