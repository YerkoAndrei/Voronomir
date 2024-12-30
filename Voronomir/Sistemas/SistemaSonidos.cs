using System.Globalization;
using System.Threading.Tasks;
using Stride.Audio;
using Stride.Engine;

namespace Voronomir;
using static Constantes;

public class SistemaSonidos : AsyncScript
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

    private static SoundInstance botónEntra;
    private static SoundInstance botónSale;

    private static SoundInstance botón;
    private static SoundInstance llave;
    private static SoundInstance puerta;
    private static SoundInstance cerrado;
    private static SoundInstance secreto;
    private static SoundInstance saltador;

    private static SoundInstance daño;
    private static SoundInstance salto;
    private static SoundInstance morir;
    private static SoundInstance finalizar;

    private static SoundInstance espada;
    private static SoundInstance escopeta;
    private static SoundInstance metralleta;
    private static SoundInstance rifle;
    private static SoundInstance lanzagranadas;

    private static SoundInstance poderDaño;
    private static SoundInstance poderInvulnerabilidad;
    private static SoundInstance poderVelocidad;

    private static ISonidoMundo[] sonidosMundo;

    public override async Task Execute()
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

        while (Game.IsRunning)
        {
            if (ControladorJuego.ObtenerActivo() && sonidosMundo != null)
                ActualizarVolúmenesMundo();

            await Script.NextFrame();
        }
    }

    public static void ActualizarVolúmenesMundo()
    {
        foreach (var sonido in sonidosMundo)
        {
            sonido.ActualizarVolumen();
        }
    }

    public static void PausarSonidosMundo(bool pausa)
    {
        foreach (var sonido in sonidosMundo)
        {
            sonido.PausarSonidos(pausa);
        }
    }

    public static void AsignarSonidosMundo(ISonidoMundo[] _sonidosMundo)
    {
        sonidosMundo = _sonidosMundo;
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
        botónEntra.Stop();
        botónEntra.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        botónEntra.PlayExclusive();
    }

    public static void SonarBotónSale()
    {
        botónSale.Stop();
        botónSale.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        botónSale.PlayExclusive();
    }

    // Juego
    public static void SonarBotón()
    {
        botón.Stop();
        botón.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        botón.PlayExclusive();
    }

    public static void SonarLlave()
    {
        llave.Stop();
        llave.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        llave.PlayExclusive();
    }

    public static void SonarPuerta()
    {
        puerta.Stop();
        puerta.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        puerta.PlayExclusive();
    }

    public static void SonarCerrado()
    {
        cerrado.Stop();
        cerrado.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        cerrado.PlayExclusive();
    }

    public static void SonarSecreto()
    {
        secreto.Stop();
        secreto.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        secreto.PlayExclusive();
    }

    public static void SonarSaltador()
    {
        saltador.Stop();
        saltador.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        saltador.PlayExclusive();
    }

    public static void SonarDaño()
    {
        daño.Stop();
        daño.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        daño.PlayExclusive();
    }

    public static void SonarSalto()
    {
        salto.Stop();
        salto.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        salto.PlayExclusive();
    }

    public static void SonarMorir()
    {
        morir.Stop();
        morir.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        morir.PlayExclusive();
    }

    public static void SonarFinalizar()
    {
        finalizar.Stop();
        finalizar.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
        finalizar.PlayExclusive();
    }

    public static void SonarDisparo(Armas arma)
    {
        switch (arma)
        {
            case Armas.espada:
                espada.Stop();
                espada.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
                espada.PlayExclusive();
                break;
            case Armas.escopeta:
                escopeta.Stop();
                escopeta.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
                escopeta.PlayExclusive();
                break;
            case Armas.metralleta:
                metralleta.Stop();
                metralleta.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
                metralleta.PlayExclusive();
                break;
            case Armas.rifle:
                rifle.Stop();
                rifle.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
                rifle.PlayExclusive();
                break;
            case Armas.lanzagranadas:
                lanzagranadas.Stop();
                lanzagranadas.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
                lanzagranadas.PlayExclusive();
                break;
        }
    }

    public static void SonarPoder(Poderes poder)
    {
        switch (poder)
        {
            case Poderes.daño:
                poderDaño.Stop();
                poderDaño.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
                poderDaño.PlayExclusive();
                break;
            case Poderes.invulnerabilidad:
                poderInvulnerabilidad.Stop();
                poderInvulnerabilidad.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
                poderInvulnerabilidad.PlayExclusive();
                break;
            case Poderes.velocidad:
                poderVelocidad.Stop();
                poderVelocidad.Volume = ObtenerVolumen(Configuraciones.volumenEfectos);
                poderVelocidad.PlayExclusive();
                break;
        }
    }

    // Música
    public static void ActualizarVolumenMúsica()
    {
        //música.Volume = ObtenerVolumen(Configuraciones.volumenMúsica);
    }
}
