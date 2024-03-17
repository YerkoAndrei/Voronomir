using System.Collections.Generic;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Rendering;

namespace Bozobaralika;

public class AnimadorProcedural : SyncScript
{
    //pruebas
    private Vector3 objetivoPrueba0 = new Vector3(5, 0, 4);
    private Vector3 objetivoPrueba1 = new Vector3(5, 0, 0);

    private Vector3 objetivoKI0 = new Vector3(0, 0, 0.5f);
    private Vector3 objetivoKI1 = new Vector3(0, 0, -0.1f);
    private Vector3 baseIzq;

    //real
    public TransformComponent objetivo;
    public List<string> huesos = new List<string> { };

    private ModelComponent modelo;
    private SkeletonUpdater esqueleto;

    private int iteraciones;
    private int[] idHuesos;
    private float[] longitudHuesos;

    private int cantidadHuesos;
    private Vector3[] posicionesFinales;
    private Vector3[] posicionesInversas;
    private Vector3[] posicionesRectas;

    public override void Start()
    {
        modelo = Entity.Get<ModelComponent>();
        esqueleto = modelo.Skeleton;
        iteraciones = 5;

        // Inicialización
        cantidadHuesos = huesos.Count;
        posicionesFinales = new Vector3[cantidadHuesos];
        posicionesInversas = new Vector3[cantidadHuesos];
        posicionesRectas = new Vector3[cantidadHuesos];
        idHuesos = new int[cantidadHuesos];
        longitudHuesos = new float[cantidadHuesos];

        // Encuentra huesos por nombre, punta debe estar al último
        for (int i = 0; i < esqueleto.Nodes.Length; i++)
        {
            for (int ii = 0; ii < cantidadHuesos; ii++)
            {
                if (esqueleto.Nodes[i].Name == huesos[ii])
                    idHuesos[ii] = i;
            }
        }

        // Longitud de huesos
        for (int i = 0; i < cantidadHuesos; i++)
        {
            if (i < (cantidadHuesos - 1))
                longitudHuesos[i] = (esqueleto.NodeTransformations[idHuesos[i + 1]].Transform.Position - esqueleto.NodeTransformations[idHuesos[i]].Transform.Position).Length();
            else
                longitudHuesos[i] = 0;
        }

        baseIzq = objetivo.Position;
        ProbarKI(objetivoKI0);
        //ProbarCaminata(objetivoPrueba0);
    }

    public override void Update()
    {
        AplicarKI();
    }

    private void AplicarKI()
    {
        // Inicializando posiciones
        for (int i = 0; i < cantidadHuesos; i++)
        {
            posicionesFinales[i] = esqueleto.NodeTransformations[idHuesos[i]].Transform.Position;
        }

        // Aplicando FABRIK
        for (int i = 0; i < iteraciones; i++)
        {
            posicionesFinales = PosicionarRecto(PosicionarInverso(posicionesFinales));
        }

        // Aplicando resultados
        for (int i = 0; i < cantidadHuesos; i++)
        {
            esqueleto.NodeTransformations[idHuesos[i]].Transform.Position = posicionesFinales[i];

            if (i != (cantidadHuesos - 1))
                esqueleto.NodeTransformations[idHuesos[i]].Transform.Rotation = Quaternion.LookRotation(posicionesFinales[i + 1] - esqueleto.NodeTransformations[idHuesos[i]].Transform.Position, Vector3.UnitY);
            else
                esqueleto.NodeTransformations[idHuesos[i]].Transform.Rotation = Quaternion.LookRotation(objetivo.Position - esqueleto.NodeTransformations[idHuesos[i]].Transform.Position, Vector3.UnitY);            
        }
    }

    // FABRIK Backward
    private Vector3[] PosicionarInverso(Vector3[] _posicionesRectas)
    {
        // Cálculo desde punta
        for (int i = (cantidadHuesos - 1); i >= 0; i--)
        {
            if (i == (cantidadHuesos - 1))
                posicionesInversas[i] = objetivo.WorldMatrix.TranslationVector;
            else
            {
                var dirección = Vector3.Normalize(_posicionesRectas[i] - posicionesInversas[i + 1]);
                posicionesInversas[i] = posicionesInversas[i + 1] + (dirección * longitudHuesos[i]);
            }
        }
        return posicionesInversas;
    }

    // FABRIK Forward
    private Vector3[] PosicionarRecto(Vector3[] _posicionesInversas)
    {
        // Cálculo desde raíz
        for (int i = 0; i < cantidadHuesos; i++)
        {
            if (i  == 0)
                posicionesRectas[i] = esqueleto.NodeTransformations[idHuesos[0]].Transform.Position;
            else
            {
                var dirección = Vector3.Normalize(_posicionesInversas[i] - posicionesRectas[i - 1]);
                posicionesRectas[i] = posicionesRectas[i - 1] + (dirección * longitudHuesos[i - 1]);
            }
        }
        return posicionesRectas;
    }




    private async void ProbarKI(Vector3 objetivoKI)
    {
        float duración = 0.5f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);

            if (objetivoKI == objetivoKI0)
                objetivo.Position = baseIzq + Vector3.Lerp(objetivoKI1, objetivoKI0, tiempo);
            else
                objetivo.Position = baseIzq + Vector3.Lerp(objetivoKI0, objetivoKI1, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Task.Delay(1);
        }

        await Task.Delay(200);

        if (objetivoKI == objetivoKI0)
            ProbarKI(objetivoKI1);
        else
            ProbarKI(objetivoKI0);
    }

    private async void ProbarCaminata(Vector3 objetivo)
    {
        float duración = 2f;
        float tiempoLerp = 0;
        float tiempo = 0;

        while (tiempoLerp < duración)
        {
            tiempo = SistemaAnimación.EvaluarSuave(tiempoLerp / duración);
            
            if (objetivo == objetivoPrueba0)
                Entity.Transform.Position = Vector3.Lerp(objetivoPrueba1, objetivoPrueba0, tiempo);
            else
                Entity.Transform.Position = Vector3.Lerp(objetivoPrueba0, objetivoPrueba1, tiempo);

            tiempoLerp += (float)Game.UpdateTime.Elapsed.TotalSeconds;
            await Task.Delay(1);
        }

        await Task.Delay(200);

        if(objetivo == objetivoPrueba0)
            ProbarCaminata(objetivoPrueba1);
        else
            ProbarCaminata(objetivoPrueba0);
    }
}
