using System.Collections.Generic;
using Stride.Core.Mathematics;
using Stride.Physics;

namespace Bozobaralika;

public interface IDañable
{
    public List<RigidbodyComponent> cuerpos { get; set; }
    public void RecibirDaño(float daño);
    public void Empujar(Vector3 dirección);
}
