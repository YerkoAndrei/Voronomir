using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Navigation;
using Stride.Physics;
using System.Collections.Generic;
using System.Linq;

namespace Bozobaralika;

public class ControladorPersecusión : SyncScript
{
    private CharacterComponent cuerpo;
    private TransformComponent jugador;
    private NavigationComponent navegador;

    private List<Vector3> ruta;
    private int índiceRuta;

    private Vector3 movimiento;
    private float distanciaRuta;
    private float distanciaJugador;
    private float distanciaMínima;

    private ControladorEnemigo controlador;

    private float velocidad;
    private float distanciaAtaque;

    private float tempoBusqueda;
    private float tiempoBusqueda;

    public void Iniciar(ControladorEnemigo _controlador, float _tiempoBusqueda, float _velocidad, float _distanciaAtaque)
    {
        jugador = Entity.Scene.Entities.Where(o => o.Get<ControladorJugador>() != null).FirstOrDefault().Transform;

        cuerpo = Entity.Get<CharacterComponent>();
        navegador = Entity.Get<NavigationComponent>();
        ruta = new List<Vector3>();

        distanciaMínima = 0.1f;

        controlador = _controlador;
        tiempoBusqueda = _tiempoBusqueda;
        tempoBusqueda = tiempoBusqueda;

        velocidad = _velocidad;
        distanciaAtaque = _distanciaAtaque;
    }

    public override void Update()
    {
        if (!controlador.ObtenerActivo())
            return;

        // Busca cada cierto tiempo
        tempoBusqueda -= (float)Game.UpdateTime.Elapsed.TotalSeconds;
        if(tempoBusqueda <= 0)
            BuscarJugador();

        Perseguir();
    }

    private void BuscarJugador()
    {
        ruta.Clear();
        índiceRuta = 0;
        tempoBusqueda = tiempoBusqueda;
        navegador.TryFindPath(jugador.Position, ruta);
    }

    private void Perseguir()
    {
        if (ruta.Count == 0)
            return;

        distanciaRuta = Vector3.Distance(Entity.Transform.WorldMatrix.TranslationVector, ruta[índiceRuta]);
        distanciaJugador = Vector3.Distance(Entity.Transform.WorldMatrix.TranslationVector, jugador.Position);

        // Ataque
        if (distanciaJugador <= distanciaAtaque)
        {
            controlador.Atacar();
            return;
        }

        // Movimiento
        if (distanciaRuta > distanciaMínima)
        {
            movimiento = ruta[índiceRuta] - Entity.Transform.WorldMatrix.TranslationVector;
            movimiento.Normalize();
            movimiento *= (float)Game.UpdateTime.Elapsed.TotalSeconds;

            cuerpo.SetVelocity(movimiento * 100 * velocidad);
        }
        else
        {
            if (índiceRuta < (ruta.Count - 1))
                índiceRuta++;
            else
                ruta.Clear();
        }
    }
}
