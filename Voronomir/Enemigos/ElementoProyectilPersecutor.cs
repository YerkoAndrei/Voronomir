using System.Collections.Generic;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Particles.Components;
using Stride.Engine;
using Stride.Physics;

namespace Voronomir;
using static Utilidades;
using static Constantes;

public class ElementoProyectilPersecutor: AsyncScript, IProyectil, IDañable
{
    public ModelComponent modelo;
    public RigidbodyComponent cuerpoDañable;
    public ParticleSystemComponent partículas;

    private Enemigos disparador;
    private RigidbodyComponent cuerpo;
    private Vector3 altura;
    private Vector3 dirección;
    private float velocidadRotación;
    private float velocidad;
    private float tempo;
    private float daño;
    private float vida;

    public override async Task Execute()
    {
        cuerpo = Entity.Get<RigidbodyComponent>();
        Apagar();

        while (Game.IsRunning)
        {
            var colisión = await cuerpo.NewCollision();
            if (!cuerpo.Enabled)
                continue;

            if (TocaEntorno(colisión))
            {
                Destruir();
                continue;
            }

            var dañable = colisión.ColliderA.Entity.Get<ElementoDañable>();
            if (dañable == null)
                dañable = colisión.ColliderB.Entity.Get<ElementoDañable>();

            if (dañable == null)
                continue;

            if (TocaJugador(colisión))
            {
                // Daña jugador
                dañable.RecibirDaño(daño, false);
                Destruir();
            }
            else if (TocaEnemigo(colisión))
            {
                // No daña a su mismo tipo
                if (disparador == dañable.controlador.Get<ControladorEnemigo>().enemigo)
                    continue;

                // Daña enemigos un 50%
                dañable.RecibirDaño(daño * 0.5f, false);
                Destruir();
            }
            await Script.NextFrame();
        }
    }

    public void Apagar()
    {
        cuerpo.LinearVelocity = Vector3.Zero;
        modelo.Enabled = false;
        cuerpo.IsKinematic = true;
        cuerpo.Enabled = false;
        cuerpoDañable.Enabled = false;

        if (partículas != null)
            partículas.Enabled = false;
    }

    public void Destruir()
    {
        ControladorCofres.IniciarEfectoEnemigo(disparador, Entity.Transform.WorldMatrix.TranslationVector, Vector3.Zero);
        Apagar();
    }

    public void IniciarPersecutor(float _velocidadRotación, Vector3 _altura)
    {
        altura = _altura;
        velocidadRotación = _velocidadRotación;
    }

    public void Iniciar(float _daño, float _velocidad, Quaternion _rotación, Vector3 _posición, Enemigos _disparador)
    {
        Apagar();

        Entity.Transform.Position = _posición;
        Entity.Transform.Rotation = _rotación;

        vida = _daño;
        daño = _daño;
        velocidad = _velocidad;
        disparador = _disparador;

        // Dirección
        cuerpo.IsKinematic = false;
        cuerpo.UpdatePhysicsTransformation();
        cuerpo.LinearVelocity = Entity.Transform.WorldMatrix.Forward * velocidad;

        modelo.Enabled = true;
        cuerpo.Enabled = true;
        cuerpoDañable.Enabled = true;

        if (partículas != null)
        {
            partículas.Enabled = true;
            partículas.ParticleSystem.ResetSimulation();
        }

        // Proyectiles persecutores duran 30 segundos
        tempo = 30f;
        ContarVida();
    }

    private async void ContarVida()
    {
        while (cuerpo.Enabled)
        {
            // Sigue jugador
            if (velocidadRotación > 0)
            {
                dirección = Vector3.Normalize(Entity.Transform.WorldMatrix.TranslationVector - (ControladorPartida.ObtenerPosiciónJugador() + altura));
                Entity.Transform.Rotation = Quaternion.Lerp(Entity.Transform.Rotation, Quaternion.LookRotation(dirección, Vector3.UnitY), velocidadRotación * (float)Game.UpdateTime.WarpElapsed.TotalSeconds);

                Entity.Transform.UpdateWorldMatrix();
                cuerpo.UpdatePhysicsTransformation();
                cuerpo.LinearVelocity = Entity.Transform.WorldMatrix.Forward * velocidad;

                // Mientras más tiempo, más se acerca al jugador 
                velocidadRotación += 0.01f;
            }

            tempo -= (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            if (tempo <= 0)
                Destruir();

            await Task.Delay(1);
        }
    }

    public void RecibirDaño(float daño)
    {
        vida -= daño;
        if (vida <= 0)
            Destruir();
    }

    public void Empujar(Vector3 dirección)
    {

    }
}
