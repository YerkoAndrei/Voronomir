using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    private float duración;
    private float[] tiempo;
    private float[] tiempoLerp;
    private bool[] expandiendo;

    private float tiempoCaminar;
    private float tiempoLerpCaminar;
    private bool expandiendoCaminar;

    public void Iniciar()
    {
        esqueleto = modelo.Skeleton;

        idCuerpos = new int[cuerpos.Count];
        tamañosInicioCuerpos = new Vector3[cuerpos.Count];
        tamañosMaxCuerpos = new Vector3[cuerpos.Count];

        duración = 0.5f;

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
        tiempo = new float[cuerpos.Count];
        tiempoLerp = new float[cuerpos.Count];
        expandiendo = new bool[cuerpos.Count];

        for (int i = 0; i < idCuerpos.Length; i++)
        {
            tiempoLerp[i] = RangoAleatorio(0, 1);
            expandiendo[i] = true;
        }
    }

    public void Actualizar()
    {
        for (int i = 0; i < idCuerpos.Length; i++)
        {
            tiempo[i] = SistemaAnimación.EvaluarSuave(tiempoLerp[i] / duración);
            tiempoLerp[i] += (float)Game.UpdateTime.Elapsed.TotalSeconds;

            if (expandiendo[i])
                esqueleto.NodeTransformations[idCuerpos[i]].Transform.Scale = Vector3.Lerp(tamañosInicioCuerpos[i], tamañosMaxCuerpos[i], tiempo[i]);
            else
                esqueleto.NodeTransformations[idCuerpos[i]].Transform.Scale = Vector3.Lerp(tamañosMaxCuerpos[i], tamañosInicioCuerpos[i], tiempo[i]);

            if (tiempoLerp[i] > duración)
            {
                tiempoLerp[i] = 0;
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
        tiempoCaminar = SistemaAnimación.EvaluarSuave(tiempoLerpCaminar / duración);
        tiempoLerpCaminar += (float)Game.UpdateTime.Elapsed.TotalSeconds;

        if (expandiendoCaminar)
            esqueleto.NodeTransformations[idCola].Transform.Scale = Vector3.Lerp(tamañoInicioCola, tamañoMaxCola, tiempoCaminar);
        else
            esqueleto.NodeTransformations[idCola].Transform.Scale = Vector3.Lerp(tamañoMaxCola, tamañoInicioCola, tiempoCaminar);

        if (tiempoLerpCaminar > duración)
        {
            tiempoLerpCaminar = 0;
            expandiendoCaminar = !expandiendoCaminar;
        }
    }

    public void Atacar()
    {
        AnimarAtaque();
    }

    public void Morir()
    {
        modelo.Entity.Transform.Position = new Vector3(0, 0.2f, 0.5f);
        modelo.Entity.Transform.Rotation = Quaternion.RotationX(MathUtil.DegreesToRadians(-90));

        esqueleto.NodeTransformations[idCabeza].Transform.Scale = Vector3.Zero;
        esqueleto.NodeTransformations[idCola].Transform.Scale = Vector3.Zero;
    }

    private async void AnimarAtaque()
    {
        var tamañoDisparo = Vector3.One * 0.2f;
        float duración = 0.2f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            esqueleto.NodeTransformations[idCabeza].Transform.Scale = Vector3.Lerp(tamañoDisparo, tamañoInicioCabeza, tiempo);
            
            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        // Fin
        esqueleto.NodeTransformations[idCabeza].Transform.Scale = tamañoInicioCabeza;
    }
}
