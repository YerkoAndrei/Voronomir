using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Particles.Components;

namespace Voronomir;
using static Constantes;

public class ElementoEfecto : StartupScript
{
    // Efectos entorno
    public ParticleSystemComponent bala;
    public ParticleSystemComponent explosión;
    public ParticleSystemComponent granada;

    // Efectos enemigos
    public ParticleSystemComponent sangre;
    public ParticleSystemComponent rayos;
    public ParticleSystemComponent hemolinfa;

    public override void Start()
    {
        Apagar();
    }

    public void IniciarEfectoEntorno(Armas arma, Vector3 posición, Vector3 normal)
    {
        Apagar();
        switch (arma)
        {
            case Armas.espada:
                bala.Entity.Transform.Scale = Vector3.One * 0.2f;
                bala.Enabled = true;
                bala.ParticleSystem.ResetSimulation();
                break;
            case Armas.escopeta:
                bala.Entity.Transform.Scale = Vector3.One * 0.25f;
                bala.Enabled = true;
                bala.ParticleSystem.ResetSimulation();
                break;
            case Armas.metralleta:
                bala.Entity.Transform.Scale = Vector3.One * 0.4f;
                bala.Enabled = true;
                bala.ParticleSystem.ResetSimulation();
                break;
            case Armas.rifle:
                explosión.Entity.Transform.Scale = Vector3.One * 0.8f;
                explosión.Enabled = true;
                explosión.ParticleSystem.ResetSimulation();
                break;
            case Armas.lanzagranadas:
                explosión.Entity.Transform.Scale = Vector3.One * 1.2f;
                explosión.Enabled = true;
                explosión.ParticleSystem.ResetSimulation();

                granada.Entity.Transform.Scale = Vector3.One * 0.4f;
                granada.Enabled = true;
                granada.ParticleSystem.ResetSimulation();
                break;
        }

        Entity.Transform.Position = posición + (normal * 0.01f);
    }

    public void IniciarEfectoEnemigo(Enemigos enemigo, float multiplicadorDaño, Vector3 posición, Vector3 normal)
    {
        Apagar();
        switch (enemigo)
        {
            case Enemigos.meléLigero:
                sangre.Entity.Transform.Scale = Vector3.One * multiplicadorDaño;
                sangre.Enabled = true;
                sangre.ParticleSystem.ResetSimulation();
                break;
            case Enemigos.meléMediano:
                sangre.Entity.Transform.Scale = Vector3.One * multiplicadorDaño;
                sangre.Enabled = true;
                sangre.ParticleSystem.ResetSimulation();
                break;
            case Enemigos.meléPesado:
                rayos.Entity.Transform.Scale = Vector3.One * multiplicadorDaño;
                rayos.Enabled = true;
                rayos.ParticleSystem.ResetSimulation();
                break;
            case Enemigos.rangoLigero:
                rayos.Entity.Transform.Scale = Vector3.One * multiplicadorDaño;
                rayos.Enabled = true;
                rayos.ParticleSystem.ResetSimulation();
                break;
            case Enemigos.rangoMediano:
                sangre.Entity.Transform.Scale = Vector3.One * multiplicadorDaño;
                sangre.Enabled = true;
                sangre.ParticleSystem.ResetSimulation();
                break;
            case Enemigos.rangoPesado:
                hemolinfa.Entity.Transform.Scale = Vector3.One * multiplicadorDaño;
                hemolinfa.Enabled = true;
                hemolinfa.ParticleSystem.ResetSimulation();
                break;
        }

        Entity.Transform.Position = posición + (normal * 0.01f);
    }

    private void Apagar()
    {
        bala.Enabled = false;
        explosión.Enabled = false;
        granada.Enabled = false;

        sangre.Enabled = false;
        rayos.Enabled = false;
        hemolinfa.Enabled = false;
    }
}
