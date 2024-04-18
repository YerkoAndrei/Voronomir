using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;
using static Utilidades;
using static Constantes;

public class ControladorArmaRango : StartupScript
{
    public Prefab prefabProyectil;

    private Enemigos disparador;
    private IProyectil[] proyectiles;

    private Vector3 alturaObjetivo;
    private float velocidadRotación;
    private float velocidad;
    private float daño;
    private int proyectilActual;
    private int maxProyectiles;

    public void Iniciar(float _velocidad, float _velocidadRotación, Vector3 _alturaObjetivo, Enemigos _disparador)
    {
        alturaObjetivo = _alturaObjetivo;
        velocidad = _velocidad;
        velocidadRotación = _velocidadRotación;
        disparador = _disparador;

        maxProyectiles = 4;

        // Proyectiles
        proyectiles = new IProyectil[maxProyectiles];
        for (int i = 0; i < maxProyectiles; i++)
        {
            var proyectil = prefabProyectil.Instantiate()[0];
            proyectiles[i] = ObtenerInterfaz<IProyectil>(proyectil);
            Entity.Scene.Entities.Add(proyectil);
        }
    }

    public void Disparar(float _daño)
    {
        var dirección = Vector3.Normalize(Entity.Transform.WorldMatrix.TranslationVector - (ControladorPartida.ObtenerPosiciónJugador() + alturaObjetivo));
        var rotación = Quaternion.LookRotation(dirección, Vector3.UnitY);
        daño = _daño;

        if (velocidadRotación > 0)
            proyectiles[proyectilActual].IniciarPersecutor(velocidadRotación, alturaObjetivo);
        
        proyectiles[proyectilActual].Iniciar(daño, velocidad, rotación, Entity.Transform.WorldMatrix.TranslationVector, disparador);
        
        proyectilActual++;
        if (proyectilActual >= maxProyectiles)
            proyectilActual = 0;
    }
}
