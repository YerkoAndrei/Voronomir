using Stride.Core.Mathematics;
using Stride.Engine;

namespace Voronomir;
using static Constantes;

public class ControladorArmaRango : StartupScript
{
    private Enemigos disparador;
    private Vector3 alturaObjetivo;
    private float velocidadRotación;
    private float velocidad;
    private float daño;

    public void Iniciar(float _daño, float _velocidad, float _velocidadRotación, Vector3 _alturaObjetivo, Enemigos _disparador)
    {
        daño = _daño;
        alturaObjetivo = _alturaObjetivo;
        velocidad = _velocidad;

        velocidadRotación = _velocidadRotación;
        disparador = _disparador;
    }

    public void Disparar()
    {
        var dirección = Vector3.Normalize(Entity.Transform.WorldMatrix.TranslationVector - (ControladorPartida.ObtenerPosiciónJugador() + alturaObjetivo));
        var rotación = Quaternion.LookRotation(dirección, Vector3.UnitY);

        ControladorCofres.IniciarProyectil(disparador, daño, velocidad, Entity.Transform.WorldMatrix.TranslationVector, rotación, 
                                            velocidadRotación, alturaObjetivo);
    }
}
