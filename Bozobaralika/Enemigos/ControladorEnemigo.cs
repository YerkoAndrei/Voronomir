using System.Collections.Generic;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace Bozobaralika;
using static Constantes;

public class ControladorEnemigo : SyncScript, IDañable
{
    public Enemigos enemigo;
    public List<RigidbodyComponent> cuerpos { get; set; }

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
                persecutor.Iniciar(this, 1f, 16f, 0f, ObtenerDistanciaAtaque(), true);
                break;
            case Enemigos.rangoMediano:
                vida = 400;
                melé = false;
                persecutor.Iniciar(this, 0.2f, 2f, 3f, ObtenerDistanciaAtaque(), false);
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
            armaRango.Iniciar(ObtenerVelocidadProyectil(), ObtenerVelocidadSeguimientoProyectil(), ObtenerObjetivoProyectil(), cuerpos.ToArray());

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
            armaMelé.Atacar(ObtenerDaño());

        else if (!melé && armaRango != null)
            armaRango.Disparar(ObtenerDaño());
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
        // PENDIENTE: ragdoll
        Entity.Scene.Entities.Remove(Entity);
    }

    private float ObtenerDaño()
    {
        switch (enemigo)
        {
            // Melé
            case Enemigos.meléLigero:
                return 10;
            case Enemigos.meléMediano:
                return 25;
            case Enemigos.meléPesado:
                return 0;
            // Proyectil
            case Enemigos.rangoLigero:
                return 5;
            case Enemigos.rangoMediano:
                return 20;
            case Enemigos.rangoPesado:
                return 25;
            // Melé especial
            case Enemigos.especialLigero:
                return 1f;
            case Enemigos.especialPesado:
                return 1f;

            case Enemigos.minijefeMelé:
                return 1f;
            case Enemigos.minijefeRango:
                return 1f;

            default:
                return 0;
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
                return 12f;
            case Enemigos.rangoPesado:
                return 12f;

            case Enemigos.especialLigero:
                return 1f;
            case Enemigos.especialPesado:
                return 1f;

            case Enemigos.minijefeMelé:
                return 1f;
            case Enemigos.minijefeRango:
                return 1f;

            default:
                return 0;
        }
    }

    public float ObtenerCadenciaAtaque()
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
                return 1.5f;
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

    public float ObtenerPreparaciónAtaqueMelé()
    {
        switch (enemigo)
        {
            case Enemigos.meléLigero:
                return 0.25f;
            case Enemigos.meléMediano:
                return 0.4f;
            default:
                return 0;
        }
    }

    private float ObtenerVelocidadProyectil()
    {
        switch (enemigo)
        {
            case Enemigos.rangoLigero:
                return 15;
            case Enemigos.rangoMediano:
                return 10;
            case Enemigos.rangoPesado:
                return 20;
            default:
                return 0;
        }
    }

    private float ObtenerVelocidadSeguimientoProyectil()
    {
        switch (enemigo)
        {
            case Enemigos.rangoMediano:
                return 5;
            default:
                return 0;
        }
    }

    private Vector3 ObtenerObjetivoProyectil()
    {
        // Jugador mide 160cm, tiene los ojos en 150cm
        switch (enemigo)
        {
            case Enemigos.rangoLigero:
                return Vector3.UnitY * 1.4f;
            case Enemigos.rangoMediano:
                return Vector3.UnitY * 0.6f;
            case Enemigos.rangoPesado:
                return Vector3.UnitY * 1f;
            default:
                return Vector3.UnitY;
        }
    }
}
