using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Navigation;
using Stride.Physics;

namespace Bozobaralika;

public class ControladorPersecusión : StartupScript
{
    private ControladorPersecusionesTrigonométricas persecutor;
    private ControladorEnemigo controlador;
    private CharacterComponent cuerpo;
    private TransformComponent jugador;
    private NavigationComponent navegador;
    private IAnimador animador;

    private List<Vector3> ruta;
    private int índiceRuta;

    private Vector3 dirección;
    private Vector3 direciónJugador;
    private float distanciaRuta;
    private float distanciaJugador;

    private int índiceTrigonométrico;
    private bool persecutorTrigonométrico;

    private bool atacando;
    private float velocidad;
    private float aceleración;
    private float distanciaAtaque;

    private float tiempoRotación;
    private float velocidadRotación;

    private float tempoAceleración;
    private float tiempoAceleración;

    private float tempoBusqueda;
    private float tiempoBusqueda;

    public void Iniciar(ControladorEnemigo _controlador, float _tiempoBusqueda, float _velocidad, float _rotación, float _distanciaAtaque, bool _persecutorTrigonométrico)
    {
        jugador = Entity.Scene.Entities.Where(o => o.Get<ControladorJugador>() != null).FirstOrDefault().Transform;

        // Busca animador
        foreach (var componente in Entity.Components)
        {
            if (componente is IAnimador)
            {
                animador = (IAnimador)componente;
                break;
            }
        }
        animador.Iniciar();

        cuerpo = Entity.Get<CharacterComponent>();
        navegador = Entity.Get<NavigationComponent>();
        ruta = new List<Vector3>();

        tiempoAceleración = 1;

        controlador = _controlador;
        tiempoBusqueda = _tiempoBusqueda;
        tempoBusqueda = tiempoBusqueda;

        velocidad = _velocidad;
        velocidadRotación = _rotación;
        distanciaAtaque = _distanciaAtaque;

        persecutorTrigonométrico = _persecutorTrigonométrico;
        if (persecutorTrigonométrico)
        {
            // Busca persecutor trigonométrico
            var entidad = Entity.Scene.Entities.Where(o => o.Get<ControladorPersecusionesTrigonométricas>() != null).FirstOrDefault();
            var persecutores = entidad.GetAll<ControladorPersecusionesTrigonométricas>();
            persecutor = persecutores.Where(o => o.enemigo == controlador.enemigo).FirstOrDefault();

            índiceTrigonométrico = persecutor.ObtenerÍndiceTrigonométrico(this);
        }
    }

    public void Actualizar()
    {
        animador.Actualizar();

        if (atacando)
        {
            MirarJugador(velocidadRotación * 0.5f);

            // Se mueve mientras ataca
            if (persecutorTrigonométrico && persecutor.PosibleRodearJugador())
                ForzarPersecusiónTrigonométrica();

            return;
        }

        // Busca cada cierto tiempo
        tempoBusqueda -= (float)Game.UpdateTime.Elapsed.TotalSeconds;
        if(tempoBusqueda <= 0)
            BuscarObjetivo();

        Perseguir();
        MirarJugador(velocidadRotación);
    }

    private void BuscarObjetivo()
    {
        ruta.Clear();
        índiceRuta = 0;
        tempoBusqueda = tiempoBusqueda;

        if(persecutorTrigonométrico && persecutor.PosibleRodearJugador())
            navegador.TryFindPath(persecutor.ObtenerPosiciónCircular(índiceTrigonométrico), ruta);
        else
            navegador.TryFindPath(jugador.WorldMatrix.TranslationVector, ruta);
    }

    private void MirarJugador(float velocidad)
    {
        direciónJugador = jugador.WorldMatrix.TranslationVector - Entity.Transform.WorldMatrix.TranslationVector;
        direciónJugador.Y = 0f;
        direciónJugador.Normalize();

        tiempoRotación = velocidad * (float)Game.UpdateTime.Elapsed.TotalSeconds;
        cuerpo.Orientation = Quaternion.Lerp(cuerpo.Orientation, Quaternion.LookRotation(direciónJugador, Vector3.UnitY), tiempoRotación);
    }

    private void Perseguir()
    {
        if (ruta.Count == 0)
            return;

        distanciaRuta = Vector3.Distance(Entity.Transform.WorldMatrix.TranslationVector, ruta[índiceRuta]);
        distanciaJugador = Vector3.Distance(Entity.Transform.WorldMatrix.TranslationVector, jugador.WorldMatrix.TranslationVector);
        
        // Ataque
        if (distanciaJugador <= distanciaAtaque)
        {
            Atacar();
            return;
        }
        
        // Movimiento
        if (distanciaRuta > 0.1f)
        {
            tempoAceleración += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            aceleración = MathUtil.SmoothStep(tempoAceleración / tiempoAceleración);
            
            // Mientras salta va directo al jugador
            if(cuerpo.IsGrounded)
                dirección = ruta[índiceRuta] - Entity.Transform.WorldMatrix.TranslationVector;
            else
                dirección = jugador.WorldMatrix.TranslationVector - Entity.Transform.WorldMatrix.TranslationVector;
            
            dirección.Normalize();
            dirección *= (float)Game.UpdateTime.Elapsed.TotalSeconds;
                        
            cuerpo.SetVelocity(dirección * 100 * velocidad * aceleración);
        }
        else
        {
            if (índiceRuta < (ruta.Count - 1))
                índiceRuta++;
            else
                ruta.Clear();
        }
    }

    public void ForzarPersecusiónTrigonométrica()
    {
        distanciaRuta = Vector3.Distance(Entity.Transform.WorldMatrix.TranslationVector, persecutor.ObtenerPosiciónCircular(índiceTrigonométrico));
        if (distanciaRuta < 0.1f)
            return;

        dirección = persecutor.ObtenerPosiciónCircular(índiceTrigonométrico) - Entity.Transform.WorldMatrix.TranslationVector;
        dirección.Normalize();
        dirección *= (float)Game.UpdateTime.Elapsed.TotalSeconds;
        cuerpo.SetVelocity(dirección * 100 * velocidad * distanciaRuta * 0.2f);
    }

    public async void Atacar()
    {
        atacando = true;
        tempoAceleración = 0;

        // Delay de preparación de ataque
        cuerpo.SetVelocity(Vector3.Zero);
        await Task.Delay((int)(controlador.ObtenerPreparaciónAtaque() * 1000));

        if (!controlador.ObtenerActivo())
            return;

        animador.Atacar();
        controlador.Atacar();
        await Task.Delay((int)(controlador.ObtenerDescansoAtaque() * 1000));

        BuscarObjetivo();
        atacando = false;
    }

    public void EliminarPersecutor()
    {
        persecutor.EliminarPersecutor(this);
    }

    public void ReasignarÍndice(int índice)
    {
        índiceTrigonométrico = índice;
    }
}
