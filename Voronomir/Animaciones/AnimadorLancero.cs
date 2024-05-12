using System.Collections.Generic;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Rendering;

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

    private Quaternion rotaciónInicio0;
    private Quaternion rotaciónInicio1;

    public void Iniciar()
    {
        esqueleto = modelo.Skeleton;
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

        rotaciónInicio0 = esqueleto.NodeTransformations[idBrazos[0]].Transform.Rotation;
        rotaciónInicio1 = esqueleto.NodeTransformations[idBrazos[1]].Transform.Rotation;

        pociciónLanzaInicio = lanza.Position;
        pociciónLanzaAtaque = lanza.Position + new Vector3(0, 0, 1.5f);

        rotaciónLanzaInicial = Quaternion.RotationYawPitchRoll(0, 0, -lanza.RotationEulerXYZ.Y);
    }

    public void Actualizar()
    {

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

        ApuntarLanza();
    }

    public void Atacar()
    {
        ApuntarLanza();
        AnimarAtaque();
    }

    private void ApuntarLanza()
    {
        ánguloDiferencia = ControladorPartida.ObtenerCabezaJugador().Y - lanza.WorldMatrix.TranslationVector.Y;
        lanza.Rotation = rotaciónLanzaInicial * Quaternion.RotationX(MathUtil.DegreesToRadians(85 - (ánguloDiferencia * 10)));
    }

    private async void AnimarAtaque()
    {
        var rotaciónAtaque0 = Quaternion.RotationZ(MathUtil.DegreesToRadians(-120));
        var rotaciónAtaque1 = Quaternion.RotationZ(MathUtil.DegreesToRadians(120));
        float duración = 0.4f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);

            lanza.Position = Vector3.Lerp(pociciónLanzaAtaque, pociciónLanzaInicio, tiempo);

            esqueleto.NodeTransformations[idBrazos[0]].Transform.Rotation = Quaternion.Lerp(rotaciónAtaque0, rotaciónInicio0, tiempo);
            esqueleto.NodeTransformations[idBrazos[1]].Transform.Rotation = Quaternion.Lerp(rotaciónAtaque1, rotaciónInicio1, tiempo);

            tiempoLerp += (float)Game.UpdateTime.WarpElapsed.TotalSeconds;
            await Task.Delay(1);
        }

        // Fin
        lanza.Position = pociciónLanzaInicio;

        esqueleto.NodeTransformations[idBrazos[0]].Transform.Rotation = rotaciónInicio0;
        esqueleto.NodeTransformations[idBrazos[1]].Transform.Rotation = rotaciónInicio1;
    }
}
