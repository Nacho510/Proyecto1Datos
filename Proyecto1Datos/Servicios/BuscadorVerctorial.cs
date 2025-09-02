using PruebaRider.Estructura.Nodo;
using PruebaRider.Estructura.Vector;
using PruebaRider.Modelo;

namespace PruebaRider.Servicios
{
    /// <summary>
    /// Buscador vectorial simplificado y optimizado
    /// - Enfocado en similitud coseno perfecta
    /// - Usa el nuevo índice con vector ordenado y RadixSort
    /// - Sin complejidad innecesaria
    /// </summary>
    public class BuscadorVectorial
    {
        private readonly IndiceInvertido indice;

        public BuscadorVectorial(IndiceInvertido indice)
        {
            this.indice = indice ?? throw new ArgumentNullException(nameof(indice));
        }

        /// <summary>
        /// BÚSQUEDA VECTORIAL PRINCIPAL
        /// Este es el corazón del sistema de búsqueda
        /// </summary>
        public ListaDobleEnlazada<ResultadoBusquedaVectorial> Buscar(string consulta)
        {
            var resultados = new ListaDobleEnlazada<ResultadoBusquedaVectorial>();

            if (string.IsNullOrWhiteSpace(consulta))
                return resultados;

            Console.WriteLine($"🔍 Buscador vectorial: '{consulta}'");

            // 1. CREAR VECTOR DE CONSULTA
            var vectorConsulta = CrearVectorConsulta(consulta);
            if (vectorConsulta == null)
            {
                Console.WriteLine("❌ No se pudo crear vector de consulta");
                return resultados;
            }

            if (!vectorConsulta.TieneValoresSignificativos())
            {
                Console.WriteLine("❌ Vector de consulta sin valores significativos");
                return resultados;
            }

            Console.WriteLine($"📊 Vector consulta: magnitud = {vectorConsulta.Magnitud():F4}");

            // 2. COMPARAR CON TODOS LOS DOCUMENTOS
            var documentos = indice.GetDocumentos();
            var iteradorDocs = new Iterador<Documento>(documentos);
            int procesados = 0;
            int conSimilitud = 0;

            while (iteradorDocs.Siguiente())
            {
                var documento = iteradorDocs.Current;
                procesados++;

                // Crear vector TF-IDF del documento
                var vectorDoc = CrearVectorDocumento(documento);
                
                if (vectorDoc == null || !vectorDoc.TieneValoresSignificativos())
                    continue;

                // CALCULAR SIMILITUD COSENO
                double similitud = vectorConsulta.SimilitudCoseno(vectorDoc);

                if (similitud > 0.0001) // Umbral muy bajo para capturar cualquier similitud
                {
                    var resultado = new ResultadoBusquedaVectorial(documento, similitud);
                    resultados.Agregar(resultado);
                    conSimilitud++;
                    
                    Console.WriteLine($"   📄 {Path.GetFileName(documento.Ruta)}: {similitud:F4} ({similitud * 100:F1}%)");
                }
            }

            Console.WriteLine($"📊 Procesados: {procesados}, Con similitud: {conSimilitud}");

            // 3. ORDENAR POR SIMILITUD DESCENDENTE
            if (resultados.Count > 0)
            {
                resultados.OrdenarDescendente(r => r.SimilitudCoseno);
            }

            return resultados;
        }

        /// <summary>
        /// Crear vector TF-IDF para la consulta
        /// </summary>
        private Vector CrearVectorConsulta(string consulta)
        {
            // Procesar consulta
            var procesador = new ProcesadorDeTexto();
            var tokens = procesador.ProcesarTextoCompleto(consulta);
            
            if (tokens.Count == 0)
                return null;

            // Contar frecuencias en la consulta
            var frecuenciasConsulta = ContarFrecuencias(tokens);

            // Obtener vocabulario completo del índice
            var indiceTerminos = indice.GetIndiceTerminos();
            if (indiceTerminos.Count == 0)
                return null;

            // Crear vector con dimensión = tamaño del vocabulario
            var vector = new Vector(indiceTerminos.Count);
            var iterador = indiceTerminos.ObtenerIterador();
            int posicion = 0;

            while (iterador.Siguiente())
            {
                var termino = iterador.Current;
                
                // Buscar frecuencia del término en la consulta
                int frecuenciaEnConsulta = ObtenerFrecuencia(frecuenciasConsulta, termino.Palabra);
                
                if (frecuenciaEnConsulta > 0)
                {
                    // TF-IDF para consulta: TF * IDF
                    double tfIdf = frecuenciaEnConsulta * termino.Idf;
                    vector[posicion] = tfIdf;
                    Console.WriteLine($"   🔤 '{termino.Palabra}': TF={frecuenciaEnConsulta}, IDF={termino.Idf:F3}, TF-IDF={tfIdf:F3}");
                }
                else
                {
                    vector[posicion] = 0.0;
                }
                
                posicion++;
            }

            return vector;
        }

        /// <summary>
        /// Crear vector TF-IDF para un documento
        /// </summary>
        private Vector CrearVectorDocumento(Documento documento)
        {
            var indiceTerminos = indice.GetIndiceTerminos();
            if (indiceTerminos.Count == 0)
                return null;

            var vector = new Vector(indiceTerminos.Count);
            var iterador = indiceTerminos.ObtenerIterador();
            int posicion = 0;

            while (iterador.Siguiente())
            {
                var termino = iterador.Current;
                
                // Obtener TF-IDF del término para este documento
                double tfIdf = termino.ObtenerTfIdf(documento.Id);
                vector[posicion] = tfIdf;
                
                posicion++;
            }

            return vector;
        }

        /// <summary>
        /// Contar frecuencias de tokens sin usar estructuras prohibidas
        /// </summary>
        private TokenConteo[] ContarFrecuencias(List<string> tokens)
        {
            var conteos = new TokenConteo[tokens.Count]; // Máximo posible
            int cantidadUnicos = 0;

            foreach (var token in tokens)
            {
                if (string.IsNullOrWhiteSpace(token)) continue;

                string tokenNorm = token.ToLowerInvariant();
                bool encontrado = false;

                // Buscar si ya existe
                for (int i = 0; i < cantidadUnicos; i++)
                {
                    if (conteos[i].Token == tokenNorm)
                    {
                        conteos[i].Frecuencia++;
                        encontrado = true;
                        break;
                    }
                }

                if (!encontrado)
                {
                    conteos[cantidadUnicos] = new TokenConteo 
                    { 
                        Token = tokenNorm, 
                        Frecuencia = 1 
                    };
                    cantidadUnicos++;
                }
            }

            // Crear array del tamaño exacto
            var resultado = new TokenConteo[cantidadUnicos];
            Array.Copy(conteos, resultado, cantidadUnicos);
            return resultado;
        }

        /// <summary>
        /// Obtener frecuencia de un token específico
        /// </summary>
        private int ObtenerFrecuencia(TokenConteo[] conteos, string token)
        {
            for (int i = 0; i < conteos.Length; i++)
            {
                if (string.Equals(conteos[i].Token, token, StringComparison.OrdinalIgnoreCase))
                    return conteos[i].Frecuencia;
            }
            return 0;
        }

        /// <summary>
        /// Estructura simple para conteo de tokens
        /// </summary>
        private struct TokenConteo
        {
            public string Token { get; set; }
            public int Frecuencia { get; set; }
        }
    }
    
}