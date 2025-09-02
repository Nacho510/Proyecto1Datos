using PruebaRider.Estructura.Nodo;
using PruebaRider.Estructura.Vector;

namespace PruebaRider.Modelo
{
    /// <summary>
    /// Término reestructurado para el índice invertido
    /// - Implementa IComparable para Radix Sort
    /// - Mantiene lista de documentos con frecuencias TF-IDF
    /// - Optimizado para búsqueda vectorial
    /// </summary>
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

        // Propiedades
        public string Palabra => palabra;
        public ListaDobleEnlazada<DocumentoFrecuencia> Documentos => documentos;
        public double Idf 
        { 
            get => idf; 
            set => idf = value; 
        }
        public int CantidadDocumentos => documentos.Count;
        public int TotalApariciones => totalApariciones;

        /// <summary>
        /// Agregar documento con su frecuencia TF
        /// </summary>
        public void AgregarDocumento(Documento documento, int frecuenciaTf)
        {
            if (documento == null)
                throw new ArgumentNullException(nameof(documento));

            if (frecuenciaTf <= 0)
                throw new ArgumentException("La frecuencia debe ser mayor a 0");

            // Verificar si el documento ya existe
            var iterador = new Iterador<DocumentoFrecuencia>(documentos);
            while (iterador.Siguiente())
            {
                if (iterador.Current.Documento.Id == documento.Id)
                {
                    // Actualizar frecuencia existente
                    totalApariciones = totalApariciones - iterador.Current.FrecuenciaTf + frecuenciaTf;
                    iterador.Current.FrecuenciaTf = frecuenciaTf;
                    iterador.Current.TfIdf = frecuenciaTf * idf;
                    return;
                }
            }
        }

        /// <summary>
        /// Calcular IDF: log(N / DF)
        /// </summary>
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

        /// <summary>
        /// Actualizar valores TF-IDF después de cambiar IDF
        /// </summary>
        private void ActualizarTfIdfDocumentos()
        {
            var iterador = new Iterador<DocumentoFrecuencia>(documentos);
            while (iterador.Siguiente())
            {
                var docFrec = iterador.Current;
                docFrec.TfIdf = docFrec.FrecuenciaTf * idf;
            }
        }

        /// <summary>
        /// Obtener valor TF-IDF para un documento específico
        /// </summary>
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

        /// <summary>
        /// Obtener frecuencia TF para un documento específico
        /// </summary>
        public int ObtenerFrecuenciaTf(int documentoId)
        {
            var iterador = new Iterador<DocumentoFrecuencia>(documentos);
            while (iterador.Siguiente())
            {
                if (iterador.Current.Documento.Id == documentoId)
                    return iterador.Current.FrecuenciaTf;
            }
            return 0;
        }

        /// <summary>
        /// Verificar si el término aparece en un documento
        /// </summary>
        public bool ApareceEnDocumento(int documentoId)
        {
            var iterador = new Iterador<DocumentoFrecuencia>(documentos);
            while (iterador.Siguiente())
            {
                if (iterador.Current.Documento.Id == documentoId)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Crear vector TF-IDF para todos los documentos del corpus
        /// </summary>
       /* public Vector<int> CrearVectorTfIdf(ListaDobleEnlazada<Documento> todosDocumentos)
        {
            if (todosDocumentos == null || todosDocumentos.Count == 0)
                return new Vector(1);

            var vector = new Vector(todosDocumentos.Count);
            var iteradorDocs = new Iterador<Documento>(todosDocumentos);
            int indice = 0;

            while (iteradorDocs.Siguiente())
            {
                var doc = iteradorDocs.Current;
                double tfIdf = ObtenerTfIdf(doc.Id);
                vector[indice] = tfIdf;
                indice++;
            }

            return vector;
        }*/

        /// <summary>
        /// Implementación de IComparable para ordenamiento
        /// CRUCIAL para que funcione el RadixSort en VectorOrdenado
        /// </summary>
        public int CompareTo(Termino other)
        {
            if (other == null) return 1;
            return string.Compare(this.palabra, other.palabra, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Limpiar el término
        /// </summary>
        public void Limpiar()
        {
            documentos.Limpiar();
            idf = 0.0;
            totalApariciones = 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is Termino otro)
                return string.Equals(palabra, otro.palabra, StringComparison.OrdinalIgnoreCase);
            return false;
        }

        public override int GetHashCode()
        {
            return palabra.GetHashCode();
        }

        public override string ToString()
        {
            return $"{palabra} (docs: {documentos.Count}, IDF: {idf:F3}, apariciones: {totalApariciones})";
        }
    }

    /// <summary>
    /// Clase para almacenar documento con sus métricas TF-IDF
    /// </summary>
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
            return $"Doc{Documento.Id}: TF={FrecuenciaTf}, TF-IDF={TfIdf:F3}";
        }
    }
}