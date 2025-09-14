namespace PruebaRider.Modelo
{
    public class DocumentoFrecuencia
    {
        public Documento Documento { get; set; }
        public int FrecuenciaTf { get; set; }
        public double TfIdf { get; set; }

        public DocumentoFrecuencia(Documento documento, int frecuenciaTf, double tfIdf)
        {
            Documento = documento ?? throw new ArgumentNullException(nameof(documento));
            FrecuenciaTf = frecuenciaTf;
            TfIdf = tfIdf;
        }

        public override string ToString()
        {
            return $"Doc[{Documento.Id}] TF:{FrecuenciaTf} TF-IDF:{TfIdf:F3}";
        }
    }
}