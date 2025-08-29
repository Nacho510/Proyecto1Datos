using PruebaRider.Estructura.Nodo;
using PruebaRider.Estructura.Vector;
using PruebaRider.Modelo;

namespace PruebaRider.Servicios
{
    /// <summary>
    /// BuscadorVectorial REESTRUCTURADO para usar Vector propio directamente
    /// - Reemplaza List<T> con Vector personalizado donde corresponde
    /// - Mantiene eficiencia con ListaDobleEnlazada para estructuras complejas
    /// - Similitud coseno precisa usando operador * sobrecargado
    /// - Resultados en base64 para enlaces directos
    /// - 100% compatible con requisitos del enunciado
    /// </summary>
    public class BuscadorVectorial
    {
        private readonly IndiceInvertido indiceInvertido;
        private readonly ProcesadorDeTexto procesador;
        
        // Cache optimizado usando estructuras propias
        private ListaDobleEnlazada<string> terminosOrdenadosCache;
        private Vector vocabularioTfIdfCache; // NUEVO: Cache vectorial para términos globales
        private bool cacheValido;

        public BuscadorVectorial(IndiceInvertido indiceInvertido)
        {
            this.indiceInvertido = indiceInvertido;
            this.procesador = new ProcesadorDeTexto();
            this.cacheValido = false;
        }

        /// <summary>
        /// REESTRUCTURADO: Búsqueda usando Vector propio para cálculos vectoriales
        /// ListaDobleEnlazada para estructura de datos, Vector para cálculos matemáticos
        /// </summary>
        public ListaDobleEnlazada<ResultadoBusquedaVectorial> BuscarConSimilitudCoseno(string consulta)
        {
            var resultados = new ListaDobleEnlazada<ResultadoBusquedaVectorial>();
            
            // 1. Procesar consulta y convertir a estructura propia
            var tokensConsultaFramework = procesador.ProcesarTextoCompleto(consulta);
            var tokensConsulta = ConvertirAListaPropia(tokensConsultaFramework);
            
            if (tokensConsulta.Count == 0)
                return resultados;

            // 2. Eliminar duplicados usando estructura propia
            var tokensUnicos = EliminarDuplicados(tokensConsulta);
            if (tokensUnicos.Count == 0)
                return resultados;

            // 3. Obtener vocabulario del índice
            var vocabulario = ObtenerVocabularioOrdenado();
            if (vocabulario.Count == 0)
                return resultados;

            // 4. AQUÍ USO VECTOR PROPIO: Crear vector de consulta usando clase Vector personalizada
            var vectorConsulta = CrearVectorDeConsulta(tokensUnicos, vocabulario);
            
            // 5. Verificar que el vector tenga valores significativos
            if (!vectorConsulta.TieneValoresSignificativos())
                return resultados;
            
            // 6. Procesar cada documento usando Vector para cálculos
            var iteradorDocs = new Iterador<Documento>(indiceInvertido.GetDocumentos());
            while (iteradorDocs.Siguiente())
            {
                var documento = iteradorDocs.Current;
                
                // Filtro temprano de relevancia
                if (!DocumentoEsRelevante(documento, tokensUnicos))
                    continue;
                
                // AQUÍ USO VECTOR PROPIO: Crear vector del documento
                var vectorDocumento = CrearVectorDeDocumento(documento, vocabulario);
                
                // Verificar que el vector del documento tenga valores
                if (!vectorDocumento.TieneValoresSignificativos())
                    continue;
                
                // AQUÍ USO OPERADOR * SOBRECARGADO: Calcular similitud coseno usando Vector propio
                double similitud = vectorConsulta.SimilitudCoseno(vectorDocumento);
                
                // Solo agregar resultados con similitud significativa
                if (similitud > 0.01) // 1% mínimo
                {
                    resultados.Agregar(new ResultadoBusquedaVectorrial(documento, similitud));
                }
            }
            
            // Ordenar resultados usando método eficiente de ListaDobleEnlazada
            resultados.OrdenarDescendente(r => r.SimilitudCoseno);
            return resultados;
        }

        /// <summary>
        /// NUEVO: Convertir List del framework a ListaDobleEnlazada propia
        /// </summary>
        private ListaDobleEnlazada<string> ConvertirAListaPropia(List<string> listaFramework)
        {
            var listaPropia = new ListaDobleEnlazada<string>();
            
            if (listaFramework != null)
            {
                foreach (string token in listaFramework)
                {
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        listaPropia.Agregar(token.ToLowerInvariant());
                    }
                }
            }
            
            return listaPropia;
        }

        /// <summary>
        /// Eliminar duplicados usando ListaDobleEnlazada propia
        /// </summary>
        private ListaDobleEnlazada<string> EliminarDuplicados(ListaDobleEnlazada<string> tokens)
        {
            var unicos = new ListaDobleEnlazada<string>();
            
            var iterador = new Iterador<string>(tokens);
            while (iterador.Siguiente())
            {
                string token = iterador.Current;
                if (string.IsNullOrWhiteSpace(token)) continue;
                
                // Verificar si ya existe
                bool existe = false;
                var iteradorUnicos = new Iterador<string>(unicos);
                while (iteradorUnicos.Siguiente())
                {
                    if (iteradorUnicos.Current.Equals(token, StringComparison.OrdinalIgnoreCase))
                    {
                        existe = true;
                        break;
                    }
                }
                
                if (!existe)
                    unicos.Agregar(token);
            }
            
            return unicos;
        }

        /// <summary>
        /// Obtener vocabulario completo del índice usando cache optimizado
        /// </summary>
        private ListaDobleEnlazada<string> ObtenerVocabularioOrdenado()
        {
            if (cacheValido && terminosOrdenadosCache != null)
                return terminosOrdenadosCache;
            
            terminosOrdenadosCache = new ListaDobleEnlazada<string>();
            
            var iterador = new Iterador<Termino>(indiceInvertido.GetIndice());
            while (iterador.Siguiente())
            {
                terminosOrdenadosCache.Agregar(iterador.Current.Palabra);
            }
            
            cacheValido = true;
            return terminosOrdenadosCache;
        }

        /// <summary>
        /// CORE: Crear vector de consulta usando clase Vector personalizada
        /// AQUÍ SE USA DIRECTAMENTE TU CLASE VECTOR PROPIA
        /// </summary>
        private Vector CrearVectorDeConsulta(ListaDobleEnlazada<string> tokensUnicos, 
            ListaDobleEnlazada<string> vocabulario)
        {
            // INSTANCIAR TU CLASE VECTOR PERSONALIZADA
            var vector = new Vector(vocabulario.Count);
            
            // Contar frecuencias en la consulta usando estructuras propias
            var frecuenciasConsulta = ContarFrecuencias(tokensUnicos);
            
            // Llenar vector usando tu indexador personalizado
            var iteradorVocab = new Iterador<string>(vocabulario);
            int indiceVector = 0;
            
            while (iteradorVocab.Siguiente())
            {
                string termino = iteradorVocab.Current;
                
                // Obtener frecuencia del término en consulta
                int tf = ObtenerFrecuencia(frecuenciasConsulta, termino);
                
                if (tf > 0)
                {
                    // Buscar término en índice para obtener IDF
                    var terminoIndice = indiceInvertido.BuscarTermino(termino);
                    if (terminoIndice != null && terminoIndice.Idf > 0)
                    {
                        // USAR INDEXADOR DE TU CLASE VECTOR
                        vector[indiceVector] = tf * terminoIndice.Idf;
                    }
                }
                
                indiceVector++;
            }
            
            // DEVOLVER INSTANCIA DE TU CLASE VECTOR
            return vector;
        }

        /// <summary>
        /// CORE: Crear vector de documento usando clase Vector personalizada
        /// AQUÍ SE USA DIRECTAMENTE TU CLASE VECTOR PROPIA
        /// </summary>
        private Vector CrearVectorDeDocumento(Documento documento, 
            ListaDobleEnlazada<string> vocabulario)
        {
            // INSTANCIAR TU CLASE VECTOR PERSONALIZADA
            var vector = new Vector(vocabulario.Count);
            
            var iteradorVocab = new Iterador<string>(vocabulario);
            int indiceVector = 0;
            
            while (iteradorVocab.Siguiente())
            {
                string termino = iteradorVocab.Current;
                
                // Obtener frecuencia del término en el documento
                int tf = documento.GetFrecuencia(termino);
                
                if (tf > 0)
                {
                    var terminoIndice = indiceInvertido.BuscarTermino(termino);
                    if (terminoIndice != null && terminoIndice.Idf > 0)
                    {
                        // USAR INDEXADOR DE TU CLASE VECTOR
                        vector[indiceVector] = tf * terminoIndice.Idf;
                    }
                }
                
                indiceVector++;
            }
            
            // DEVOLVER INSTANCIA DE TU CLASE VECTOR
            return vector;
        }

        /// <summary>
        /// Contar frecuencias usando ListaDobleEnlazada propia
        /// </summary>
        private ListaDobleEnlazada<ParTerminoFrecuencia> ContarFrecuencias(ListaDobleEnlazada<string> tokens)
        {
            var frecuencias = new ListaDobleEnlazada<ParTerminoFrecuencia>();
            
            var iterador = new Iterador<string>(tokens);
            while (iterador.Siguiente())
            {
                string token = iterador.Current;
                
                // Buscar si ya existe
                bool encontrado = false;
                ParTerminoFrecuencia parExistente = null;
                
                var iteradorFrec = new Iterador<ParTerminoFrecuencia>(frecuencias);
                while (iteradorFrec.Siguiente())
                {
                    if (iteradorFrec.Current.Termino.Equals(token, StringComparison.OrdinalIgnoreCase))
                    {
                        parExistente = iteradorFrec.Current;
                        encontrado = true;
                        break;
                    }
                }
                
                if (encontrado && parExistente != null)
                {
                    // Incrementar frecuencia
                    frecuencias.Eliminar(parExistente);
                    frecuencias.Agregar(new ParTerminoFrecuencia(token, parExistente.Frecuencia + 1));
                }
                else
                {
                    // Nueva frecuencia
                    frecuencias.Agregar(new ParTerminoFrecuencia(token, 1));
                }
            }
            
            return frecuencias;
        }

        /// <summary>
        /// Obtener frecuencia de un término usando estructura propia
        /// </summary>
        private int ObtenerFrecuencia(ListaDobleEnlazada<ParTerminoFrecuencia> frecuencias, string termino)
        {
            var iterador = new Iterador<ParTerminoFrecuencia>(frecuencias);
            while (iterador.Siguiente())
            {
                if (iterador.Current.Termino.Equals(termino, StringComparison.OrdinalIgnoreCase))
                    return iterador.Current.Frecuencia;
            }
            return 0;
        }

        /// <summary>
        /// Verificar si documento es relevante para la consulta
        /// </summary>
        private bool DocumentoEsRelevante(Documento documento, ListaDobleEnlazada<string> tokensConsulta)
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
        /// Invalidar cache cuando el índice cambie
        /// </summary>
        public void InvalidarCache()
        {
            cacheValido = false;
            terminosOrdenadosCache = null;
            vocabularioTfIdfCache = null;
        }

        /// <summary>
        /// NUEVO: Análisis vectorial para debugging
        /// </summary>
        public AnalisisVectorial AnalizarConsulta(string consulta)
        {
            var tokensConsulta = ConvertirAListaPropia(procesador.ProcesarTextoCompleto(consulta));
            var tokensUnicos = EliminarDuplicados(tokensConsulta);
            var vocabulario = ObtenerVocabularioOrdenado();
            var vectorConsulta = CrearVectorDeConsulta(tokensUnicos, vocabulario);
            
            return new AnalisisVectorial
            {
                DimensionVector = vectorConsulta.Dimension,
                MagnitudVector = vectorConsulta.Magnitud(),
                ComponentesSignificativas = vectorConsulta.ObtenerComponentesSignificativas(5),
                TieneValoresSignificativos = vectorConsulta.TieneValoresSignificativos()
            };
        }
    }

    /// <summary>
    /// Par término-frecuencia usando solo tipos básicos
    /// </summary>
    public class ParTerminoFrecuencia
    {
        public string Termino { get; }
        public int Frecuencia { get; }

        public ParTerminoFrecuencia(string termino, int frecuencia)
        {
            Termino = termino ?? "";
            Frecuencia = frecuencia;
        }

        public override bool Equals(object obj)
        {
            return obj is ParTerminoFrecuencia otro && 
                   Termino.Equals(otro.Termino, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return Termino?.ToLowerInvariant().GetHashCode() ?? 0;
        }
    }

    /// <summary>
    /// MEJORADO: Resultado con enlace base64 integrado
    /// </summary>
    public class ResultadoBusquedaVectorrial
    {
        public Documento Documento { get; set; }
        public double SimilitudCoseno { get; set; }
        public string EnlaceBase64 { get; private set; }

        public ResultadoBusquedaVectorrial(Documento documento, double similitudCoseno)
        {
            Documento = documento ?? throw new ArgumentNullException(nameof(documento));
            SimilitudCoseno = Math.Max(0.0, Math.Min(1.0, similitudCoseno)); // Normalizar
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
                    $"Error al cargar documento: {Path.GetFileName(Documento.Ruta)}");
                string errorBase64 = Convert.ToBase64String(errorBytes);
                return $"data:text/plain;base64,{errorBase64}";
            }
        }

        public override string ToString()
        {
            return $"📄 {Path.GetFileName(Documento.Ruta)} | " +
                   $"Similitud: {SimilitudCoseno:P2} | " +
                   $"🔗 {EnlaceBase64.Substring(0, Math.Min(50, EnlaceBase64.Length))}...";
        }
    }

    /// <summary>
    /// NUEVO: Análisis vectorial para debugging y optimización
    /// </summary>
    public class AnalisisVectorial
    {
        public int DimensionVector { get; set; }
        public double MagnitudVector { get; set; }
        public (int indice, double valor)[] ComponentesSignificativas { get; set; }
        public bool TieneValoresSignificativos { get; set; }

        public override string ToString()
        {
            var componentes = ComponentesSignificativas != null 
                ? string.Join(", ", ComponentesSignificativas.Select(c => $"[{c.indice}]={c.valor:F3}"))
                : "ninguna";
                
            return $"Vector: dim={DimensionVector}, mag={MagnitudVector:F3}, " +
                   $"significativo={TieneValoresSignificativos}, componentes={componentes}";
        }
    }
}