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
    public Prefab prefabMarca;

    public ModelComponent modeloEspada;
    public ModelComponent modeloPistola;
    public ModelComponent modeloEscopeta;
    public ModelComponent modeloMetralleta;
    public ModelComponent modeloRife;
    
    public TransformComponent ejePistola;
    public TransformComponent ejeEscopeta;
    public TransformComponent ejeMetralleta;
    public TransformComponent ejeRife;

    private ControladorMovimiento movimiento;
    private CameraComponent cámara;

    private Armas armaActual;
    private float últimoDisparo;
    private float dañoMínimo;
    private float dañoMáximo;

    private bool cambiandoArma;

    // Metralleta
    private bool metralletaAtascada;
    private float tempoMetralleta;
    private float tiempoMaxMetralleta;
    private float tiempoAtascamientoMetralleta;

    public void Iniciar(ControladorMovimiento _movimiento, CameraComponent _cámara)
    {
        movimiento = _movimiento;
        cámara = _cámara;

        dañoMínimo = 1f;
        dañoMáximo = 60f;

        tiempoMaxMetralleta = 4f;
        tiempoAtascamientoMetralleta = 2f;
        tempoMetralleta = tiempoAtascamientoMetralleta;

        // Arma por defecto
        ApagarArmas();
        armaActual = Armas.pistola;
        modeloEspada.Entity.Get<ModelComponent>().Enabled = true;
        modeloPistola.Entity.Get<ModelComponent>().Enabled = true;
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
        if ((Input.IsKeyPressed(Keys.E) || Input.IsMouseButtonPressed(MouseButton.Right)) && armaActual == Armas.pistola)
            Atacar();

        // Rifle
        if (Input.IsMouseButtonPressed(MouseButton.Right) && armaActual == Armas.rifle)
            AcercarMira(true);

        if (Input.IsMouseButtonReleased(MouseButton.Right) && armaActual == Armas.rifle)
            AcercarMira(false);

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
        if (cambiandoArma)
            return;

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
        if (cambiandoArma)
            return;

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
            enemigo.RecibirDaño(ObtenerDaño(Armas.espada));
        }
    }

    private void Curar()
    {

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
        if(acercar)
            cámara.VerticalFieldOfView = 20;
        else
            cámara.VerticalFieldOfView = 80;
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
                return 0.1f;
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

    private void CambiarArma(Armas nuevaArma)
    {
        if (cambiandoArma || nuevaArma == armaActual)
            return;

        var armaSale = ejePistola;
        var modeloSale = modeloPistola;
        switch (armaActual)
        {
            case Armas.pistola:
                armaSale = ejePistola;
                modeloSale = modeloPistola;
                break;
            case Armas.escopeta:
                armaSale = ejeEscopeta;
                modeloSale = modeloEscopeta;
                break;
            case Armas.metralleta:
                armaSale = ejeMetralleta;
                modeloSale = modeloMetralleta;
                break;
            case Armas.rifle:
                armaSale = ejeRife;
                modeloSale = modeloRife;
                break;
        }

        armaActual = nuevaArma;
        switch (armaActual)
        {
            case Armas.pistola:
                modeloEspada.Entity.Get<ModelComponent>().Enabled = true;
                modeloPistola.Entity.Get<ModelComponent>().Enabled = true;
                AnimarArmas(ejePistola, armaSale, modeloSale);
                break;
            case Armas.escopeta:
                modeloEscopeta.Entity.Get<ModelComponent>().Enabled = true;
                AnimarArmas(ejeEscopeta, armaSale, modeloSale);
                break;
            case Armas.metralleta:
                modeloMetralleta.Entity.Get<ModelComponent>().Enabled = true;
                AnimarArmas(ejeMetralleta, armaSale, modeloSale);
                break;
            case Armas.rifle:
                modeloRife.Entity.Get<ModelComponent>().Enabled = true;
                AnimarArmas(ejeRife, armaSale, modeloSale);
                break;
        }
    }

    private void ApagarArmas()
    {
        modeloEspada.Entity.Get<ModelComponent>().Enabled = false;
        modeloPistola.Entity.Get<ModelComponent>().Enabled = false;
        modeloEscopeta.Entity.Get<ModelComponent>().Enabled = false;
        modeloMetralleta.Entity.Get<ModelComponent>().Enabled = false;
        modeloRife.Entity.Get<ModelComponent>().Enabled = false;
    }

    private async void AnimarArmas(TransformComponent entra, TransformComponent sale, ModelComponent modeloSale)
    {
        cambiandoArma = true;

        var rotaciónCentro = Quaternion.Identity;
        var rotaciónEntra = Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(-90), 0, 0);
        var rotaciónSale = Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(90), 0, 0);

        float duración = 0.1f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);

            entra.Rotation = Quaternion.Lerp(rotaciónEntra, rotaciónCentro, tiempo);
            sale.Rotation = Quaternion.Lerp(rotaciónCentro, rotaciónSale, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Task.Delay(1);
        }
        
        cambiandoArma = false;
        modeloSale.Entity.Get<ModelComponent>().Enabled = false;

        if(modeloSale == modeloPistola)
            modeloEspada.Entity.Get<ModelComponent>().Enabled = false;
    }
}
