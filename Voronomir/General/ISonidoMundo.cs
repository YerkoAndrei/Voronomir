namespace Voronomir;

public interface ISonidoMundo
{
    float distanciaSonido { get; set; }
    float distanciaJugador { get; set; }
    void ActualizarVolumen();
    void PausarSonidos(bool pausa);
}
