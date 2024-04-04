using System;
using System.Collections.Generic;
using System.Linq;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;

public class ControladorPersecusionesTrigonométricas : SyncScript
{
    private TransformComponent jugador;
    private Vector3[] posicionesCirculares;
    private List<ControladorPersecusión> persecutores;

    public override void Start()
    {
        jugador = Entity.Scene.Entities.Where(o => o.Get<ControladorJugador>() != null).FirstOrDefault().Transform;
        persecutores = new List<ControladorPersecusión>();
    }

    public override void Update()
    {
        if (!PosibleRodearJugador())
            return;
        
        // Circulizar posiciones con trigonometría
        var círculoRelativo = 360f / persecutores.Count;
        var radioEsfera = 4f;
        posicionesCirculares = new Vector3[persecutores.Count];

        for (int i = 0; i < persecutores.Count; i++)
        {
            var ángulo = i * MathUtil.DegreesToRadians(círculoRelativo);

            Vector3 dirección = new Vector3(MathF.Cos(ángulo), 0, MathF.Sin(ángulo));
            dirección = Vector3.Normalize(dirección) * radioEsfera;

            posicionesCirculares[i] = (jugador.WorldMatrix.TranslationVector + dirección);
        }
    }

    public Vector3 ObtenerPosiciónCircular(int índice)
    {
        // Persecutores pueden descordinarse por 1 cuadro
        if (índice >= posicionesCirculares.Count())
            return jugador.WorldMatrix.TranslationVector;

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
