using Stride.Core.Mathematics;
using Stride.Engine;

namespace Bozobaralika;

public class ControladorArmaRango : StartupScript
{
    public Prefab prefabProyectil;
    public Prefab prefabImpacto;

    private PhysicsComponent[] cuerposDisparador;

    private IProyectil[] proyectiles;
    private IImpacto[] impactos;

    private Vector3 alturaObjetivo;
    private float velocidadRotación;
    private float velocidad;
    private int proyectilActual;
    private int impactoActual;
    private int maxProyectiles;
    private int maxImpactos;

    public void Iniciar(float _velocidad, float _velocidadRotación, Vector3 _alturaObjetivo, PhysicsComponent[] _cuerposDisparador)
    {
        alturaObjetivo = _alturaObjetivo;
        velocidad = _velocidad;
        velocidadRotación = _velocidadRotación;
        cuerposDisparador = _cuerposDisparador;

        maxProyectiles = 4;
        maxImpactos = maxProyectiles * 2;

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
                foreach (var componente in impacto.Components)
                {
                    if (componente is IImpacto)
                    {
                        impactos[i] = (IImpacto)componente;
                        break;
                    }
                }
                Entity.Scene.Entities.Add(impacto);
            }
        }
    }

    public void Disparar(float daño)
    {
        var dirección = Vector3.Normalize(Entity.Transform.WorldMatrix.TranslationVector - (ControladorPartida.ObtenerPosiciónJugador() + alturaObjetivo));
        var rotación = Quaternion.LookRotation(dirección, Vector3.UnitY);

        if (velocidadRotación > 0)
            proyectiles[proyectilActual].IniciarPersecutor(velocidadRotación, alturaObjetivo);
        
        proyectiles[proyectilActual].Iniciar(daño, velocidad, rotación, Entity.Transform.WorldMatrix.TranslationVector, cuerposDisparador);
        
        proyectilActual++;
        if (proyectilActual >= maxProyectiles)
            proyectilActual = 0;
    }

    public void IniciarImpacto(Vector3 posición)
    {
        impactos[impactoActual].Iniciar(posición);

        impactoActual++;
        if (impactoActual >= maxImpactos)
            impactoActual = 0;
    }
}
