using Stride.Audio;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Particles.Components;

namespace Voronomir;
using static Constantes;

public class ElementoEfecto : StartupScript, ISonidoMundo
{
    // Efectos jugador
    public ParticleSystemComponent bala;
    public ParticleSystemComponent explosión;
    public ParticleSystemComponent granada;

    // Efectos daño enemigos
    public ParticleSystemComponent sangre;
    public ParticleSystemComponent hemolinfa;
    public ParticleSystemComponent rayos;

    // Efectos muerte enemigos
    public ParticleSystemComponent muerteSangre;
    public ParticleSystemComponent muerteHemolinfa;
    public ParticleSystemComponent muerteRayos;

    // Efectos entorno
    public ParticleSystemComponent aparición;
    public ParticleSystemComponent lava;
    public AudioEmitterComponent emisor;

    private AudioEmitterSoundController sonidoAparición;

    [DataMemberIgnore] public float distanciaSonido { get; set; }
    [DataMemberIgnore] public float distanciaJugador { get; set; }

    public override void Start()
    {
        emisor.UseHRTF = bool.Parse(SistemaMemoria.ObtenerConfiguración(Configuraciones.hrtf));
        sonidoAparición = emisor["aparición"];
        distanciaSonido = 20;

        Apagar();
    }

    public void IniciarEfectoEntorno(Armas arma, Vector3 posición, Vector3 normal)
    {
        Apagar();
        switch (arma)
        {
            case Armas.espada:
                bala.Entity.Transform.Scale = Vector3.One * 0.1f;
                bala.Enabled = true;
                bala.ParticleSystem.ResetSimulation();
                break;
            case Armas.escopeta:
                bala.Entity.Transform.Scale = Vector3.One * 0.14f;
                bala.Enabled = true;
                bala.ParticleSystem.ResetSimulation();
                break;
            case Armas.metralleta:
                bala.Entity.Transform.Scale = Vector3.One * 0.2f;
                bala.Enabled = true;
                bala.ParticleSystem.ResetSimulation();
                break;
            case Armas.rifle:
                explosión.Entity.Transform.Scale = Vector3.One * 0.4f;
                explosión.Enabled = true;
                explosión.ParticleSystem.ResetSimulation();
                break;
            case Armas.lanzagranadas:
                explosión.Entity.Transform.Scale = Vector3.One * 0.6f;
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
                sangre.Entity.Transform.Scale = Vector3.One * multiplicadorDaño;
                sangre.Enabled = true;
                sangre.ParticleSystem.ResetSimulation();
                break;
            case Enemigos.rangoLigero:
                hemolinfa.Entity.Transform.Scale = Vector3.One * multiplicadorDaño;
                hemolinfa.Enabled = true;
                hemolinfa.ParticleSystem.ResetSimulation();
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
            case Enemigos.especialLigero:
                rayos.Entity.Transform.Scale = Vector3.One * multiplicadorDaño;
                rayos.Enabled = true;
                rayos.ParticleSystem.ResetSimulation();
                break;
            case Enemigos.especialMediano:

                break;
            case Enemigos.especialPesado:
                rayos.Entity.Transform.Scale = Vector3.One * multiplicadorDaño;
                rayos.Enabled = true;
                rayos.ParticleSystem.ResetSimulation();
                break;
        }

        Entity.Transform.Position = posición + (normal * 0.01f);
    }

    public void IniciarAparición(Enemigos enemigo, Vector3 posición)
    {
        Apagar();
        switch (enemigo)
        {
            case Enemigos.meléLigero:
                aparición.Entity.Transform.Scale = Vector3.One * 1f;
                Entity.Transform.Position = posición + Vector3.UnitY * 1f;
                break;
            case Enemigos.meléMediano:
                aparición.Entity.Transform.Scale = Vector3.One * 1f;
                Entity.Transform.Position = posición + Vector3.UnitY * 1f;
                break;
            case Enemigos.meléPesado:
                aparición.Entity.Transform.Scale = Vector3.One * 1.5f;
                Entity.Transform.Position = posición + Vector3.UnitY * 2f;
                break;
            case Enemigos.rangoLigero:
                aparición.Entity.Transform.Scale = Vector3.One * 1f;
                Entity.Transform.Position = posición + Vector3.UnitY * 1f;
                break;
            case Enemigos.rangoMediano:
                aparición.Entity.Transform.Scale = Vector3.One * 1f;
                Entity.Transform.Position = posición + Vector3.UnitY * 1f;
                break;
            case Enemigos.rangoPesado:
                aparición.Entity.Transform.Scale = Vector3.One * 2f;
                Entity.Transform.Position = posición + Vector3.UnitY * 1f;
                break;
            case Enemigos.especialLigero:
                aparición.Entity.Transform.Scale = Vector3.One * 1f;
                Entity.Transform.Position = posición + Vector3.UnitY * 3f;
                break;
            case Enemigos.especialMediano:

                break;
            case Enemigos.especialPesado:
                aparición.Entity.Transform.Scale = Vector3.One * 1f;
                Entity.Transform.Position = posición + Vector3.UnitY * 1f;
                break;
        }

        aparición.Enabled = true;
        aparición.ParticleSystem.ResetSimulation();

        ActualizarVolumen();
        sonidoAparición.PlayAndForget();
    }

    public void IniciarEfectoMuerte(Enemigos enemigo, Vector3 posición)
    {
        Apagar();
        switch (enemigo)
        {
            case Enemigos.meléLigero:
                muerteSangre.Entity.Transform.Scale = Vector3.One * 0.6f;
                muerteSangre.Entity.Transform.Position += (Vector3.UnitY * 0.5f);
                muerteSangre.Enabled = true;
                muerteSangre.ParticleSystem.ResetSimulation();
                break;
            case Enemigos.meléMediano:
                muerteSangre.Entity.Transform.Scale = Vector3.One * 0.6f;
                muerteSangre.Entity.Transform.Position += (Vector3.UnitY * 0.5f);
                muerteSangre.Enabled = true;
                muerteSangre.ParticleSystem.ResetSimulation();
                break;
            case Enemigos.meléPesado:
                muerteSangre.Entity.Transform.Scale = Vector3.One * 1f;
                muerteSangre.Entity.Transform.Position += (Vector3.UnitY * 1f);
                muerteSangre.Enabled = true;
                muerteSangre.ParticleSystem.ResetSimulation();
                break;
            case Enemigos.rangoLigero:
                muerteHemolinfa.Entity.Transform.Scale = Vector3.One * 0.5f;
                muerteHemolinfa.Entity.Transform.Position += (Vector3.UnitY * 0.2f);
                muerteHemolinfa.Enabled = true;
                muerteHemolinfa.ParticleSystem.ResetSimulation();
                break;
            case Enemigos.rangoMediano:
                muerteSangre.Entity.Transform.Scale = Vector3.One * 0.8f;
                muerteSangre.Entity.Transform.Position += (Vector3.UnitY * 0.5f);
                muerteSangre.Enabled = true;
                muerteSangre.ParticleSystem.ResetSimulation();
                break;
            case Enemigos.rangoPesado:
                muerteHemolinfa.Entity.Transform.Scale = Vector3.One * 1f;
                muerteHemolinfa.Entity.Transform.Position += (Vector3.UnitY * 0.5f);
                muerteHemolinfa.Enabled = true;
                muerteHemolinfa.ParticleSystem.ResetSimulation();
                break;
            case Enemigos.especialLigero:
                muerteRayos.Entity.Transform.Scale = Vector3.One * 0.4f;
                muerteRayos.Entity.Transform.Position += (Vector3.UnitY * 2.8f);
                muerteRayos.Enabled = true;
                muerteRayos.ParticleSystem.ResetSimulation();
                break;
            case Enemigos.especialMediano:

                break;
            case Enemigos.especialPesado:
                muerteRayos.Entity.Transform.Scale = Vector3.One * 0.5f;
                muerteRayos.Entity.Transform.Position += (Vector3.UnitY * 0.5f);
                muerteRayos.Enabled = true;
                muerteRayos.ParticleSystem.ResetSimulation();
                break;
        }

        Entity.Transform.Position = posición;
    }

    public void IniciarEfectoDañoContinuo(Armas arma, Vector3 posición, Vector3 normal)
    {
        Apagar();
        switch (arma)
        {
            case Armas.espada:
                lava.Entity.Transform.Scale = Vector3.One * 0.1f;
                break;
            case Armas.escopeta:
                lava.Entity.Transform.Scale = Vector3.One * 0.14f;
                break;
            case Armas.metralleta:
                lava.Entity.Transform.Scale = Vector3.One * 0.2f;
                break;
            case Armas.rifle:
                lava.Entity.Transform.Scale = Vector3.One * 0.4f;
                break;
            case Armas.lanzagranadas:
                lava.Entity.Transform.Scale = Vector3.One * 0.6f;
                break;
        }

        lava.Enabled = true;
        lava.ParticleSystem.ResetSimulation();
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

        aparición.Enabled = false;
    }

    public void ActualizarVolumen()
    {
        distanciaJugador = Vector3.Distance(Entity.Transform.WorldMatrix.TranslationVector, ControladorPartida.ObtenerCabezaJugador());

        if (distanciaJugador > distanciaSonido)
            sonidoAparición.Volume = 0;
        else
            sonidoAparición.Volume = ((distanciaSonido - distanciaJugador) / distanciaSonido) * SistemaSonidos.ObtenerVolumen(Configuraciones.volumenEfectos);
    }

    public void PausarSonidos(bool pausa)
    {
        if (pausa)
            sonidoAparición.Pause();
        else
        {
            ActualizarVolumen();
            emisor.UseHRTF = bool.Parse(SistemaMemoria.ObtenerConfiguración(Configuraciones.hrtf));
            sonidoAparición.Play();
        }
    }
}
