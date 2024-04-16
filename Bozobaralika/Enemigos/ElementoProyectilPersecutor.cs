using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;
using static Utilidades;
using static Constantes;

public class ElementoProyectilPersecutor: AsyncScript, IProyectil, IDañable
{
    public ModelComponent modelo;
    public RigidbodyComponent cuerpoDañable;
    public List<RigidbodyComponent> cuerpos { get; set; }

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
                dañable.RecibirDaño(daño);
                Destruir();
            }
            else if (TocaEnemigo(colisión))
            {
                // No daña a su mismo tipo
                if (disparador == dañable.controlador.Get<ControladorEnemigo>().enemigo)
                    continue;

                // Daña enemigos un 50%
                dañable.RecibirDaño(daño * 0.5f);
                Destruir();
            }
            await Script.NextFrame();
        }
    }

    public void Destruir()
    {
        // PENDIENTE: efecto disparo enemigo
        Apagar();
    }

    private void Apagar()
    {
        cuerpo.LinearVelocity = Vector3.Zero;
        modelo.Enabled = false;
        cuerpo.IsKinematic = true;
        cuerpo.Enabled = false;
        cuerpoDañable.Enabled = false;
    }

    public void AsignarImpacto(Action<Vector3, Vector3, bool> _iniciarImpacto)
    {

    }

    public void IniciarPersecutor(float _velocidadRotación, Vector3 _altura)
    {
        altura = _altura;
        velocidadRotación = _velocidadRotación;
    }

    public void Iniciar(float _daño, float _velocidad, Quaternion _rotación, Vector3 _posición, Enemigos _disparador)
    {
        Destruir();

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
                Entity.Transform.Rotation = Quaternion.Lerp(Entity.Transform.Rotation, Quaternion.LookRotation(dirección, Vector3.UnitY), velocidadRotación * SistemaAnimación.TiempoTranscurrido());

                Entity.Transform.UpdateWorldMatrix();
                cuerpo.UpdatePhysicsTransformation();
                cuerpo.LinearVelocity = Entity.Transform.WorldMatrix.Forward * velocidad;

                // Mientras más tiempo, más se acerca al jugador 
                velocidadRotación += 0.01f;
            }

            tempo -= SistemaAnimación.TiempoTranscurrido();
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
}
