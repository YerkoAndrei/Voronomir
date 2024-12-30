using System.Collections.Generic;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Voronomir;
using static Utilidades;
using static Constantes;

public class ControladorBotón : AsyncScript
{
    public Llaves llave;
    public List<Entity> activables = new List<Entity> { };
    public TransformComponent modelo;

    private PhysicsComponent cuerpo;
    private Vector3 posiciónActivado;
    private IActivable[] interfaces;

    public override async Task Execute()
    {
        cuerpo = Entity.Get<PhysicsComponent>();
        posiciónActivado = modelo.Position + new Vector3(0, -0.3f, 0);

        interfaces = new IActivable[activables.Count];
        for (int i = 0; i < activables.Count; i++)
        {
            interfaces[i] = ObtenerInterfaz<IActivable>(activables[i]);
        }

        while (Game.IsRunning)
        {
            var colisión = await cuerpo.NewCollision();
            var jugador = RetornaJugador(colisión);

            if (jugador != null && cuerpo.Enabled)
                ActivarPorJugador(jugador.ObtenerLlave(llave));

            await Script.NextFrame();            
        }
    }

    private void ActivarPorJugador(bool posible)
    {
        // Llave necesaria
        if (!posible)
        {
            ControladorJuego.MostrarMensaje(SistemaTraducción.ObtenerTraducción("necesita") + " " + SistemaTraducción.ObtenerTraducción(llave.ToString()));
            SistemaSonidos.SonarCerrado();
            return;
        }

        Activar();
    }

    public void ActivarPorDisparo()
    {
        // Disparo no reemplaza llave
        if (llave != Llaves.nada || !cuerpo.Enabled)
            return;

        Activar();
    }

    private void Activar()
    {
        cuerpo.Enabled = false;
        AnimarActivación();

        foreach (var activable in interfaces)
        {
            activable.Activar();
        }
        SistemaSonidos.SonarBotón();
    }

    private async void AnimarActivación()
    {
        var inicio = modelo.Position;
        float duración = 0.25f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            modelo.Position = Vector3.Lerp(inicio, posiciónActivado, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }
    }
}
