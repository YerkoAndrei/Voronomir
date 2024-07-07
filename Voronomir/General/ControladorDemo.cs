using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;
using Stride.Navigation;

namespace Voronomir;
using static Utilidades;

public class ControladorDemo : AsyncScript
{
    public Prefab zombi;
    public Prefab lancero;
    public Prefab carnicero;
    public Prefab pulga;
    public Prefab babosa;
    public Prefab araña;
    public Prefab dron;
    public Prefab robot;
    public Prefab cerebro;

    private Vector3[] posiciones;
    private int posiciónActual;

    public override async Task Execute()
    {
        posiciones = new Vector3[9]
        {
            new Vector3 (0, 0, 0),
            new Vector3 (0, 0, 0.5f),
            new Vector3 (0, 0, -0.5f),
            new Vector3 (0.5f, 0, 0),
            new Vector3 (0.5f, 0, 0.5f),
            new Vector3 (0.5f, 0, -0.5f),
            new Vector3 (-0.5f, 0, 0),
            new Vector3 (-0.5f, 0, 0.5f),
            new Vector3 (-0.5f, 0, -0.5f)
        };

        //var jugador = Entity.Scene.Entities.Where(o => o.Get<ControladorJugador>() != null).FirstOrDefault().Get<ControladorJugador>();

        while (Game.IsRunning)
        {
            /*
            // Armas
            DebugText.Print(jugador.armas.metralletaAtascada.ToString(), new Int2(x: 20, y: 20));
            DebugText.Print(jugador.armas.tempoMetralleta.ToString(), new Int2(x: 20, y: 40));
            */

            if (Input.IsKeyPressed(Keys.F1))
            {
                var enemigo = zombi.Instantiate()[0];
                Inicializar(enemigo);
            }
            if (Input.IsKeyPressed(Keys.F2))
            {
                var enemigo = lancero.Instantiate()[0];
                Inicializar(enemigo);
            }
            if (Input.IsKeyPressed(Keys.F3))
            {
                var enemigo = carnicero.Instantiate()[0];
                Inicializar(enemigo);
            }
            if (Input.IsKeyPressed(Keys.F4))
            {
                var enemigo = pulga.Instantiate()[0];
                Inicializar(enemigo);
            }
            if (Input.IsKeyPressed(Keys.F5))
            {
                var enemigo = babosa.Instantiate()[0];
                Inicializar(enemigo);
            }
            if (Input.IsKeyPressed(Keys.F6))
            {
                var enemigo = araña.Instantiate()[0];
                Inicializar(enemigo);
            }
            if (Input.IsKeyPressed(Keys.F7))
            {
                var enemigo = dron.Instantiate()[0];
                Inicializar(enemigo);
            }
            if (Input.IsKeyPressed(Keys.F8))
            {
                var enemigo = robot.Instantiate()[0];
                Inicializar(enemigo);                
            }
            if (Input.IsKeyPressed(Keys.F9))
            {
                var enemigo = cerebro.Instantiate()[0];
                Inicializar(enemigo);
            }
            await Script.NextFrame();
        }
    }

    private void Inicializar(Entity entidad)
    {
        posiciónActual++;
        if (posiciónActual >= 9)
            posiciónActual = 0;

        entidad.Transform.Position = posiciones[posiciónActual];
        entidad.Get<NavigationComponent>().NavigationMesh = Entity.Get<ControladorPartida>().navegación;
        Entity.Scene.Entities.Add(entidad);
        Activar(entidad);
    }

    private async void Activar(Entity entidad)
    {
        await Task.Delay(2);
        var temp = ObtenerInterfaz<IActivable>(entidad);
        temp.Activar();
    }
}
