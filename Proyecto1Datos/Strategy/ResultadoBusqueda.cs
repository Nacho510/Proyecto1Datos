using PruebaRider.Modelo;

namespace PruebaRider.Servicios
{
    /// <summary>
    /// Resultado de búsqueda tradicional TF-IDF
    /// </summary>
    public class ResultadoBusqueda
    {
        public Documento Documento { get; set; }
        public double Score { get; set; }

        public ResultadoBusqueda(Documento documento, double score)
        {
            Documento = documento ?? throw new ArgumentNullException(nameof(documento));
            Score = score;
        }

        public override string ToString()
        {
            return $"📄 {Path.GetFileName(Documento.Ruta)} | Score: {Score:F3}";
        }
    }
}