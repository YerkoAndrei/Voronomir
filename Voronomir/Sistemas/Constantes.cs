﻿namespace Voronomir;

public static class Constantes
{
    // Filtros:
    // Default      - Disparos jugador, daño ambiente
    // Static       - Entorno
    // Kinematic    - Enemigos (navegación y dañable)
    // Debris       - Escombros, muertos
    // Sensor       - Llaves, botones, puertas, activadores, saltadores, escudo
    // Character    - Jugador
    // Custom1      - Proyectiles enemigos simples
    // Custom2      - Proyectiles enemigos persecutores

    // Juego
    public enum Escenas
    {
        menú,
        demo,
        //supervivencia,
        E1M1,
        E1M2,
        E1M3,
        E1M4
    }

    public enum Armas
    {
        espada,
        escopeta,
        metralleta,
        rifle,
        lanzagranadas
    }

    public enum Llaves
    {
        nada,
        azul,
        roja
    }

    public enum Poderes
    {
        nada,
        daño,
        invulnerabilidad,
        velocidad
    }

    public enum TipoDisparo
    {
        espejo,
        izquierda,
        derecha
    }

    public enum Enemigos
    {
        nada,
        meléLigero,     // Zombi
        meléMediano,    // Lancero
        meléPesado,     // Carnicero

        rangoLigero,    // Pulga
        rangoMediano,   // Babosa
        rangoPesado,    // Araña

        especialLigero, // Dron
        especialMediano,// 
        especialPesado, // Cerebro
    }

    public enum Jefes
    {

    }

    // Configuración
    public enum TipoCurva
    {
        nada,
        suave,
        rápida
    }

    public enum Direcciones
    {
        arriba,
        abajo,
        izquierda,
        derecha
    }

    public enum Idiomas
    {
        sistema,
        español,
        inglés
    }

    public enum Configuraciones
    {
        idioma,
        gráficos,
        sombras,
        pantallaCompleta,
        resolución,
        vSync,
        volumenGeneral,
        volumenMúsica,
        volumenEfectos,
        hrtf,
        sensibilidad,
        campoVisión,
        datos
    }

    public enum NivelesConfiguración
    {
        bajo,
        medio,
        alto
    }
}
