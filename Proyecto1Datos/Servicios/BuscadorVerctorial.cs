using PruebaRider.Estructura.Nodo;
using PruebaRider.Estructura.Vector;
using PruebaRider.Modelo;
using PruebaRider.Servicios;

namespace PruebaRider.Servicios
{
    /// <summary>
    /// BuscadorVectorial optimizado para similitud coseno
    /// Mejoras: Pre-cálculo de vectores, eliminación de cálculos redundantes, 
    /// filtrado temprano de resultados irrelevantes
    /// </summary>
    public class BuscadorVectorial
    {
        private readonly IndiceInvertido indiceInvertido;
        private readonly ProcesadorDeTexto procesador;
        
        // Optimizaciones de cache temporal para una consulta
        private ListaDobleEnlazada<string> terminosOrdenadosCache;
        private bool cacheValido;

        public BuscadorVectorial(IndiceInvertido indiceInvertido)
        {
            this.indiceInvertido = indiceInvertido;
            this.procesador = new ProcesadorDeTexto();
            this.cacheValido = false;
        }

        /// <summary>
        /// Búsqueda con similitud coseno OPTIMIZADA
        /// </summary>
        public ListaDobleEnlazada<ResultadoBusquedaVectorial> BuscarConSimilitudCoseno(string consulta)
        {
            var resultados = new ListaDobleEnlazada<ResultadoBusquedaVectorial>();
            
            // 1. Procesar consulta
            var tokensConsulta = procesador.ProcesarTextoCompleto(consulta);
            if (tokensConsulta.Count == 0)
                return resultados;

            // 2. Obtener términos únicos sin duplicados (OPTIMIZACIÓN: evita procesamiento redundante)
            var tokensUnicos = EliminarDuplicadosRapido(tokensConsulta);
            if (tokensUnicos.Count == 0)
                return resultados;

            // 3. Pre-construir lista de términos ordenada para vectorización eficiente
            var terminosIndice = ObtenerTerminosOrdenadosOptimizado();
            
            // 4. Crear vector de consulta una sola vez (OPTIMIZACIÓN: reutilizable)
            var vectorConsulta = CrearVectorConsultaOptimizado(tokensUnicos, terminosIndice);
            
            // 5. Pre-calcular magnitud de consulta (OPTIMIZACIÓN: evita recalcular)
            double magnitudConsulta = vectorConsulta.Magnitud();
            if (magnitudConsulta == 0) return resultados;
            
            // 6. Procesar documentos con filtrado temprano
            var iteradorDocs = new Iterador<Documento>(indiceInvertido.GetDocumentos());
            while (iteradorDocs.Siguiente())
            {
                var documento = iteradorDocs.Current;
                
                // OPTIMIZACIÓN: Filtrado temprano - verificar si documento contiene algún término
                if (!DocumentoContienePacialaTerminosConsulta(documento, tokensUnicos))
                    continue;
                
                // Crear vector de documento solo si es relevante
                var vectorDocumento = CrearVectorDocumentoOptimizado(documento, terminosIndice);
                
                // OPTIMIZACIÓN: Evitar cálculo si vector está vacío
                double magnitudDocumento = vectorDocumento.Magnitud();
                if (magnitudDocumento == 0)
                    continue;
                
                // Calcular similitud coseno optimizada
                double similitud = CalcularSimilitudCosenoOptimizada(
                    vectorConsulta, vectorDocumento, 
                    magnitudConsulta, magnitudDocumento);
                
                // OPTIMIZACIÓN: Solo agregar si similitud es significativa (> 0.01)
                if (similitud > 0.01)
                {
                    resultados.Agregar(new ResultadoBusquedaVectorial(documento, similitud));
                }
            }
            
            // Ordenar resultados por similitud descendente
            resultados.OrdenarDescendente(r => r.SimilitudCoseno);
            return resultados;
        }

        /// <summary>
        /// Eliminar duplicados de forma más eficiente para consultas
        /// OPTIMIZACIÓN: O(n²) pero con break temprano y menos comparaciones
        /// </summary>
        private List<string> EliminarDuplicadosRapido(List<string> tokens)
        {
            var unicos = new List<string>();
            
            foreach (var token in tokens)
            {
                if (string.IsNullOrWhiteSpace(token)) continue;
                
                string tokenLimpio = token.ToLowerInvariant();
                
                // OPTIMIZACIÓN: Búsqueda con break temprano
                bool encontrado = false;
                for (int i = 0; i < unicos.Count; i++)
                {
                    if (unicos[i] == tokenLimpio) // Comparación exacta más rápida
                    {
                        encontrado = true;
                        break;
                    }
                }
                
                if (!encontrado)
                    unicos.Add(tokenLimpio);
            }
            
            return unicos;
        }

        /// <summary>
        /// Obtener términos ordenados con cache para múltiples vectorizaciones
        /// OPTIMIZACIÓN: Reutiliza la lista ordenada entre llamadas
        /// </summary>
        private ListaDobleEnlazada<string> ObtenerTerminosOrdenadosOptimizado()
        {
            // OPTIMIZACIÓN: Cache simple para evitar reconstruir la lista constantemente
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
        /// Crear vector de consulta optimizado
        /// OPTIMIZACIÓN: Solo procesa términos que existen en el índice
        /// </summary>
        private Vector CrearVectorConsultaOptimizado(List<string> tokensConsulta, ListaDobleEnlazada<string> terminosIndice)
        {
            var vector = new Vector(terminosIndice.Count);
            
            // Pre-calcular frecuencias de términos en consulta
            var frecuenciasConsulta = ContarFrecuenciasOptimizado(tokensConsulta);
            
            var iteradorTerminos = new Iterador<string>(terminosIndice);
            int indice = 0;
            
            while (iteradorTerminos.Siguiente())
            {
                string termino = iteradorTerminos.Current;
                
                // OPTIMIZACIÓN: Buscar frecuencia usando búsqueda lineal simple
                int tf = ObtenerFrecuenciaRapida(frecuenciasConsulta, termino);
                
                if (tf > 0)
                {
                    // Buscar término en índice para obtener IDF
                    var terminoIndice = BuscarTerminoEnIndiceOptimizado(termino);
                    if (terminoIndice != null)
                    {
                        vector[indice] = tf * terminoIndice.Idf;
                    }
                }
                // OPTIMIZACIÓN: No asignar 0 explícitamente (ya está inicializado en 0)
                
                indice++;
            }
            
            return vector;
        }

        /// <summary>
        /// Crear vector de documento optimizado
        /// OPTIMIZACIÓN: Acceso directo a frecuencias sin búsquedas redundantes
        /// </summary>
        private Vector CrearVectorDocumentoOptimizado(Documento documento, ListaDobleEnlazada<string> terminosIndice)
        {
            var vector = new Vector(terminosIndice.Count);
            
            var iteradorTerminos = new Iterador<string>(terminosIndice);
            int indice = 0;
            
            while (iteradorTerminos.Siguiente())
            {
                string termino = iteradorTerminos.Current;
                
                // OPTIMIZACIÓN: Usar método optimizado del documento
                int tf = documento.GetFrecuencia(termino);
                
                if (tf > 0)
                {
                    // OPTIMIZACIÓN: Buscar término con cache o búsqueda binaria
                    var terminoIndice = BuscarTerminoEnIndiceOptimizado(termino);
                    if (terminoIndice != null)
                    {
                        vector[indice] = tf * terminoIndice.Idf;
                    }
                }
                
                indice++;
            }
            
            return vector;
        }

        /// <summary>
        /// Verificación temprana si documento contiene términos de consulta
        /// OPTIMIZACIÓN: Evita crear vectores para documentos irrelevantes
        /// </summary>
        private bool DocumentoContienePacialaTerminosConsulta(Documento documento, List<string> tokensConsulta)
        {
            // OPTIMIZACIÓN: Solo necesita encontrar UN término para ser relevante
            foreach (var token in tokensConsulta)
            {
                if (documento.GetFrecuencia(token) > 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Cálculo de similitud coseno optimizado con magnitudes pre-calculadas
        /// OPTIMIZACIÓN: Evita recalcular magnitudes
        /// </summary>
        private double CalcularSimilitudCosenoOptimizada(Vector v1, Vector v2, double magnitud1, double magnitud2)
        {
            // OPTIMIZACIÓN: Evitar división por cero sin verificaciones redundantes
            if (magnitud1 == 0 || magnitud2 == 0) return 0;
            
            // Producto punto usando operador sobrecargado optimizado
            double productoPunto = v1 * v2;
            
            // Resultado final
            return productoPunto / (magnitud1 * magnitud2);
        }

        /// <summary>
        /// Contar frecuencias optimizado para consultas pequeñas
        /// </summary>
        private List<ParFrecuencia> ContarFrecuenciasOptimizado(List<string> tokens)
        {
            var frecuencias = new List<ParFrecuencia>();
            
            foreach (var token in tokens)
            {
                bool encontrado = false;
                
                // OPTIMIZACIÓN: Lista pequeña, búsqueda lineal es eficiente
                for (int i = 0; i < frecuencias.Count; i++)
                {
                    if (frecuencias[i].Token == token)
                    {
                        frecuencias[i] = new ParFrecuencia(token, frecuencias[i].Frecuencia + 1);
                        encontrado = true;
                        break;
                    }
                }
                
                if (!encontrado)
                {
                    frecuencias.Add(new ParFrecuencia(token, 1));
                }
            }
            
            return frecuencias;
        }

        /// <summary>
        /// Obtener frecuencia de forma rápida de lista pequeña
        /// </summary>
        private int ObtenerFrecuenciaRapida(List<ParFrecuencia> frecuencias, string token)
        {
            // OPTIMIZACIÓN: Búsqueda lineal en lista pequeña es muy eficiente
            for (int i = 0; i < frecuencias.Count; i++)
            {
                if (frecuencias[i].Token == token)
                    return frecuencias[i].Frecuencia;
            }
            return 0;
        }

        /// <summary>
        /// Buscar término en índice de forma optimizada
        /// OPTIMIZACIÓN: Delegar al índice que ya tiene búsqueda binaria
        /// </summary>
        private Termino BuscarTerminoEnIndiceOptimizado(string palabra)
        {
            // OPTIMIZACIÓN: Usar la búsqueda optimizada del índice (binaria si es grande)
            return indiceInvertido.BuscarTermino(palabra);
        }

        /// <summary>
        /// Invalidar cache cuando el índice cambie
        /// </summary>
        public void InvalidarCache()
        {
            cacheValido = false;
            terminosOrdenadosCache = null;
        }

        /// <summary>
        /// Clase auxiliar para frecuencias optimizada
        /// </summary>
        private struct ParFrecuencia
        {
            public string Token { get; }
            public int Frecuencia { get; }

            public ParFrecuencia(string token, int frecuencia)
            {
                Token = token;
                Frecuencia = frecuencia;
            }
        }
    }
    
    public class ResultadoBusquedaVectorial
    {
        public Documento Documento { get; set; }
        public double SimilitudCoseno { get; set; }

        public ResultadoBusquedaVectorial(Documento documento, double similitudCoseno)
        {
            Documento = documento;
            SimilitudCoseno = similitudCoseno;
        }

        public override string ToString()
        {
            return $"Documento: {Documento.Ruta} | Similitud: {SimilitudCoseno:F4}";
        }
    }
}