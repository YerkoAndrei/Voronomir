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

    private Arma armaActual;

    public override void Start()
    {
        ApagarArmas();
        CambiarArma(Arma.pistola);
    }

    public override void Update()
    {
        if (Input.IsMouseButtonPressed(MouseButton.Left))
            Disparar();

        if (Input.IsKeyPressed(Keys.E) || Input.IsMouseButtonPressed(MouseButton.Right))
            Atacar();

        if (Input.IsKeyPressed(Keys.F))
            Curar();

        if (Input.IsKeyPressed(Keys.D1) || Input.IsKeyPressed(Keys.NumPad1))
            CambiarArma(Arma.pistola);
        if (Input.IsKeyPressed(Keys.D2) || Input.IsKeyPressed(Keys.NumPad2))
            CambiarArma(Arma.escopeta);
        if (Input.IsKeyPressed(Keys.D3) || Input.IsKeyPressed(Keys.NumPad3))
            CambiarArma(Arma.metralleta);
        if (Input.IsKeyPressed(Keys.D4) || Input.IsKeyPressed(Keys.NumPad4))
            CambiarArma(Arma.rifle);

        // Debug
        DebugText.Print(armaActual.ToString(), new Int2(x: 20, y: 60));
    }

    private void Disparar()
    {
        switch (armaActual)
        {
            case Arma.pistola:
                CalcularRayo(0);
                break;
            case Arma.escopeta:
                movimiento.DetenerMovimiento();
                for (int i = 0; i < ObtenerCantidadPerdigones(); i++)
                {
                    CalcularRayo(0.2f);
                }
                break;
            case Arma.metralleta:
                CalcularRayo(0.1f);
                break;
            case Arma.rifle:
                movimiento.DetenerMovimiento();
                CalcularRayo(0);
                break;
        }
    }

    private void CalcularRayo(float imprecisión)
    {
        var aleatorioX = RangoAleatorio(-(imprecisión), imprecisión);
        var aleatorioY = RangoAleatorio(-(imprecisión), imprecisión);
        var aleatorio = new Vector3(aleatorioX, aleatorioY, 0);

        // Distancia máxima de cálculo: 1000
        var dirección = cámara.Entity.Transform.WorldMatrix.TranslationVector +
                        (cámara.Entity.Transform.WorldMatrix.Forward + aleatorio) * 1000;

        var resultado = this.GetSimulation().Raycast(cámara.Entity.Transform.WorldMatrix.TranslationVector,
                                                     dirección,
                                                     CollisionFilterGroups.DefaultFilter);

        if (resultado.Succeeded && resultado.Collider != null)
        {
            // Distancia
            var distancia = Vector3.Distance(cámara.Entity.Transform.WorldMatrix.TranslationVector, resultado.Point);

            // dañar segun distancia
            var dañoFinal = ObtenerDaño(armaActual);// * distancia;

            //resultado.Collider.Entity.Get<Enemigo>().Dañar(dañoFinal);
            //Log.Warning(resultado.Collider.Entity.Name + " - " + distancia.ToString());

            // PENDIENTE: usar piscina
            // Marca balazo
            var marca = prefabMarca.Instantiate()[0];
            marca.Transform.Position = resultado.Point;
            Entity.Scene.Entities.Add(marca);
        }
    }

    private void Atacar()
    {
        // Distancia máxima de cálculo: 2
        var dirección = cámara.Entity.Transform.WorldMatrix.TranslationVector +
                        cámara.Entity.Transform.WorldMatrix.Forward * 2;

        var resultado = this.GetSimulation().Raycast(cámara.Entity.Transform.WorldMatrix.TranslationVector,
                                                     dirección,
                                                     CollisionFilterGroups.DefaultFilter);

        if (resultado.Succeeded && resultado.Collider != null)
        {
            var dañoFinal = ObtenerDaño(Arma.espada);

            //resultado.Collider.Entity.Get<Enemigo>().Dañar(dañoFinal);

            // Marca ataque
            var marca = prefabMarca.Instantiate()[0];
            marca.Transform.Position = resultado.Point;
            Entity.Scene.Entities.Add(marca);
        }
    }

    private void Curar()
    {

    }

    private float ObtenerDaño(Arma arma)
    {
        switch (armaActual)
        {
            case Arma.espada:
                return 4;
            case Arma.pistola:
                return 1;
            case Arma.escopeta:
                return 1;
            case Arma.metralleta:
                return 2;
            case Arma.rifle:
                return 4;
            default:
                return 0;
        }
    }

    private int ObtenerCantidadPerdigones()
    {
        // PENDIENTE: mejoras
        return 10;
    }

    private void CambiarArma(Arma nuevaArma)
    {
        ApagarArmas();
        armaActual = nuevaArma;

        switch (armaActual)
        {
            case Arma.pistola:
                espada.Entity.Get<ModelComponent>().Enabled = true;
                pistola.Entity.Get<ModelComponent>().Enabled = true;
                break;
            case Arma.escopeta:
                escopeta.Entity.Get<ModelComponent>().Enabled = true;
                break;
            case Arma.metralleta:
                espada.Entity.Get<ModelComponent>().Enabled = true;
                metralleta.Entity.Get<ModelComponent>().Enabled = true;
                break;
            case Arma.rifle:
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
