using System.Collections.Generic;
using Stride.Engine;

namespace Voronomir;
using static Utilidades;

public class ControladorActivadorMuerte : StartupScript
{
    public List<ControladorEnemigo> enemigos = new List<ControladorEnemigo> { };
    public List<Entity> activables = new List<Entity> { };

    private IActivable[] interfaces;
    private bool activo;

    public override void Start()
    {
        activo = true;
        interfaces = new IActivable[activables.Count];
        for (int i=0; i < activables.Count; i++)
        {
            interfaces[i] = ObtenerInterfaz<IActivable>(activables[i]);
        }
    }

    public void Activar()
    {
        if (!activo)
            return;

        // Revisa que todos estén muertos
        foreach (var enemigo in enemigos)
        {
            if (!enemigo.ObtenerMuerto())
                return;
        }

        foreach (var activable in interfaces)
        {
            activable.Activar();
        }

        activo = false;
    }
}
