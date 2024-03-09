using Stride.Input;
using Stride.Engine;

namespace Bozobaralika;
using static Constantes;

public class ControladorArmas : SyncScript
{

    public override void Start()
    {

    }

    public override void Update()
    {
        if (Input.IsMouseButtonPressed(MouseButton.Left))
            Disparar();

        if (Input.IsKeyDown(Keys.D1) || Input.IsKeyDown(Keys.NumPad1))
            CambiarArma(Arma.pistola);
        if (Input.IsKeyDown(Keys.D2) || Input.IsKeyDown(Keys.NumPad2))
            CambiarArma(Arma.escopeta);
        if (Input.IsKeyDown(Keys.D3) || Input.IsKeyDown(Keys.NumPad3))
            CambiarArma(Arma.metralleta);
        if (Input.IsKeyDown(Keys.D4) || Input.IsKeyDown(Keys.NumPad4))
            CambiarArma(Arma.rifle);
    }

    private void Disparar()
    {

    }

    private void CambiarArma(Arma nuevaArma)
    {

    }
}
