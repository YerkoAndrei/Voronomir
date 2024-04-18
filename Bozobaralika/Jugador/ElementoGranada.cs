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
    private float dañoImpacto;
    private float velocidad;
    private float tempo;

    public override async Task Execute()
    {
        cuerpo = Entity.Get<RigidbodyComponent>();
        Destruir();

        while (Game.IsRunning)
        {
            await cuerpo.NewCollision();
            if (cuerpo.Enabled)
            {
                CrearImpacto();
                Destruir();
            }

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

    public void AsignarImpacto(Action<Vector3, Vector3> _iniciarImpacto)
    {

    }

    public void IniciarPersecutor(float _velocidadRotación, Vector3 _altura)
    {

    }

    public void Iniciar(float _daño, float _velocidad, Quaternion _rotación, Vector3 _posición, Enemigos _disparador)
    {
        Destruir();

        dañoImpacto = _daño;
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
            tempo -= (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
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
            ControladorEfectos.IniciarImpactoGranada(resultado.Point, resultado.Normal, dañoImpacto);
        else
            ControladorEfectos.IniciarImpactoGranada(Entity.Transform.WorldMatrix.TranslationVector, Vector3.Zero, dañoImpacto);
    }
}
