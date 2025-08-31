using PruebaRider.Estructura.Nodo;
using PruebaRider.Estructura.Vector;

namespace PruebaRider.Modelo
{
    /// <summary>
    /// Termino MEJORADO que usa Vector propio para almacenar documentos
    /// - Vector de IDs de documentos para acceso O(1)
    /// - Lista doble enlazada para referencias completas
    /// - Eficiencia optimizada para búsquedas
    /// - Cumple requisitos: solo estructuras propias
    /// </summary>
    public class Termino
    {
        private string palabra;
        private ListaDobleEnlazada<Documento> listaDocumentos;
        private Vector vectorDocumentosIds; // NUEVO: Vector para IDs rápidos
        private double idf;
        private int capacidadVector; // Gestión de tamaño dinámico
        private int documentosActuales;
        
        public Termino(string palabra, int capacidadInicialVector = 100)
        {
            this.palabra = palabra ?? throw new ArgumentNullException(nameof(palabra));
            this.listaDocumentos = new ListaDobleEnlazada<Documento>();
            this.vectorDocumentosIds = new Vector(capacidadInicialVector);
            this.capacidadVector = capacidadInicialVector;
            this.documentosActuales = 0;
            this.idf = 0.0;
        }

        // Propiedades
        public string Palabra
        {
            get => palabra;
            set => palabra = value ?? throw new ArgumentNullException(nameof(value));
        }

        public ListaDobleEnlazada<Documento> ListaDocumentos => listaDocumentos;

        public Vector VectorDocumentosIds => vectorDocumentosIds; // NUEVO: Acceso al vector

        public double Idf
        {
            get => idf;
            set => idf = value;
        }

        public int CantidadDocumentos => documentosActuales;

        /// <summary>
        /// MEJORADO: Agregar documento con actualización vectorial
        /// O(n) para verificación + O(1) para inserción vectorial
        /// </summary>
        public void AgregarDocumento(Documento documento)
        {
            if (documento == null)
                throw new ArgumentNullException(nameof(documento));

            // Verificar si ya existe (evitar duplicados)
            if (ExisteDocumento(documento.Id))
                return;

            // Expandir vector si es necesario
            if (documentosActuales >= capacidadVector)
            {
                ExpandirVector();
            }

            // Agregar a lista doble enlazada (referencia completa)
            listaDocumentos.Agregar(documento);

            // Agregar ID al vector (acceso rápido)
            vectorDocumentosIds[documentosActuales] = documento.Id;
            documentosActuales++;
        }

        /// <summary>
        /// NUEVO: Verificar existencia usando vector (más rápido para IDs)
        /// O(n) en el peor caso, pero con mejor localidad de memoria
        /// </summary>
        public bool ExisteDocumento(int documentoId)
        {
            // Búsqueda en vector de IDs (más eficiente)
            for (int i = 0; i < documentosActuales; i++)
            {
                if (Math.Abs(vectorDocumentosIds[i] - documentoId) < 0.1) // Comparación de doubles
                    return true;
            }
            return false;
        }

        /// <summary>
        /// NUEVO: Obtener documento por ID usando búsqueda híbrida
        /// Vector para localizar, lista para obtener referencia completa
        /// </summary>
        public Documento ObtenerDocumentoPorId(int documentoId)
        {
            // Primero buscar posición en vector
            int posicion = -1;
            for (int i = 0; i < documentosActuales; i++)
            {
                if (Math.Abs(vectorDocumentosIds[i] - documentoId) < 0.1)
                {
                    posicion = i;
                    break;
                }
            }

            if (posicion == -1)
                return null;

            // Buscar en lista enlazada (podríamos optimizar esto con índices)
            var iterador = new Iterador<Documento>(listaDocumentos);
            int contador = 0;
            while (iterador.Siguiente())
            {
                if (contador == posicion)
                    return iterador.Current;
                contador++;
            }

            return null;
        }

        /// <summary>
        /// Calcular IDF con validación mejorada - O(1)
        /// </summary>
        public void CalcularIdf(int totalDocumentos)
        {
            if (totalDocumentos <= 0)
            {
                idf = 0.0;
                return;
            }

            if (documentosActuales > 0 && documentosActuales <= totalDocumentos)
            {
                idf = Math.Log10((double)totalDocumentos / documentosActuales);
                
                // Validar resultado
                if (double.IsNaN(idf) || double.IsInfinity(idf))
                    idf = 0.0;
            }
            else
            {
                idf = 0.0;
            }
        }

        /// <summary>
        /// Calcular TF-IDF para un documento específico - O(1) con cache
        /// </summary>
        public double GetTfIdf(Documento documento)
        {
            if (documento == null)
                return 0.0;

            int tf = documento.GetFrecuencia(palabra);
            if (tf == 0)
                return 0.0;

            double resultado = tf * idf;
            
            // Validar resultado
            if (double.IsNaN(resultado) || double.IsInfinity(resultado))
                return 0.0;
                
            return resultado;
        }

        /// <summary>
        /// NUEVO: Crear vector TF-IDF para este término
        /// Vector con valores TF-IDF para cada documento que contiene el término
        /// </summary>
        public Vector CrearVectorTfIdf(ListaDobleEnlazada<Documento> todosDocumentos)
        {
            if (todosDocumentos == null || todosDocumentos.Count == 0)
                return new Vector(1);

            var vectorTfIdf = new Vector(todosDocumentos.Count);
            
            var iteradorDocs = new Iterador<Documento>(todosDocumentos);
            int indice = 0;
            
            while (iteradorDocs.Siguiente())
            {
                var documento = iteradorDocs.Current;
                double tfIdf = GetTfIdf(documento);
                vectorTfIdf[indice] = tfIdf;
                indice++;
            }
            
            return vectorTfIdf;
        }

        /// <summary>
        /// NUEVO: Obtener estadísticas vectoriales del término
        /// </summary>
        public EstadisticasTermino ObtenerEstadisticas()
        {
            double sumaFrec = 0.0;
            double maxFrec = 0.0;
            double minFrec = double.MaxValue;
            
            var iterador = new Iterador<Documento>(listaDocumentos);
            while (iterador.Siguiente())
            {
                double freq = iterador.Current.GetFrecuencia(palabra);
                sumaFrec += freq;
                maxFrec = Math.Max(maxFrec, freq);
                if (freq > 0) // Solo considerar frecuencias positivas para el mínimo
                    minFrec = Math.Min(minFrec, freq);
            }
            
            if (minFrec == double.MaxValue) minFrec = 0.0;
            
            return new EstadisticasTermino
            {
                Palabra = palabra,
                CantidadDocumentos = documentosActuales,
                FrecuenciaPromedio = documentosActuales > 0 ? sumaFrec / documentosActuales : 0.0,
                FrecuenciaMaxima = maxFrec,
                FrecuenciaMinima = minFrec,
                Idf = idf,
                CapacidadVector = capacidadVector
            };
        }

        /// <summary>
        /// NUEVO: Compactar vector eliminando espacios no utilizados
        /// </summary>
        public void CompactarVector()
        {
            if (documentosActuales == 0)
            {
                vectorDocumentosIds = new Vector(10); // Vector mínimo
                capacidadVector = 10;
                return;
            }

            // Crear vector del tamaño exacto
            var valoresCompactos = new double[documentosActuales];
            for (int i = 0; i < documentosActuales; i++)
            {
                valoresCompactos[i] = vectorDocumentosIds[i];
            }
            
            vectorDocumentosIds = new Vector(valoresCompactos);
            capacidadVector = documentosActuales;
        }

        /// <summary>
        /// Expandir capacidad del vector cuando se llena
        /// </summary>
        private void ExpandirVector()
        {
            int nuevaCapacidad = capacidadVector * 2;
            var nuevosValores = new double[nuevaCapacidad];
            
            // Copiar valores existentes
            for (int i = 0; i < documentosActuales; i++)
            {
                nuevosValores[i] = vectorDocumentosIds[i];
            }
            
            vectorDocumentosIds = new Vector(nuevosValores);
            capacidadVector = nuevaCapacidad;
        }

        /// <summary>
        /// NUEVO: Eliminar documento del término
        /// </summary>
        public bool EliminarDocumento(int documentoId)
        {
            // Encontrar posición en vector
            int posicion = -1;
            for (int i = 0; i < documentosActuales; i++)
            {
                if (Math.Abs(vectorDocumentosIds[i] - documentoId) < 0.1)
                {
                    posicion = i;
                    break;
                }
            }
            
            if (posicion == -1)
                return false; // No encontrado
            
            // Eliminar de lista enlazada
            var iterador = new Iterador<Documento>(listaDocumentos);
            int contador = 0;
            NodoDoble<Documento> nodoAEliminar = null;
            
            while (iterador.Siguiente())
            {
                if (contador == posicion)
                {
                    // Encontrar el nodo en la lista
                    var nodoActual = listaDocumentos.Root;
                    for (int i = 0; i < posicion; i++)
                    {
                        nodoActual = nodoActual.Sig;
                    }
                    nodoAEliminar = nodoActual;
                    break;
                }
                contador++;
            }
            
            if (nodoAEliminar != null)
            {
                listaDocumentos.EliminarNodo(nodoAEliminar);
            }
            
            // Compactar vector (mover elementos hacia adelante)
            for (int i = posicion; i < documentosActuales - 1; i++)
            {
                vectorDocumentosIds[i] = vectorDocumentosIds[i + 1];
            }
            
            documentosActuales--;
            vectorDocumentosIds[documentosActuales] = 0.0; // Limpiar última posición
            
            return true;
        }

        /// <summary>
        /// Limpiar término
        /// </summary>
        public void Limpiar()
        {
            listaDocumentos.Limpiar();
            vectorDocumentosIds = new Vector(10);
            capacidadVector = 10;
            documentosActuales = 0;
            idf = 0.0;
        }

        public override bool Equals(object obj)
        {
            if (obj is Termino otro)
                return string.Equals(palabra, otro.palabra, StringComparison.OrdinalIgnoreCase);
            return false;
        }

        public override int GetHashCode()
        {
            return palabra?.ToLowerInvariant().GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return $"Término[{palabra}|{documentosActuales}docs|IDF:{idf:F3}]";
        }
    }

    /// <summary>
    /// NUEVO: Estadísticas del término usando solo tipos básicos
    /// </summary>
    public class EstadisticasTermino
    {
        public string Palabra { get; set; }
        public int CantidadDocumentos { get; set; }
        public double FrecuenciaPromedio { get; set; }
        public double FrecuenciaMaxima { get; set; }
        public double FrecuenciaMinima { get; set; }
        public double Idf { get; set; }
        public int CapacidadVector { get; set; }

        public override string ToString()
        {
            return $"{Palabra}: {CantidadDocumentos} docs, " +
                   $"Freq avg:{FrecuenciaPromedio:F2}, " +
                   $"IDF:{Idf:F3}, " +
                   $"Vector:{CapacidadVector}";
        }
    }
}