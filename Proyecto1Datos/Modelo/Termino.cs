using PruebaRider.Estructura.Nodo;
using PruebaRider.Estructura.Vector;

namespace PruebaRider.Modelo
{
    public class Termino : IComparable<Termino>
    {
        private string palabra;
        private ListaDobleEnlazada<DocumentoFrecuencia> documentos;
        private double idf;
        private int totalApariciones;

        public Termino(string palabra)
        {
            this.palabra = palabra?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(palabra));
            this.documentos = new ListaDobleEnlazada<DocumentoFrecuencia>();
            this.idf = 0.0;
            this.totalApariciones = 0;
        }
        
        public string Palabra => palabra;
        public ListaDobleEnlazada<DocumentoFrecuencia> Documentos => documentos;
        public double Idf 
        { 
            get => idf; 
            set => idf = value; 
        }
        
        public void AgregarDocumento(Documento documento, int frecuenciaTf)
        {
            if (documento == null)
                throw new ArgumentNullException(nameof(documento));
            
            if (frecuenciaTf <= 0)
                throw new ArgumentException("La frecuencia debe ser mayor a 0");
            
            var iterador = new Iterador<DocumentoFrecuencia>(documentos);
            while (iterador.Siguiente())
            {
                if (iterador.Current.Documento.Id == documento.Id)
                {
                    totalApariciones = totalApariciones - iterador.Current.FrecuenciaTf + frecuenciaTf;
                    iterador.Current.FrecuenciaTf = frecuenciaTf;
                    iterador.Current.TfIdf = frecuenciaTf * idf;
                    return;
                }
            }
            
            var docFrec = new DocumentoFrecuencia(documento, frecuenciaTf, frecuenciaTf * idf);
            documentos.Agregar(docFrec);
            totalApariciones += frecuenciaTf;
        }
        
        public void CalcularIdf(int totalDocumentos)
        {
            if (totalDocumentos <= 0 || documentos.Count == 0)
            {
                idf = 0.0;
                return;
            }

            idf = Math.Log10((double)totalDocumentos / documentos.Count);
            
            // Recalcular TF-IDF para todos los documentos
            ActualizarTfIdfDocumentos();
        }
        
        private void ActualizarTfIdfDocumentos()
        {
            var iterador = new Iterador<DocumentoFrecuencia>(documentos);
            while (iterador.Siguiente())
            {
                var docFrec = iterador.Current;
                docFrec.TfIdf = docFrec.FrecuenciaTf * idf;
            }
        }
        
        public double ObtenerTfIdf(int documentoId)
        {
            var iterador = new Iterador<DocumentoFrecuencia>(documentos);
            while (iterador.Siguiente())
            {
                if (iterador.Current.Documento.Id == documentoId)
                    return iterador.Current.TfIdf;
            }
            return 0.0;
        }
        
        public int CompareTo(Termino other)
        {
            if (other == null) return 1;
            return string.Compare(this.palabra, other.palabra, StringComparison.OrdinalIgnoreCase);
        }
        
    }
    
}