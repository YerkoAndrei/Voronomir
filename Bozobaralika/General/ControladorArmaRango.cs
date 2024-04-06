using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;

public class ControladorArmaRango: StartupScript
{
    public Prefab prefabProyectil;

    private PhysicsComponent[] cuerposDisparador;

    private ElementoProyectil[] proyectiles;
    private ElementoProyectilPersecutor[] proyectilesPersecutores;

    private Vector3 alturaObjetivo;
    private float velocidadSeguimiento;
    private float velocidad;
    private bool persecutor;
    private int proyectilActual;
    private int maxProyectiles;

    public void Iniciar( float _velocidad, float _velocidadSeguimiento, Vector3 _alturaObjetivo, PhysicsComponent[] _cuerposDisparador)
    {
        alturaObjetivo = _alturaObjetivo;
        velocidad = _velocidad;
        velocidadSeguimiento = _velocidadSeguimiento;
        cuerposDisparador = _cuerposDisparador;
        persecutor = (velocidadSeguimiento > 0);

        maxProyectiles = 4;

        if (persecutor)
        {
            proyectilesPersecutores = new ElementoProyectilPersecutor[maxProyectiles];
            for (int i = 0; i < maxProyectiles; i++)
            {
                var marca = prefabProyectil.Instantiate()[0];
                proyectilesPersecutores[i] = marca.Get<ElementoProyectilPersecutor>();
                Entity.Scene.Entities.Add(marca);
            }
        }
        else
        {
            proyectiles = new ElementoProyectil[maxProyectiles];
            for (int i = 0; i < maxProyectiles; i++)
            {
                var marca = prefabProyectil.Instantiate()[0];
                proyectiles[i] = marca.Get<ElementoProyectil>();
                Entity.Scene.Entities.Add(marca);
            }
        }
    }

    public void Disparar(float daño)
    {
        var dirección = Vector3.Normalize(Entity.Transform.WorldMatrix.TranslationVector - (ControladorPartida.ObtenerPosiciónJugador() + alturaObjetivo));
        var rotación = Quaternion.LookRotation(dirección, Vector3.UnitY);

        if (persecutor)
            proyectilesPersecutores[proyectilActual].Iniciar(daño, velocidad, velocidadSeguimiento, rotación, alturaObjetivo, Entity.Transform.WorldMatrix.TranslationVector, cuerposDisparador);
        else
            proyectiles[proyectilActual].Iniciar(daño, velocidad, rotación, Entity.Transform.WorldMatrix.TranslationVector, cuerposDisparador);
        
        proyectilActual++;
        if (proyectilActual >= maxProyectiles)
            proyectilActual = 0;
    }
}
