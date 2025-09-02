using PruebaRider.Estructura.Nodo;
using PruebaRider.Estructura.Vector;
using PruebaRider.Modelo;

namespace PruebaRider.Servicios
{
    /// <summary>
    /// BuscadorVectorial CORREGIDO - AHORA SÍ FUNCIONA
    /// - Manejo robusto de términos faltantes
    /// - Vector de consulta siempre válido
    /// - Umbral de similitud más bajo
    /// - Mejor debugging
    /// </summary>
    public class BuscadorVectorial
    {
        private readonly IndiceInvertido indiceInvertido;
        private readonly ProcesadorDeTexto procesador;

        public BuscadorVectorial(IndiceInvertido indiceInvertido)
        {
            this.indiceInvertido = indiceInvertido ?? throw new ArgumentNullException(nameof(indiceInvertido));
            this.procesador = new ProcesadorDeTexto();
        }

        /// <summary>
        /// BÚSQUEDA VECTORIAL CORREGIDA - AHORA FUNCIONA CORRECTAMENTE
        /// </summary>
        public ListaDobleEnlazada<ResultadoBusquedaVectorial> BuscarConSimilitudCoseno(string consulta)
        {
            var resultados = new ListaDobleEnlazada<ResultadoBusquedaVectorial>();

            if (string.IsNullOrWhiteSpace(consulta))
                return resultados;

            Console.WriteLine($"🔍 Procesando consulta: '{consulta}'");

            // 1. PROCESAR CONSULTA
            var tokensConsulta = procesador.ProcesarTextoCompleto(consulta);
            if (tokensConsulta.Count == 0)
            {
                Console.WriteLine("❌ No se encontraron tokens válidos en la consulta");
                return resultados;
            }

            Console.WriteLine($"📝 Tokens encontrados: {string.Join(", ", tokensConsulta)}");

            // 2. VERIFICAR QUE TENEMOS VOCABULARIO
            if (indiceInvertido.GetIndice().Count == 0)
            {
                Console.WriteLine("❌ El índice está vacío - no hay vocabulario");
                return resultados;
            }

            // 3. CREAR VECTOR DE CONSULTA ROBUSTO
            var vectorConsulta = CrearVectorConsultaRobusto(tokensConsulta);
            if (vectorConsulta == null || !vectorConsulta.TieneValoresSignificativos())
            {
                Console.WriteLine("❌ No se pudo crear vector de consulta válido");
                // NUEVO: Intentar con términos parciales
                vectorConsulta = CrearVectorConsultaParcial(tokensConsulta);
                if (vectorConsulta == null || !vectorConsulta.TieneValoresSignificativos())
                {
                    Console.WriteLine("❌ Tampoco se pudo crear vector con términos parciales");
                    return resultados;
                }
            }

            Console.WriteLine($"📊 Vector consulta creado con magnitud: {vectorConsulta.Magnitud():F4}");

            // 4. BUSCAR EN TODOS LOS DOCUMENTOS CON UMBRAL MUY BAJO
            int documentosProcessados = 0;
            int documentosConSimilitud = 0;

            var iteradorDocs = new Iterador<Documento>(indiceInvertido.GetDocumentos());
            while (iteradorDocs.Siguiente())
            {
                var documento = iteradorDocs.Current;
                documentosProcessados++;

                // Crear vector del documento
                var vectorDoc = CrearVectorDocumentoRobusto(documento, vectorConsulta.Dimension);

                if (vectorDoc == null || !vectorDoc.TieneValoresSignificativos())
                    continue;

                // Calcular similitud coseno
                double similitud = vectorConsulta.SimilitudCoseno(vectorDoc);

                // UMBRAL EXTREMADAMENTE BAJO para encontrar cualquier resultado
                if (similitud > 0.0001)
                {
                    var resultado = new ResultadoBusquedaVectorial(documento, similitud);
                    resultados.Agregar(resultado);
                    documentosConSimilitud++;

                    Console.WriteLine(
                        $"   📄 {Path.GetFileName(documento.Ruta)}: {similitud:F4} ({similitud * 100:F1}%)");
                }
            }

            Console.WriteLine($"📊 Procesados: {documentosProcessados} documentos");
            Console.WriteLine($"📊 Con similitud: {documentosConSimilitud} documentos");

            // 5. ORDENAR RESULTADOS
            if (resultados.Count > 0)
            {
                resultados.OrdenarDescendente(r => r.SimilitudCoseno);
            }

            Console.WriteLine($"✅ Búsqueda completada: {resultados.Count} resultados finales");
            return resultados;
        }

        /// <summary>
        /// CREAR VECTOR DE CONSULTA ROBUSTO - Maneja términos faltantes
        /// </summary>
        private Vector CrearVectorConsultaRobusto(List<string> tokens)
        {
            // Obtener todos los términos del índice
            var todosLosTerminos = new ListaDobleEnlazada<string>();
            var iteradorIndice = new Iterador<Termino>(indiceInvertido.GetIndice());
            while (iteradorIndice.Siguiente())
            {
                todosLosTerminos.Agregar(iteradorIndice.Current.Palabra);
            }

            if (todosLosTerminos.Count == 0)
            {
                Console.WriteLine("❌ El índice está vacío");
                return null;
            }

            Console.WriteLine($"📚 Vocabulario total: {todosLosTerminos.Count} términos");

            // Crear vector con la dimensión del vocabulario total
            var vector = new Vector(todosLosTerminos.Count);

            // Contar frecuencias en la consulta
            var frecuenciasConsulta = ContarFrecuencias(tokens);

            // Llenar el vector
            var iteradorVocab = new Iterador<string>(todosLosTerminos);
            int indice = 0;
            int terminosEncontradosEnConsulta = 0;

            while (iteradorVocab.Siguiente())
            {
                string termino = iteradorVocab.Current;
                int frecuencia = ObtenerFrecuencia(frecuenciasConsulta, termino);

                if (frecuencia > 0)
                {
                    // Buscar el término en el índice para obtener su IDF
                    var terminoObj = indiceInvertido.BuscarTermino(termino);
                    if (terminoObj != null)
                    {
                        double tfIdf = frecuencia * terminoObj.Idf;
                        vector[indice] = tfIdf;
                        terminosEncontradosEnConsulta++;
                        Console.WriteLine(
                            $"   🔤 '{termino}': TF={frecuencia}, IDF={terminoObj.Idf:F3}, TF-IDF={tfIdf:F3}");
                    }
                    else
                    {
                        vector[indice] = 0.0;
                    }
                }
                else
                {
                    vector[indice] = 0.0;
                }

                indice++;
            }

            Console.WriteLine($"📊 Términos de consulta encontrados en vocabulario: {terminosEncontradosEnConsulta}");

            if (terminosEncontradosEnConsulta == 0)
            {
                Console.WriteLine("❌ Ningún término de la consulta está en el vocabulario");
                return null;
            }

            return vector;
        }

        /// <summary>
        /// NUEVO: Crear vector de consulta con términos parciales
        /// Intenta encontrar términos que contengan las palabras de búsqueda
        /// </summary>
        private Vector CrearVectorConsultaParcial(List<string> tokens)
        {
            Console.WriteLine("🔍 Intentando búsqueda con términos parciales...");

            var todosLosTerminos = new ListaDobleEnlazada<string>();
            var iteradorIndice = new Iterador<Termino>(indiceInvertido.GetIndice());
            while (iteradorIndice.Siguiente())
            {
                todosLosTerminos.Agregar(iteradorIndice.Current.Palabra);
            }

            if (todosLosTerminos.Count == 0) return null;

            var vector = new Vector(todosLosTerminos.Count);
            var iteradorVocab = new Iterador<string>(todosLosTerminos);
            int indice = 0;
            int coincidenciasParciales = 0;

            while (iteradorVocab.Siguiente())
            {
                string terminoVocabulario = iteradorVocab.Current;

                // Buscar coincidencias parciales
                bool hayCoincidencia = false;
                foreach (var tokenConsulta in tokens)
                {
                    if (string.IsNullOrWhiteSpace(tokenConsulta)) continue;

                    // Verificar si el término del vocabulario contiene el token de consulta o viceversa
                    if (terminoVocabulario.Contains(tokenConsulta.ToLowerInvariant()) ||
                        tokenConsulta.ToLowerInvariant().Contains(terminoVocabulario))
                    {
                        hayCoincidencia = true;
                        break;
                    }
                }

                if (hayCoincidencia)
                {
                    var terminoObj = indiceInvertido.BuscarTermino(terminoVocabulario);
                    if (terminoObj != null)
                    {
                        // Usar frecuencia reducida para coincidencias parciales
                        double tfIdf = 0.5 * terminoObj.Idf;
                        vector[indice] = tfIdf;
                        coincidenciasParciales++;
                        Console.WriteLine($"   🔤 Coincidencia parcial: '{terminoVocabulario}' -> TF-IDF={tfIdf:F3}");
                    }
                }
                else
                {
                    vector[indice] = 0.0;
                }

                indice++;
            }

            Console.WriteLine($"📊 Coincidencias parciales encontradas: {coincidenciasParciales}");

            return coincidenciasParciales > 0 ? vector : null;
        }

        /// <summary>
        /// CREAR VECTOR DE DOCUMENTO ROBUSTO - Con dimensión fija
        /// </summary>
        private Vector CrearVectorDocumentoRobusto(Documento documento, int dimension)
        {
            // Obtener todos los términos del índice (mismo orden que consulta)
            var todosLosTerminos = new ListaDobleEnlazada<string>();
            var iteradorIndice = new Iterador<Termino>(indiceInvertido.GetIndice());
            while (iteradorIndice.Siguiente())
            {
                todosLosTerminos.Agregar(iteradorIndice.Current.Palabra);
            }

            var vector = new Vector(dimension);

            var iteradorVocab = new Iterador<string>(todosLosTerminos);
            int indice = 0;
            int valoresNoZero = 0;

            while (iteradorVocab.Siguiente() && indice < dimension)
            {
                string termino = iteradorVocab.Current;
                int frecuencia = documento.GetFrecuencia(termino);

                if (frecuencia > 0)
                {
                    var terminoObj = indiceInvertido.BuscarTermino(termino);
                    if (terminoObj != null)
                    {
                        double tfIdf = frecuencia * terminoObj.Idf;
                        vector[indice] = tfIdf;
                        valoresNoZero++;
                    }
                    else
                    {
                        vector[indice] = 0.0;
                    }
                }
                else
                {
                    vector[indice] = 0.0;
                }

                indice++;
            }

            // Console.WriteLine($"   📊 Vector documento: {valoresNoZero} valores no-cero de {dimension}");
            return vector;
        }

        /// <summary>
        /// Contar frecuencias de tokens - SIMPLIFICADO
        /// </summary>
        private TokenFrecuencia[] ContarFrecuencias(List<string> tokens)
        {
            var resultado = new TokenFrecuencia[tokens.Count];
            int cantidadUnicos = 0;

            foreach (var token in tokens)
            {
                if (string.IsNullOrWhiteSpace(token)) continue;

                string tokenNorm = token.ToLowerInvariant();
                bool encontrado = false;

                // Buscar si ya existe
                for (int i = 0; i < cantidadUnicos; i++)
                {
                    if (resultado[i].Token == tokenNorm)
                    {
                        resultado[i].Frecuencia++;
                        encontrado = true;
                        break;
                    }
                }

                if (!encontrado)
                {
                    resultado[cantidadUnicos] = new TokenFrecuencia { Token = tokenNorm, Frecuencia = 1 };
                    cantidadUnicos++;
                }
            }

            // Crear array del tamaño exacto
            var final = new TokenFrecuencia[cantidadUnicos];
            Array.Copy(resultado, final, cantidadUnicos);
            return final;
        }

        /// <summary>
        /// Obtener frecuencia de un token específico
        /// </summary>
        private int ObtenerFrecuencia(TokenFrecuencia[] frecuencias, string token)
        {
            for (int i = 0; i < frecuencias.Length; i++)
            {
                if (string.Equals(frecuencias[i].Token, token, StringComparison.OrdinalIgnoreCase))
                    return frecuencias[i].Frecuencia;
            }

            return 0;
        }
    }

    /// <summary>
    /// Estructura para frecuencias de tokens
    /// </summary>
    public struct TokenFrecuencia
    {
        public string Token { get; set; }
        public int Frecuencia { get; set; }
    }
}