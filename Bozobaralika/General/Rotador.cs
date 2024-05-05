using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;

public class Rotador : AsyncScript
{
    public float ánguloY;

    public override async Task Execute()
    {
        while(Game.IsRunning)
        {
            Entity.Transform.Rotation *= Quaternion.RotationY(ánguloY * (float)Game.UpdateTime.WarpElapsed.TotalSeconds);
            await Script.NextFrame();
        }
    }
}
