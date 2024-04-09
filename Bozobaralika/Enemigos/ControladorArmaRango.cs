using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;
using static Utilidades;
using static Constantes;

public class ControladorArmaRango : StartupScript
{
    public Prefab prefabProyectil;
    public Prefab prefabImpacto;

    private Enemigos disparador;
    private IProyectil[] proyectiles;
    private IImpacto[] impactos;

    private Vector3 alturaObjetivo;
    private float velocidadRotación;
    private float velocidad;
    private float daño;
    private int proyectilActual;
    private int impactoActual;
    private int maxProyectiles;
    private int maxImpactos;

    public void Iniciar(float _velocidad, float _velocidadRotación, Vector3 _alturaObjetivo, Enemigos _disparador)
    {
        alturaObjetivo = _alturaObjetivo;
        velocidad = _velocidad;
        velocidadRotación = _velocidadRotación;
        disparador = _disparador;

        maxProyectiles = 4;
        maxImpactos = maxProyectiles * 2;

        // Proyectiles
        proyectiles = new IProyectil[maxProyectiles];
        for (int i = 0; i < maxProyectiles; i++)
        {
            var proyectil = prefabProyectil.Instantiate()[0];
            proyectiles[i] = ObtenerInterfaz<IProyectil>(proyectil);
            Entity.Scene.Entities.Add(proyectil);

            // Impactos son explosiones o veneno
            if (prefabImpacto != null)
                proyectiles[i].AsignarImpacto(IniciarImpacto);
        }

        // Impactos
        if (prefabImpacto != null)
        {
            impactos = new IImpacto[maxImpactos];
            for (int i = 0; i < maxImpactos; i++)
            {
                var impacto = prefabImpacto.Instantiate()[0];
                impactos[i] = ObtenerInterfaz<IImpacto>(impacto);
                Entity.Scene.Entities.Add(impacto);
            }
        }
    }

    public void Disparar(float _daño)
    {
        daño = _daño;
        var dirección = Vector3.Normalize(Entity.Transform.WorldMatrix.TranslationVector - (ControladorPartida.ObtenerPosiciónJugador() + alturaObjetivo));
        var rotación = Quaternion.LookRotation(dirección, Vector3.UnitY);

        if (velocidadRotación > 0)
            proyectiles[proyectilActual].IniciarPersecutor(velocidadRotación, alturaObjetivo);
        
        proyectiles[proyectilActual].Iniciar(daño, velocidad, rotación, Entity.Transform.WorldMatrix.TranslationVector, disparador);
        
        proyectilActual++;
        if (proyectilActual >= maxProyectiles)
            proyectilActual = 0;
    }

    public void IniciarImpacto(Vector3 posición, Quaternion rotación, bool soloEfecto)
    {
        impactos[impactoActual].Iniciar(posición, rotación, daño);

        impactoActual++;
        if (impactoActual >= maxImpactos)
            impactoActual = 0;
    }
}
