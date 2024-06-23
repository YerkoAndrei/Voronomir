using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using Stride.Core.Serialization;
using Stride.Engine;
using System.Globalization;

namespace Voronomir;
using static Constantes;

public class SistemaTraducción : StartupScript
{
    private static SistemaTraducción instancia;

    private static string jsonEspañol;
    private static string jsonInglés;

    public static Idiomas Idioma;
    public static CultureInfo Cultura;

    private static Dictionary<string, string> diccionario;

    public UrlReference textosEspañol;
    public UrlReference textosInglés;

    public override void Start()
    {
        instancia = this;

        // Lee RawAsset como textos
        using (var stream = Content.OpenAsStream(textosEspañol))
        {
            using (var reader = new StreamReader(stream))
            {
                jsonEspañol = reader.ReadToEnd();
            }
        }
        using (var stream = Content.OpenAsStream(textosInglés))
        {
            using (var reader = new StreamReader(stream))
            {
                jsonInglés = reader.ReadToEnd();
            }
        }

        // Usa guardada o idioma de sistema la primera vez
        var idiomaGuardado = (Idiomas)Enum.Parse(typeof(Idiomas), SistemaMemoria.ObtenerConfiguración(Configuraciones.idioma));
        if (idiomaGuardado == Idiomas.sistema)
        {
            // Revisa el idioma instalado. ISO 639-1
            var lenguajeSistema = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
            switch (lenguajeSistema)
            {
                default:
                case "es":
                    CambiarIdioma(Idiomas.español, false);
                    break;
                case "en":
                    CambiarIdioma(Idiomas.inglés, false);
                    break;
            }
        }
        else
            CambiarIdioma(idiomaGuardado, false);
    }

    public static void CambiarIdioma(Idiomas nuevoIdioma, bool actualizar = true)
    {
        Idioma = nuevoIdioma;
        SistemaMemoria.GuardarConfiguración(Configuraciones.idioma, Idioma.ToString());

        switch (nuevoIdioma)
        {
            default:
            case Idiomas.español:
                Cultura = CultureInfo.GetCultureInfo("es-cl");
                diccionario = CrearDiccionario(jsonEspañol);
                break;
            case Idiomas.inglés:
                Cultura = CultureInfo.GetCultureInfo("en");
                diccionario = CrearDiccionario(jsonInglés);
                break;
        }

        if (actualizar)
            ActualizarTextosEscena();
    }

    private static Dictionary<string, string> CrearDiccionario(string traducciones)
    {
        var diccionario = JsonSerializer.Deserialize<Dictionary<string, string>>(traducciones);
        return diccionario;
    }

    public static void ActualizarTextosEscena()
    {
        // Busca controladores de traducción en escena hija (SistemaEscena solo permite una escena hija al mismo tiempo)
        var entidades = instancia.SceneSystem.SceneInstance.RootScene.Children[0].Entities.Where(o => o.Get<InterfazTraducciones>() != null).ToArray();
        foreach (var entidad in entidades)
        {
            var traductores = entidad.GetAll<InterfazTraducciones>();
            foreach (var traductor in traductores)
            {
                traductor.Traducir();
            }
        }
    }

    public static string ObtenerTraducción(string código)
    {
        if (diccionario.ContainsKey(código))
            return diccionario[código];
        else
            return "¿?";
    }
}
