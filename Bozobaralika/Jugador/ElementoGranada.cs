using System;
using System.Linq;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;
using static Utilidades;
using static Constantes;

public class ElementoGranada : AsyncScript, IProyectil
{
    public ModelComponent modelo;
    public TransformComponent cola;

    private RigidbodyComponent cuerpo;
    private Action<Vector3, Quaternion, bool> iniciarImpacto;
    private float velocidad;
    private float tempo;

    public override async Task Execute()
    {
        cuerpo = Entity.Get<RigidbodyComponent>();
        Destruir();

        while (Game.IsRunning)
        {
            var colisión = await cuerpo.NewCollision();
            if (!cuerpo.Enabled)
                continue;

            CrearImpacto(colisión);
            Destruir();
            await Script.NextFrame();
        }
    }

    public void Destruir()
    {
        cuerpo.LinearVelocity = Vector3.Zero;
        modelo.Enabled = false;
        cuerpo.IsKinematic = true;
        cuerpo.Enabled = false;
    }

    public void AsignarImpacto(Action<Vector3, Quaternion, bool> _iniciarImpacto)
    {
        iniciarImpacto = _iniciarImpacto;
    }

    public void IniciarPersecutor(float _velocidadRotación, Vector3 _altura)
    {

    }

    public void Iniciar(float _daño, float _velocidad, Quaternion _rotación, Vector3 _posición, Enemigos _disparador)
    {
        Destruir();

        Entity.Transform.Position = _posición;
        Entity.Transform.Rotation = _rotación;

        velocidad = _velocidad;

        // Dirección
        cuerpo.IsKinematic = false;
        cuerpo.UpdatePhysicsTransformation();
        cuerpo.LinearVelocity = Entity.Transform.WorldMatrix.Forward * velocidad;

        modelo.Enabled = true;
        cuerpo.Enabled = true;

        // Granadas duran 10 segundos
        tempo = 10f;
        ContarVida();
    }

    private async void ContarVida()
    {
        while (cuerpo.Enabled)
        {
            tempo -= (float)Game.UpdateTime.Elapsed.TotalSeconds;
            if (tempo <= 0)
                Destruir();

            await Task.Delay(1);
        }
    }

    private void CrearImpacto(Collision colisión)
    {
        // Crea pequeño rayo para crear marca
        var dirección = cola.WorldMatrix.TranslationVector + cola.Entity.Transform.WorldMatrix.Forward;
        var resultado = this.GetSimulation().Raycast(cola.WorldMatrix.TranslationVector,
                                                     dirección,
                                                     CollisionFilterGroups.DefaultFilter,
                                                     colisionesMarca);
        if (resultado.Succeeded)
            iniciarImpacto.Invoke(resultado.Point, Quaternion.LookRotation(resultado.Normal, resultado.Point), false);
        else
            iniciarImpacto.Invoke(colisión.Contacts.ToArray()[0].PositionOnA, Quaternion.Identity, false);
    }
}
