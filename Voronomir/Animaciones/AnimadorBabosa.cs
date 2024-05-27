using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Rendering;
using Stride.Engine;

namespace Voronomir;
using static Utilidades;

public class AnimadorBabosa : StartupScript, IAnimador
{
    public ModelComponent modelo;

    public string cabeza;
    public string cola;
    public List<string> cuerpos = new List<string> { };

    private SkeletonUpdater esqueleto;
    private int idCabeza;
    private int idCola;
    private int[] idCuerpos;

    private Vector3 tamañoInicioCabeza;
    private Vector3 tamañoInicioCola;
    private Vector3 tamañoMaxCola;

    private Vector3[] tamañosInicioCuerpos;
    private Vector3[] tamañosMaxCuerpos;

    private float duraciónCaminata;
    private float duraciónPalpitar;
    private float[] tiempos;
    private float[] tiemposLerp;
    private bool[] expandiendo;

    private float tiempoCaminar;
    private float tiempoLerpCaminar;
    private bool expandiendoCaminar;

    private CancellationTokenSource tokenAtaque;

    public void Iniciar()
    {
        esqueleto = modelo.Skeleton;
        tokenAtaque = new CancellationTokenSource();

        idCuerpos = new int[cuerpos.Count];
        tamañosInicioCuerpos = new Vector3[cuerpos.Count];
        tamañosMaxCuerpos = new Vector3[cuerpos.Count];

        duraciónCaminata = 0.8f;
        duraciónPalpitar = 0.4f;

        // Encuentra huesos por nombre
        for (int i = 0; i < esqueleto.Nodes.Length; i++)
        {
            for (int ii = 0; ii < cuerpos.Count; ii++)
            {
                if (esqueleto.Nodes[i].Name == cuerpos[ii])
                {
                    idCuerpos[ii] = i;
                    tamañosInicioCuerpos[ii] = esqueleto.NodeTransformations[ii].Transform.Scale;
                    tamañosMaxCuerpos[ii] = tamañosInicioCuerpos[ii] * 1.5f;
                }
            }

            if (esqueleto.Nodes[i].Name == cabeza)
                idCabeza = i;

            if (esqueleto.Nodes[i].Name == cola)
                idCola = i;
        }

        tamañoInicioCabeza = esqueleto.NodeTransformations[idCabeza].Transform.Scale;
        tamañoInicioCola = esqueleto.NodeTransformations[idCola].Transform.Scale;
        tamañoMaxCola = tamañoInicioCola * 1.5f;

        // Aleatorización
        tiempos = new float[cuerpos.Count];
        tiemposLerp = new float[cuerpos.Count];
        expandiendo = new bool[cuerpos.Count];

        for (int i = 0; i < idCuerpos.Length; i++)
        {
            tiemposLerp[i] = RangoAleatorio(0, 1);
            expandiendo[i] = true;
        }
    }

    public void Actualizar()
    {
        // Palpitación aleatorioa del cuerpo
        for (int i = 0; i < idCuerpos.Length; i++)
        {
            tiempos[i] = SistemaAnimación.EvaluarSuave(tiemposLerp[i] / duraciónPalpitar);
            tiemposLerp[i] += (float)Game.UpdateTime.Elapsed.TotalSeconds;

            if (expandiendo[i])
                esqueleto.NodeTransformations[idCuerpos[i]].Transform.Scale = Vector3.Lerp(tamañosInicioCuerpos[i], tamañosMaxCuerpos[i], tiempos[i]);
            else
                esqueleto.NodeTransformations[idCuerpos[i]].Transform.Scale = Vector3.Lerp(tamañosMaxCuerpos[i], tamañosInicioCuerpos[i], tiempos[i]);

            if (tiemposLerp[i] > duraciónPalpitar)
            {
                tiemposLerp[i] = 0;
                expandiendo[i] = !expandiendo[i];
            }
        }
    }

    public void Activar(bool activar)
    {
        modelo.Enabled = activar;
    }

    public void Caminar(float velocidad)
    {
        tiempoCaminar = SistemaAnimación.EvaluarSuave(tiempoLerpCaminar / duraciónCaminata);
        tiempoLerpCaminar += (float)Game.UpdateTime.Elapsed.TotalSeconds;

        if (expandiendoCaminar)
            esqueleto.NodeTransformations[idCola].Transform.Scale = Vector3.Lerp(tamañoInicioCola, tamañoMaxCola, tiempoCaminar);
        else
            esqueleto.NodeTransformations[idCola].Transform.Scale = Vector3.Lerp(tamañoMaxCola, tamañoInicioCola, tiempoCaminar);

        if (tiempoLerpCaminar > duraciónCaminata)
        {
            tiempoLerpCaminar = 0;
            expandiendoCaminar = !expandiendoCaminar;
        }
    }

    public void Atacar()
    {
        tokenAtaque.Cancel();
        tokenAtaque = new CancellationTokenSource();

        AnimarAtaque(Vector3.One * 0.5f);
    }

    public void Morir()
    {
        tokenAtaque.Cancel();
        tokenAtaque = new CancellationTokenSource();

        AnimarMuerte(modelo.Entity.Transform.Position, modelo.Entity.Transform.Scale,
                     new Vector3(0, -0.2f, -0.2f), Vector3.One * 0.65f);

        esqueleto.NodeTransformations[idCabeza].Transform.Scale = Vector3.Zero;
        esqueleto.NodeTransformations[idCola].Transform.Scale = Vector3.Zero;
    }

    // Animaciones
    private async void AnimarAtaque(Vector3 tamañoObjetivo)
    {
        float duración = 0.2f;
        float tiempoLerp = 0;
        float tiempo = 0;

        var token = tokenAtaque.Token;
        while (tiempoLerp < duración)
        {
            if (token.IsCancellationRequested)
                break;

            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            esqueleto.NodeTransformations[idCabeza].Transform.Scale = Vector3.Lerp(tamañoObjetivo, tamañoInicioCabeza, tiempo);
            
            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        // Fin
        esqueleto.NodeTransformations[idCabeza].Transform.Scale = tamañoInicioCabeza;
    }

    private async void AnimarMuerte(Vector3 posiciónInicio, Vector3 tamañoInicio, Vector3 posiciónObjetivo, Vector3 tamañoObjetivo)
    {
        float duración = 0.4f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = tiempoLerp / duración;

            modelo.Entity.Transform.Position = Vector3.Lerp(posiciónInicio, posiciónObjetivo, tiempo);
            modelo.Entity.Transform.Scale = Vector3.Lerp(tamañoInicio, tamañoObjetivo, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        // Fin
        modelo.Entity.Transform.Position = posiciónObjetivo;
        modelo.Entity.Transform.Scale = tamañoObjetivo;
    }
}
