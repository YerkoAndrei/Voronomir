using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;

namespace Bozobaralika;
using static Utilidades;

public class Depurador : AsyncScript
{
    public Prefab zombi;
    public Prefab lancero;
    public Prefab cerebro;
    public Prefab dron;
    public Prefab babosa;
    public Prefab araña;

    public override async Task Execute()
    {/*
        var jugador = Entity.Scene.Entities.Where(o => o.Get<ControladorJugador>() != null).FirstOrDefault().Get<ControladorJugador>();
        var animaciónArmas = Entity.Scene.Entities.Where(o => o.Get<ControladorArmas>() != null).FirstOrDefault().Get<ControladorArmas>();
        */

        while (Game.IsRunning)
        {
            if (Input.IsKeyPressed(Keys.T))
            {
                var enemigo = zombi.Instantiate()[0];
                Entity.Scene.Entities.Add(enemigo);
                Activar(enemigo);
            }
            if (Input.IsKeyPressed(Keys.Y))
            {
                var enemigo = lancero.Instantiate()[0];
                Entity.Scene.Entities.Add(enemigo);
                Activar(enemigo);
            }
            if (Input.IsKeyPressed(Keys.U))
            {
                var enemigo = cerebro.Instantiate()[0];
                Entity.Scene.Entities.Add(enemigo);
                Activar(enemigo);
            }
            if (Input.IsKeyPressed(Keys.I))
            {
                var enemigo = dron.Instantiate()[0];
                Entity.Scene.Entities.Add(enemigo);
                Activar(enemigo);
            }
            if (Input.IsKeyPressed(Keys.O))
            {
                var enemigo = babosa.Instantiate()[0];
                Entity.Scene.Entities.Add(enemigo);
                Activar(enemigo);
            }
            if (Input.IsKeyPressed(Keys.P))
            {
                var enemigo = araña.Instantiate()[0];
                Entity.Scene.Entities.Add(enemigo);
                Activar(enemigo);
            }

            /*
            // Armas
            DebugText.Print(jugador.armas.metralletaAtascada.ToString(), new Int2(x: 20, y: 80));
            DebugText.Print(jugador.armas.tempoMetralleta.ToString(), new Int2(x: 20, y: 100));

            // Animacion armas
            switch (jugador.armas.armaActual)
            {
                case Armas.espada:
                    DebugText.Print(jugador.armas.animadorEspada.duraciónAnimación.ToString(), new Int2(x: 20, y: 110));
                    DebugText.Print(jugador.armas.animadorEspada.fuerzaAnimación.ToString(), new Int2(x: 20, y: 120));
                    break;
                case Armas.escopeta:
                    DebugText.Print(jugador.armas.animadorEspada.duraciónAnimación.ToString(), new Int2(x: 20, y: 110));
                    DebugText.Print(jugador.armas.animadorEspada.fuerzaAnimación.ToString(), new Int2(x: 20, y: 120));
                    break;
                case Armas.metralleta:
                    DebugText.Print(jugador.armas.animadorEspada.duraciónAnimación.ToString(), new Int2(x: 20, y: 110));
                    DebugText.Print(jugador.armas.animadorEspada.fuerzaAnimación.ToString(), new Int2(x: 20, y: 120));
                    break;
                case Armas.rifle:
                    DebugText.Print(jugador.armas.animadorEspada.duraciónAnimación.ToString(), new Int2(x: 20, y: 110));
                    DebugText.Print(jugador.armas.animadorEspada.fuerzaAnimación.ToString(), new Int2(x: 20, y: 120));
                    break;
            }
            */
            await Script.NextFrame();
        }
    }

    private async void Activar(Entity entidad)
    {
        await Task.Delay(2);
        var temp = ObtenerInterfaz<IActivable>(entidad);
        temp.Activar();
    }
}
