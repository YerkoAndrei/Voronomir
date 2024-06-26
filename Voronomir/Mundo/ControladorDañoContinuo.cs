﻿using System.Linq;
using System.Threading.Tasks;
using Stride.Engine;
using Stride.Physics;

namespace Voronomir;

public class ControladorDañoContinuo : AsyncScript
{
    public float daño;

    private PhysicsComponent cuerpo;

    public override async Task Execute()
    {
        cuerpo = Entity.Get<PhysicsComponent>();

        while (Game.IsRunning)
        {
            await cuerpo.NewCollision();

            if (cuerpo.Enabled)
                IntentarDañar();

            await Script.NextFrame();
        }
    }

    private async void IntentarDañar()
    {
        while(cuerpo.Enabled && cuerpo.Collisions.Count > 0)
        {
            Dañar(cuerpo.Collisions.ToArray());
            await Task.Delay(500);
        }
    }

    private void Dañar(Collision[] colisiones)
    {
        // También daña enemigos, pero reciben menos daño por el multiplicador de los pies
        foreach (var colisión in colisiones)
        {
            var dañable = colisión.ColliderA.Entity.Get<ElementoDañable>();
            if (dañable == null)
                dañable = colisión.ColliderB.Entity.Get<ElementoDañable>();

            if (dañable == null)
                continue;

            dañable.RecibirDaño(daño);
        }
    }
}
