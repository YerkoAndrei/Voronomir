using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using Stride.Engine;

namespace Voronomir;
using static Utilidades;
using static Constantes;

public class SistemaMemoria : StartupScript
{
    private static string carpetaPersistente = string.Empty;
    private static string desarrollador = "YerkoAndrei";
    private static string producto = "Voronomir";

    private static string archivoConfiguración = "Configuración";
    private static string archivoTiempos = "Tiempos";
    private static string rutaConfiguración;
    private static string rutaTiempos;

    public static Dificultades Dificultad;

    public override void Start()
    {
        EstablecerRutas();

        // Dificultad
        Dificultad = (Dificultades)Enum.Parse(typeof(Dificultades), ObtenerConfiguración(Configuraciones.dificultad));
    }

    private static void EstablecerRutas()
    {
        carpetaPersistente = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), desarrollador, producto);
        rutaConfiguración = Path.Combine(carpetaPersistente, archivoConfiguración);
        rutaTiempos = Path.Combine(carpetaPersistente, archivoTiempos);
    }

    // Configuración
    public static void EstablecerConfiguraciónPredeterminada(int ancho, int alto)
    {
        EstablecerRutas();

        if (!Directory.Exists(carpetaPersistente))
            Directory.CreateDirectory(carpetaPersistente);

        if (File.Exists(rutaConfiguración))
            return;

        // Guarda valores predeterminados
        var json = JsonSerializer.Serialize(ObtenerConfiguraciónPredeterminada(ancho, alto));
        var encriptado = DesEncriptar(json);
        File.WriteAllText(rutaConfiguración, encriptado);
    }

    private static Dictionary<string, string> ObtenerConfiguraciónPredeterminada(int ancho, int alto)
    {
        // Resolución original
        var resolución = "1280x720";
        if (ancho > 0 && alto > 0)
            resolución = ancho.ToString() + "x" + alto.ToString();

        return new Dictionary<string, string>
        {
            { Configuraciones.idioma.ToString(),            Idiomas.sistema.ToString() },
            { Configuraciones.dificultad.ToString(),        Dificultades.normal.ToString() },
            { Configuraciones.gráficos.ToString(),          Calidades.alto.ToString() },
            { Configuraciones.efectos.ToString(),           Calidades.medio.ToString() },
            { Configuraciones.sombras.ToString(),           Calidades.alto.ToString() },
            { Configuraciones.pantallaCompleta.ToString(),  true.ToString() },
            { Configuraciones.resolución.ToString(),        resolución },
            { Configuraciones.vSync.ToString(),             false.ToString() },
            { Configuraciones.volumenGeneral.ToString(),    "1" },
            { Configuraciones.volumenMúsica.ToString(),     "0.5" },
            { Configuraciones.volumenEfectos.ToString(),    "0.5" },
            { Configuraciones.hrtf.ToString(),              true.ToString() },
            { Configuraciones.sensibilidad.ToString(),      "1.00" },
            { Configuraciones.campoVisión.ToString(),       "90" },
            { Configuraciones.colorMira.ToString(),         "100,255,100" },
            { Configuraciones.datos.ToString(),             false.ToString() },
        };
    }

    public static void GuardarConfiguración(Configuraciones configuración, string valor)
    {
        var configuraciones = ObtenerConfiguraciones();

        // Nuevo o remplazo
        configuraciones[configuración.ToString()] = valor;

        // Sobreescribe archivo
        var json = JsonSerializer.Serialize(configuraciones);
        var encriptado = DesEncriptar(json);
        File.WriteAllText(rutaConfiguración, encriptado);
    }

    private static Dictionary<string, string> ObtenerConfiguraciones()
    {
        if (!Directory.Exists(carpetaPersistente))
            Directory.CreateDirectory(carpetaPersistente);

        var configuraciones = ObtenerConfiguraciónPredeterminada(1280, 720);

        // Lee archivo
        if (File.Exists(rutaConfiguración))
        {
            var archivo = File.ReadAllText(rutaConfiguración);
            var desencriptado = DesEncriptar(archivo);
            configuraciones = JsonSerializer.Deserialize<Dictionary<string, string>>(desencriptado);
        }
        return configuraciones;
    }

    public static string ObtenerConfiguración(Configuraciones llave)
    {
        var configuraciones = ObtenerConfiguraciones();
        return configuraciones[llave.ToString()];
    }

    public static bool ObtenerExistenciaArchivo()
    {
        if (!Directory.Exists(carpetaPersistente))
            return false;

        return File.Exists(rutaConfiguración);
    }

    // Partidas
    public static void GuardarTiempo(Escenas escena, float segundos)
    {
        var tiempos = new Tiempos();

        // Lee archivo
        if (File.Exists(rutaTiempos))
        {
            var archivo = File.ReadAllText(rutaTiempos);
            var desencriptado = DesEncriptar(archivo);
            tiempos = JsonSerializer.Deserialize<Tiempos>(desencriptado);
        }

        // Sobreescribe si es menor o crea si no existe
        switch (Dificultad)
        {
            case Dificultades.fácil:
                if (!tiempos.TiemposFácil.ContainsKey(escena.ToString()) || segundos < tiempos.TiemposFácil[escena.ToString()])
                    tiempos.TiemposFácil[escena.ToString()] = segundos;
                break;
            case Dificultades.normal:
                if (!tiempos.TiemposNormal.ContainsKey(escena.ToString()) || segundos < tiempos.TiemposNormal[escena.ToString()])
                    tiempos.TiemposNormal[escena.ToString()] = segundos;
                break;
            case Dificultades.difícil:
                if (!tiempos.TiemposDifícil.ContainsKey(escena.ToString()) || segundos < tiempos.TiemposDifícil[escena.ToString()])
                    tiempos.TiemposDifícil[escena.ToString()] = segundos;
                break;
        }

        var json = JsonSerializer.Serialize(tiempos);
        var encriptado = DesEncriptar(json);
        File.WriteAllText(rutaTiempos, encriptado);
    }

    public static string ObtenerTiempo(Escenas escena)
    {
        // Lee archivo
        if (!File.Exists(rutaTiempos))
            return string.Empty;

        var archivo = File.ReadAllText(rutaTiempos);
        var desencriptado = DesEncriptar(archivo);
        var tiempos = JsonSerializer.Deserialize<Tiempos>(desencriptado);
        var diccionario = new Dictionary<string, float>();

        switch (Dificultad)
        {
            case Dificultades.fácil:
                diccionario = tiempos.TiemposFácil;
                break;
            case Dificultades.normal:
                diccionario = tiempos.TiemposNormal;
                break;
            case Dificultades.difícil:
                diccionario = tiempos.TiemposDifícil;
                break;
        }

        if (!diccionario.ContainsKey(escena.ToString()))
            return string.Empty;

        return FormatearTiempo(diccionario[escena.ToString()]);
    }

    // XOR
    private static string DesEncriptar(string texto)
    {
        var entrada = new StringBuilder(texto);
        var salida = new StringBuilder(texto.Length);
        char c;

        for (int i = 0; i < texto.Length; i++)
        {
            c = entrada[i];
            c = (char)(c ^ 08021996);
            salida.Append(c);
        }
        return salida.ToString();
    }
}

public class Tiempos
{
    public Dictionary<string, float> TiemposFácil {  get; set; }
    public Dictionary<string, float> TiemposNormal { get; set; }
    public Dictionary<string, float> TiemposDifícil { get; set; }

    public Tiempos()
    {
        TiemposFácil = new Dictionary<string, float>();
        TiemposNormal = new Dictionary<string, float>();
        TiemposDifícil = new Dictionary<string, float>();
    }
}