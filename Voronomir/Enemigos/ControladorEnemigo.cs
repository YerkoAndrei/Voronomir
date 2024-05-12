using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Physics;
using Stride.Audio;
using Stride.Engine;

namespace Voronomir;
using static Utilidades;
using static Constantes;

public class ControladorEnemigo : SyncScript, IDañable, IActivable, ISonidoMundo
{
    public Enemigos enemigo;
    public bool invisible;

    public ControladorArmaMelé armaMelé;
    public ControladorArmaRango armaRango;
    public AudioEmitterComponent emisor;

    private CharacterComponent cuerpo;
    private ControladorPersecusión persecutor;
    private IAnimador animador;
    private float vida;
    private bool despierto;
    private bool activo;

    private AudioEmitterSoundController sonidoAtacar;
    private AudioEmitterSoundController sonidoDaño;
    private AudioEmitterSoundController sonidoMorir;

    // Invisible
    private Vector3 posiciónInicial;
    private Vector3 gravedadInicial;

    public float distanciaSonido { get; set; }
    public float distanciaJugador { get; set; }

    public override void Start()
    {
        cuerpo = Entity.Get<CharacterComponent>();
        persecutor = Entity.Get<ControladorPersecusión>();

        vida = ObtenerVida();
        despierto = false;
        activo = false;

        sonidoAtacar = emisor["atacar"];
        sonidoDaño = emisor["daño"];
        sonidoMorir = emisor["morir"];
        ActualizarVolumen();

        var alturaSonido = RangoAleatorio(0.9f, 1.1f);
        sonidoAtacar.Pitch = alturaSonido;
        sonidoDaño.Pitch = alturaSonido;
        sonidoMorir.Pitch = alturaSonido;

        // Busca animador
        animador = ObtenerInterfaz<IAnimador>(Entity);
        animador.Iniciar();

        // Mueve a cofre si empieza invicible
        if (invisible)
        {
            posiciónInicial = Entity.Transform.WorldMatrix.TranslationVector;
            gravedadInicial = cuerpo.Gravity;
            Esconder();
        }

        // Armas
        if (armaMelé != null)
            armaMelé.Iniciar(ObtenerDaño());
        else if (armaRango != null)
            armaRango.Iniciar(ObtenerDaño(), ObtenerVelocidadProyectil(), ObtenerRotaciónProyectil(), ObtenerObjetivoProyectil(), enemigo);

        // Persecución
        switch (enemigo)
        {
            case Enemigos.meléLigero:
                distanciaSonido = 10;
                persecutor.Iniciar(this, animador, 0.1f, 7f, 6f, ObtenerDistanciaAtaque(), ObtenerDistanciaSalto(), false);
                break;
            case Enemigos.meléMediano:
                distanciaSonido = 15;
                persecutor.Iniciar(this, animador, 0.1f, 4f, 5f, ObtenerDistanciaAtaque(), ObtenerDistanciaSalto(), false);
                break;
            case Enemigos.meléPesado:
                distanciaSonido = 20;
                persecutor.Iniciar(this, animador, 1f, 10f, 2f, ObtenerDistanciaAtaque(), ObtenerDistanciaSalto(),  false);
                break;
            case Enemigos.rangoLigero:
                distanciaSonido = 10;
                persecutor.Iniciar(this, animador, 1f, 4f, 6f, ObtenerDistanciaAtaque(), ObtenerDistanciaSalto(), false);
                break;
            case Enemigos.rangoMediano:
                distanciaSonido = 15;
                persecutor.Iniciar(this, animador, 0.2f, 2f, 3f, ObtenerDistanciaAtaque(), ObtenerDistanciaSalto(), false);
                break;
            case Enemigos.rangoPesado:
                distanciaSonido = 20;
                persecutor.Iniciar(this, animador, 0.2f, 8f, 6f, ObtenerDistanciaAtaque(), ObtenerDistanciaSalto(), false);
                break;
            case Enemigos.especialLigero:
                distanciaSonido = 10;
                persecutor.Iniciar(this, animador, 1f, 16f, 0f, ObtenerDistanciaAtaque(), ObtenerDistanciaSalto(), true);
                break;
            case Enemigos.especialMediano:

                break;
            case Enemigos.especialPesado:
                distanciaSonido = 15;
                persecutor.Iniciar(this, animador, 1f, 10f, 4f, ObtenerDistanciaAtaque(), ObtenerDistanciaSalto(), true);
                break;
        }
    }

    public void Activar()
    {
        if (despierto)
            return;

        if (invisible)
        {
            cuerpo.Teleport(posiciónInicial);
            cuerpo.Gravity = gravedadInicial;
            animador.Activar(true);
            ControladorCofres.IniciarAparición(enemigo, posiciónInicial);
        }

        despierto = true;
        activo = true;
    }

    public override void Update()
    {
        if (!ControladorPartida.ObtenerActivo())
            return;

        if (!activo)
            return;

        // Updates
        persecutor.Actualizar();
    }

    public void Atacar()
    {
        if (armaMelé != null)
            armaMelé.Atacar();
        else if (armaRango != null)
            armaRango.Disparar();

        // Sonido
        sonidoAtacar.Stop();
        sonidoAtacar.Volume = SistemaSonidos.ObtenerVolumen(Configuraciones.volumenEfectos);
        sonidoAtacar.Play();
    }

    public void RecibirDaño(float daño)
    {
        Activar();
        vida -= daño;

        if (vida <= 0 && activo)
        {
            sonidoMorir.Volume = SistemaSonidos.ObtenerVolumen(Configuraciones.volumenEfectos);
            sonidoMorir.PlayAndForget();

            Morir();
        }
        else
        {
            sonidoDaño.Stop();
            sonidoDaño.Volume = SistemaSonidos.ObtenerVolumen(Configuraciones.volumenEfectos);
            sonidoDaño.Play();
        }
    }

    public void Empujar(Vector3 dirección)
    {
        // Evita que salten tanto
        dirección.Y *= 0.5f;
        dirección.Y = MathUtil.Clamp(dirección.Y, 0, 2);

        cuerpo.Jump(dirección * cuerpo.JumpSpeed);
    }

    public bool ObtenerActivo()
    {
        return activo;
    }

    public bool ObtenerMuerto()
    {
        return despierto && !activo;
    }

    private async void Morir()
    {
        activo = false;
        cuerpo.SetVelocity(Vector3.Zero);
        persecutor.EliminarPersecutor();

        ControladorPartida.SumarEnemigo();
        CrearMarcaMuerte();
        Esconder();

        // 1 frame de física
        await Task.Delay(17);
        cuerpo.Enabled = false;

        // PENDIENTE: efectos
        // PENDIENTE: ragdoll
    }

    private void Esconder()
    {
        animador.Activar(false);
        cuerpo.Gravity = Vector3.Zero;
        cuerpo.Teleport(ControladorCofres.ObtenerCofreEnemigos());
    }

    private async void CrearMarcaMuerte()
    {
        await Task.Delay(100);

        // Rayo para crear marca
        var inicioRayo = Entity.Transform.WorldMatrix.TranslationVector + (Vector3.UnitY * 0.5f);
        var dirección = inicioRayo - Vector3.UnitY;
        var resultado = this.GetSimulation().Raycast(inicioRayo,
                                                     dirección,
                                                     CollisionFilterGroups.DefaultFilter,
                                                     CollisionFilterGroupFlags.StaticFilter);
        if (resultado.Succeeded)
            ControladorCofres.IniciarEfectoEntornoMuerte(enemigo, resultado.Point, resultado.Normal);
    }

    public void ActualizarVolumen()
    {
        if (!activo)
            return;

        distanciaJugador = Vector3.Distance(Entity.Transform.WorldMatrix.TranslationVector, ControladorPartida.ObtenerCabezaJugador());

        if (distanciaJugador > distanciaSonido)
        {
            sonidoAtacar.Volume = 0;
            sonidoDaño.Volume = 0;
            sonidoMorir.Volume = 0;
        }
        else
        {
            sonidoAtacar.Volume = ((distanciaSonido - distanciaJugador) / distanciaSonido) * SistemaSonidos.ObtenerVolumen(Configuraciones.volumenEfectos);
            sonidoDaño.Volume = ((distanciaSonido - distanciaJugador) / distanciaSonido) * SistemaSonidos.ObtenerVolumen(Configuraciones.volumenEfectos);
            sonidoMorir.Volume = ((distanciaSonido - distanciaJugador) / distanciaSonido) * SistemaSonidos.ObtenerVolumen(Configuraciones.volumenEfectos);
        }
    }
    public void PausarSonidos(bool pausa)
    {
        if (pausa)
        {
            sonidoAtacar.Pause();
            sonidoDaño.Pause();
            sonidoMorir.Pause();
        }
        else
        {
            sonidoAtacar.Play();
            sonidoDaño.Play();
            sonidoMorir.Pause();
        }
    }

    private int ObtenerVida()
    {
        switch (enemigo)
        {
            case Enemigos.meléLigero:
                return 80;
            case Enemigos.meléMediano:
                return 200;
            case Enemigos.meléPesado:
                return 1000;
            case Enemigos.rangoLigero:
                return 60;
            case Enemigos.rangoMediano:
                return 600;
            case Enemigos.rangoPesado:
                return 800;
            case Enemigos.especialLigero:
                return 40;
            case Enemigos.especialMediano:
                return 0;
            case Enemigos.especialPesado:
                return 100;
            default:
                return 0;
        }
    }

    private float ObtenerDaño()
    {
        switch (enemigo)
        {
            // Melé
            case Enemigos.meléLigero:
                return 10;
            case Enemigos.meléMediano:
                return 25;
            case Enemigos.meléPesado:
                return 40;
            // Rango
            case Enemigos.rangoLigero:
                return 6;
            case Enemigos.rangoMediano:
                return 20;
            case Enemigos.rangoPesado:
                return 25;
            // Especial
            case Enemigos.especialLigero:
                return 5;
            case Enemigos.especialMediano:
                return 1;
            case Enemigos.especialPesado:
                return 0;
            default:
                return 0;
        }
    }

    public float ObtenerDistanciaAtaque()
    {
        switch (enemigo)
        {
            case Enemigos.meléLigero:
                return 1.5f;
            case Enemigos.meléMediano:
                return 2.5f;
            case Enemigos.meléPesado:
                return 2f;
            case Enemigos.rangoLigero:
                return 10f;
            case Enemigos.rangoMediano:
                return 12f;
            case Enemigos.rangoPesado:
                return 16f;
            case Enemigos.especialLigero:
                return 6f;
            case Enemigos.especialMediano:
                return 0f;
            case Enemigos.especialPesado:
                return 4f;
            default:
                return 0;
        }
    }

    public float ObtenerCadenciaAtaque()
    {
        switch (enemigo)
        {
            case Enemigos.meléLigero:
                return 0.5f;
            case Enemigos.meléMediano:
                return 0.8f;
            case Enemigos.meléPesado:
                return 1.5f;
            case Enemigos.rangoLigero:
                return 0.4f;
            case Enemigos.rangoMediano:
                return 2f;
            case Enemigos.rangoPesado:
                return 1f;
            case Enemigos.especialLigero:
                return 0.5f;
            case Enemigos.especialMediano:
                return 0;
            case Enemigos.especialPesado:
                return 0.5f;
            default:
                return 0;
        }
    }

    public float ObtenerPreparaciónAtaqueMelé()
    {
        switch (enemigo)
        {
            case Enemigos.meléLigero:
                return 0.25f;
            case Enemigos.meléMediano:
                return 0.4f;
            case Enemigos.meléPesado:
                return 0.1f;
            default:
                return 0;
        }
    }

    public float ObtenerFuerzaSalto()
    {
        switch (enemigo)
        {
            case Enemigos.rangoLigero:
                return 12;
            default:
                return 0;
        }
    }

    public float ObtenerDistanciaSalto()
    {
        switch (enemigo)
        {
            case Enemigos.rangoLigero:
                return 8;
            default:
                return 0;
        }
    }

    private float ObtenerVelocidadProyectil()
    {
        switch (enemigo)
        {
            case Enemigos.rangoLigero:
                return 20;
            case Enemigos.rangoMediano:
                return 10;
            case Enemigos.rangoPesado:
                return 25;
            case Enemigos.especialLigero:
                return 15;
            default:
                return 0;
        }
    }

    private float ObtenerRotaciónProyectil()
    {
        switch (enemigo)
        {
            case Enemigos.rangoMediano:
                return 5;
            default:
                return 0;
        }
    }

    private Vector3 ObtenerObjetivoProyectil()
    {
        // Jugador mide 160cm, tiene los ojos en 150cm
        switch (enemigo)
        {
            case Enemigos.rangoLigero:
                return Vector3.UnitY * 1.7f;
            case Enemigos.rangoMediano:
                return Vector3.UnitY * 0.6f;
            case Enemigos.rangoPesado:
                return Vector3.UnitY * 0.33f;
            case Enemigos.especialLigero:
                return Vector3.UnitY * 1.4f;
            default:
                return Vector3.UnitY;
        }
    }
}
