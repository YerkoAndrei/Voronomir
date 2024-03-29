using System.Collections.Generic;
using Stride.Physics;

namespace Bozobaralika;

public interface IDañable
{
    public List<RigidbodyComponent> cuerpos { get; set; }
    public void RecibirDaño(float daño);
}
