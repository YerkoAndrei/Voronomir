using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;
using static Utilidades;
using static Constantes;

public class ControladorEfectos : StartupScript
{
    private static ControladorEfectos instancia;

    public Prefab prefabMarca;
    public Prefab prefabEfecto;
    public Prefab prefabGranada;
    public Prefab prefabExplosión;

    // Marcas
    private ElementoMarca[] marcas;
    private int marcaActual;
    private int maxMarcas;

    // Efectos
    private ElementoEfecto[] efectos;
    private int efectoActual;
    private int maxEfectos;

    // Lanzagranadas
    private IProyectil[] granadas;
    private IImpacto[] explosiones;
    private int granadaActual;
    private int explosiónActual;
    private int maxGranadas;
    private int maxExplosiones;

    // marcas
    // efectos
    // impactos

    public override void Start()
    {
        instancia = this;

        // Cofre marcas
        maxMarcas = 100;
        marcas = new ElementoMarca[maxMarcas];
        for (int i = 0; i < maxMarcas; i++)
        {
            var marca = prefabMarca.Instantiate()[0];
            marcas[i] = marca.Get<ElementoMarca>();
            Entity.Scene.Entities.Add(marca);
        }

        // Cofre efectos
        maxEfectos = 100;
        efectos = new ElementoEfecto[maxEfectos];
        for (int i = 0; i < maxEfectos; i++)
        {
            var efecto = prefabEfecto.Instantiate()[0];
            efectos[i] = efecto.Get<ElementoEfecto>();
            Entity.Scene.Entities.Add(efecto);
        }

        // Cofre granadas
        maxGranadas = 4;
        granadas = new IProyectil[maxGranadas];
        for (int i = 0; i < maxGranadas; i++)
        {
            var granada = prefabGranada.Instantiate()[0];
            granadas[i] = ObtenerInterfaz<IProyectil>(granada);
            Entity.Scene.Entities.Add(granada);
        }

        // Cofre explosiones
        maxExplosiones = 4;
        explosiones = new IImpacto[maxExplosiones];
        for (int i = 0; i < maxExplosiones; i++)
        {
            var explosión = prefabExplosión.Instantiate()[0];
            explosiones[i] = ObtenerInterfaz<IImpacto>(explosión);
            Entity.Scene.Entities.Add(explosión);
        }

        // Cofre proyectiles enemigos
    }

    public static void IniciarEfectoEntorno(Armas arma, Vector3 posición, Vector3 normal)
    {
        // Marca
        instancia.marcas[instancia.marcaActual].IniciarMarca(arma, posición, normal);
        instancia.marcaActual++;

        if (instancia.marcaActual >= instancia.maxMarcas)
            instancia.marcaActual = 0;

        // Efecto
        instancia.efectos[instancia.efectoActual].IniciarEfectoEntorno(arma, posición, normal);
        instancia.efectoActual++;

        if (instancia.efectoActual >= instancia.maxEfectos)
            instancia.efectoActual = 0;
    }

    public static void IniciarEfectoDaño(Armas arma, Enemigos enemigo, float multiplicadorDaño, Vector3 posición, Vector3 normal)
    {
        switch (arma)
        {
            case Armas.espada:
                multiplicadorDaño *= 0.5f;
                break;
            case Armas.escopeta:
                multiplicadorDaño *= 0.4f;
                break;
            case Armas.metralleta:
                multiplicadorDaño *= 1f;
                break;
            case Armas.rifle:
                multiplicadorDaño *= 2f;
                break;
            case Armas.lanzagranadas:
                multiplicadorDaño *= 2f;
                break;
        }

        instancia.efectos[instancia.efectoActual].IniciarEfectoEnemigo(enemigo, multiplicadorDaño, posición, normal);
        instancia.efectoActual++;

        if (instancia.efectoActual >= instancia.maxEfectos)
            instancia.efectoActual = 0;
    }

    public static void IniciarGranada(float daño, float velocidad, Vector3 posición, Quaternion rotación)
    {
        instancia.granadas[instancia.granadaActual].Iniciar(daño, velocidad, rotación, posición, Enemigos.nada);

        instancia.granadaActual++;
        if (instancia.granadaActual >= instancia.maxGranadas)
            instancia.granadaActual = 0;
    }

    public static void IniciarImpactoGranada(float daño, Vector3 posición, Vector3 normal)
    {
        instancia.explosiones[instancia.explosiónActual].Iniciar(posición, normal, daño);

        instancia.explosiónActual++;
        if (instancia.explosiónActual >= instancia.maxExplosiones)
            instancia.explosiónActual = 0;

        // Marca solo en entorno
        if (normal != Vector3.Zero)
        {
            instancia.marcas[instancia.marcaActual].IniciarMarca(Armas.lanzagranadas, posición, normal);
            instancia.marcaActual++;

            if (instancia.marcaActual >= instancia.maxMarcas)
                instancia.marcaActual = 0;
        }

        // Efecto
        instancia.efectos[instancia.efectoActual].IniciarEfectoEntorno(Armas.lanzagranadas, posición, normal);
        instancia.efectoActual++;

        if (instancia.efectoActual >= instancia.maxEfectos)
            instancia.efectoActual = 0;
    }

    // Enemigos
    public static void IniciarProyectil(Enemigos enemigo, float daño, Vector3 posición, Vector3 normal)
    {

    }

    public static void IniciarImpacto(Enemigos enemigo, float daño, Vector3 posición, Vector3 normal)
    {

    }
}
