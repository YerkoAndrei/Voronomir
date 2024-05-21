using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Stride.Engine;
using Newtonsoft.Json;

namespace Voronomir;
using static Utilidades;
using static Constantes;

public class SistemaMemoria : StartupScript
{
    private static string carpetaPersistente = "C:\\Users\\{0}\\AppData\\LocalLow\\{1}\\{2}";
    private static string desarrollador = "YerkoAndrei";
    private static string producto = "Voronomir";

    private static string archivoConfiguración = "Configuración";
    private static string archivoMáximos = "Máximos";
    private static string rutaConfiguración;
    private static string rutaMáximos;

    public static Dificultades Dificultad;

    public override void Start()
    {
        EstablecerRutas();

        // Dificultad
        Dificultad = (Dificultades)Enum.Parse(typeof(Dificultades), ObtenerConfiguración(Configuraciones.dificultad));
    }

    private static void EstablecerRutas()
    {
        carpetaPersistente = string.Format(carpetaPersistente, Environment.UserName, desarrollador, producto);
        rutaConfiguración = Path.Combine(carpetaPersistente, archivoConfiguración);
        rutaMáximos = Path.Combine(carpetaPersistente, archivoMáximos);
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
        var json = JsonConvert.SerializeObject(ObtenerConfiguraciónPredeterminada(ancho, alto));
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
            { Configuraciones.gráficos.ToString(),          NivelesCalidad.alto.ToString() },
            { Configuraciones.sombras.ToString(),           NivelesCalidad.alto.ToString() },
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
        var json = JsonConvert.SerializeObject(configuraciones);
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
            configuraciones = JsonConvert.DeserializeObject<Dictionary<string, string>>(desencriptado);
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
    public static void GuardarMáximo(Escenas escena, float segundos)
    {
        var diccionarioTiempo = new Dictionary<string, float>();

        // Lee archivo
        if (File.Exists(rutaMáximos))
        {
            var archivo = File.ReadAllText(rutaMáximos);
            var desencriptado = DesEncriptar(archivo);
            diccionarioTiempo = JsonConvert.DeserializeObject<Dictionary<string, float>>(desencriptado);

            // Solo tiempo menores
            if (segundos >= diccionarioTiempo[escena.ToString()])
                return;
        }

        // Sobreescribe nuevo máximo
        diccionarioTiempo[escena.ToString()] = segundos;
        var json = JsonConvert.SerializeObject(diccionarioTiempo);
        var encriptado = DesEncriptar(json);
        File.WriteAllText(rutaMáximos, encriptado);
    }

    public static string ObtenerMáximo(Escenas escena)
    {
        // Lee archivo
        if (!File.Exists(rutaMáximos))
            return string.Empty;

        var archivo = File.ReadAllText(rutaMáximos);
        var desencriptado = DesEncriptar(archivo);
        var diccionarioTiempo = JsonConvert.DeserializeObject<Dictionary<string, float>>(desencriptado);

        if(!diccionarioTiempo.ContainsKey(escena.ToString()))
            return string.Empty;

        return FormatearTiempo(diccionarioTiempo[escena.ToString()]);
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
