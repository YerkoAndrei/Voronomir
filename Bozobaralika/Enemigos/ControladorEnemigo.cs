﻿using System.Collections.Generic;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;
using static Constantes;

public class ControladorEnemigo : SyncScript, IDañable
{
    public Enemigos enemigo;
    public List<RigidbodyComponent> cuerpos { get ; set ; }

    public ControladorArmaMelé armaMelé;
    public ControladorArmaRango armaRango;

    private CharacterComponent cuerpo;
    private ControladorPersecusión persecutor;
    private float vida;
    private bool activo;
    private bool melé;

    public override void Start()
    {
        cuerpo = Entity.Get<CharacterComponent>();
        persecutor = Entity.Get<ControladorPersecusión>();

        switch (enemigo)
        {
            case Enemigos.meléLigero:
                vida = 80;
                melé = true;
                persecutor.Iniciar(this, 0.1f, 7f, 6f, ObtenerDistanciaAtaque(), false);
                break;
            case Enemigos.meléMediano:
                vida = 200;
                melé = true;
                persecutor.Iniciar(this, 0.1f, 4f, 5f, ObtenerDistanciaAtaque(), false);
                break;
            case Enemigos.meléPesado:
                vida = 50;
                melé = true;
                persecutor.Iniciar(this, 1f, 10f, 4f, ObtenerDistanciaAtaque(), true);
                break;

            case Enemigos.rangoLigero:
                vida = 40;
                melé = false;
                persecutor.Iniciar(this, 0.5f, 16f, 0f, ObtenerDistanciaAtaque(), true);
                break;
            case Enemigos.rangoMediano:
                vida = 100;
                melé = false;
                persecutor.Iniciar(this, 0.5f, 2f, 5f, ObtenerDistanciaAtaque(), false);
                break;
            case Enemigos.rangoPesado:
                vida = 500;
                melé = false;
                persecutor.Iniciar(this, 0.5f, 5f, 5f, ObtenerDistanciaAtaque(), false);
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

        // Cerebro no tiene arma
        if (melé && armaMelé != null)
            armaMelé.Iniciar();

        else if (!melé && armaRango != null)
            armaRango.Iniciar(ObtenerVelocidadDisparo(), cuerpos.ToArray());

        activo = true;
    }

    public override void Update()
    {
        if (!activo)
            return;

        // Updates
        persecutor.Actualizar();
    }

    public void Atacar()
    {
        // Daño puede variar en algún momento
        if (melé && armaMelé != null)
            armaMelé.Atacar(ObtenerDañoMelé());

        else if (!melé && armaRango != null)
            armaRango.Disparar(ObtenerDañoRango(), ObtenerPosiciónDisparo());
    }

    public void RecibirDaño(float daño)
    {
        vida -= daño;

        if (vida <= 0 && activo)
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
        persecutor.EliminarPersecutor();
        Entity.Scene.Entities.Remove(Entity);
    }

    private float ObtenerDañoMelé()
    {
        switch (enemigo)
        {
            case Enemigos.meléLigero:
                return 10;
            case Enemigos.meléMediano:
                return 25;
            case Enemigos.meléPesado:
                return 0;
            default:
                return 0;
        }
    }

    private float ObtenerDañoRango()
    {
        switch (enemigo)
        {
            case Enemigos.rangoLigero:
                return 5;
            case Enemigos.rangoMediano:
                return 10;
            case Enemigos.rangoPesado:
                return 25;
            default:
                return 0;
        }
    }

    private float ObtenerVelocidadDisparo()
    {
        switch (enemigo)
        {
            case Enemigos.rangoLigero:
                return 15;
            case Enemigos.rangoMediano:
                return 5;
            case Enemigos.rangoPesado:
                return 20;
            default:
                return 0;
        }
    }

    private Vector3 ObtenerPosiciónDisparo()
    {
        // Jugador mide 160cm, tiene los ojos en 150cm
        switch (enemigo)
        {
            case Enemigos.rangoLigero:
                return ControladorPartida.ObtenerPosiciónJugador() + new Vector3(0, 1.25f, 0);
            case Enemigos.rangoMediano:
                return ControladorPartida.ObtenerPosiciónJugador() + new Vector3(0, 1f, 0);
            case Enemigos.rangoPesado:
                return ControladorPartida.ObtenerPosiciónJugador() + new Vector3(0, 1.1f, 0);
            default:
                return ControladorPartida.ObtenerPosiciónJugador();
        }
    }

    public float ObtenerDistanciaAtaque()
    {
        switch (enemigo)
        {
            case Enemigos.meléLigero:
                return 1.5f;
            case Enemigos.meléMediano:
                return 2.5f;
            case Enemigos.meléPesado:
                return 4f;
            case Enemigos.rangoLigero:
                return 6f;
            case Enemigos.rangoMediano:
                return 10f;
            case Enemigos.rangoPesado:
                return 12f;

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

    public float ObtenerPreparaciónAtaque()
    {
        switch (enemigo)
        {
            case Enemigos.meléLigero:
                return 0.25f;
            case Enemigos.meléMediano:
                return 0.4f;
            case Enemigos.meléPesado:
                return 0f;
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
                return 0.8f;
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
