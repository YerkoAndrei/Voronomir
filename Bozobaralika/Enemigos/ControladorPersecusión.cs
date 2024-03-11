using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;
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
    private float distanciaMínima;

    private float velocidad;
    private float distanciaAtaque;

    public void Iniciar(float _velocidad, float _distanciaAtaque)
    {
        jugador = Entity.Scene.Entities.Where(o => o.Get<ControladorJugador>() != null).FirstOrDefault().Transform;

        cuerpo = Entity.Get<CharacterComponent>();
        navegador = Entity.Get<NavigationComponent>();
        ruta = new List<Vector3>();

        distanciaMínima = 0.1f;

        velocidad = _velocidad;
        distanciaAtaque = _distanciaAtaque;
    }

    public override void Update()
    {
        Perseguir();

        if (Input.IsMouseButtonPressed(MouseButton.Right))
            BuscarJugador();
    }

    private void BuscarJugador()
    {
        ruta.Clear();
        navegador.TryFindPath(jugador.Position, ruta);
    }

    private void Perseguir()
    {
        if (ruta.Count == 0)
            return;

        var distanciaRuta = Vector3.Distance(Entity.Transform.WorldMatrix.TranslationVector, ruta[índiceRuta]);
        var distanciaJugador = Vector3.Distance(Entity.Transform.WorldMatrix.TranslationVector, jugador.Position);

        // Ataque
        if (distanciaJugador <= distanciaAtaque)
        {

            return;
        }

        // Movimiento
        if (distanciaRuta > distanciaMínima)
        {
            var direction = ruta[índiceRuta] - Entity.Transform.WorldMatrix.TranslationVector;
            direction.Normalize();
            direction *= velocidad * (float)Game.UpdateTime.Elapsed.TotalSeconds;

            Entity.Transform.Position += direction;
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
