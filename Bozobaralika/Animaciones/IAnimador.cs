namespace Bozobaralika;

public interface IAnimador
{
    public void Iniciar();
    public void Actualizar();
    public void Caminar(float velocidad);
    public void Atacar();
}
