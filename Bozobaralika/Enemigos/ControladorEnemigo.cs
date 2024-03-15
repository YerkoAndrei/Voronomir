using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;
using static Constantes;

public class ControladorEnemigo : StartupScript
{
    public Enemigos enemigo;
    public ControladorArmaMelé armaMelé;
    //public ControladorArmaEnemigo armaDisparo;

    private CharacterComponent cuerpo;
    private ControladorPersecusión persecusión;

    private float vida;
    private bool activo;
    private bool melé;

    public override void Start()
    {
        cuerpo = Entity.Get<CharacterComponent>();
        persecusión = Entity.Get<ControladorPersecusión>();

        switch (enemigo)
        {
            case Enemigos.meléLigero:
                vida = 100;
                melé = true;
                persecusión.Iniciar(this, 0.1f, 6f, 1f);
                break;
            case Enemigos.meléMediano:
                vida = 400;
                melé = true;
                persecusión.Iniciar(this, 0.5f, 5f, 2f);
                break;
            case Enemigos.meléPesado:
                vida = 200;
                melé = true;
                persecusión.Iniciar(this, 0.5f, 3f, 5f);
                break;
            case Enemigos.rangoLigero:
                vida = 40;
                melé = false;
                persecusión.Iniciar(this, 0.5f, 10f, 2f);
                break;
            case Enemigos.rangoMediano:
                vida = 100;
                melé = false;
                persecusión.Iniciar(this, 0.5f, 2f, 10f);
                break;
            case Enemigos.rangoPesado:
                vida = 500;
                melé = false;
                persecusión.Iniciar(this, 0.5f, 5f, 12f);
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

        if (melé)
            armaMelé.Iniciar(false);
        //else
            //armaDisparo.Iniciar();

        activo = true;
    }

    public void Atacar()
    {
        if (melé)
            armaMelé.Atacar(ObtenerDañoMelé());
    }

    public void RecibirDaño(float daño)
    {
        vida -= daño;

        if (vida <= 0)
            Morir();
    }

    public bool ObtenerActivo()
    {
        return activo;
    }

    private void Morir()
    {
        activo = false;
        cuerpo.Enabled = false;
        Entity.Scene.Entities.Remove(Entity);
    }

    private float ObtenerDañoMelé()
    {
        switch (enemigo)
        {
            case Enemigos.meléLigero:
                return 10;
            case Enemigos.meléMediano:
                return 20;
            case Enemigos.meléPesado:
                return 20;
            default:
                return 0;
        }
    }

    public float ObtenerPreparaciónAtaque()
    {
        switch (enemigo)
        {
            case Enemigos.meléLigero:
                return 0.2f;
            case Enemigos.meléMediano:
                return 0.2f;
            case Enemigos.meléPesado:
                return 0.2f;
            case Enemigos.rangoLigero:
                return 0.2f;
            case Enemigos.rangoMediano:
                return 0.2f;
            case Enemigos.rangoPesado:
                return 0.2f;

            case Enemigos.especialLigero:
                return 0.2f;
            case Enemigos.especialPesado:
                return 0.2f;

            case Enemigos.minijefeMelé:
                return 0.2f;
            case Enemigos.minijefeRango:
                return 0.2f;

            default:
                return 0;
        }
    }

    public float ObtenerDescansoAtaque()
    {
        switch (enemigo)
        {
            case Enemigos.meléLigero:
                return 0.5f;
            case Enemigos.meléMediano:
                return 0.5f;
            case Enemigos.meléPesado:
                return 0.5f;
            case Enemigos.rangoLigero:
                return 0.5f;
            case Enemigos.rangoMediano:
                return 0.5f;
            case Enemigos.rangoPesado:
                return 0.5f;

            case Enemigos.especialLigero:
                return 0.5f;
            case Enemigos.especialPesado:
                return 0.5f;

            case Enemigos.minijefeMelé:
                return 0.5f;
            case Enemigos.minijefeRango:
                return 0.5f;

            default:
                return 0;
        }
    }
}
