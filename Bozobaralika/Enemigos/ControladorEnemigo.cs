using Stride.Engine;

namespace Bozobaralika;
using static Constantes;

public class ControladorEnemigo : SyncScript
{
    public Enemigos enemigo;

    private float vida;

    public override void Start()
    {
        switch(enemigo)
        {
            case Enemigos.meléLigero:
                vida = 100;
                break;
            case Enemigos.meléMediano:
                vida = 400;
                break;
            case Enemigos.meléPesado:
                vida = 200;
                break;
            case Enemigos.rangoLigero:
                vida = 40;
                break;
            case Enemigos.rangoMediano:
                vida = 100;
                break;
            case Enemigos.rangoPesado:
                vida = 500;
                break;

            case Enemigos.especialLigero:
                vida = 100;
                break;
            case Enemigos.especialPesado:
                vida = 1000;
                break;

            case Enemigos.minijefeMelé:
                vida = 5000;
                break;
            case Enemigos.minijefeRango:
                vida = 3000;
                break;
        }
    }

    public override void Update()
    {

    }

    public void RecibirDaño(float daño)
    {
        vida -= daño;

        if (vida <= 0)
            Morir();
    }

    private void Morir()
    {

    }
}
