using System.Linq;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;
using static Constantes;

public class Depurador : AsyncScript
{
    public override async Task Execute()
    {
        var jugador = Entity.Scene.Entities.Where(o => o.Get<ControladorJugador>() != null).FirstOrDefault().Get<ControladorJugador>();
        var animaciónArmas = Entity.Scene.Entities.Where(o => o.Get<ControladorArmas>() != null).FirstOrDefault().Get<ControladorArmas>();

        while (Game.IsRunning)
        {/*
            // Jugador
            DebugText.Print(jugador.vida + "/" + jugador.vidaMax, new Int2(x: 20, y: 20));

            // Movimiento
            DebugText.Print(jugador.movimiento.cuerpo.LinearVelocity.Lenght().ToString(), new Int2(x: 20, y: 40));
            DebugText.Print(jugador.movimiento.aceleración.ToString(), new Int2(x: 20, y: 60));

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
            }*/            
            await Script.NextFrame();
        }
    }
}
