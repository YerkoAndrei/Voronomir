using Stride.Core.Mathematics;
using Stride.Particles.Components;
using Stride.Engine;
using Stride.Physics;
using System.Threading.Tasks;

namespace Voronomir;

public class AnimadorCerebro : StartupScript, IAnimador
{
    public TransformComponent rueda;
    public ModelComponent cerebro;
    public ModelComponent modeloEscudo;
    public RigidbodyComponent cuerpoEscudo;
    public Entity escudoMuerte;
    public ParticleSystemComponent partículas;
    public RigidbodyComponent saltador;

    public void Iniciar()
    {
        escudoMuerte.Get<ModelComponent>().Enabled = false;
        escudoMuerte.Get<RigidbodyComponent>().Enabled = false;
        escudoMuerte.Transform.Position = Vector3.One * -2;
        escudoMuerte.Transform.Scale = Vector3.Zero;
    }

    public void Actualizar()
    {

    }

    public void Activar(bool activar)
    {
        partículas.Enabled = activar;
        saltador.Enabled = activar;

        if (activar)
            partículas.ParticleSystem.ResetSimulation();
    }

    public void Caminar(float velocidad)
    {
        rueda.Rotation *= Quaternion.RotationX(velocidad * 20 *(float)Game.UpdateTime.WarpElapsed.TotalSeconds);
    }

    public void Atacar()
    {

    }

    public void Morir()
    {
        Activar(false);

        AnimarMuerteRueda(rueda.Position, rueda.Rotation,
                          Vector3.UnitY * 0.05f, Quaternion.RotationZ(MathUtil.DegreesToRadians(90)));

        cerebro.Enabled = false;
        modeloEscudo.Enabled = false;
        cuerpoEscudo.Enabled = false;

        escudoMuerte.Get<ModelComponent>().Enabled = true;
        escudoMuerte.Get<RigidbodyComponent>().Enabled = true;
        escudoMuerte.Transform.Scale = Vector3.One;

        AnimarMuerteEscudo(Vector3.Zero,
                          Vector3.UnitY * -0.6f);
    }

    // Animaciones
    private async void AnimarMuerteEscudo(Vector3 posiciónInicio, Vector3 posiciónObjetivo)
    {
        float duración = 0.25f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = tiempoLerp / duración;

            escudoMuerte.Transform.Position = Vector3.Lerp(posiciónInicio, posiciónObjetivo, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        // Fin
        escudoMuerte.Transform.Position = posiciónObjetivo;
    }

    private async void AnimarMuerteRueda(Vector3 posiciónInicio, Quaternion rotaciónInicio, Vector3 posiciónObjetivo, Quaternion rotaciónObjetivo)
    {
        float duración = 0.4f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = tiempoLerp / duración;

            rueda.Position = Vector3.Lerp(posiciónInicio, posiciónObjetivo, tiempo);
            rueda.Rotation = Quaternion.Lerp(rotaciónInicio, rotaciónObjetivo, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        // Fin
        rueda.Position = posiciónObjetivo;
        rueda.Rotation = rotaciónObjetivo;
    }
}
