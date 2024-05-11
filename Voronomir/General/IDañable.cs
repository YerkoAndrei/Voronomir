using Stride.Core.Mathematics;

namespace Voronomir;

public interface IDañable
{
    public void RecibirDaño(float daño);
    public void Empujar(Vector3 dirección);
}
