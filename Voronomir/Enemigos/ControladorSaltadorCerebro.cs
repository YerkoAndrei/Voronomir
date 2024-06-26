﻿﻿using System.Linq;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace Voronomir;

public class ControladorSaltadorCerebro : AsyncScript
{
    public float fuerza;
    public ControladorPersecusión persecutor;

    private Vector3 dirección;

    public override async Task Execute()
    {
        var cuerpo = Entity.Get<PhysicsComponent>();

        while (Game.IsRunning)
        {
            await cuerpo.NewCollision();

            if(cuerpo.Enabled)
                Saltar(cuerpo.Collisions.ToArray());

            await Script.NextFrame();
        }
    }

    private void Saltar(Collision[] colisiones)
    {
        foreach (var colisión in colisiones)
        {
            var cuerpo = colisión.ColliderA.Entity.Get<CharacterComponent>();
            if (cuerpo == null)
                cuerpo = colisión.ColliderB.Entity.Get<CharacterComponent>();

            if (cuerpo == null)
                continue;

            dirección = Entity.Transform.WorldMatrix.Backward * 20;
            dirección.Y = fuerza;

            persecutor.Atacar();
            cuerpo.Jump(dirección);

            if (cuerpo.Entity.Get<ControladorJugador>() != null)
                SistemaSonidos.SonarSaltador();
        }
    }
}