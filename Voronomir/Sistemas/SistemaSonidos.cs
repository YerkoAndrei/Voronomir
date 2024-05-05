using System.Globalization;
using Stride.Audio;
using Stride.Engine;

namespace Voronomir;
using static Constantes;

public class SistemaSonidos : StartupScript
{
    //public Sound música;

    public Sound sonidoBotónEntra;
    public Sound sonidoBotónSale;

    public Sound sonidoBotón;
    public Sound sonidoLlave;
    public Sound sonidoPuerta;
    public Sound sonidoCerrado;
    public Sound sonidoSecreto;
    public Sound sonidoSaltador;

    public Sound sonidoDaño;
    public Sound sonidoSalto;
    public Sound sonidoMorir;
    public Sound sonidoFinalizar;

    public Sound sonidoEspada;
    public Sound sonidoEscopeta;
    public Sound sonidoMetralleta;
    public Sound sonidoRifle;
    public Sound sonidoLanzagranadas;

    public Sound sonidoPoderDaño;
    public Sound sonidoPoderInvulnerabilidad;
    public Sound sonidoPoderVelocidad;

    private static SistemaSonidos instancia;

    //private SoundInstance música;

    private SoundInstance botónEntra;
    private SoundInstance botónSale;

    private SoundInstance botón;
    private SoundInstance llave;
    private SoundInstance puerta;
    private SoundInstance cerrado;
    private SoundInstance secreto;
    private SoundInstance saltador;

    private SoundInstance daño;
    private SoundInstance salto;
    private SoundInstance morir;
    private SoundInstance finalizar;

    private SoundInstance espada;
    private SoundInstance escopeta;
    private SoundInstance metralleta;
    private SoundInstance rifle;
    private SoundInstance lanzagranadas;

    private SoundInstance poderDaño;
    private SoundInstance poderInvulnerabilidad;
    private SoundInstance poderVelocidad;

    public override void Start()
    {
        instancia = this;

        botónEntra = sonidoBotónEntra.CreateInstance();
        botónSale = sonidoBotónSale.CreateInstance();

        botón = sonidoBotón.CreateInstance();
        llave = sonidoLlave.CreateInstance();
        puerta = sonidoPuerta.CreateInstance();
        cerrado = sonidoCerrado.CreateInstance();
        secreto = sonidoSecreto.CreateInstance();
        saltador = sonidoSaltador.CreateInstance();

        daño = sonidoDaño.CreateInstance();
        salto = sonidoSalto.CreateInstance();
        morir = sonidoMorir.CreateInstance();
        finalizar = sonidoFinalizar.CreateInstance();

        espada = sonidoDaño.CreateInstance();
        escopeta = sonidoMorir.CreateInstance();
        metralleta = sonidoSalto.CreateInstance();
        rifle = sonidoMorir.CreateInstance();
        lanzagranadas = sonidoMorir.CreateInstance();

        poderDaño = sonidoPoderDaño.CreateInstance();
        poderInvulnerabilidad = sonidoPoderInvulnerabilidad.CreateInstance();
        poderVelocidad = sonidoPoderVelocidad.CreateInstance();
    }

    // Mezclador
    public static float ObtenerVolumen(Configuraciones volumen)
    {
        var volumenGeneral = float.Parse(SistemaMemoria.ObtenerConfiguración(Configuraciones.volumenGeneral), CultureInfo.InvariantCulture);
        switch (volumen)
        {
            default:
                return 0;
            case Configuraciones.volumenMúsica:
                var volumenMúsica = float.Parse(SistemaMemoria.ObtenerConfiguración(Configuraciones.volumenMúsica), CultureInfo.InvariantCulture);
                return volumenGeneral * volumenMúsica;
            case Configuraciones.volumenEfectos:
                var volumenEfectos = float.Parse(SistemaMemoria.ObtenerConfiguración(Configuraciones.volumenEfectos), CultureInfo.InvariantCulture);
                return volumenGeneral * volumenEfectos;
        }
    }

    // Botones
    public static void SonarBotónEntra()
    {
        instancia.botónEntra.Stop();
        instancia.botónEntra.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        instancia.botónEntra.PlayExclusive();
    }

    public static void SonarBotónSale()
    {
        instancia.botónSale.Stop();
        instancia.botónSale.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        instancia.botónSale.PlayExclusive();
    }

    // Juego
    public static void SonarBotón()
    {
        instancia.botón.Stop();
        instancia.botón.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        instancia.botón.PlayExclusive();
    }

    public static void SonarLlave()
    {
        instancia.llave.Stop();
        instancia.llave.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        instancia.llave.PlayExclusive();
    }

    public static void SonarPuerta()
    {
        instancia.puerta.Stop();
        instancia.puerta.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        instancia.puerta.PlayExclusive();
    }

    public static void SonarCerrado()
    {
        instancia.cerrado.Stop();
        instancia.cerrado.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        instancia.cerrado.PlayExclusive();
    }

    public static void SonarSecreto()
    {
        instancia.secreto.Stop();
        instancia.secreto.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        instancia.secreto.PlayExclusive();
    }

    public static void SonarSaltador()
    {
        instancia.saltador.Stop();
        instancia.saltador.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        instancia.saltador.PlayExclusive();
    }

    public static void SonarDaño()
    {
        instancia.daño.Stop();
        instancia.daño.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        instancia.daño.PlayExclusive();
    }

    public static void SonarSalto()
    {
        instancia.salto.Stop();
        instancia.salto.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        instancia.salto.PlayExclusive();
    }

    public static void SonarMorir()
    {
        instancia.morir.Stop();
        instancia.morir.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        instancia.morir.PlayExclusive();
    }

    public static void SonarFinalizar()
    {
        instancia.finalizar.Stop();
        instancia.finalizar.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        instancia.finalizar.PlayExclusive();
    }

    public static void SonarDisparo(Armas arma)
    {
        switch (arma)
        {
            case Armas.espada:
                instancia.espada.Stop();
                instancia.espada.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
                instancia.espada.PlayExclusive();
                break;
            case Armas.escopeta:
                instancia.escopeta.Stop();
                instancia.escopeta.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
                instancia.escopeta.PlayExclusive();
                break;
            case Armas.metralleta:
                instancia.metralleta.Stop();
                instancia.metralleta.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
                instancia.metralleta.PlayExclusive();
                break;
            case Armas.rifle:
                instancia.rifle.Stop();
                instancia.rifle.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
                instancia.rifle.PlayExclusive();
                break;
            case Armas.lanzagranadas:
                instancia.lanzagranadas.Stop();
                instancia.lanzagranadas.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
                instancia.lanzagranadas.PlayExclusive();
                break;
        }
    }

    public static void SonarPoder(Poderes poder)
    {
        switch (poder)
        {
            case Poderes.daño:
                instancia.poderDaño.Stop();
                instancia.poderDaño.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
                instancia.poderDaño.PlayExclusive();
                break;
            case Poderes.invulnerabilidad:
                instancia.poderInvulnerabilidad.Stop();
                instancia.poderInvulnerabilidad.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
                instancia.poderInvulnerabilidad.PlayExclusive();
                break;
            case Poderes.velocidad:
                instancia.poderVelocidad.Stop();
                instancia.poderVelocidad.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
                instancia.poderVelocidad.PlayExclusive();
                break;
        }
    }

    /*
    public static void SonarPitch()
    {
        instancia.algo.Stop();
        instancia.algo.Volume = MathUtil.Clamp(volumen, 0, 1);
        instancia.algo.Pitch = RangoAleatorio(0.8f, 1.2f);
        instancia.algo.Play();
    }*/
}
