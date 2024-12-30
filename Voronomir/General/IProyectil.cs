using Stride.Core.Mathematics;

namespace Voronomir;
using static Constantes;

public interface IProyectil
{
    void Iniciar(float daño, float velocidad, Quaternion rotación, Vector3 posición, Enemigos disparador);
    void IniciarPersecutor(float velocidadRotación, Vector3 altura);
    void Apagar();
    void Destruir();
}
