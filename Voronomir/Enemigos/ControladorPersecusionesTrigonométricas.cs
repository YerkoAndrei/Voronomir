﻿using System;
using System.Collections.Generic;
using System.Linq;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Voronomir;
using static Constantes;

public class ControladorPersecusionesTrigonométricas : SyncScript
{
    public Enemigos enemigo;

    private List<ControladorPersecusión> persecutores;
    private Vector3[] posicionesCirculares;
    private float radio;

    private Vector3 dirección;
    private float ángulo;

    public override void Start()
    {
        persecutores = new List<ControladorPersecusión>();
        radio = ControladorEnemigo.ObtenerDistanciaPersecuciónTrigonométrica(enemigo);
    }

    public override void Update()
    {
        if (!PosibleRodearJugador())
            return;
        
        // Circulizar posiciones con trigonometría
        posicionesCirculares = new Vector3[persecutores.Count];

        for (int i = 0; i < persecutores.Count; i++)
        {
            ángulo = i * MathUtil.DegreesToRadians(360f / persecutores.Count);
            dirección = new Vector3(MathF.Cos(ángulo), 0, MathF.Sin(ángulo));
            dirección = Vector3.Normalize(dirección) * radio;

            posicionesCirculares[i] = (ControladorJuego.ObtenerPosiciónJugador() + dirección);
        }
    }

    public Vector3 ObtenerPosiciónCircular(int índice)
    {
        // Persecutores pueden descordinarse por 1 cuadro
        if (posicionesCirculares == null || índice >= posicionesCirculares.Count())
            return ControladorJuego.ObtenerPosiciónJugador();

        return posicionesCirculares[índice];
    }

    public int ObtenerÍndiceTrigonométrico(ControladorPersecusión persecutor)
    {
        persecutores.Add(persecutor);
        return (persecutores.Count - 1);
    }

    public void EliminarPersecutor(ControladorPersecusión persecutor)
    {
        persecutores.Remove(persecutor);
        for (int i = 0; i < persecutores.Count; i++)
        {
            persecutores[i].ReasignarÍndice(i);
        }
    }

    public bool PosibleRodearJugador()
    {
        return persecutores.Count > 2;
    }
}
