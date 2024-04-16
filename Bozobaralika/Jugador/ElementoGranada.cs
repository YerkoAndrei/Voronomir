using System;
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
    private Action<Vector3, Vector3, bool> iniciarImpacto;
    private float velocidad;
    private float tempo;

    public override async Task Execute()
    {
        cuerpo = Entity.Get<RigidbodyComponent>();
        Destruir();

        while (Game.IsRunning)
        {
            await cuerpo.NewCollision();
            if (!cuerpo.Enabled)
                continue;

            CrearImpacto();
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

    public void AsignarImpacto(Action<Vector3, Vector3, bool> _iniciarImpacto)
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

        // Granadas duran 10 segundos
        tempo = 10f;
        ContarVida();
        cuerpo.Enabled = true;
    }

    private async void ContarVida()
    {
        while (cuerpo.Enabled)
        {
            tempo -= SistemaAnimación.TiempoTranscurrido();
            if (tempo <= 0)
                Destruir();

            await Task.Delay(1);
        }
    }

    private void CrearImpacto()
    {
        // Rayo para crear marca
        var dirección = cola.WorldMatrix.TranslationVector + cola.WorldMatrix.Forward * 2;
        var resultado = this.GetSimulation().Raycast(cola.WorldMatrix.TranslationVector,
                                                     dirección,
                                                     CollisionFilterGroups.DefaultFilter,
                                                     colisionesMarca);
        if (resultado.Succeeded)
            iniciarImpacto.Invoke(resultado.Point, resultado.Normal, false);
        else
            iniciarImpacto.Invoke(Entity.Transform.WorldMatrix.TranslationVector, Vector3.Zero, false);
    }
}
