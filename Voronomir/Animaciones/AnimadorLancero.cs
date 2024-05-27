using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Rendering;
using Stride.Engine;

namespace Voronomir;

public class AnimadorLancero : StartupScript, IAnimador
{
    public ModelComponent modelo;
    public ModelComponent modeloLanza;
    public TransformComponent lanza;
    public List<string> brazos = new List<string> { };
    public List<string> piernas = new List<string> { };

    private SkeletonUpdater esqueleto;
    private int[] idBrazos;
    private int[] idPiernas;

    private float ánguloDiferencia;
    private Quaternion rotaciónLanzaInicial;

    private Vector3 pociciónLanzaInicio;
    private Vector3 pociciónLanzaAtaque;

    private Quaternion rotaciónInicioBrazoIzq;
    private Quaternion rotaciónInicioBrazoDer;

    private CancellationTokenSource tokenAtaque;

    public void Iniciar()
    {
        esqueleto = modelo.Skeleton;
        tokenAtaque = new CancellationTokenSource();

        idBrazos = new int[brazos.Count];
        idPiernas = new int[piernas.Count];

        // Encuentra huesos por nombre
        for (int i = 0; i < esqueleto.Nodes.Length; i++)
        {
            for (int ii = 0; ii < brazos.Count; ii++)
            {
                if (esqueleto.Nodes[i].Name == brazos[ii])
                    idBrazos[ii] = i;
            }
            for (int ii = 0; ii < piernas.Count; ii++)
            {
                if (esqueleto.Nodes[i].Name == piernas[ii])
                    idPiernas[ii] = i;
            }
        }

        rotaciónInicioBrazoIzq = Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(100), 0, MathUtil.DegreesToRadians(140));
        rotaciónInicioBrazoDer = Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(-100), 0, MathUtil.DegreesToRadians(-140));

        esqueleto.NodeTransformations[idBrazos[0]].Transform.Rotation = rotaciónInicioBrazoIzq;
        esqueleto.NodeTransformations[idBrazos[1]].Transform.Rotation = rotaciónInicioBrazoDer;

        pociciónLanzaInicio = lanza.Position;
        pociciónLanzaAtaque = lanza.Position + new Vector3(0, 0, 1.5f);

        rotaciónLanzaInicial = Quaternion.RotationYawPitchRoll(0, 0, -lanza.RotationEulerXYZ.Y);
    }

    public void Actualizar()
    {
        ApuntarLanza();
    }

    public void Activar(bool activar)
    {
        modelo.Enabled = activar;
        modeloLanza.Enabled = activar;
    }

    public void Caminar(float velocidad)
    {
        esqueleto.NodeTransformations[idPiernas[0]].Transform.Rotation *= Quaternion.RotationY(-velocidad * 10 * (float)Game.UpdateTime.WarpElapsed.TotalSeconds);
        esqueleto.NodeTransformations[idPiernas[1]].Transform.Rotation *= Quaternion.RotationY(velocidad * 10 * (float)Game.UpdateTime.WarpElapsed.TotalSeconds);
    }

    public void Atacar()
    {
        tokenAtaque.Cancel();
        tokenAtaque = new CancellationTokenSource();

        AnimarAtaque(Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(100), 0, MathUtil.DegreesToRadians(90)),
                     Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(-100), 0, MathUtil.DegreesToRadians(-90)));
    }

    public void Morir()
    {
        tokenAtaque.Cancel();
        tokenAtaque = new CancellationTokenSource();

        AnimarMuerte(modelo.Entity.Transform.Position, modelo.Entity.Transform.Rotation,
                     new Vector3(0, 0, 1.0f), Quaternion.RotationX(MathUtil.DegreesToRadians(-86)));

        AnimarLanzaMuerte(new Vector3(-0.3f, 0.08f, 0),
                          Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(25), MathUtil.DegreesToRadians(89), 0));

        // Apagar extremidades
        for (int i = 0; i < idBrazos.Length; i++)
        {
            esqueleto.NodeTransformations[idBrazos[i]].Transform.Scale = Vector3.Zero;
        }
        for (int i = 0; i < idPiernas.Length; i++)
        {
            esqueleto.NodeTransformations[idPiernas[i]].Transform.Scale = Vector3.Zero;
        }
    }

    private void ApuntarLanza()
    {
        ánguloDiferencia = ControladorPartida.ObtenerCabezaJugador().Y - lanza.WorldMatrix.TranslationVector.Y;
        lanza.Rotation = rotaciónLanzaInicial * Quaternion.RotationX(MathUtil.DegreesToRadians(85 - (ánguloDiferencia * 10)));
    }

    // Animaciones
    private async void AnimarAtaque(Quaternion objetivoIzq, Quaternion objetivoDer)
    {
        float duración = 0.4f;
        float tiempoLerp = 0;
        float tiempo = 0;

        var token = tokenAtaque.Token;
        while (tiempoLerp < duración)
        {
            if (token.IsCancellationRequested)
                break;

            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            lanza.Position = Vector3.Lerp(pociciónLanzaAtaque, pociciónLanzaInicio, tiempo);

            esqueleto.NodeTransformations[idBrazos[0]].Transform.Rotation = Quaternion.Lerp(objetivoIzq, rotaciónInicioBrazoIzq, tiempo);
            esqueleto.NodeTransformations[idBrazos[1]].Transform.Rotation = Quaternion.Lerp(objetivoDer, rotaciónInicioBrazoDer, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        // Fin
        lanza.Position = pociciónLanzaInicio;

        esqueleto.NodeTransformations[idBrazos[0]].Transform.Rotation = rotaciónInicioBrazoIzq;
        esqueleto.NodeTransformations[idBrazos[1]].Transform.Rotation = rotaciónInicioBrazoDer;
    }

    private async void AnimarMuerte(Vector3 posiciónInicio, Quaternion rotaciónInicio, Vector3 posiciónObjetivo, Quaternion rotaciónObjetivo)
    {
        float duración = 0.3f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = tiempoLerp / duración;

            modelo.Entity.Transform.Position = Vector3.Lerp(posiciónInicio, posiciónObjetivo, tiempo);
            modelo.Entity.Transform.Rotation = Quaternion.Lerp(rotaciónInicio, rotaciónObjetivo, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        // Fin
        modelo.Entity.Transform.Position = posiciónObjetivo;
        modelo.Entity.Transform.Rotation = rotaciónObjetivo;
    }

    private async void AnimarLanzaMuerte(Vector3 posiciónObjetivo, Quaternion rotaciónObjetivo)
    {
        await Task.Delay(200);

        var posiciónInicio = lanza.Position;
        var rotaciónInicio = lanza.Rotation;

        float duración = 0.2f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = tiempoLerp / duración;

            lanza.Position = Vector3.Lerp(posiciónInicio, posiciónObjetivo, tiempo);
            lanza.Rotation = Quaternion.Lerp(rotaciónInicio, rotaciónObjetivo, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        // Fin
        lanza.Position = posiciónObjetivo;
        lanza.Rotation = rotaciónObjetivo;
    }
}
