using System;
using Stride.Core.Mathematics;

namespace Bozobaralika;
using static Constantes;

public interface IProyectil
{
    void Iniciar(float daño, float velocidad, Quaternion rotación, Vector3 posición, Enemigos disparador);
    void IniciarPersecutor(float velocidadRotación, Vector3 altura);
    void AsignarImpacto(Action<Vector3, Vector3> inciarImpacto);
    void Destruir();
}
