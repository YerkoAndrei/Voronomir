using System.Collections.Generic;
using Stride.Core.Mathematics;
using Stride.Physics;

namespace Voronomir;

public interface IDañable
{
    public List<RigidbodyComponent> dañables { get; set; }
    public void RecibirDaño(float daño);
    public void Empujar(Vector3 dirección);
}
