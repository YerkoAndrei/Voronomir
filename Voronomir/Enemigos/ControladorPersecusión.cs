using System.Collections.Generic;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Navigation;
using Stride.Physics;

namespace Voronomir;
using static Utilidades;

public class ControladorPersecusión : StartupScript
{
    public bool estático;
    public List<TransformComponent> ojos = new List<TransformComponent> { };

    private ControladorPersecusionesTrigonométricas persecutor;
    private ControladorEnemigo controlador;
    private CharacterComponent cuerpo;
    private NavigationComponent navegador;
    private IAnimador animador;

    private CollisionFilterGroupFlags colisionesMirada;
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
    private float distanciaSalto;

    private float tiempoRotación;
    private float velocidadRotación;

    private float tempoAceleración;
    private float tiempoAceleración;

    private float tempoBusqueda;
    private float tiempoBusqueda;

    public void Iniciar(ControladorEnemigo _controlador, float _tiempoBusqueda, float _velocidad, float _rotación, float _distanciaAtaque, float _distanciaSalto, bool _persecutorTrigonométrico)
    {
        // Busca animador
        animador = ObtenerInterfaz<IAnimador>(Entity);
        animador.Iniciar();

        colisionesMirada = CollisionFilterGroupFlags.StaticFilter | CollisionFilterGroupFlags.CharacterFilter;
        cuerpo = Entity.Get<CharacterComponent>();
        navegador = Entity.Get<NavigationComponent>();
        ruta = new List<Vector3>();

        tiempoAceleración = 1;

        controlador = _controlador;
        tiempoBusqueda = _tiempoBusqueda;
        tempoBusqueda = 0;

        velocidad = _velocidad;
        velocidadRotación = _rotación;
        distanciaAtaque = _distanciaAtaque;

        distanciaSalto = _distanciaSalto;
        persecutorTrigonométrico = _persecutorTrigonométrico;
        if (persecutorTrigonométrico)
        {
            // Busca persecutor trigonométrico
            persecutor = ControladorPartida.ObtenerPersecutor(controlador.enemigo);
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

        MirarJugador(velocidadRotación);

        // Estáticos no persiguen y disparan mientras vea al jugador
        if (estático)
        {
            if (MirarJugador())
                Atacar();
        }
        else
        {
            // Busca cada cierto tiempo
            tempoBusqueda -= (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            if (tempoBusqueda <= 0)
                BuscarObjetivo();

            Perseguir();
        }
    }

    private void BuscarObjetivo()
    {
        ruta.Clear();
        índiceRuta = 0;
        tempoBusqueda = tiempoBusqueda;

        if(persecutorTrigonométrico && persecutor.PosibleRodearJugador())
            navegador.TryFindPath(persecutor.ObtenerPosiciónCircular(índiceTrigonométrico), ruta);
        else
            navegador.TryFindPath(ControladorPartida.ObtenerPosiciónJugador(), ruta);
    }

    private void MirarJugador(float velocidad)
    {
        direciónJugador = ControladorPartida.ObtenerPosiciónJugador() - Entity.Transform.WorldMatrix.TranslationVector;
        direciónJugador.Y = 0f;
        direciónJugador.Normalize();

        tiempoRotación = velocidad * (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
        cuerpo.Orientation = Quaternion.Lerp(cuerpo.Orientation, Quaternion.LookRotation(direciónJugador, Vector3.UnitY), tiempoRotación);
    }

    private void Perseguir()
    {
        if (ruta.Count == 0)
            return;

        distanciaRuta = Vector3.Distance(Entity.Transform.WorldMatrix.TranslationVector, ruta[índiceRuta]);
        distanciaJugador = Vector3.Distance(Entity.Transform.WorldMatrix.TranslationVector, ControladorPartida.ObtenerPosiciónJugador());

        // Salto
        if (distanciaJugador <= distanciaSalto && cuerpo.IsGrounded)
            Saltar();

        // Ataque
        if (distanciaJugador <= distanciaAtaque)
        {
            if (MirarJugador())
            {
                Atacar();
                return;
            }
        }

        // Movimiento
        if (distanciaRuta > 0.1f)
        {
            tempoAceleración += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            aceleración = MathUtil.SmoothStep(tempoAceleración / tiempoAceleración);
            
            // Mientras salta va directo al jugador
            if(cuerpo.IsGrounded)
                dirección = ruta[índiceRuta] - Entity.Transform.WorldMatrix.TranslationVector;
            else
                dirección = ControladorPartida.ObtenerPosiciónJugador() - Entity.Transform.WorldMatrix.TranslationVector;
            
            dirección.Normalize();
            dirección *= (float)Game.UpdateTime.WarpElapsed.TotalSeconds;                        
            cuerpo.SetVelocity(dirección * 100 * velocidad * aceleración);
            animador.Caminar(aceleración);
        }
        else
        {
            if (índiceRuta < (ruta.Count - 1))
                índiceRuta++;
            else
                ruta.Clear();
        }
    }

    public void Saltar()
    {
        var dirección = (ControladorPartida.ObtenerPosiciónJugador() - Entity.Transform.WorldMatrix.TranslationVector) * 3f;
        dirección.Y = controlador.ObtenerFuerzaSalto();
        cuerpo.Jump(dirección);
    }

    public void ForzarPersecusiónTrigonométrica()
    {
        distanciaRuta = Vector3.Distance(Entity.Transform.WorldMatrix.TranslationVector, persecutor.ObtenerPosiciónCircular(índiceTrigonométrico));
        if (distanciaRuta < 0.1f)
            return;

        dirección = persecutor.ObtenerPosiciónCircular(índiceTrigonométrico) - Entity.Transform.WorldMatrix.TranslationVector;
        dirección.Normalize();
        dirección *= (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
        cuerpo.SetVelocity(dirección * 100 * velocidad * distanciaRuta * 0.2f);
        animador.Caminar(distanciaRuta * 0.2f);
    }

    private bool MirarJugador()
    {
        // Si todos los ojos ven al jugador, entonces ataca
        var mirando = new bool[ojos.Count];
        for(int i=0; i< ojos.Count; i++)
        {
            var resultado = this.GetSimulation().Raycast(ojos[i].WorldMatrix.TranslationVector,
                                                         ControladorPartida.ObtenerCabezaJugador(),
                                                         CollisionFilterGroups.KinematicFilter,
                                                         colisionesMirada);

            if (resultado.Succeeded)
                mirando[i] = (resultado.Collider.CollisionGroup == CollisionFilterGroups.CharacterFilter);
            else
                mirando[i] = false;
        }

        var observando = true;
        for (int i = 0; i < mirando.Length; i++)
        {
            if (!mirando[i])
            {
                observando = false;
                break;
            }
        }

        return observando;
    }

    public async void Atacar()
    {
        atacando = true;
        tempoAceleración = 0;
        cuerpo.SetVelocity(Vector3.Zero);

        // Delay de preparación de ataque melé
        if (controlador.ObtenerPreparaciónAtaqueMelé() > 0)
            await Task.Delay((int)(controlador.ObtenerPreparaciónAtaqueMelé() * 1000));

        if (!controlador.ObtenerActivo())
            return;

        animador.Atacar();
        controlador.Atacar();
        await Task.Delay((int)(controlador.ObtenerCadenciaAtaque() * 1000));

        tempoBusqueda = 0;
        atacando = false;
    }

    public void EliminarPersecutor()
    {
        if (persecutorTrigonométrico)
            persecutor.EliminarPersecutor(this);
    }

    public void ReasignarÍndice(int índice)
    {
        índiceTrigonométrico = índice;
    }
}
