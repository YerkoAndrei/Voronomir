using Stride.Engine;

namespace Voronomir
{
    class VoronomirApp
    {
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
