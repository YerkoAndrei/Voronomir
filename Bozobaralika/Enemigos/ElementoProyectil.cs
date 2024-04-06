using System.Collections.Generic;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;

public class ElementoProyectil: AsyncScript, IDañable
{
    public ModelComponent modelo;
    public List<RigidbodyComponent> cuerpos { get; set; }

    private PhysicsComponent[] disparador;
    private RigidbodyComponent cuerpo;
    private Vector3 altura;
    private Vector3 dirección;
    private float velocidadSeguimiento;
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
            if (cuerpo.Enabled)
            {
                var colisión = await cuerpo.NewCollision();

                if (colisión.ColliderA.CollisionGroup == CollisionFilterGroups.StaticFilter ||
                    colisión.ColliderB.CollisionGroup == CollisionFilterGroups.StaticFilter ||
                    colisión.ColliderA.CollisionGroup == CollisionFilterGroups.SensorTrigger ||
                    colisión.ColliderB.CollisionGroup == CollisionFilterGroups.SensorTrigger)
                {
                    // PENDIENTE: efecto disparo enemigo
                    Apagar();
                    continue;
                }

                var dañable = colisión.ColliderA.Entity.Get<ElementoDañable>();
                if (dañable == null)
                    dañable = colisión.ColliderB.Entity.Get<ElementoDañable>();

                if (dañable == null)
                    continue;

                if (colisión.ColliderA.CollisionGroup == CollisionFilterGroups.CharacterFilter ||
                    colisión.ColliderB.CollisionGroup == CollisionFilterGroups.CharacterFilter )
                {
                    // Daña jugador
                    dañable.RecibirDaño(daño);
                    Apagar();
                }
                else if (colisión.ColliderA.CollisionGroup == CollisionFilterGroups.KinematicFilter ||
                         colisión.ColliderB.CollisionGroup == CollisionFilterGroups.KinematicFilter)
                {
                    // No se daña a sí mismo
                    var seToca = false;
                    for(int i=0; i< disparador.Length; i++)
                    {
                        if (colisión.ColliderA == disparador[i] || colisión.ColliderB == disparador[i])
                            seToca = true;
                    }

                    // Daña enemigo un 25%
                    if (!seToca)
                    {
                        dañable.RecibirDaño(daño * 0.25f);
                        Apagar();
                    }
                }
            }
            await Script.NextFrame();
        }
    }

    private void Apagar()
    {
        cuerpo.LinearVelocity = Vector3.Zero;
        modelo.Enabled = false;
        cuerpo.IsKinematic = true;
        cuerpo.Enabled = false;
    }

    public void Iniciar(float _daño, float _velocidad, float _velocidadSeguimiento, Quaternion _rotación, Vector3 _altura, Vector3 _posición, PhysicsComponent[] _disparador)
    {
        Apagar();

        Entity.Transform.Position = _posición;
        Entity.Transform.Rotation = _rotación;

        vida = _daño;
        daño = _daño;
        disparador = _disparador;
        velocidad = _velocidad;
        altura = _altura;
        velocidadSeguimiento = _velocidadSeguimiento;

        // Dirección
        cuerpo.IsKinematic = false;
        cuerpo.UpdatePhysicsTransformation();
        cuerpo.LinearVelocity = Entity.Transform.WorldMatrix.Forward * velocidad;

        modelo.Enabled = true;
        cuerpo.Enabled = true;

        // Ningún proyectil dura más de 10 segundos
        tempo = 10f;
        ContarVida();
    }

    private async void ContarVida()
    {
        while (cuerpo.Enabled)
        {
            // Sigue jugador
            if (velocidadSeguimiento > 0)
            {
                dirección = Vector3.Normalize(Entity.Transform.WorldMatrix.TranslationVector - (ControladorPartida.ObtenerPosiciónJugador() + altura));
                Entity.Transform.Rotation = Quaternion.Lerp(Entity.Transform.Rotation, Quaternion.LookRotation(dirección, Vector3.UnitY), velocidadSeguimiento * (float)Game.UpdateTime.Elapsed.TotalSeconds);

                Entity.Transform.UpdateWorldMatrix();
                cuerpo.UpdatePhysicsTransformation();
                cuerpo.LinearVelocity = Entity.Transform.WorldMatrix.Forward * velocidad;
            }

            tempo -= (float)Game.UpdateTime.Elapsed.TotalSeconds;
            if (tempo <= 0)
                Apagar();

            await Task.Delay(1);
        }
    }

    public void RecibirDaño(float daño)
    {
        vida -= daño;
        if (vida <= 0)
            Apagar();
    }
}
