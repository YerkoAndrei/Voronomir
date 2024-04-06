using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;

public class ControladorArmaRango: StartupScript
{
    public Prefab prefabDisparo;

    private PhysicsComponent[] cuerposDisparador;

    private ElementoDisparo[] disparos;
    private Vector3 altura;
    private float velocidadSeguimiento;
    private float velocidad;
    private int disparoActual;
    private int maxDisparos;

    public void Iniciar(float _velocidad, float _velocidadSeguimiento, Vector3 _altura, PhysicsComponent[] _cuerposDisparador)
    {
        altura = _altura;
        velocidad = _velocidad;
        velocidadSeguimiento = _velocidadSeguimiento;
        cuerposDisparador = _cuerposDisparador;

        maxDisparos = 4;
        disparos = new ElementoDisparo[maxDisparos];
        for (int i = 0; i < maxDisparos; i++)
        {
            var marca = prefabDisparo.Instantiate()[0];
            disparos[i] = marca.Get<ElementoDisparo>();
            Entity.Scene.Entities.Add(marca);
        }
    }

    public void Disparar(float daño)
    {
        var dirección = Vector3.Normalize(Entity.Transform.WorldMatrix.TranslationVector - (ControladorPartida.ObtenerPosiciónJugador() + altura));
        var rotación = Quaternion.LookRotation(dirección, Vector3.UnitY);

        disparos[disparoActual].Iniciar(daño, velocidad, velocidadSeguimiento, rotación, altura, Entity.Transform.WorldMatrix.TranslationVector, cuerposDisparador);
        disparoActual++;

        if (disparoActual >= maxDisparos)
            disparoActual = 0;
    }
}
