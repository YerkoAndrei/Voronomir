using Stride.Engine;

namespace Bozobaralika;

public class ControladorPartida : SyncScript
{
    private bool activo;
    private float tiempo;

    public override void Start()
    {
        activo = true;
        tiempo = 0;

        // Debug
        Input.LockMousePosition(true);
        Game.IsMouseVisible = false;
    }

    public override void Update()
    {
        if (!activo)
            return;

        tiempo += (float)Game.UpdateTime.Elapsed.TotalSeconds;
    }

    public void Perder()
    {
        activo = false;
    }

    public float ObtenerTiempo()
    {
        return tiempo;
    }
}
