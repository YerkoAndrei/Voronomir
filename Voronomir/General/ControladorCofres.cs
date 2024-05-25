using System.Linq;
using System.Collections.Generic;
using Stride.Core.Mathematics;
using Stride.Engine;
using System;

namespace Voronomir;
using static Utilidades;
using static Constantes;

public class ControladorCofres : StartupScript
{
    public bool permiteInstanciación;
    public Prefab prefabMarca;
    public Prefab prefabEfecto;
    public Prefab prefabGranada;
    public Prefab prefabExplosión;

    public Prefab prefabProyectilPulga;
    public Prefab prefabProyectilBabosa;
    public Prefab prefabProyectilAraña;
    public Prefab prefabProyectilDron;
    public Prefab prefabImpactoAraña;

    // Cofres
    private static Vector3 cofreProyectiles;
    private static Vector3 cofreEnemigos;

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

    // Pulga
    private static IProyectil[] proyectilesPulga;
    private static int proyectilPulgaActual;
    private static int maxProyectilesPulga;

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

    // Dron
    private static IProyectil[] proyectilesDron;
    private static int proyectilDronActual;
    private static int maxProyectilesDron;

    public override void Start()
    {
        // Posición cofre enemigos
        cofreProyectiles = new Vector3(0, -10, 0);
        cofreEnemigos = new Vector3(-13, -10, 0);

        // Cofre jugador
        maxGranadas = 4;
        maxExplosiones = 8;

        // Cofre efectos
        switch ((Calidades)Enum.Parse(typeof(Calidades), SistemaMemoria.ObtenerConfiguración(Configuraciones.efectos)))
        {
            case Calidades.bajo:
                maxMarcas = 50;
                maxEfectos = 50;
                break;
            case Calidades.medio:
                maxMarcas = 100;
                maxEfectos = 100;
                break;
            case Calidades.alto:
                maxMarcas = 200;
                maxEfectos = 200;
                break;
        }

        // Marcas
        marcas = new ElementoMarca[maxMarcas];
        for (int i = 0; i < maxMarcas; i++)
        {
            var marca = prefabMarca.Instantiate()[0];
            marcas[i] = marca.Get<ElementoMarca>();
            marca.Transform.Position = cofreProyectiles;
            Entity.Scene.Entities.Add(marca);
        }

        // Efectos
        efectos = new ElementoEfecto[maxEfectos];
        for (int i = 0; i < maxEfectos; i++)
        {
            var efecto = prefabEfecto.Instantiate()[0];
            efectos[i] = efecto.Get<ElementoEfecto>();
            efecto.Transform.Position = cofreProyectiles;
            efecto.Transform.Position = cofreProyectiles;
            Entity.Scene.Entities.Add(efecto);
        }

        // Granadas
        granadas = new IProyectil[maxGranadas];
        for (int i = 0; i < maxGranadas; i++)
        {
            var granada = prefabGranada.Instantiate()[0];
            granadas[i] = ObtenerInterfaz<IProyectil>(granada);
            granada.Transform.Position = cofreProyectiles;
            Entity.Scene.Entities.Add(granada);
        }

        // Explosiones
        explosiones = new IImpacto[maxExplosiones];
        for (int i = 0; i < maxExplosiones; i++)
        {
            var explosión = prefabExplosión.Instantiate()[0];
            explosiones[i] = ObtenerInterfaz<IImpacto>(explosión);
            explosión.Transform.Position = cofreProyectiles;
            Entity.Scene.Entities.Add(explosión);
        }

        // Cofre enemigos
        if (permiteInstanciación)
        {
            maxProyectilesPulga = 20;
            maxProyectilesBabosa = 20;
            maxProyectilesAraña = 20;
            maxImpactosAraña = 40;
            maxProyectilesDron = 20;
        }
        else
        {
            var controladores = Entity.Scene.Entities.Where(o => o.Get<ControladorEnemigo>() != null)
                                                     .Select(o => o.Get<ControladorEnemigo>()).ToArray();
            // Cuenta enemigos
            if (controladores.Where(o => o.enemigo == Enemigos.rangoLigero).Count() > 0)
                maxProyectilesPulga = 20;

            if (controladores.Where(o => o.enemigo == Enemigos.rangoMediano).Count() > 0)
                maxProyectilesBabosa = 20;

            if (controladores.Where(o => o.enemigo == Enemigos.rangoPesado).Count() > 0)
            {
                maxProyectilesAraña = 20;
                maxImpactosAraña = 40;
            }

            if (controladores.Where(o => o.enemigo == Enemigos.especialLigero).Count() > 0)
                maxProyectilesDron = 20;
        }

        // Pulga
        proyectilesPulga = new IProyectil[maxProyectilesPulga];
        for (int i = 0; i < maxProyectilesPulga; i++)
        {
            var proyectil = prefabProyectilPulga.Instantiate()[0];
            proyectilesPulga[i] = ObtenerInterfaz<IProyectil>(proyectil);
            proyectil.Transform.Position = cofreProyectiles;
            Entity.Scene.Entities.Add(proyectil);
        }

        // Babosa
        proyectilesBabosa = new IProyectil[maxProyectilesBabosa];
        for (int i = 0; i < maxProyectilesBabosa; i++)
        {
            var proyectil = prefabProyectilBabosa.Instantiate()[0];
            proyectilesBabosa[i] = ObtenerInterfaz<IProyectil>(proyectil);
            proyectil.Transform.Position = cofreProyectiles;
            Entity.Scene.Entities.Add(proyectil);
        }

        // Araña
        proyectilesAraña = new IProyectil[maxProyectilesAraña];
        for (int i = 0; i < maxProyectilesAraña; i++)
        {
            var proyectil = prefabProyectilAraña.Instantiate()[0];
            proyectilesAraña[i] = ObtenerInterfaz<IProyectil>(proyectil);
            proyectil.Transform.Position = cofreProyectiles;
            Entity.Scene.Entities.Add(proyectil);
        }
        impactosAraña = new IImpacto[maxImpactosAraña];
        for (int i = 0; i < maxImpactosAraña; i++)
        {
            var impacto = prefabImpactoAraña.Instantiate()[0];
            impactosAraña[i] = ObtenerInterfaz<IImpacto>(impacto);
            impacto.Transform.Position = cofreProyectiles;
            Entity.Scene.Entities.Add(impacto);
        }

        // Dron
        proyectilesDron = new IProyectil[maxProyectilesDron];
        for (int i = 0; i < maxProyectilesDron; i++)
        {
            var proyectil = prefabProyectilDron.Instantiate()[0];
            proyectilesDron[i] = ObtenerInterfaz<IProyectil>(proyectil);
            proyectil.Transform.Position = cofreProyectiles;
            Entity.Scene.Entities.Add(proyectil);
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

    public static void IniciarEfectoEntornoMuerte(Enemigos enemigo, float multiplicador, Vector3 posición, Vector3 normal)
    {
        marcas[marcaActual].IniciarMarcaMuerte(enemigo, multiplicador, posición, normal);
        marcaActual++;

        if (marcaActual >= maxMarcas)
            marcaActual = 0;
    }

    public static void IniciarEfectoDaño(Armas arma, Enemigos enemigo, float multiplicadorDaño, Vector3 posición, Vector3 normal)
    {
        // Cerebro / escudo
        if (enemigo == Enemigos.especialPesado && multiplicadorDaño == 1)
            enemigo = Enemigos.meléLigero;

        switch (arma)
        {
            case Armas.espada:
                multiplicadorDaño *= 0.2f;
                break;
            case Armas.escopeta:
                multiplicadorDaño *= 0.12f;
                break;
            case Armas.metralleta:
                multiplicadorDaño *= 0.18f;
                break;
            case Armas.rifle:
                multiplicadorDaño *= 0.4f;
                break;
            case Armas.lanzagranadas:
                multiplicadorDaño *= 0.4f;
                break;
        }

        efectos[efectoActual].IniciarEfectoEnemigo(enemigo, multiplicadorDaño, posición, normal);
        efectoActual++;

        if (efectoActual >= maxEfectos)
            efectoActual = 0;
    }

    public static void IniciarEfectoMuerte(Enemigos enemigo, Vector3 posición)
    {
        // PENDIENTE: objetos físicos o ragdoll
        switch (enemigo)
        {
            case Enemigos.meléLigero:
                break;
            case Enemigos.meléMediano:
                break;
            case Enemigos.meléPesado:
                break;
            case Enemigos.rangoMediano:
                break;
            case Enemigos.rangoPesado:
                break;
            case Enemigos.especialPesado:
                break;
        }

        efectos[efectoActual].IniciarEfectoMuerte(enemigo, posición);
        efectoActual++;

        if (efectoActual >= maxEfectos)
            efectoActual = 0;
    }

    public static void IniciarAparición(Enemigos enemigo, Vector3 posición)
    {
        efectos[efectoActual].IniciarAparición(enemigo, posición);
        efectoActual++;

        if (efectoActual >= maxEfectos)
            efectoActual = 0;
    }

    public static void IniciarEfectoDañoContinuo(Armas arma, Vector3 posición, Vector3 normal)
    {
        // Lava
        efectos[efectoActual].IniciarEfectoDañoContinuo(arma, posición, normal);
        efectoActual++;

        if (efectoActual >= maxEfectos)
            efectoActual = 0;
    }

    // Granada
    public static void IniciarGranada(float daño, float velocidad, Vector3 posición, Quaternion rotación)
    {
        granadas[granadaActual].Iniciar(daño, velocidad, rotación, posición, Enemigos.nada);

        granadaActual++;
        if (granadaActual >= maxGranadas)
            granadaActual = 0;
    }

    public static void IniciarExplosión(float daño, Vector3 posición, Vector3 normal)
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
        efectos[efectoActual].IniciarEfectoEnemigo(enemigo, 0.2f, posición, normal);
        efectoActual++;

        if (efectoActual >= maxEfectos)
            efectoActual = 0;
    }

    public static void IniciarProyectil(Enemigos enemigo, float daño, float velocidad, Vector3 posición, Quaternion rotación, float velocidadRotación, Vector3 alturaObjetivo)
    {
        switch (enemigo)
        {
            case Enemigos.rangoLigero:
                proyectilesPulga[proyectilPulgaActual].Iniciar(daño, velocidad, rotación, posición, enemigo);

                proyectilPulgaActual++;
                if (proyectilPulgaActual >= maxProyectilesPulga)
                    proyectilPulgaActual = 0;
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
            case Enemigos.especialLigero:
                proyectilesDron[proyectilDronActual].Iniciar(daño, velocidad, rotación, posición, enemigo);

                proyectilDronActual++;
                if (proyectilDronActual >= maxProyectilesDron)
                    proyectilDronActual = 0;
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

    public static Vector3 ObtenerCofreEnemigos()
    {
        cofreEnemigos += (Vector3.UnitX * 3);
        return cofreEnemigos;
    }

    public static void ApagarFísicas()
    {
        var proyectiles = new List<IProyectil>();
        proyectiles.AddRange(granadas);
        proyectiles.AddRange(proyectilesPulga);
        proyectiles.AddRange(proyectilesBabosa);
        proyectiles.AddRange(proyectilesAraña);
        proyectiles.AddRange(proyectilesDron);

        foreach (var proyectil in proyectiles)
        {
            proyectil.Apagar();
        }
    }
}
