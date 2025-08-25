using PruebaRider.Modelo;

namespace PruebaRider.Strategy;

public class ResultadoBusqueda
{
    public Documento Documento { get; set; }
    public double Score { get; set; }

    public ResultadoBusqueda(Documento documento, double score)
    {
        Documento = documento;
        Score = score;
    }
}