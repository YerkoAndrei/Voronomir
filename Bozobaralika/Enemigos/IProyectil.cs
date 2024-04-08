using System;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;

public interface IProyectil
{
    void Iniciar(float daño, float velocidad, Quaternion rotación, Vector3 posición, PhysicsComponent[] disparador);
    void IniciarPersecutor(float velocidadRotación, Vector3 altura);
    void AsignarImpacto(Action<Vector3, Vector3, bool> inciarImpacto);
    void Desviar(Vector3 dirección);
}
