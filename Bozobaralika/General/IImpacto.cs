﻿using Stride.Core.Mathematics;

namespace Bozobaralika;

public interface IImpacto
{
    void Iniciar(Vector3 posición, Quaternion rotación, float daño);
}