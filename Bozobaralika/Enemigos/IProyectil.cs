using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;

public interface IProyectil
{
    void Iniciar(float _daño, float _velocidad, Quaternion _rotación, Vector3 _posición, PhysicsComponent[] _disparador);
    void IniciarPersecutor(float _velocidadRotación, Vector3 _altura);
}
