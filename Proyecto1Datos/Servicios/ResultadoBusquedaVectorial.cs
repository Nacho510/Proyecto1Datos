// ===============================================================
using PruebaRider.Modelo;

namespace PruebaRider.Servicios
{
    /// <summary>
    /// CORREGIDO: Resultado de búsqueda vectorial con enlaces base64
    /// Compatible con el nuevo sistema que usa Vector personalizado
    /// </summary>
    public class ResultadoBusquedaVectorial
    {
        public Documento Documento { get; set; }
        public double SimilitudCoseno { get; set; }
        public string EnlaceBase64 { get; private set; }

        public ResultadoBusquedaVectorial(Documento documento, double similitudCoseno)
        {
            Documento = documento ?? throw new ArgumentNullException(nameof(documento));
            SimilitudCoseno = Math.Max(0.0, Math.Min(1.0, similitudCoseno));
            EnlaceBase64 = GenerarEnlaceBase64();
        }

        private string GenerarEnlaceBase64()
        {
            try
            {
                string contenido = File.Exists(Documento.Ruta) 
                    ? File.ReadAllText(Documento.Ruta)
                    : Documento.TextoOriginal ?? "Contenido no disponible";
                
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(contenido);
                string base64 = Convert.ToBase64String(bytes);
                return $"data:text/plain;base64,{base64}";
            }
            catch (Exception)
            {
                byte[] errorBytes = System.Text.Encoding.UTF8.GetBytes(
                    $"Error al cargar: {Path.GetFileName(Documento.Ruta)}");
                string errorBase64 = Convert.ToBase64String(errorBytes);
                return $"data:text/plain;base64,{errorBase64}";
            }
        }

        public override string ToString()
        {
            return $"📄 {Path.GetFileName(Documento.Ruta)} | {SimilitudCoseno * 100:F1}% | 🔗 Base64";
        }
    }
}

