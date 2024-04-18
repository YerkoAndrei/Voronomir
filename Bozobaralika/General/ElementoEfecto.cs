using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Particles.Components;

namespace Bozobaralika;
using static Constantes;

public class ElementoEfecto : StartupScript
{
    // Efectos entorno
    public ParticleSystemComponent partículasBala;
    public ParticleSystemComponent partículasRifle;
    public ParticleSystemComponent partículasGranada;

    // Efectos enemigos
    public ParticleSystemComponent partículasSangre;
    public ParticleSystemComponent partículasRayos;
    public ParticleSystemComponent partículasHemolinfa;

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
                partículasBala.Entity.Transform.Scale = Vector3.One * 0.2f;
                partículasBala.Enabled = true;
                partículasBala.ParticleSystem.ResetSimulation();
                break;
            case Armas.escopeta:
                partículasBala.Entity.Transform.Scale = Vector3.One * 0.25f;
                partículasBala.Enabled = true;
                partículasBala.ParticleSystem.ResetSimulation();
                break;
            case Armas.metralleta:
                partículasBala.Entity.Transform.Scale = Vector3.One * 0.4f;
                partículasBala.Enabled = true;
                partículasBala.ParticleSystem.ResetSimulation();
                break;
            case Armas.rifle:
                partículasRifle.Entity.Transform.Scale = Vector3.One * 0.5f;
                partículasRifle.Enabled = true;
                partículasRifle.ParticleSystem.ResetSimulation();
                break;
            case Armas.lanzagranadas:
                partículasGranada.Entity.Transform.Scale = Vector3.One * 0.6f;
                partículasGranada.Enabled = true;
                partículasGranada.ParticleSystem.ResetSimulation();
                break;
        }

        Entity.Transform.Position = posición + (normal * 0.01f);
        Entity.Transform.Rotation = Quaternion.LookRotation(normal, posición);
    }

    public void IniciarEfectoEnemigo(Enemigos enemigo, float multiplicadorDaño, Vector3 posición, Vector3 normal)
    {
        Apagar();
        switch (enemigo)
        {
            case Enemigos.meléLigero:
                partículasSangre.Entity.Transform.Scale = Vector3.One * multiplicadorDaño;
                partículasSangre.Enabled = true;
                partículasSangre.ParticleSystem.ResetSimulation();
                break;
            case Enemigos.meléMediano:
                partículasSangre.Entity.Transform.Scale = Vector3.One * multiplicadorDaño;
                partículasSangre.Enabled = true;
                partículasSangre.ParticleSystem.ResetSimulation();
                break;
            case Enemigos.meléPesado:
                partículasRayos.Entity.Transform.Scale = Vector3.One * multiplicadorDaño;
                partículasRayos.Enabled = true;
                partículasRayos.ParticleSystem.ResetSimulation();
                break;
            case Enemigos.rangoLigero:
                partículasRayos.Entity.Transform.Scale = Vector3.One * multiplicadorDaño;
                partículasRayos.Enabled = true;
                partículasRayos.ParticleSystem.ResetSimulation();
                break;
            case Enemigos.rangoMediano:
                partículasSangre.Entity.Transform.Scale = Vector3.One * multiplicadorDaño;
                partículasSangre.Enabled = true;
                partículasSangre.ParticleSystem.ResetSimulation();
                break;
            case Enemigos.rangoPesado:
                partículasHemolinfa.Entity.Transform.Scale = Vector3.One * multiplicadorDaño;
                partículasHemolinfa.Enabled = true;
                partículasHemolinfa.ParticleSystem.ResetSimulation();
                break;
        }

        Entity.Transform.Position = posición + (normal * 0.01f);
        Entity.Transform.Rotation = Quaternion.LookRotation(normal, posición);
    }

    private void Apagar()
    {
        partículasBala.Enabled = false;
        partículasRifle.Enabled = false;
        partículasGranada.Enabled = false;

        partículasSangre.Enabled = false;
        partículasRayos.Enabled = false;
        partículasHemolinfa.Enabled = false;
    }
}
