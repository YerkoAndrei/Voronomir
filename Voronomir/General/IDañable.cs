using System.Collections.Generic;
using Stride.Core.Mathematics;

namespace Voronomir;

public interface IDañable
{
    public List<ElementoDañable> dañables { get; set; }
    public void RecibirDaño(float daño);
    public void Empujar(Vector3 dirección);
}
