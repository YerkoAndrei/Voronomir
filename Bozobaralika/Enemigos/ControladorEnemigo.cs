﻿using Stride.Engine;
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

    public override void Start()
    {
        cuerpo = Entity.Get<CharacterComponent>();
        persecusión = Entity.Get<ControladorPersecusión>();

        switch (enemigo)
        {
            case Enemigos.meléLigero:
                vida = 100;
                persecusión.Iniciar(this, 0.1f, 6f, 0.2f);
                //armaMelé.Iniciar(false);
                break;
            case Enemigos.meléMediano:
                vida = 400;
                persecusión.Iniciar(this, 0.5f, 5f, 2f);
                //armaMelé.Iniciar(false);
                break;
            case Enemigos.meléPesado:
                vida = 200;
                persecusión.Iniciar(this, 0.5f, 3f, 5f);
                //armaMelé.Iniciar(false);
                break;
            case Enemigos.rangoLigero:
                vida = 40;
                persecusión.Iniciar(this, 0.5f, 10f, 2f);
                //armaDisparo.Iniciar();
                break;
            case Enemigos.rangoMediano:
                vida = 100;
                persecusión.Iniciar(this, 0.5f, 2f, 10f);
                //armaDisparo.Iniciar();
                break;
            case Enemigos.rangoPesado:
                vida = 500;
                persecusión.Iniciar(this, 0.5f, 5f, 12f);
                //armaDisparo.Iniciar();
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

        activo = true;
    }

    public void Atacar()
    {

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
}
