namespace Voronomir;

public interface IAnimador
{
    public void Iniciar();
    public void Actualizar();
    public void Activar(bool activar);
    public void Caminar(float velocidad);
    public void Atacar();
}
