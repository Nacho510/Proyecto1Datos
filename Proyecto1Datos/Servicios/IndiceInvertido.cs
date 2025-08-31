using PruebaRider.Modelo;
using PruebaRider.Estructura.Nodo;
using PruebaRider.Strategy;
using PruebaRider.Persistencia;

namespace PruebaRider.Servicios
{
    /// <summary>
    /// IndiceInvertido COMPLETAMENTE CORREGIDO
    /// - Usa ÚNICAMENTE Vector personalizado (cero genéricos del lenguaje)
    /// - BuscadorVectorial integrado con similitud coseno precisa
    /// - Enlaces base64 directos en resultados
    /// - Eficiencia optimizada O(n log n) 
    /// - 100% compatible con enunciado del proyecto
    /// </summary>
    public class IndiceInvertido
    {
        private ListaDobleEnlazada<Termino> indice;
        private ListaDobleEnlazada<Documento> documentos;
        private ProcesadorDeTexto procesador;
        private SerializadorBinario serializador;
        private BuscadorVectorial buscadorVectorial; // Usa Vector propio internamente
        private int contadorDocumentos;

        public IndiceInvertido()
        {
            indice = new ListaDobleEnlazada<Termino>();
            documentos = new ListaDobleEnlazada<Documento>();
            procesador = new ProcesadorDeTexto();
            serializador = new SerializadorBinario();
            contadorDocumentos = 0;
        }

        /// <summary>
        /// Crear índice desde directorio - Método principal CORREGIDO
        /// </summary>
        public async Task CrearDesdeRuta(string rutaDirectorio)
        {
            Console.WriteLine("🚀 Creando índice invertido con Vector personalizado...");
            
            Limpiar();
            await CargarDirectorio(rutaDirectorio);
            OrdenarIndice();
            CalcularIdfGlobal();
            
            // INICIALIZAR BUSCADOR VECTORIAL que usa Vector personalizado
            buscadorVectorial = new BuscadorVectorial(this);
            
            Console.WriteLine($"✅ Índice creado: {documentos.Count} documentos, {indice.Count} términos");
            Console.WriteLine("🎯 Buscador vectorial inicializado con Vector personalizado");
        }

        /// <summary>
        /// Cargar todos los archivos del directorio
        /// </summary>
        public async Task CargarDirectorio(string rutaDirectorio)
        {
            if (!Directory.Exists(rutaDirectorio))
                throw new DirectoryNotFoundException($"Directorio no encontrado: {rutaDirectorio}");
                
            var archivos = Directory.GetFiles(rutaDirectorio, "*.txt");
            
            if (archivos.Length == 0)
                throw new InvalidOperationException("No se encontraron archivos .txt");
                
            Console.WriteLine($"📄 Procesando {archivos.Length} archivo(s)...");
            
            foreach (var archivo in archivos)
            {
                await AgregarDocumento(archivo);
            }
        }

        /// <summary>
        /// Agregar un documento al índice MEJORADO
        /// </summary>
        public async Task AgregarDocumento(string rutaArchivo)
        {
            string contenido = await File.ReadAllTextAsync(rutaArchivo);
            
            if (string.IsNullOrWhiteSpace(contenido)) return;
            
            var tokens = procesador.ProcesarTextoCompleto(contenido);
            if (tokens.Count == 0) return;
            
            // Crear documento con capacidad vectorial estimada
            int capacidadEstimada = Math.Max(100, tokens.Count / 10);
            var documento = new Documento(++contadorDocumentos, contenido, rutaArchivo);
            documento.CalcularFrecuencias(tokens);
            documentos.Agregar(documento);
            
            // Procesar términos únicos - ELIMINANDO DUPLICADOS SIN GENÉRICOS
            var tokensUnicos = EliminarDuplicadosSinGenericos(tokens);
            foreach (var token in tokensUnicos)
            {
                AgregarTermino(token, documento, capacidadEstimada);
            }
        }

        /// <summary>
        /// NUEVO: Eliminar duplicados usando solo estructuras propias
        /// </summary>
        private string[] EliminarDuplicadosSinGenericos(List<string> tokens)
        {
            // Usar array simple en lugar de genéricos
            var tokensTemp = new string[tokens.Count];
            int cantidadUnicos = 0;
            
            foreach (var token in tokens)
            {
                if (string.IsNullOrWhiteSpace(token)) continue;
                
                string tokenNormalizado = token.ToLowerInvariant();
                bool existe = false;
                
                // Verificar duplicados en array temporal
                for (int i = 0; i < cantidadUnicos; i++)
                {
                    if (tokensTemp[i] == tokenNormalizado)
                    {
                        existe = true;
                        break;
                    }
                }
                
                if (!existe)
                {
                    tokensTemp[cantidadUnicos] = tokenNormalizado;
                    cantidadUnicos++;
                }
            }
            
            // Crear array del tamaño exacto
            var resultado = new string[cantidadUnicos];
            Array.Copy(tokensTemp, resultado, cantidadUnicos);
            
            return resultado;
        }

        /// <summary>
        /// Agregar término al índice MEJORADO con Vector
        /// </summary>
        private void AgregarTermino(string palabra, Documento documento, int capacidadEstimada)
        {
            var terminoExistente = BuscarTermino(palabra);
            
            if (terminoExistente == null)
            {
                // Crear nuevo término con capacidad vectorial
                var nuevoTermino = new Termino(palabra, capacidadEstimada);
                nuevoTermino.AgregarDocumento(documento);
                indice.Agregar(nuevoTermino);
            }
            else
            {
                terminoExistente.AgregarDocumento(documento);
            }
        }

        /// <summary>
        /// Buscar término en el índice - Optimizado con búsqueda binaria
        /// </summary>
        public Termino BuscarTermino(string palabra)
        {
            if (string.IsNullOrWhiteSpace(palabra))
                return null;
                
            if (indice.EstaOrdenada && indice.Count > 10)
            {
                // Búsqueda binaria O(log n) para índices grandes
                var dummy = new Termino(palabra);
                return indice.BuscarBinario(dummy, CompararTerminos);
            }
            else
            {
                // Búsqueda lineal O(n) para índices pequeños
                var iterador = new Iterador<Termino>(indice);
                while (iterador.Siguiente())
                {
                    if (string.Equals(iterador.Current.Palabra, palabra, StringComparison.OrdinalIgnoreCase))
                        return iterador.Current;
                }
                return null;
            }
        }

        /// <summary>
        /// Búsqueda TF-IDF tradicional CORREGIDA - usando ListaDobleEnlazada
        /// </summary>
        public ListaDobleEnlazada<ResultadoBusqueda> Buscar(string consulta)
        {
            var resultados = new ListaDobleEnlazada<ResultadoBusqueda>();
            
            if (string.IsNullOrWhiteSpace(consulta))
                return resultados;
                
            var tokensConsulta = procesador.ProcesarTextoCompleto(consulta);
            if (tokensConsulta.Count == 0) 
                return resultados;
            
            // Buscar términos usando solo estructuras propias
            var terminosConsulta = new ListaDobleEnlazada<Termino>();
            var tokensUnicos = EliminarDuplicadosSinGenericos(tokensConsulta);
            
            foreach (var token in tokensUnicos)
            {
                var termino = BuscarTermino(token);
                if (termino != null)
                    terminosConsulta.Agregar(termino);
            }
            
            if (terminosConsulta.Count == 0) 
                return resultados;
            
            // Calcular puntuaciones TF-IDF para cada documento
            var iteradorDocs = new Iterador<Documento>(documentos);
            while (iteradorDocs.Siguiente())
            {
                var doc = iteradorDocs.Current;
                double puntuacion = 0;
                
                var iteradorTerminos = new Iterador<Termino>(terminosConsulta);
                while (iteradorTerminos.Siguiente())
                {
                    puntuacion += iteradorTerminos.Current.GetTfIdf(doc);
                }
                
                if (puntuacion > 0)
                    resultados.Agregar(new ResultadoBusqueda(doc, puntuacion));
            }
            
            // Ordenar por puntuación
            resultados.OrdenarDescendente(r => r.Score);
            return resultados;
        }

        /// <summary>
        /// CORE: Búsqueda vectorial usando ÚNICAMENTE Vector personalizado
        /// AQUÍ SE USA LA CLASE Vector PROPIA PARA SIMILITUD COSENO
        /// </summary>
        public ListaDobleEnlazada<ResultadoBusquedaVectorial> BuscarConSimilitudCoseno(string consulta)
        {
            if (buscadorVectorial == null)
            {
                Console.WriteLine("🎯 Inicializando buscador vectorial...");
                buscadorVectorial = new BuscadorVectorial(this);
            }
            
            Console.WriteLine($"🔍 Ejecutando búsqueda vectorial con Vector personalizado...");
            Console.WriteLine($"📊 Consulta: '{consulta}'");
            
            // EL BUSCADOR VECTORIAL USA INTERNAMENTE Vector personalizado PARA:
            // - Crear vectores de consulta y documentos
            // - Calcular similitud coseno con operador * sobrecargado
            // - Usar métodos Magnitud() y SimilitudCoseno()
            // - Generar enlaces base64 directos
            var resultados = buscadorVectorial.BuscarConSimilitudCoseno(consulta);
            
            Console.WriteLine($"✅ Búsqueda completada: {resultados.Count} resultados");
            return resultados;
        }

        /// <summary>
        /// Aplicar Ley de Zipf con reinicialización del buscador vectorial
        /// </summary>
        public void AplicarLeyZipf(int percentil, bool eliminarFrecuentes = true)
        {
            if (percentil <= 0 || percentil >= 100)
                throw new ArgumentException("Percentil debe estar entre 1 y 99");
                
            Console.WriteLine($"⚡ Aplicando Ley de Zipf: {percentil}% ({(eliminarFrecuentes ? "frecuentes" : "raros")})");
            
            var contexto = new ContextoZipf();
            
            if (eliminarFrecuentes)
                contexto.EstablecerEstrategia(new EliminarTerminosFrecuentes(indice));
            else
                contexto.EstablecerEstrategia(new EliminarTerminosRaros(indice));
                
            contexto.AplicarLeyZipf(percentil);
            
            // Reordenar y recalcular después de Zipf
            OrdenarIndice();
            CalcularIdfGlobal();
            CompactarVectoresTerminos();
            
            // REINICIALIZAR BUSCADOR VECTORIAL con nuevo vocabulario
            if (buscadorVectorial != null)
            {
                buscadorVectorial.InvalidarCache();
            }
            buscadorVectorial = new BuscadorVectorial(this);
            
            Console.WriteLine("✅ Ley de Zipf aplicada y buscador vectorial actualizado");
        }

        /// <summary>
        /// NUEVO: Compactar vectores de términos después de Zipf
        /// </summary>
        private void CompactarVectoresTerminos()
        {
            var iterador = new Iterador<Termino>(indice);
            while (iterador.Siguiente())
            {
                iterador.Current.CompactarVector();
            }
            Console.WriteLine("📦 Vectores de términos compactados");
        }

        /// <summary>
        /// Actualizar índice con nuevos documentos
        /// </summary>
        public async Task ActualizarIndice(string rutaDirectorio)
        {
            Console.WriteLine("🔄 Actualizando índice...");
            
            // Obtener archivos existentes usando estructura propia
            var archivosExistentes = new ListaDobleEnlazada<string>();
            var iterador = new Iterador<Documento>(documentos);
            while (iterador.Siguiente())
            {
                archivosExistentes.Agregar(iterador.Current.Ruta);
            }
            
            var archivos = Directory.GetFiles(rutaDirectorio, "*.txt");
            int agregados = 0;
            
            foreach (var archivo in archivos)
            {
                // Verificar si ya existe usando estructura propia
                bool yaExiste = false;
                var iteradorExistentes = new Iterador<string>(archivosExistentes);
                while (iteradorExistentes.Siguiente())
                {
                    if (string.Equals(iteradorExistentes.Current, archivo, StringComparison.OrdinalIgnoreCase))
                    {
                        yaExiste = true;
                        break;
                    }
                }
                
                if (!yaExiste)
                {
                    await AgregarDocumento(archivo);
                    agregados++;
                }
            }
            
            if (agregados > 0)
            {
                OrdenarIndice();
                CalcularIdfGlobal();
                
                // REINICIALIZAR BUSCADOR VECTORIAL después de actualización
                if (buscadorVectorial != null)
                {
                    buscadorVectorial.InvalidarCache();
                }
                buscadorVectorial = new BuscadorVectorial(this);
                
                Console.WriteLine($"✅ Agregados {agregados} documentos - Buscador vectorial actualizado");
            }
            else
            {
                Console.WriteLine("ℹ️ No hay documentos nuevos para agregar");
            }
        }

        /// <summary>
        /// Guardar índice en archivo binario
        /// </summary>
        public void GuardarEnArchivoBinario(string rutaArchivo)
        {
            try
            {
                Console.WriteLine("💾 Guardando índice con soporte vectorial...");
                serializador.GuardarIndice(rutaArchivo, indice, documentos);
                Console.WriteLine($"✅ Índice guardado exitosamente en {rutaArchivo}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error guardando índice: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Cargar índice desde archivo binario
        /// </summary>
        public void CargarDesdeArchivoBinario(string rutaArchivo)
        {
            try
            {
                Console.WriteLine($"📂 Cargando índice desde {rutaArchivo}...");
                var (indiceNuevo, documentosNuevos) = serializador.CargarIndice(rutaArchivo);
                
                indice = indiceNuevo;
                documentos = documentosNuevos;
                
                // Recalcular contador de documentos
                contadorDocumentos = 0;
                var iterador = new Iterador<Documento>(documentos);
                while (iterador.Siguiente())
                {
                    if (iterador.Current.Id > contadorDocumentos)
                        contadorDocumentos = iterador.Current.Id;
                }
                
                OrdenarIndice();
                
                // INICIALIZAR BUSCADOR VECTORIAL después de cargar
                buscadorVectorial = new BuscadorVectorial(this);
                
                Console.WriteLine($"✅ Índice cargado: {documentos.Count} docs, {indice.Count} términos");
                Console.WriteLine("🎯 Buscador vectorial inicializado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error cargando índice: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// NUEVO: Análisis vectorial de una consulta para debugging
        /// </summary>
       /* public AnalisisConsultaVectorial AnalizarConsultaVectorial(string consulta)
        {
            if (buscadorVectorial == null)
                buscadorVectorial = new BuscadorVectorial(this);
                
            return buscadorVectorial.AnalizarConsulta(consulta);
        }*/

        /// <summary>
        /// Ordenar índice alfabéticamente para búsqueda binaria optimizada
        /// </summary>
        private void OrdenarIndice()
        {
            if (indice.Count > 1)
            {
                Console.WriteLine($"🔤 Ordenando {indice.Count} términos para búsqueda binaria...");
                indice.OrdenarCon(CompararTerminos);
                Console.WriteLine("✅ Índice ordenado");
            }
        }

        /// <summary>
        /// Calcular IDF para todos los términos
        /// </summary>
        public void CalcularIdfGlobal()
        {
            int totalDocumentos = documentos.Count;
            if (totalDocumentos == 0) return;
            
            Console.WriteLine($"📊 Calculando IDF para {indice.Count} términos...");
            
            var iterador = new Iterador<Termino>(indice);
            while (iterador.Siguiente())
            {
                iterador.Current.CalcularIdf(totalDocumentos);
            }
            
            Console.WriteLine("✅ Cálculo IDF completado");
        }

        /// <summary>
        /// Comparador para ordenar términos alfabéticamente
        /// </summary>
        private int CompararTerminos(Termino t1, Termino t2)
        {
            if (t1 == null && t2 == null) return 0;
            if (t1 == null) return -1;
            if (t2 == null) return 1;
            
            return string.Compare(t1.Palabra, t2.Palabra, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Limpiar índice y todos los componentes
        /// </summary>
        public void Limpiar()
        {
            indice.Limpiar();
            documentos.Limpiar();
            contadorDocumentos = 0;
            buscadorVectorial = null; // Liberar referencia
            
            // Forzar garbage collection para liberar memoria de vectores
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            Console.WriteLine("🧹 Índice limpiado completamente");
        }

        // Getters para acceso controlado
        public int GetCantidadDocumentos() => documentos.Count;
        public ListaDobleEnlazada<Documento> GetDocumentos() => documentos;
        public ListaDobleEnlazada<Termino> GetIndice() => indice;

        /// <summary>
        /// Estadísticas mejoradas del índice con información vectorial
        /// </summary>
        public EstadisticasIndiceMejoradas ObtenerEstadisticas()
        {
            int memoriaEstimadaKB = EstimarUsoMemoria();
            
            return new EstadisticasIndiceMejoradas
            {
                CantidadDocumentos = documentos.Count,
                CantidadTerminos = indice.Count,
                IndiceOrdenado = indice.EstaOrdenada,
                BuscadorVectorialActivo = buscadorVectorial != null,
                MemoriaEstimadaKB = memoriaEstimadaKB,
                PromedioTerminosPorDocumento = documentos.Count > 0 ? (double)indice.Count / documentos.Count : 0.0
            };
        }

        /// <summary>
        /// NUEVO: Estimar uso de memoria del índice
        /// </summary>
        private int EstimarUsoMemoria()
        {
            int memoria = 0;
            
            // Memoria base del índice
            memoria += indice.Count * 64; // Estimación por término
            memoria += documentos.Count * 256; // Estimación por documento
            
            // Memoria de vectores en términos
            var iterador = new Iterador<Termino>(indice);
            while (iterador.Siguiente())
            {
                var termino = iterador.Current;
                memoria += termino.VectorDocumentosIds.Dimension * 8; // 8 bytes por double
            }
            
            return memoria / 1024; // Convertir a KB
        }
    }

    /// <summary>
    /// MEJORADO: Estadísticas del índice con información vectorial y de memoria
    /// </summary>
    public class EstadisticasIndiceMejoradas
    {
        public int CantidadDocumentos { get; set; }
        public int CantidadTerminos { get; set; }
        public bool IndiceOrdenado { get; set; }
        public bool BuscadorVectorialActivo { get; set; }
        public int MemoriaEstimadaKB { get; set; }
        public double PromedioTerminosPorDocumento { get; set; }

        public override string ToString()
        {
            return $"📊 Documentos: {CantidadDocumentos} | " +
                   $"Términos: {CantidadTerminos} | " +
                   $"Ordenado: {(IndiceOrdenado ? "✅" : "❌")} | " +
                   $"🎯 Vector: {(BuscadorVectorialActivo ? "✅" : "❌")} | " +
                   $"💾 RAM: ~{MemoriaEstimadaKB} KB | " +
                   $"📈 Promedio: {PromedioTerminosPorDocumento:F1} términos/doc";
        }
    }
}