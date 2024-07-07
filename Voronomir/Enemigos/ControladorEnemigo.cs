using System.Threading.Tasks;
using System.Collections.Generic;
using Stride.Core.Mathematics;
using Stride.Physics;
using Stride.Audio;
using Stride.Engine;
using Stride.Core;

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

    public List<ElementoDañable> dañables { get; set; }

    private Enemigo datos;
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

    [DataMemberIgnore] public float distanciaSonido { get; set; }
    [DataMemberIgnore] public float distanciaJugador { get; set; }

    public override void Start()
    {
        cuerpo = Entity.Get<CharacterComponent>();
        persecutor = Entity.Get<ControladorPersecusión>();
        foreach (var dañable in dañables)
        {
            dañable.Iniciar(this, enemigo);
        }

        datos = GenerarDatos(enemigo);
        vida = datos.Vida;
        despierto = false;
        activo = false;

        emisor.UseHRTF = bool.Parse(SistemaMemoria.ObtenerConfiguración(Configuraciones.hrtf));
        distanciaSonido = datos.DistanciaSonido;
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
            armaMelé.Iniciar(datos.Daño);
        else if (armaRango != null)
            armaRango.Iniciar(datos.Daño, datos.VelocidadProyectil, datos.RotaciónProyectil, datos.AlturaObjetivo, enemigo);

        // Persecución
        persecutor.Iniciar(this, datos, animador);
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
        if (ObtenerMuerto())
            return;

        Activar();
        vida -= daño;

        if (vida > 0)
        {
            sonidoDaño.Stop();
            sonidoDaño.Volume = SistemaSonidos.ObtenerVolumen(Configuraciones.volumenEfectos);
            sonidoDaño.Play();
        }
        else
        {
            sonidoMorir.Volume = SistemaSonidos.ObtenerVolumen(Configuraciones.volumenEfectos);
            sonidoMorir.PlayAndForget();

            // Si recibe mucho daño al morir, explota (10% de la vida máxima)
            Morir(vida < -(datos.Vida * 0.1f));
        }
    }

    public void Empujar(Vector3 dirección)
    {
        if (ObtenerMuerto())
            return;

        // Evita que salten tanto
        dirección.Y *= 0.5f;
        dirección.Y = MathUtil.Clamp(dirección.Y, 0, 2);

        cuerpo.Jump(dirección * cuerpo.JumpSpeed);
    }

    private async void Morir(bool explotar)
    {
        activo = false;
        cuerpo.SetVelocity(Vector3.Zero);
        persecutor.EliminarPersecutor();

        ControladorPartida.SumarEnemigo();
        MarcarMuerte();

        // Desactivar Dañables
        foreach (var dañable in dañables)
        {
            dañable.Desactivar();
        }

        // En aire siempre explotan
        if (!cuerpo.IsGrounded)
            explotar = true;

        // Enemigos pequeños siempre explotan y cerebro nunca
        if (enemigo == Enemigos.rangoLigero || enemigo == Enemigos.especialLigero || enemigo == Enemigos.especialMediano)
            explotar = true;
        else if (enemigo == Enemigos.especialPesado)
            explotar = false;

        // Animación de muerte o explosión
        if (explotar)
            Esconder();
        else
            animador.Morir();

        await EsperarCuadroFísica();
        Entity.Remove(cuerpo);
    }

    private void Esconder()
    {
        animador.Activar(false);
        cuerpo.Gravity = Vector3.Zero;
        cuerpo.Teleport(ControladorCofres.ObtenerCofreEnemigos());
    }

    private async void MarcarMuerte()
    {
        // Efecto y cuerpo
        ControladorCofres.IniciarEfectoMuerte(enemigo, Entity.Transform.WorldMatrix.TranslationVector);

        // Dron no deja marca
        if (enemigo == Enemigos.especialLigero)
            return;

        // Marcas
        var inicioRayo = Entity.Transform.WorldMatrix.TranslationVector + (Vector3.UnitY * 0.5f);

        // Rayo para crear marca principal
        await Task.Delay(100);
        var dirección = inicioRayo - Vector3.UnitY;
        var resultado = this.GetSimulation().Raycast(inicioRayo, dirección,
                                                     CollisionFilterGroups.DefaultFilter,
                                                     CollisionFilterGroupFlags.StaticFilter);
        if (resultado.Succeeded)
            ControladorCofres.IniciarEfectoEntornoMuerte(enemigo, 1, resultado.Point, resultado.Normal);

        for (int i = 0; i < 2; i++)
        {
            await Task.Delay(100);
            var tamaño = RangoAleatorio(0.2f, 1f);
            inicioRayo.X += RangoAleatorio(-0.6f, 0.6f);
            inicioRayo.Z += RangoAleatorio(-0.6f, 0.6f);

            // Rayo para crear marcas
            dirección = inicioRayo - Vector3.UnitY;
            resultado = this.GetSimulation().Raycast(inicioRayo, dirección,
                                                         CollisionFilterGroups.DefaultFilter,
                                                         CollisionFilterGroupFlags.StaticFilter);
            if (resultado.Succeeded)
                ControladorCofres.IniciarEfectoEntornoMuerte(enemigo, tamaño, resultado.Point, resultado.Normal);
        }
    }

    public bool ObtenerActivo()
    {
        return activo;
    }

    public bool ObtenerMuerto()
    {
        return despierto && !activo;
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
            ActualizarVolumen();
            emisor.UseHRTF = bool.Parse(SistemaMemoria.ObtenerConfiguración(Configuraciones.hrtf));
            sonidoAtacar.Play();
            sonidoDaño.Play();
            sonidoMorir.Pause();
        }
    }

    public static float ObtenerDistanciaPersecuciónTrigonométrica(Enemigos enemigo)
    {
        // Radio = DistanciaAtaque - 1;
        switch (enemigo)
        {
            case Enemigos.especialLigero:
                return 5;
            case Enemigos.especialMediano:
                return 2;
            case Enemigos.especialPesado:
                return 3;
            default:
                return 0;
        }
    }

    private Enemigo GenerarDatos(Enemigos enemigo)
    {
        var data = new Enemigo();
        switch (enemigo)
        {
            // Melé
            case Enemigos.meléLigero:
                data.Vida = 80;
                data.Daño = 10;
                data.Cadencia = 0.5f;
                data.DistanciaAtaque = 1.5f;
                data.VelocidadMovimiento = 7;
                data.VelocidadRotación = 6;
                data.DistanciaSonido = 10;
                data.TiempoBusqueda = RangoAleatorio(0.1f, 0.4f);

                data.PreparaciónMelé = 0.25f;
                break;
            case Enemigos.meléMediano:
                data.Vida = 200;
                data.Daño = 25;
                data.Cadencia = 0.8f;
                data.DistanciaAtaque = 2.5f;
                data.VelocidadMovimiento = 4;
                data.VelocidadRotación = 5;
                data.DistanciaSonido = 15;
                data.TiempoBusqueda = 0.1f;

                data.PreparaciónMelé = 0.4f;
            break;
            case Enemigos.meléPesado:
                data.Vida = 1000;
                data.Daño = 40;
                data.Cadencia = 1.5f;
                data.DistanciaAtaque = 2;
                data.VelocidadMovimiento = 10;
                data.VelocidadRotación = 2;
                data.DistanciaSonido = 20;
                data.TiempoBusqueda = 0.1f;

                data.PreparaciónMelé = 0.2f;
            break;

            // Rango
            case Enemigos.rangoLigero:
                data.Vida = 60;
                data.Daño = 6;
                data.Cadencia = 0.4f;
                data.DistanciaAtaque = 10;
                data.VelocidadMovimiento = 4;
                data.VelocidadRotación = 6;
                data.DistanciaSonido = 10;
                data.TiempoBusqueda = 1;

                data.VelocidadProyectil = 20;
                data.AlturaObjetivo = Vector3.UnitY * 1.7f;
                data.FuerzaSalto = 12;
                data.DistanciaSalto = 8;
                break;
            case Enemigos.rangoMediano:
                data.Vida = 600;
                data.Daño = 20;
                data.Cadencia = 2;
                data.DistanciaAtaque = 12;
                data.VelocidadMovimiento = 2;
                data.VelocidadRotación = 3;
                data.DistanciaSonido = 15;
                data.TiempoBusqueda = 0.2f;

                data.VelocidadProyectil = 10;
                data.RotaciónProyectil = 5;
                data.AlturaObjetivo = Vector3.UnitY * 0.6f;
                break;
            case Enemigos.rangoPesado:
                data.Vida = 800;
                data.Daño = 25;
                data.Cadencia = 1;
                data.DistanciaAtaque = 16;
                data.VelocidadMovimiento = 8;
                data.VelocidadRotación = 6;
                data.DistanciaSonido = 20;
                data.TiempoBusqueda = 0.2f;

                data.VelocidadProyectil = 25;
                data.AlturaObjetivo = Vector3.UnitY * 0.33f;
                break;

            // Especial
            case Enemigos.especialLigero:
                data.Vida = 40;
                data.Daño = 5;
                data.Cadencia = 0.5f;
                data.DistanciaAtaque = 6;
                data.VelocidadMovimiento = 16;
                data.VelocidadRotación = 0;
                data.DistanciaSonido = 10;
                data.TiempoBusqueda = 0.1f;
                data.PersecutorTrigonométrico = true;

                data.VelocidadProyectil = 15;
                data.AlturaObjetivo = Vector3.UnitY * 1.4f;
                break;
            case Enemigos.especialMediano:
                data.Vida = 60;
                data.Daño = 0;
                data.Cadencia = 0.5f;
                data.VelocidadMovimiento = 6;
                data.VelocidadRotación = 4;
                data.DistanciaSonido = 10;
                data.TiempoBusqueda = 0.1f;
                data.PersecutorTrigonométrico = true;

                data.DistanciaAtaque = 3;
                break;
            case Enemigos.especialPesado:
                data.Vida = 100;
                data.Daño = 0;
                data.Cadencia = 0.5f;
                data.VelocidadMovimiento = 10;
                data.VelocidadRotación = 4;
                data.DistanciaSonido = 15;
                data.TiempoBusqueda = 0.1f;
                data.PersecutorTrigonométrico = true;

                data.DistanciaAtaque = 4;
                break;
        }
        return data;
    }
}

public class Enemigo
{
    public float Vida { get; set; }
    public float Daño { get; set; }
    public float Cadencia { get; set; }
    public float DistanciaAtaque { get; set; }

    // Persecusión
    public float VelocidadMovimiento { get; set; }
    public float VelocidadRotación { get; set; }
    public float DistanciaSonido { get; set; }
    public float TiempoBusqueda { get; set; }
    public bool PersecutorTrigonométrico { get; set; }

    // Melé
    public float PreparaciónMelé { get; set; }

    // Rango
    public float VelocidadProyectil { get; set; }

    // Persecutor
    public float RotaciónProyectil { get; set; }

    // Jugador mide 160cm, tiene los ojos en 150cm
    public Vector3 AlturaObjetivo { get; set; }

    // Pulga
    public float FuerzaSalto { get; set; }
    public float DistanciaSalto { get; set; }
}