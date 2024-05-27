using System.Collections.Generic;
using Stride.Core.Mathematics;
using Stride.Particles.Components;
using Stride.Engine;

namespace Voronomir;
using static Utilidades;

public class ControladorBarril : StartupScript, IDañable
{
    public ModelComponent modelo;
    public ParticleSystemComponent partículas;
    public List<ElementoDañable> dañables { get; set; }

    private PhysicsComponent cuerpo;
    private ElementoDañable dañable;
    private float vida;
    private float dañoExplosión;

    public override void Start()
    {
        cuerpo = Entity.Get<PhysicsComponent>();
        foreach (var dañable in dañables)
        {
            dañable.Iniciar(this);
        }

        vida = 20;          // 2 balas de metralleta
        dañoExplosión = 70; // 10 menos que lanzagranadas
    }

    public void RecibirDaño(float daño)
    {
        if (vida <= 0)
            return;

        vida -= daño;
        if (vida > 0)
            return;

        // Explotar
        dañable.Desactivar();
        modelo.Enabled = false;
        cuerpo.Enabled = false;
        partículas.ParticleSystem.StopEmitters();
        EsperarCuadro();
    }

    public async void EsperarCuadro()
    {
        await EsperarCuadroFísica();
        ControladorCofres.IniciarExplosión(dañoExplosión, Entity.Transform.WorldMatrix.TranslationVector + (Vector3.One * 0.5f), Vector3.Zero);
    }

    public void Empujar(Vector3 dirección)
    {

    }
}
