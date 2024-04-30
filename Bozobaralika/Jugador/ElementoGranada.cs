using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Particles.Components;
using Stride.Physics;

namespace Bozobaralika;
using static Utilidades;
using static Constantes;

public class ElementoGranada : AsyncScript, IProyectil
{
    public ModelComponent modelo;
    public TransformComponent cola;
    public ParticleSystemComponent partícula;

    private RigidbodyComponent cuerpo;
    private float dañoImpacto;
    private float velocidad;
    private float tempo;

    public override async Task Execute()
    {
        cuerpo = Entity.Get<RigidbodyComponent>();
        Apagar();

        while (Game.IsRunning)
        {
            var colisión = await cuerpo.NewCollision();
            if (cuerpo.Enabled)
            {
                CrearImpacto(colisión);
                Destruir();
            }

            await Script.NextFrame();
        }
    }

    public void Apagar()
    {
        partícula.Enabled = false;
        cuerpo.LinearVelocity = Vector3.Zero;
        modelo.Enabled = false;
        cuerpo.IsKinematic = true;
        cuerpo.Enabled = false;
    }

    public void Destruir()
    {
        Apagar();
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
        cuerpo.Enabled = true;
        partícula.Enabled = true;
        partícula.ParticleSystem.ResetSimulation();

        tempo = 10f;
        ContarVida();
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

    private void CrearImpacto(Collision colisión)
    {
        if (TocaEntorno(colisión))
        {
            // Rayo para crear marca
            var dirección = cola.WorldMatrix.TranslationVector + cola.WorldMatrix.Forward * 2;
            var resultado = this.GetSimulation().Raycast(cola.WorldMatrix.TranslationVector,
                                                         dirección,
                                                         CollisionFilterGroups.DefaultFilter,
                                                         colisionesMarca);

            if (resultado.Succeeded)
                ControladorCofres.IniciarImpactoGranada(dañoImpacto, resultado.Point, resultado.Normal);
            else
                ControladorCofres.IniciarImpactoGranada(dañoImpacto, Entity.Transform.WorldMatrix.TranslationVector, Vector3.Zero);
        }
        else
            ControladorCofres.IniciarImpactoGranada(dañoImpacto, Entity.Transform.WorldMatrix.TranslationVector, Vector3.Zero);
    }
}
