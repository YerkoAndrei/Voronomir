﻿using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;

public class ElementoVeneno : StartupScript
{
    public float tiempoVida;

    private PhysicsComponent cuerpo;
    private Vector3 escalaInicial;

    public override void Start()
    {
        cuerpo = Entity.Get<PhysicsComponent>();
        escalaInicial = Entity.Transform.Scale;
        cuerpo.Enabled = true;

        ContarVida();
    }

    private async void ContarVida()
    {
        float tiempoLerp = 0;
        float tiempo = 0;

        await Task.Delay(400);
        while (tiempoLerp < tiempoVida)
        {
            tiempo = tiempoLerp / tiempoVida;
            Entity.Transform.Scale = Vector3.Lerp(escalaInicial, Vector3.Zero, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Task.Delay(1);
        }

        cuerpo.Enabled = false;
        Entity.Scene.Entities.Remove(Entity);
    }
}
