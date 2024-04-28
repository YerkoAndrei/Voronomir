using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;
using static Utilidades;
using static Constantes;

public class ControladorCofres : StartupScript
{
    public Prefab prefabMarca;
    public Prefab prefabEfecto;
    public Prefab prefabGranada;
    public Prefab prefabExplosión;

    public Prefab prefabProyectilDron;
    public Prefab prefabProyectilBabosa;
    public Prefab prefabProyectilAraña;
    public Prefab prefabImpactoAraña;

    // Marcas
    private static ElementoMarca[] marcas;
    private static int marcaActual;
    private static int maxMarcas;

    // Efectos
    private static ElementoEfecto[] efectos;
    private static int efectoActual;
    private static int maxEfectos;

    // Lanzagranadas
    private static IProyectil[] granadas;
    private static IImpacto[] explosiones;
    private static int granadaActual;
    private static int explosiónActual;
    private static int maxGranadas;
    private static int maxExplosiones;

    // Dron
    private static IProyectil[] proyectilesDron;
    private static int proyectilDronActual;
    private static int maxProyectilesDron;

    // Babosa
    private static IProyectil[] proyectilesBabosa;
    private static int proyectilBabosaActual;
    private static int maxProyectilesBabosa;

    // Araña
    private static IProyectil[] proyectilesAraña;
    private static int proyectilArañaActual;
    private static int maxProyectilesAraña;

    private static IImpacto[] impactosAraña;
    private static int impactoArañaActual;
    private static int maxImpactosAraña;

    public override void Start()
    {
        // Cofre jugador / efectos
        maxMarcas = 100;
        maxEfectos = 100;
        maxGranadas = 4;
        maxExplosiones = 4;

        // Marcas
        marcas = new ElementoMarca[maxMarcas];
        for (int i = 0; i < maxMarcas; i++)
        {
            var marca = prefabMarca.Instantiate()[0];
            marcas[i] = marca.Get<ElementoMarca>();
            Entity.Scene.Entities.Add(marca);
        }

        // Efectos
        efectos = new ElementoEfecto[maxEfectos];
        for (int i = 0; i < maxEfectos; i++)
        {
            var efecto = prefabEfecto.Instantiate()[0];
            efectos[i] = efecto.Get<ElementoEfecto>();
            Entity.Scene.Entities.Add(efecto);
        }

        // Granadas
        granadas = new IProyectil[maxGranadas];
        for (int i = 0; i < maxGranadas; i++)
        {
            var granada = prefabGranada.Instantiate()[0];
            granadas[i] = ObtenerInterfaz<IProyectil>(granada);
            Entity.Scene.Entities.Add(granada);
        }

        // Explosiones
        explosiones = new IImpacto[maxExplosiones];
        for (int i = 0; i < maxExplosiones; i++)
        {
            var explosión = prefabExplosión.Instantiate()[0];
            explosiones[i] = ObtenerInterfaz<IImpacto>(explosión);
            Entity.Scene.Entities.Add(explosión);
        }

        // PENDIENTE: buscar cantidad enemigos
        // Cofre enemigos
        maxProyectilesDron = 20;
        maxProyectilesBabosa = 20;
        maxProyectilesAraña = 20;
        maxImpactosAraña = maxProyectilesAraña * 2;

        // Dron
        proyectilesDron = new IProyectil[maxProyectilesDron];
        for (int i = 0; i < maxProyectilesDron; i++)
        {
            var impacto = prefabProyectilDron.Instantiate()[0];
            proyectilesDron[i] = ObtenerInterfaz<IProyectil>(impacto);
            Entity.Scene.Entities.Add(impacto);
        }

        // Babosa
        proyectilesBabosa = new IProyectil[maxProyectilesBabosa];
        for (int i = 0; i < maxProyectilesBabosa; i++)
        {
            var impacto = prefabProyectilBabosa.Instantiate()[0];
            proyectilesBabosa[i] = ObtenerInterfaz<IProyectil>(impacto);
            Entity.Scene.Entities.Add(impacto);
        }

        // Araña
        proyectilesAraña = new IProyectil[maxProyectilesAraña];
        for (int i = 0; i < maxProyectilesAraña; i++)
        {
            var impacto = prefabProyectilAraña.Instantiate()[0];
            proyectilesAraña[i] = ObtenerInterfaz<IProyectil>(impacto);
            Entity.Scene.Entities.Add(impacto);
        }
        impactosAraña = new IImpacto[maxImpactosAraña];
        for (int i = 0; i < maxImpactosAraña; i++)
        {
            var impacto = prefabImpactoAraña.Instantiate()[0];
            impactosAraña[i] = ObtenerInterfaz<IImpacto>(impacto);
            Entity.Scene.Entities.Add(impacto);
        }
    }

    public static void IniciarEfectoEntorno(Armas arma, Vector3 posición, Vector3 normal, bool iniciarMarca)
    {
        // Marca
        if (iniciarMarca)
        {
            marcas[marcaActual].IniciarMarcaDisparo(arma, posición, normal);
            marcaActual++;

            if (marcaActual >= maxMarcas)
                marcaActual = 0;
        }

        // Efecto
        efectos[efectoActual].IniciarEfectoEntorno(arma, posición, normal);
        efectoActual++;

        if (efectoActual >= maxEfectos)
            efectoActual = 0;
    }

    public static void IniciarEfectoEntornoMuerte(Enemigos enemigo, Vector3 posición, Vector3 normal)
    {
        marcas[marcaActual].IniciarMarcaMuerte(enemigo, posición, normal);
        marcaActual++;

        if (marcaActual >= maxMarcas)
            marcaActual = 0;
    }

    public static void IniciarEfectoDaño(Armas arma, Enemigos enemigo, float multiplicadorDaño, Vector3 posición, Vector3 normal)
    {
        switch (arma)
        {
            case Armas.espada:
                multiplicadorDaño *= 0.2f;
                break;
            case Armas.escopeta:
                multiplicadorDaño *= 0.1f;
                break;
            case Armas.metralleta:
                multiplicadorDaño *= 0.2f;
                break;
            case Armas.rifle:
                multiplicadorDaño *= 0.5f;
                break;
            case Armas.lanzagranadas:
                multiplicadorDaño *= 0.5f;
                break;
        }

        efectos[efectoActual].IniciarEfectoEnemigo(enemigo, multiplicadorDaño, posición, normal);
        efectoActual++;

        if (efectoActual >= maxEfectos)
            efectoActual = 0;
    }

    public static void IniciarGranada(float daño, float velocidad, Vector3 posición, Quaternion rotación)
    {
        granadas[granadaActual].Iniciar(daño, velocidad, rotación, posición, Enemigos.nada);

        granadaActual++;
        if (granadaActual >= maxGranadas)
            granadaActual = 0;
    }

    public static void IniciarImpactoGranada(float daño, Vector3 posición, Vector3 normal)
    {
        explosiones[explosiónActual].Iniciar(posición, normal, daño);

        explosiónActual++;
        if (explosiónActual >= maxExplosiones)
            explosiónActual = 0;

        // Marca solo en entorno
        IniciarEfectoEntorno(Armas.lanzagranadas, posición, normal, (normal != Vector3.Zero));
    }

    // Enemigos
    public static void IniciarEfectoEnemigo(Enemigos enemigo, Vector3 posición, Vector3 normal)
    {
        efectos[efectoActual].IniciarEfectoEnemigo(enemigo, 0.5f, posición, normal);
        efectoActual++;

        if (efectoActual >= maxEfectos)
            efectoActual = 0;
    }

    public static void IniciarProyectil(Enemigos enemigo, float daño, float velocidad, Vector3 posición, Quaternion rotación, float velocidadRotación, Vector3 alturaObjetivo)
    {
        switch (enemigo)
        {
            case Enemigos.rangoLigero:
                proyectilesDron[proyectilDronActual].Iniciar(daño, velocidad, rotación, posición, enemigo);

                proyectilDronActual++;
                if (proyectilDronActual >= maxProyectilesDron)
                    proyectilDronActual = 0;
                break;
            case Enemigos.rangoMediano:
                proyectilesBabosa[proyectilBabosaActual].IniciarPersecutor(velocidadRotación, alturaObjetivo);
                proyectilesBabosa[proyectilBabosaActual].Iniciar(daño, velocidad, rotación, posición, enemigo);

                proyectilBabosaActual++;
                if (proyectilBabosaActual >= maxProyectilesBabosa)
                    proyectilBabosaActual = 0;
                break;
            case Enemigos.rangoPesado:
                proyectilesAraña[proyectilArañaActual].Iniciar(daño, velocidad, rotación, posición, enemigo);

                proyectilArañaActual++;
                if (proyectilArañaActual >= maxProyectilesAraña)
                    proyectilArañaActual = 0;
                break;
        }
    }

    public static void IniciarImpacto(Enemigos enemigo, float daño, Vector3 posición, Vector3 normal)
    {
        switch (enemigo)
        {
            case Enemigos.rangoPesado:
                impactosAraña[impactoArañaActual].Iniciar(posición, normal, daño);

                impactoArañaActual++;
                if (impactoArañaActual >= maxImpactosAraña)
                    impactoArañaActual = 0;
                break;
        }
    }
}
