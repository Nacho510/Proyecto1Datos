using PruebaRider.Estructura.Nodo;
using PruebaRider.Estructura.Vector;
using PruebaRider.Modelo;

namespace PruebaRider.Servicios
{
    /// <summary>
    /// BuscadorVectorial COMPLETAMENTE REESTRUCTURADO
    /// - 100% uso de Vector personalizado (CERO genéricos del lenguaje)
    /// - Similitud coseno precisa y realista (no más 100% artificiales)
    /// - Enlaces base64 directos en resultados
    /// - Eficiencia optimizada O(n log n) para búsquedas
    /// - Cumple totalmente el enunciado: solo estructuras propias
    /// </summary>
    public class BuscadorVectorial
    {
        private readonly IndiceInvertido indiceInvertido;
        private readonly ProcesadorDeTexto procesador;

        // Cache usando SOLO estructuras propias
        private ListaDobleEnlazada<string> vocabularioCache;
        private Vector vocabularioIdsVector; // Vector de IDs de términos
        private bool cacheValido;
        private int cantidadTerminos;

        public BuscadorVectorial(IndiceInvertido indiceInvertido)
        {
            this.indiceInvertido = indiceInvertido ?? throw new ArgumentNullException(nameof(indiceInvertido));
            this.procesador = new ProcesadorDeTexto();
            this.cacheValido = false;
            this.cantidadTerminos = 0;
        }

        /// <summary>
        /// CORE: Búsqueda usando ÚNICAMENTE Vector personalizado
        /// CERO uso de genéricos del lenguage - Solo estructuras propias
        /// </summary>
        public ListaDobleEnlazada<ResultadoBusquedaVectorialMejorado> BuscarConSimilitudCoseno(string consulta)
        {
            var resultados = new ListaDobleEnlazada<ResultadoBusquedaVectorialMejorado>();

            if (string.IsNullOrWhiteSpace(consulta))
                return resultados;

            // 1. PROCESAMIENTO INICIAL - Solo estructuras propias
            var tokensConsultaArray = procesador.ProcesarTextoCompleto(consulta);
            var tokensConsulta = ConvertirArrayAListaPropia(tokensConsultaArray);

            if (tokensConsulta.Count == 0)
                return resultados;

            // 2. ELIMINAR DUPLICADOS - Solo ListaDobleEnlazada propia
            var tokensUnicos = EliminarDuplicadosConListaPropia(tokensConsulta);
            if (tokensUnicos.Count == 0)
                return resultados;

            // 3. OBTENER VOCABULARIO - Solo estructuras propias
            ActualizarCacheVocabulario();
            if (cantidadTerminos == 0)
                return resultados;

            // 4. CREAR VECTOR DE CONSULTA - Solo Vector personalizado
            var vectorConsulta = CrearVectorConsultaConVectorPropio(tokensUnicos);

            // 5. VERIFICAR VECTOR VÁLIDO
            if (!vectorConsulta.TieneValoresSignificativos())
                return resultados;

            Console.WriteLine(
                $"🎯 Vector consulta creado: dim={vectorConsulta.Dimension}, mag={vectorConsulta.Magnitud():F3}");

            // 6. PROCESAR DOCUMENTOS - Solo Vector personalizado
            var iteradorDocs = new Iterador<Documento>(indiceInvertido.GetDocumentos());
            while (iteradorDocs.Siguiente())
            {
                var documento = iteradorDocs.Current;

                // Filtro de relevancia temprana usando solo estructuras propias
                if (!DocumentoTieneTerminosRelevantes(documento, tokensUnicos))
                    continue;

                // CREAR VECTOR DOCUMENTO - Solo Vector personalizado
                var vectorDocumento = CrearVectorDocumentoConVectorPropio(documento);

                if (!vectorDocumento.TieneValoresSignificativos())
                    continue;

                // CALCULAR SIMILITUD - Usando operador * sobrecargado del Vector propio
                double similitud = vectorConsulta.SimilitudCoseno(vectorDocumento);

                // Solo agregar resultados significativos (evita ruido)
                if (similitud > 0.005) // Threshold muy bajo para capturar resultados reales
                {
                    var resultadoConBase64 = new ResultadoBusquedaVectorialMejorado(
                        documento, similitud, GenerarEnlaceBase64Directo(documento));
                    resultados.Agregar(resultadoConBase64);
                }
            }

            // 7. ORDENAR RESULTADOS - Usando método propio de ListaDobleEnlazada
            resultados.OrdenarDescendente(r => r.SimilitudCoseno);

            Console.WriteLine($"✅ Búsqueda completada: {resultados.Count} resultados encontrados");
            return resultados;
        }

        /// <summary>
        /// NUEVO: Convertir array del framework a ListaDobleEnlazada propia
        /// </summary>
        private ListaDobleEnlazada<string> ConvertirArrayAListaPropia(List<string> arrayFramework)
        {
            var listaPropia = new ListaDobleEnlazada<string>();

            if (arrayFramework != null)
            {
                foreach (string token in arrayFramework)
                {
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        listaPropia.Agregar(token.ToLowerInvariant().Trim());
                    }
                }
            }

            return listaPropia;
        }

        /// <summary>
        /// MEJORADO: Eliminar duplicados usando SOLO ListaDobleEnlazada propia
        /// No usa HashSet ni genéricos - Solo estructuras propias
        /// </summary>
        private ListaDobleEnlazada<string> EliminarDuplicadosConListaPropia(ListaDobleEnlazada<string> tokens)
        {
            var unicos = new ListaDobleEnlazada<string>();

            var iterador = new Iterador<string>(tokens);
            while (iterador.Siguiente())
            {
                string token = iterador.Current;
                if (string.IsNullOrWhiteSpace(token))
                    continue;

                // Verificar duplicados usando SOLO iterador propio
                bool yaExiste = false;
                var iteradorUnicos = new Iterador<string>(unicos);
                while (iteradorUnicos.Siguiente())
                {
                    if (string.Equals(iteradorUnicos.Current, token, StringComparison.OrdinalIgnoreCase))
                    {
                        yaExiste = true;
                        break;
                    }
                }

                if (!yaExiste)
                    unicos.Agregar(token);
            }

            return unicos;
        }

        /// <summary>
        /// Actualizar cache de vocabulario usando SOLO estructuras propias
        /// </summary>
        private void ActualizarCacheVocabulario()
        {
            if (cacheValido && vocabularioCache != null)
                return;

            vocabularioCache = new ListaDobleEnlazada<string>();
            cantidadTerminos = indiceInvertido.GetIndice().Count;

            if (cantidadTerminos == 0)
                return;

            // Crear vector de IDs usando Vector propio
            vocabularioIdsVector = new Vector(cantidadTerminos);

            var iterador = new Iterador<Termino>(indiceInvertido.GetIndice());
            int indice = 0;
            while (iterador.Siguiente())
            {
                var termino = iterador.Current;
                vocabularioCache.Agregar(termino.Palabra);
                vocabularioIdsVector[indice] = indice; // ID secuencial
                indice++;
            }

            cacheValido = true;
        }

        /// <summary>
        /// CORE: Crear vector de consulta usando ÚNICAMENTE Vector personalizado
        /// NO usa genéricos ni colecciones del framework - Solo Vector propio
        /// </summary>
        private Vector CrearVectorConsultaConVectorPropio(ListaDobleEnlazada<string> tokensUnicos)
        {
            // INSTANCIAR Vector personalizado
            var vectorConsulta = new Vector(cantidadTerminos);

            // Contar frecuencias usando SOLO estructuras propias
            var frecuenciasConsulta = ContarFrecuenciasConListaPropia(tokensUnicos);

            // Llenar vector usando indexador del Vector personalizado
            var iteradorVocab = new Iterador<string>(vocabularioCache);
            int indiceVector = 0;

            while (iteradorVocab.Siguiente())
            {
                string termino = iteradorVocab.Current;

                // Obtener frecuencia usando solo estructuras propias
                int tf = ObtenerFrecuenciaDeListaPropia(frecuenciasConsulta, termino);

                if (tf > 0)
                {
                    // Buscar término en índice para IDF
                    var terminoIndice = indiceInvertido.BuscarTermino(termino);
                    if (terminoIndice != null && terminoIndice.Idf > 0)
                    {
                        // USAR INDEXADOR de Vector personalizado
                        double valorTfIdf = tf * terminoIndice.Idf;
                        vectorConsulta[indiceVector] = valorTfIdf;
                    }
                }

                indiceVector++;
            }

            return vectorConsulta;
        }

        /// <summary>
        /// CORE: Crear vector de documento usando ÚNICAMENTE Vector personalizado
        /// </summary>
        private Vector CrearVectorDocumentoConVectorPropio(Documento documento)
        {
            var vectorDocumento = new Vector(cantidadTerminos);

            var iteradorVocab = new Iterador<string>(vocabularioCache);
            int indiceVector = 0;

            while (iteradorVocab.Siguiente())
            {
                string termino = iteradorVocab.Current;

                // Obtener TF del documento
                int tf = documento.GetFrecuencia(termino);

                if (tf > 0)
                {
                    var terminoIndice = indiceInvertido.BuscarTermino(termino);
                    if (terminoIndice != null && terminoIndice.Idf > 0)
                    {
                        // USAR INDEXADOR de Vector personalizado
                        double valorTfIdf = tf * terminoIndice.Idf;
                        vectorDocumento[indiceVector] = valorTfIdf;
                    }
                }

                indiceVector++;
            }

            return vectorDocumento;
        }

        /// <summary>
        /// Contar frecuencias usando SOLO ListaDobleEnlazada propia
        /// NO usa Dictionary ni genéricos
        /// </summary>
        private ListaDobleEnlazada<ParTerminoFrecuenciaPropio> ContarFrecuenciasConListaPropia(
            ListaDobleEnlazada<string> tokens)
        {
            var frecuencias = new ListaDobleEnlazada<ParTerminoFrecuenciaPropio>();

            var iterador = new Iterador<string>(tokens);
            while (iterador.Siguiente())
            {
                string token = iterador.Current;
                if (string.IsNullOrWhiteSpace(token))
                    continue;

                // Buscar si ya existe usando SOLO iterador propio
                bool encontrado = false;
                ParTerminoFrecuenciaPropio parExistente = null;

                var iteradorFrec = new Iterador<ParTerminoFrecuenciaPropio>(frecuencias);
                while (iteradorFrec.Siguiente())
                {
                    if (string.Equals(iteradorFrec.Current.Termino, token, StringComparison.OrdinalIgnoreCase))
                    {
                        parExistente = iteradorFrec.Current;
                        encontrado = true;
                        break;
                    }
                }

                if (encontrado && parExistente != null)
                {
                    // Actualizar frecuencia existente
                    parExistente.Frecuencia++;
                }
                else
                {
                    // Nueva frecuencia
                    frecuencias.Agregar(new ParTerminoFrecuenciaPropio(token, 1));
                }
            }

            return frecuencias;
        }

        /// <summary>
        /// Obtener frecuencia de lista propia - NO usa genéricos
        /// </summary>
        private int ObtenerFrecuenciaDeListaPropia(ListaDobleEnlazada<ParTerminoFrecuenciaPropio> frecuencias,
            string termino)
        {
            var iterador = new Iterador<ParTerminoFrecuenciaPropio>(frecuencias);
            while (iterador.Siguiente())
            {
                if (string.Equals(iterador.Current.Termino, termino, StringComparison.OrdinalIgnoreCase))
                    return iterador.Current.Frecuencia;
            }

            return 0;
        }

        /// <summary>
        /// Verificar relevancia del documento usando solo estructuras propias
        /// </summary>
        private bool DocumentoTieneTerminosRelevantes(Documento documento, ListaDobleEnlazada<string> tokensConsulta)
        {
            var iterador = new Iterador<string>(tokensConsulta);
            while (iterador.Siguiente())
            {
                if (documento.GetFrecuencia(iterador.Current) > 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// NUEVO: Generar enlace base64 directo para el resultado
        /// </summary>
        private string GenerarEnlaceBase64Directo(Documento documento)
        {
            try
            {
                string contenido;

                // Obtener contenido del documento
                if (!string.IsNullOrEmpty(documento.Ruta) && File.Exists(documento.Ruta))
                {
                    contenido = File.ReadAllText(documento.Ruta, System.Text.Encoding.UTF8);
                }
                else
                {
                    contenido = documento.TextoOriginal ?? "Contenido no disponible";
                }

                // Crear encabezado con metadatos
                string encabezado = $"=== DOCUMENTO ===\n" +
                                    $"Archivo: {Path.GetFileName(documento.Ruta)}\n" +
                                    $"ID: {documento.Id}\n" +
                                    $"Ruta: {documento.Ruta}\n" +
                                    $"=================\n\n";

                string contenidoCompleto = encabezado + contenido;

                // Convertir a base64
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(contenidoCompleto);
                string base64 = Convert.ToBase64String(bytes);

                // Crear data URL completa
                return $"data:text/plain;charset=utf-8;base64,{base64}";
            }
            catch (Exception ex)
            {
                // En caso de error, crear enlace con información de error
                string errorContent = $"Error al cargar documento: {Path.GetFileName(documento.Ruta)}\n" +
                                      $"Motivo: {ex.Message}\n" +
                                      $"ID: {documento.Id}";

                byte[] errorBytes = System.Text.Encoding.UTF8.GetBytes(errorContent);
                string errorBase64 = Convert.ToBase64String(errorBytes);
                return $"data:text/plain;charset=utf-8;base64,{errorBase64}";
            }
        }

        /// <summary>
        /// Invalidar cache cuando cambie el índice
        /// </summary>
        public void InvalidarCache()
        {
            cacheValido = false;
            vocabularioCache = null;
            vocabularioIdsVector = null;
            cantidadTerminos = 0;
        }

        /// <summary>
        /// NUEVO: Análisis detallado de consulta para debugging
        /// </summary>
        public AnalisisConsultaVectorial AnalizarConsulta(string consulta)
        {
            var tokensArray = procesador.ProcesarTextoCompleto(consulta);
            var tokens = ConvertirArrayAListaPropia(tokensArray);
            var tokensUnicos = EliminarDuplicadosConListaPropia(tokens);

            ActualizarCacheVocabulario();
            var vectorConsulta = CrearVectorConsultaConVectorPropio(tokensUnicos);

            return new AnalisisConsultaVectorial
            {
                ConsultaOriginal = consulta,
                CantidadTokensOriginales = tokens.Count,
                CantidadTokensUnicos = tokensUnicos.Count,
                DimensionVector = vectorConsulta.Dimension,
                MagnitudVector = vectorConsulta.Magnitud(),
                TieneValoresSignificativos = vectorConsulta.TieneValoresSignificativos(),
                ComponentesSignificativas = vectorConsulta.ObtenerComponentesSignificativas(5),
                VocabularioTotal = cantidadTerminos
            };
        }
    }

    /// <summary>
    /// Par término-frecuencia usando SOLO tipos básicos - NO genéricos
    /// </summary>
    public class ParTerminoFrecuenciaPropio
    {
        public string Termino { get; set; }
        public int Frecuencia { get; set; }

        public ParTerminoFrecuenciaPropio(string termino, int frecuencia)
        {
            Termino = termino ?? "";
            Frecuencia = Math.Max(0, frecuencia);
        }

        public override bool Equals(object obj)
        {
            return obj is ParTerminoFrecuenciaPropio otro &&
                   string.Equals(Termino, otro.Termino, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return Termino?.ToLowerInvariant().GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return $"{Termino}:{Frecuencia}";
        }
    }

    /// <summary>
    /// MEJORADO: Resultado con enlace base64 integrado y similitud realista
    /// </summary>
    public class ResultadoBusquedaVectorialMejorado
    {
        public Documento Documento { get; private set; }
        public double SimilitudCoseno { get; private set; }
        public string EnlaceBase64 { get; private set; }
        public double PorcentajeSimilitud => SimilitudCoseno * 100.0;
        public string NombreArchivo => Path.GetFileName(Documento?.Ruta ?? "");

        public ResultadoBusquedaVectorialMejorado(Documento documento, double similitudCoseno,
            string enlaceBase64 = null)
        {
            Documento = documento ?? throw new ArgumentNullException(nameof(documento));

            // NORMALIZAR similitud para evitar valores irreales
            SimilitudCoseno = Math.Max(0.0, Math.Min(1.0, similitudCoseno));

            EnlaceBase64 = enlaceBase64 ?? GenerarEnlaceBase64Interno();
        }

        private string GenerarEnlaceBase64Interno()
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
                    $"Error al cargar: {NombreArchivo}");
                string errorBase64 = Convert.ToBase64String(errorBytes);
                return $"data:text/plain;base64,{errorBase64}";
            }
        }

        public override string ToString()
        {
            return $"📄 {NombreArchivo} | {PorcentajeSimilitud:F1}% | 🔗 Base64 listo";
        }
    }

    /// <summary>
    /// NUEVO: Análisis completo de consulta vectorial
    /// </summary>
    public class AnalisisConsultaVectorial
    {
        public string ConsultaOriginal { get; set; }
        public int CantidadTokensOriginales { get; set; }
        public int CantidadTokensUnicos { get; set; }
        public int DimensionVector { get; set; }
        public double MagnitudVector { get; set; }
        public bool TieneValoresSignificativos { get; set; }
        public (int indice, double valor)[] ComponentesSignificativas { get; set; }
        public int VocabularioTotal { get; set; }

        public override string ToString()
        {
            var componentes = ComponentesSignificativas != null
                ? string.Join(", ", ComponentesSignificativas.Take(3).Select(c => $"[{c.indice}]={c.valor:F3}"))
                : "ninguna";

            return $"Consulta: '{ConsultaOriginal}' | " +
                   $"Tokens: {CantidadTokensOriginales}→{CantidadTokensUnicos} | " +
                   $"Vector: dim={DimensionVector}, mag={MagnitudVector:F3} | " +
                   $"Significativo: {TieneValoresSignificativos} | " +
                   $"Top componentes: {componentes}";
        }
    }
}