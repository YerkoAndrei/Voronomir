﻿using Stride.Core.Mathematics;
using Stride.Engine;
using System.Linq;

namespace Bozobaralika;

public class AnimadorDron : StartupScript, IAnimador
{
    public TransformComponent modelo;

	private Vector3 dirección;

	public void Iniciar()
    {

	}

    public void Actualizar()
    {
        dirección = Vector3.Normalize(ControladorPartida.ObtenerCabezaJugador() - modelo.WorldMatrix.TranslationVector);
        modelo.Rotation = Quaternion.Lerp(modelo.Rotation, Quaternion.LookRotation(dirección, Vector3.UnitY), 10 * SistemaAnimación.TiempoTranscurrido());
    }

    public void Caminar(float velocidad)
    {

	}

    public void Atacar()
    {

    }

}
