﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Rendering;

namespace Voronomir;

public class AnimadorPrueba : SyncScript
{
    public TransformComponent objetivo;
    public List<string> huesos = new List<string> { };

    private ModelComponent modelo;
    private SkeletonUpdater esqueleto;

    private int iteraciones;
    private int[] idHuesos;
    private float[] longitudHuesos;

    private int cantidadHuesos;
    private Vector3[] posicionesFinales;
    private Vector3[] posicionesInversas;
    private Vector3[] posicionesRectas;

    // Pruebas
    private Vector3 objetivoPrueba0 = new Vector3(5, 0, 4);
    private Vector3 objetivoPrueba1 = new Vector3(5, 0, 0);

    private Vector3 objetivoKI0 = new Vector3(0, 0, 1);
    private Vector3 objetivoKI1 = new Vector3(0, 0, 0);
    private Vector3 baseIzq;

    public override void Start()
    {
        modelo = Entity.Get<ModelComponent>();
        esqueleto = modelo.Skeleton;
        iteraciones = 3;

        // Inicialización
        cantidadHuesos = huesos.Count;
        posicionesFinales = new Vector3[cantidadHuesos];
        posicionesInversas = new Vector3[cantidadHuesos];
        posicionesRectas = new Vector3[cantidadHuesos];
        idHuesos = new int[cantidadHuesos];
        longitudHuesos = new float[cantidadHuesos];

        // Encuentra huesos por nombre, punta debe estar al último
        for (int i = 0; i < esqueleto.Nodes.Length; i++)
        {
            for (int ii = 0; ii < cantidadHuesos; ii++)
            {
                if (esqueleto.Nodes[i].Name == huesos[ii])
                    idHuesos[ii] = i;
            }
        }

        // Longitud de huesos
        longitudHuesos[(cantidadHuesos - 1)] = 0;
        esqueleto.ResetInitialValues();
        for (int i = 0; i < (cantidadHuesos - 1); i++)
        {
            longitudHuesos[i] = Vector3.Distance(esqueleto.NodeTransformations[idHuesos[i]].Transform.Position, esqueleto.NodeTransformations[idHuesos[i + 1]].Transform.Position);
        }

        baseIzq = objetivo.Position;
        ProbarKI(objetivoKI0);
        ProbarCaminata(objetivoPrueba0);
    }

    public override void Update()
    {
        AplicarKI();
    }

    private void AplicarKI()
    {
        for (int i = 0; i < cantidadHuesos; i++)
        {
            posicionesFinales[i] = esqueleto.NodeTransformations[idHuesos[i]].Transform.Position;
        }

        for (int i = 0; i < iteraciones; i++)
        {
            // FABRIK
            posicionesFinales = PosicionarRecto(PosicionarInverso(posicionesFinales));
        }

        for (int i = 0; i < cantidadHuesos; i++)
        {
            if (i != (cantidadHuesos - 1))
                esqueleto.NodeTransformations[idHuesos[i]].Transform.Rotation = Quaternion.LookRotation(Vector3.Normalize(posicionesFinales[i] - posicionesFinales[i + 1]), Vector3.UnitY);
            else
                esqueleto.NodeTransformations[idHuesos[i]].Transform.Rotation = Quaternion.LookRotation(Vector3.Normalize(posicionesFinales[i] - objetivo.Position), Vector3.UnitY);
        }        
    }

    // FABRIK Backward
    private Vector3[] PosicionarInverso(Vector3[] _posicionesRectas)
    {
        // Cálculo desde punta
        posicionesInversas[cantidadHuesos - 1] = objetivo.Position;

        for (int i = (cantidadHuesos - 2); i >= 0; i--)
        {
            var dirección = Vector3.Normalize(posicionesInversas[i + 1] - _posicionesRectas[i]);
            posicionesInversas[i] = posicionesInversas[i + 1] + (dirección * longitudHuesos[i]);
        }
        return posicionesInversas;
    }

    // FABRIK Forward
    private Vector3[] PosicionarRecto(Vector3[] _posicionesInversas)
    {
        // Cálculo desde raíz
        posicionesRectas[0] = esqueleto.NodeTransformations[idHuesos[0]].Transform.Position;

        for (int i = 1; i < cantidadHuesos; i++)
        {
            var dirección = Vector3.Normalize(posicionesRectas[i - 1] - _posicionesInversas[i]);
            posicionesRectas[i] = posicionesRectas[i - 1] + (dirección * longitudHuesos[i - 1]);
        }
        return posicionesRectas;
    }


    private async void ProbarKI(Vector3 objetivoKI)
    {
        float duración = 0.5f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);

            if (objetivoKI == objetivoKI0)
                objetivo.Position = baseIzq + Vector3.Lerp(objetivoKI1, objetivoKI0, tiempo);
            else
                objetivo.Position = baseIzq + Vector3.Lerp(objetivoKI0, objetivoKI1, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        await Task.Delay(200);

        if (objetivoKI == objetivoKI0)
            ProbarKI(objetivoKI1);
        else
            ProbarKI(objetivoKI0);
    }

    private async void ProbarCaminata(Vector3 objetivo)
    {
        float duración = 2f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);

            if (objetivo == objetivoPrueba0)
                Entity.Transform.Position = Vector3.Lerp(objetivoPrueba1, objetivoPrueba0, tiempo);
            else
                Entity.Transform.Position = Vector3.Lerp(objetivoPrueba0, objetivoPrueba1, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        await Task.Delay(200);

        if(objetivo == objetivoPrueba0)
            ProbarCaminata(objetivoPrueba1);
        else
            ProbarCaminata(objetivoPrueba0);
    }
}
