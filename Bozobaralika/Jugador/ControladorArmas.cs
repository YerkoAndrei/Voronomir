using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;
using static Sistema;
using static Constantes;

public class ControladorArmas : SyncScript
{
    public ControladorMovimiento movimiento;
    public CameraComponent cámara;
    public Prefab prefabMarca;

    public TransformComponent espada;
    public TransformComponent pistola;
    public TransformComponent escopeta;
    public TransformComponent metralleta;
    public TransformComponent rife;

    private Armas armaActual;
    private float últimoDisparo;
    private float dañoMínimo;
    private float dañoMáximo;

    // Metralleta
    private bool metralletaAtascada;
    private float tempoMetralleta;
    private float tiempoMaxMetralleta;
    private float tiempoAtascamientoMetralleta;

    public override void Start()
    {
        dañoMínimo = 1f;
        dañoMáximo = 60f;

        tiempoMaxMetralleta = 4f;
        tiempoAtascamientoMetralleta = 2f;
        tempoMetralleta = tiempoAtascamientoMetralleta;

        ApagarArmas();
        CambiarArma(Armas.pistola);
    }

    public override void Update()
    {
        // Disparo general
        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            Disparar();

            if(tempoMetralleta >= tiempoMaxMetralleta)
                metralletaAtascada = false;
        }

        // Metralleta
        if (Input.IsMouseButtonDown(MouseButton.Left) && armaActual == Armas.metralleta && !metralletaAtascada)
        {
            tempoMetralleta -= (float)Game.UpdateTime.Elapsed.TotalSeconds;
            if (tempoMetralleta > 0)
                Disparar();
            else
                AtascarMetralleta();
        }

        if (!Input.IsMouseButtonDown(MouseButton.Left))
            EnfriarMetralleta();

        // Melé
        if (Input.IsKeyPressed(Keys.E) || Input.IsMouseButtonPressed(MouseButton.Right))
            Atacar();

        // Cura
        if (Input.IsKeyPressed(Keys.F))
            Curar();

        // Cambio armas
        if (Input.IsKeyPressed(Keys.D1) || Input.IsKeyPressed(Keys.NumPad1))
            CambiarArma(Armas.pistola);
        if (Input.IsKeyPressed(Keys.D2) || Input.IsKeyPressed(Keys.NumPad2))
            CambiarArma(Armas.escopeta);
        if (Input.IsKeyPressed(Keys.D3) || Input.IsKeyPressed(Keys.NumPad3))
            CambiarArma(Armas.metralleta);
        if (Input.IsKeyPressed(Keys.D4) || Input.IsKeyPressed(Keys.NumPad4))
            CambiarArma(Armas.rifle);

        // Debug
        DebugText.Print(armaActual.ToString(), new Int2(x: 20, y: 60));
        DebugText.Print(metralletaAtascada.ToString(), new Int2(x: 20, y: 80));
        DebugText.Print(tempoMetralleta.ToString(), new Int2(x: 20, y: 100));
        DebugText.Print(últimoDisparo.ToString(), new Int2(x: 20, y: 120));
        DebugText.Print(Game.UpdateTime.Total.TotalSeconds.ToString(), new Int2(x: 20, y: 140));
    }

    private void Disparar()
    {
        // Cadencia
        var tiempoDisparo = ObtenerCadencia(armaActual) + últimoDisparo;
        if ((float)Game.UpdateTime.Total.TotalSeconds < tiempoDisparo)
            return;

        // Metralleta
        if (armaActual == Armas.metralleta && metralletaAtascada)
            return;

        últimoDisparo = (float)Game.UpdateTime.Total.TotalSeconds;
        switch (armaActual)
        {
            case Armas.pistola:
                CalcularRayo(0);
                break;
            case Armas.escopeta:
                movimiento.DetenerMovimiento();
                for (int i = 0; i < ObtenerCantidadPerdigones(); i++)
                {
                    CalcularRayo(0.25f);
                }
                break;
            case Armas.metralleta:
                CalcularRayo(0.1f);
                break;
            case Armas.rifle:
                movimiento.DetenerMovimiento();
                CalcularRayo(0);
                break;
        }
    }

    private void CalcularRayo(float imprecisión)
    {
        var aleatorioX = RangoAleatorio(-(imprecisión), imprecisión);
        var aleatorioY = RangoAleatorio(-(imprecisión), imprecisión);
        var aleatorioZ = RangoAleatorio(-(imprecisión), imprecisión);
        var aleatorio = new Vector3(aleatorioX, aleatorioY, aleatorioZ);

        // Distancia máxima de disparo: 1000
        var dirección = cámara.Entity.Transform.WorldMatrix.TranslationVector +
                        (cámara.Entity.Transform.WorldMatrix.Forward + aleatorio) * 1000;

        var resultado = this.GetSimulation().Raycast(cámara.Entity.Transform.WorldMatrix.TranslationVector,
                                                     dirección,
                                                     CollisionFilterGroups.DefaultFilter);

        if (resultado.Succeeded && resultado.Collider != null)
        {
            var enemigo = resultado.Collider.Entity.Get<ControladorEnemigo>();
            if (enemigo == null)
            {
                // PENDIENTE: usar piscina
                // PENDIENTE: efecto
                // Marca balazo
                var marca = prefabMarca.Instantiate()[0];
                marca.Transform.Position = resultado.Point;
                Entity.Scene.Entities.Add(marca);
                return;
            }

            // PENDIENTE: efecto
            // Daño segun distancia
            var distancia = Vector3.Distance(cámara.Entity.Transform.WorldMatrix.TranslationVector, resultado.Point);
            var reducción = 0f;
            if(distancia > ObtenerDistanciaMáxima(armaActual))
                reducción = (distancia - ObtenerDistanciaMáxima(armaActual)) * 0.5f;

            // Rifle daña según distancia
            var dañoFinal = 0f;
            if(armaActual != Armas.rifle)
                dañoFinal = ObtenerDaño(armaActual) - reducción;
            else
                dañoFinal = ObtenerDaño(armaActual) + reducción;

            // Daña enemigo
            dañoFinal = MathUtil.Clamp(dañoFinal, dañoMínimo, dañoMáximo);
            enemigo.RecibirDaño(dañoFinal);
        }
    }

    private void Atacar()
    {
        // Distancia máxima melé: 2
        var dirección = cámara.Entity.Transform.WorldMatrix.TranslationVector +
                        cámara.Entity.Transform.WorldMatrix.Forward * 2;

        var resultado = this.GetSimulation().Raycast(cámara.Entity.Transform.WorldMatrix.TranslationVector,
                                                     dirección,
                                                     CollisionFilterGroups.DefaultFilter);

        if (resultado.Succeeded && resultado.Collider != null)
        {
            var enemigo = resultado.Collider.Entity.Get<ControladorEnemigo>();
            if (enemigo == null)
            {
                // PENDIENTE: usar piscina
                // PENDIENTE: efecto
                // Marca balazo
                var marca = prefabMarca.Instantiate()[0];
                marca.Transform.Position = resultado.Point;
                Entity.Scene.Entities.Add(marca);
                return;
            }

            // PENDIENTE: efecto
            // Daña enemigo
            enemigo.RecibirDaño(ObtenerDistanciaMáxima(Armas.espada));
        }
    }

    private void Curar()
    {

    }

    private float ObtenerDaño(Armas arma)
    {
        switch (arma)
        {
            case Armas.espada:
                return 40;
            case Armas.pistola:
                return 20;
            case Armas.escopeta:
                return 10;
            case Armas.metralleta:
                return 5;
            case Armas.rifle:
                return 40;
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
            case Armas.pistola:
                return 10;
            case Armas.escopeta:
                return 5;
            case Armas.metralleta:
                return 10;
            case Armas.rifle:
                return 10;
        }
    }

    private float ObtenerCadencia(Armas arma)
    {
        switch (arma)
        {
            case Armas.espada:
                return 0.2f;
            case Armas.pistola:
                return 0.2f;
            case Armas.escopeta:
                return 0.8f;
            case Armas.metralleta:
                return 0.05f;
            case Armas.rifle:
                return 2f;
            default:
                return 0;
        }
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

    private void CambiarArma(Armas nuevaArma)
    {
        ApagarArmas();
        armaActual = nuevaArma;

        switch (armaActual)
        {
            case Armas.pistola:
                espada.Entity.Get<ModelComponent>().Enabled = true;
                pistola.Entity.Get<ModelComponent>().Enabled = true;
                break;
            case Armas.escopeta:
                escopeta.Entity.Get<ModelComponent>().Enabled = true;
                break;
            case Armas.metralleta:
                espada.Entity.Get<ModelComponent>().Enabled = true;
                metralleta.Entity.Get<ModelComponent>().Enabled = true;
                break;
            case Armas.rifle:
                rife.Entity.Get<ModelComponent>().Enabled = true;
                break;
        }
    }

    private void ApagarArmas()
    {
        espada.Entity.Get<ModelComponent>().Enabled = false;
        pistola.Entity.Get<ModelComponent>().Enabled = false;
        escopeta.Entity.Get<ModelComponent>().Enabled = false;
        metralleta.Entity.Get<ModelComponent>().Enabled = false;
        rife.Entity.Get<ModelComponent>().Enabled = false;
    }
}
