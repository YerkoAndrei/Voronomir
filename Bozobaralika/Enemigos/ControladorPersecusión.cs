using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Navigation;
using Stride.Physics;

namespace Bozobaralika;

public class ControladorPersecusión : SyncScript
{
    private IAnimador animador;
    private CharacterComponent cuerpo;
    private TransformComponent jugador;
    private NavigationComponent navegador;

    private List<Vector3> ruta;
    private int índiceRuta;

    private Vector3 movimiento;
    private Vector3 posiciónJugador;
    private float distanciaRuta;
    private float distanciaJugador;
    private float distanciaMínima;

    private ControladorEnemigo controlador;

    private bool atacando;
    private float velocidad;
    private float aceleración;
    private float distanciaAtaque;

    private float tempoAceleración;
    private float tiempoAceleración;

    private float tempoBusqueda;
    private float tiempoBusqueda;

    public void Iniciar(ControladorEnemigo _controlador, float _tiempoBusqueda, float _velocidad, float _distanciaAtaque)
    {
        jugador = Entity.Scene.Entities.Where(o => o.Get<ControladorJugador>() != null).FirstOrDefault().Transform;
        foreach (var componente in Entity.Components)
        {
            if (componente is IAnimador)
            {
                animador = (IAnimador)componente;
                break;
            }
        }

        cuerpo = Entity.Get<CharacterComponent>();
        navegador = Entity.Get<NavigationComponent>();
        ruta = new List<Vector3>();

        distanciaMínima = 0.1f;
        tiempoAceleración = 1;

        controlador = _controlador;
        tiempoBusqueda = _tiempoBusqueda;
        tempoBusqueda = tiempoBusqueda;

        velocidad = _velocidad;
        distanciaAtaque = _distanciaAtaque;
    }

    public override void Update()
    {
        if (!controlador.ObtenerActivo() || atacando)
            return;

        // Busca cada cierto tiempo
        tempoBusqueda -= (float)Game.UpdateTime.Elapsed.TotalSeconds;
        if(tempoBusqueda <= 0)
            BuscarJugador();

        MirarJugador();
        Perseguir();
    }

    private void BuscarJugador()
    {
        ruta.Clear();
        índiceRuta = 0;
        tempoBusqueda = tiempoBusqueda;
        navegador.TryFindPath(jugador.Position, ruta);
    }

    private void MirarJugador()
    {
        posiciónJugador = jugador.WorldMatrix.TranslationVector - Entity.Transform.WorldMatrix.TranslationVector;
        posiciónJugador.Y = 0f;
        posiciónJugador.Normalize();

        cuerpo.Orientation = Quaternion.LookRotation(posiciónJugador, Vector3.UnitY);
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
            Atacar();
            return;
        }

        // Movimiento
        if (distanciaRuta > distanciaMínima)
        {
            movimiento = ruta[índiceRuta] - Entity.Transform.WorldMatrix.TranslationVector;
            movimiento.Normalize();
            movimiento *= (float)Game.UpdateTime.Elapsed.TotalSeconds;

            tempoAceleración += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            aceleración = MathUtil.SmoothStep(tempoAceleración / tiempoAceleración);
            
            cuerpo.SetVelocity(movimiento * 100 * velocidad * aceleración);
        }
        else
        {
            if (índiceRuta < (ruta.Count - 1))
                índiceRuta++;
            else
                ruta.Clear();
        }
    }

    private async void Atacar()
    {
        atacando = true;
        tempoAceleración = 0;

        // Delay de preparación de ataque
        await Task.Delay((int)(controlador.ObtenerPreparaciónAtaque() * 1000));

        if (!controlador.ObtenerActivo())
            return;

        animador.Atacar();
        controlador.Atacar();
        cuerpo.SetVelocity(Vector3.Zero);
        await Task.Delay((int)(controlador.ObtenerDescansoAtaque() * 1000));

        BuscarJugador();
        atacando = false;
    }
}
