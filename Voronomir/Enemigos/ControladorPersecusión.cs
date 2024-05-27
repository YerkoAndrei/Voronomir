using System.Collections.Generic;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Navigation;
using Stride.Physics;

namespace Voronomir;

public class ControladorPersecusión : StartupScript
{
    public bool estático;
    public List<TransformComponent> ojos = new List<TransformComponent> { };
    
    private Enemigo datos;
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
    private float acelaraciónTrigonométrica;

    private bool atacando;
    private float tiempoRotación;
    private float tempoBusqueda;

    private float tempoAceleración;
    private float tiempoAceleración;
    private float aceleración;

    public void Iniciar(ControladorEnemigo _controlador, Enemigo _datos, IAnimador _animador)
    {
        colisionesMirada = CollisionFilterGroupFlags.StaticFilter | CollisionFilterGroupFlags.CharacterFilter;
        cuerpo = Entity.Get<CharacterComponent>();
        navegador = Entity.Get<NavigationComponent>();
        ruta = new List<Vector3>();

        controlador = _controlador;
        animador = _animador;
        datos = _datos;

        tempoBusqueda = 0;
        tiempoAceleración = 1.2f;
        acelaraciónTrigonométrica = 0.2f;

        if (datos.PersecutorTrigonométrico)
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
            MirarJugador(datos.VelocidadRotación * 0.5f);

            // Se mueve mientras ataca
            if (datos.PersecutorTrigonométrico && persecutor.PosibleRodearJugador())
                ForzarPersecusiónTrigonométrica();

            return;
        }

        MirarJugador(datos.VelocidadRotación);

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
        tempoBusqueda = datos.TiempoBusqueda;

        if(datos.PersecutorTrigonométrico && persecutor.PosibleRodearJugador())
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
        if (datos.DistanciaSalto > 0 && distanciaJugador <= datos.DistanciaSalto && cuerpo.IsGrounded)
            Saltar();

        // Ataque
        if (distanciaJugador <= datos.DistanciaAtaque)
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
            aceleración = SistemaAnimación.EvaluarSuave(tempoAceleración / tiempoAceleración);
            aceleración = MathUtil.Clamp(aceleración, 0, 1);

            // Mientras salta va directo al jugador
            if (cuerpo.IsGrounded)
                dirección = ruta[índiceRuta] - Entity.Transform.WorldMatrix.TranslationVector;
            else
                dirección = ControladorPartida.ObtenerPosiciónJugador() - Entity.Transform.WorldMatrix.TranslationVector;
            
            dirección.Normalize();
            dirección *= (float)Game.UpdateTime.WarpElapsed.TotalSeconds;                        
            cuerpo.SetVelocity(dirección * 100 * datos.VelocidadMovimiento * aceleración);
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
        var dirección = (ControladorPartida.ObtenerPosiciónJugador() - Entity.Transform.WorldMatrix.TranslationVector) * (datos.FuerzaSalto * 0.25f);
        dirección.Y = datos.FuerzaSalto;
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
        cuerpo.SetVelocity(dirección * 100 * datos.VelocidadMovimiento * distanciaRuta * acelaraciónTrigonométrica);
        animador.Caminar(distanciaRuta * acelaraciónTrigonométrica);
    }

    private bool MirarJugador()
    {
        // Si todos los ojos ven al jugador, entonces ataca
        var mirando = new bool[ojos.Count];
        for(int i = 0; i < ojos.Count; i++)
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
        if (datos.PreparaciónMelé > 0)
            await Task.Delay((int)(datos.PreparaciónMelé * 1000));

        if (!controlador.ObtenerActivo())
            return;

        animador.Atacar();
        controlador.Atacar();
        await Task.Delay((int)(datos.Cadencia * 1000));

        tempoBusqueda = 0;
        atacando = false;
    }

    public void EliminarPersecutor()
    {
        if (datos.PersecutorTrigonométrico)
            persecutor.EliminarPersecutor(this);
    }

    public void ReasignarÍndice(int índice)
    {
        índiceTrigonométrico = índice;
    }
}
