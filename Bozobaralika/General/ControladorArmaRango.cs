using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;

public class ControladorArmaRango: StartupScript
{
    public Prefab prefabProyectil;

    private PhysicsComponent[] cuerposDisparador;

    private ElementoProyectil[] proyectiles;
    private Vector3 alturaObjetivo;
    private float velocidadSeguimiento;
    private float velocidad;
    private int proyectilActual;
    private int maxProyectiles;

    public void Iniciar(float _velocidad, float _velocidadSeguimiento, Vector3 _alturaObjetivo, PhysicsComponent[] _cuerposDisparador)
    {
        alturaObjetivo = _alturaObjetivo;
        velocidad = _velocidad;
        velocidadSeguimiento = _velocidadSeguimiento;
        cuerposDisparador = _cuerposDisparador;

        maxProyectiles = 4;
        proyectiles = new ElementoProyectil[maxProyectiles];
        for (int i = 0; i < maxProyectiles; i++)
        {
            var marca = prefabProyectil.Instantiate()[0];
            proyectiles[i] = marca.Get<ElementoProyectil>();
            Entity.Scene.Entities.Add(marca);
        }
    }

    public void Disparar(float daño)
    {
        var dirección = Vector3.Normalize(Entity.Transform.WorldMatrix.TranslationVector - (ControladorPartida.ObtenerPosiciónJugador() + alturaObjetivo));
        var rotación = Quaternion.LookRotation(dirección, Vector3.UnitY);

        proyectiles[proyectilActual].Iniciar(daño, velocidad, velocidadSeguimiento, rotación, alturaObjetivo, Entity.Transform.WorldMatrix.TranslationVector, cuerposDisparador);
        proyectilActual++;

        if (proyectilActual >= maxProyectiles)
            proyectilActual = 0;
    }
}
