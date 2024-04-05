﻿using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;

public class ControladorArmaRango: StartupScript
{
    public ControladorEnemigo controlador;
    public Prefab prefabDisparo;

    private ElementoDisparo[] disparos;
    private int disparoActual;
    private int maxDisparos;

    public void Iniciar()
    {
        maxDisparos = 8;
        disparos = new ElementoDisparo[maxDisparos];
        for (int i = 0; i < maxDisparos; i++)
        {
            var marca = prefabDisparo.Instantiate()[0];
            disparos[i] = marca.Get<ElementoDisparo>();
            Entity.Scene.Entities.Add(marca);
        }
    }

    public void Disparar(float daño, float velocidad, float duración, Vector3 objetivo)
    {
        var dirección = Vector3.Normalize(Entity.Transform.WorldMatrix.TranslationVector - objetivo);
        var rotación = Quaternion.LookRotation(dirección, Vector3.UnitY);

        disparos[disparoActual].Iniciar(daño, velocidad, duración, rotación, Entity.Transform.WorldMatrix.TranslationVector);
        disparoActual++;

        if (disparoActual >= maxDisparos)
            disparoActual = 0;
    }
}
