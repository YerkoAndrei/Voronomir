using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;

public class ControladorArmaRango : StartupScript
{
    public Prefab prefabProyectil;
    public Prefab prefabEfecto;

    private PhysicsComponent[] cuerposDisparador;

    private ElementoVeneno[] efectosVeneno;
    private IProyectil[] proyectiles;

    private Vector3 alturaObjetivo;
    private float velocidadRotación;
    private float velocidad;
    private bool persecutor;
    private int proyectilActual;
    private int efectoActual;
    private int maxProyectiles;
    private int maxEfectos;

    public void Iniciar(float _velocidad, float _velocidadRotación, Vector3 _alturaObjetivo, PhysicsComponent[] _cuerposDisparador)
    {
        alturaObjetivo = _alturaObjetivo;
        velocidad = _velocidad;
        velocidadRotación = _velocidadRotación;
        cuerposDisparador = _cuerposDisparador;
        persecutor = (velocidadRotación > 0);

        maxProyectiles = 4;
        maxEfectos = maxProyectiles * 2;

        // Proyectiles
        proyectiles = new IProyectil[maxProyectiles];
        for (int i = 0; i < maxProyectiles; i++)
        {
            var proyectil = prefabProyectil.Instantiate()[0];
            foreach (var componente in proyectil.Components)
            {
                if (componente is IProyectil)
                {
                    proyectiles[i] = (IProyectil)componente;
                    break;
                }
            }
            Entity.Scene.Entities.Add(proyectil);

            // Efectos son explosiones o veneno
            if (prefabEfecto != null)
                proyectil.Get<ElementoProyectil>().AsignarEfecto(IniciarEfecto);
        }

        // Efectos
        if (prefabEfecto != null)
        {
            efectosVeneno = new ElementoVeneno[maxEfectos];
            for (int i = 0; i < maxEfectos; i++)
            {
                var efecto = prefabEfecto.Instantiate()[0];
                efectosVeneno[i] = efecto.Get<ElementoVeneno>();
                Entity.Scene.Entities.Add(efecto);
            }
        }
    }

    public void Disparar(float daño)
    {
        var dirección = Vector3.Normalize(Entity.Transform.WorldMatrix.TranslationVector - (ControladorPartida.ObtenerPosiciónJugador() + alturaObjetivo));
        var rotación = Quaternion.LookRotation(dirección, Vector3.UnitY);

        if (persecutor)
            proyectiles[proyectilActual].IniciarPersecutor(velocidadRotación, alturaObjetivo);
        
        proyectiles[proyectilActual].Iniciar(daño, velocidad, rotación, Entity.Transform.WorldMatrix.TranslationVector, cuerposDisparador);
        
        proyectilActual++;
        if (proyectilActual >= maxProyectiles)
            proyectilActual = 0;
    }

    public void IniciarEfecto(Vector3 posición)
    {
        efectosVeneno[efectoActual].Iniciar(posición);

        efectoActual++;
        if (efectoActual >= maxEfectos)
            efectoActual = 0;
    }
}
