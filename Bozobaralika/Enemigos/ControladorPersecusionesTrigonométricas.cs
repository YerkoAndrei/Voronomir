using System;
using System.Collections.Generic;
using System.Linq;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;
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

        // Radio según tipo enemigo
        var controladorTemp = new ControladorEnemigo();
        controladorTemp.enemigo = enemigo;
        radio = controladorTemp.ObtenerDistanciaAtaque() - 1;
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

            posicionesCirculares[i] = (ControladorPartida.ObtenerPosiciónJugador() + dirección);
        }
    }

    public Vector3 ObtenerPosiciónCircular(int índice)
    {
        // Persecutores pueden descordinarse por 1 cuadro
        if (índice >= posicionesCirculares.Count())
            return ControladorPartida.ObtenerPosiciónJugador();

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
