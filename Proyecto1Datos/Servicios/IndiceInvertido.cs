using PruebaRider.Modelo;
using PruebaRider.Estructura.Nodo;
using PruebaRider.Estructura.Vector;
using PruebaRider.Persistencia;

namespace PruebaRider.Servicios
{
    /// <summary>
    /// Índice invertido completamente reestructurado
    /// - VectorOrdenado con Radix Sort para términos
    /// - Búsqueda binaria O(log n)
    /// - Optimizado para similitud coseno
    /// </summary>
    public class IndiceInvertido
    {
        // CAMBIO PRINCIPAL: VectorOrdenado en lugar de lista enlazada
        private VectorOrdenado<Termino> indiceTerminos;
        private ListaDobleEnlazada<Documento> documentos;
        private ProcesadorDeTexto procesador;
        private SerializadorBinario serializador;
        private int contadorDocumentos;

        public IndiceInvertido()
        {
            indiceTerminos = new VectorOrdenado<Termino>();
            documentos = new ListaDobleEnlazada<Documento>();
            procesador = new ProcesadorDeTexto();
            serializador = new SerializadorBinario();
            contadorDocumentos = 0;
        }

        /// <summary>
        /// CREAR DESDE RUTA - Reestructurado completamente
        /// </summary>
        public async Task CrearDesdeRuta(string rutaDirectorio)
        {
            Console.WriteLine("🚀 Creando índice invertido con Vector ordenado + Radix Sort...");

            Limpiar();
            await CargarDirectorio(rutaDirectorio);

            Console.WriteLine($"📊 Documentos cargados: {documentos.Count}");
            Console.WriteLine($"📊 Términos únicos: {indiceTerminos.Count}");

            // APLICAR RADIX SORT - Requisito específico
            Console.WriteLine("⚡ Aplicando Radix Sort a términos...");
            indiceTerminos.OrdenarRadix();

            // Calcular IDF y TF-IDF
            CalcularMetricasTfIdf();

            Console.WriteLine(
                $"✅ Índice creado con Radix Sort: {documentos.Count} docs, {indiceTerminos.Count} términos");
        }

        /// <summary>
        /// Cargar directorio de documentos
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
        /// Agregar documento individual
        /// </summary>
        public async Task AgregarDocumento(string rutaArchivo)
        {
            try
            {
                string contenido = await File.ReadAllTextAsync(rutaArchivo);
                if (string.IsNullOrWhiteSpace(contenido))
                {
                    Console.WriteLine($"⚠️ Archivo vacío: {Path.GetFileName(rutaArchivo)}");
                    return;
                }

                var tokens = procesador.ProcesarTextoCompleto(contenido);
                if (tokens.Count == 0)
                {
                    Console.WriteLine($"⚠️ Sin tokens válidos: {Path.GetFileName(rutaArchivo)}");
                    return;
                }

                var documento = new Documento(++contadorDocumentos, contenido, rutaArchivo);
                documento.CalcularFrecuencias(tokens);
                documentos.Agregar(documento);

                Console.WriteLine($"📄 {Path.GetFileName(rutaArchivo)} ({tokens.Count} tokens)");

                // Procesar términos y agregar al índice
                ProcesarTerminosDelDocumento(documento, tokens);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error procesando {Path.GetFileName(rutaArchivo)}: {ex.Message}");
            }
        }

        /// <summary>
        /// Procesar términos de un documento y agregarlos al índice
        /// </summary>
        private void ProcesarTerminosDelDocumento(Documento documento, List<string> tokens)
        {
            // Contar frecuencias únicas del documento
            var frecuenciasLocales = ContarFrecuenciasLocales(tokens);

            // Agregar cada término único al índice
            foreach (var kvp in frecuenciasLocales)
            {
                string termino = kvp.Token;
                int frecuencia = kvp.Frecuencia;

                AgregarTerminoAlIndice(termino, documento, frecuencia);
            }
        }

        /// <summary>
        /// Contar frecuencias locales sin usar Dictionary (prohibido)
        /// </summary>
        private TerminoFrecuencia[] ContarFrecuenciasLocales(List<string> tokens)
        {
            var resultado = new TerminoFrecuencia[tokens.Count]; // Máximo posible
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
                    resultado[cantidadUnicos] = new TerminoFrecuencia(tokenNorm, 1);
                    cantidadUnicos++;
                }
            }

            // Crear array del tamaño exacto
            var final = new TerminoFrecuencia[cantidadUnicos];
            Array.Copy(resultado, final, cantidadUnicos);
            return final;
        }

        /// <summary>
        /// Agregar término al índice usando vector ordenado
        /// </summary>
        private void AgregarTerminoAlIndice(string palabra, Documento documento, int frecuencia)
        {
            // Buscar término existente
            var terminoExistente = BuscarTermino(palabra);

            if (terminoExistente == null)
            {
                // Crear nuevo término y agregarlo SIN orden (más rápido)
                var nuevoTermino = new Termino(palabra);
                nuevoTermino.AgregarDocumento(documento, frecuencia);
                indiceTerminos.Agregar(nuevoTermino); // Se ordenará después con RadixSort
            }
            else
            {
                // Actualizar término existente
                terminoExistente.AgregarDocumento(documento, frecuencia);
            }
        }

        /// <summary>
        /// BÚSQUEDA DE TÉRMINO - Optimizada con búsqueda binaria
        /// </summary>
        public Termino BuscarTermino(string palabra)
        {
            if (string.IsNullOrWhiteSpace(palabra))
                return null;

            string palabraNormalizada = palabra.ToLowerInvariant();

            if (indiceTerminos.EstaOrdenado)
            {
                // Búsqueda binaria O(log n) - MUY EFICIENTE
                var terminoBusqueda = new Termino(palabraNormalizada);
                return indiceTerminos.BuscarBinario(terminoBusqueda);
            }
            else
            {
                // Búsqueda lineal como fallback (solo durante construcción)
                var iterador = indiceTerminos.ObtenerIterador();
                while (iterador.Siguiente())
                {
                    if (string.Equals(iterador.Current.Palabra, palabraNormalizada, StringComparison.OrdinalIgnoreCase))
                        return iterador.Current;
                }

                return null;
            }
        }

        /// <summary>
        /// BÚSQUEDA CON SIMILITUD COSENO - NÚCLEO DEL SISTEMA
        /// </summary>
        public ListaDobleEnlazada<ResultadoBusquedaVectorial> BuscarConSimilitudCoseno(string consulta)
        {
            var resultados = new ListaDobleEnlazada<ResultadoBusquedaVectorial>();

            if (string.IsNullOrWhiteSpace(consulta))
                return resultados;

            Console.WriteLine($"🔍 Búsqueda vectorial: '{consulta}'");

            // 1. PROCESAR CONSULTA
            var tokensConsulta = procesador.ProcesarTextoCompleto(consulta);
            if (tokensConsulta.Count == 0)
            {
                Console.WriteLine("❌ No se encontraron tokens válidos");
                return resultados;
            }

            // 2. CREAR VECTOR DE CONSULTA
            var vectorConsulta = CrearVectorConsulta(tokensConsulta);
            if (vectorConsulta == null || !vectorConsulta.TieneValoresSignificativos())
            {
                Console.WriteLine("❌ No se pudo crear vector de consulta válido");
                return resultados;
            }

            Console.WriteLine($"📊 Vector consulta creado (magnitud: {vectorConsulta.Magnitud():F4})");

            // 3. CALCULAR SIMILITUD PARA CADA DOCUMENTO
            var iteradorDocs = new Iterador<Documento>(documentos);
            int documentosProcessados = 0;

            while (iteradorDocs.Siguiente())
            {
                var documento = iteradorDocs.Current;
                documentosProcessados++;

                // Crear vector TF-IDF del documento
                var vectorDoc = CrearVectorDocumento(documento);
                if (vectorDoc == null || !vectorDoc.TieneValoresSignificativos())
                    continue;

                // Calcular similitud coseno
                double similitud = vectorConsulta.SimilitudCoseno(vectorDoc);

                if (similitud > 0.001) // Umbral mínimo
                {
                    var resultado = new ResultadoBusquedaVectorial(documento, similitud);
                    resultados.Agregar(resultado);
                }
            }

            Console.WriteLine($"📊 Procesados: {documentosProcessados} documentos, {resultados.Count} con similitud");

            // 4. ORDENAR RESULTADOS POR SIMILITUD DESCENDENTE
            if (resultados.Count > 0)
            {
                resultados.OrdenarDescendente(r => r.SimilitudCoseno);
            }

            return resultados;
        }

        /// <summary>
        /// Crear vector de consulta TF-IDF
        /// </summary>
        private Vector CrearVectorConsulta(List<string> tokens)
        {
            if (indiceTerminos.Count == 0)
                return null;

            // Vector con dimensión igual al vocabulario completo
            var vector = new Vector(indiceTerminos.Count);

            // Contar frecuencias en la consulta
            var frecuenciasConsulta = ContarFrecuenciasLocales(tokens);

            // Para cada término del vocabulario
            var iterador = indiceTerminos.ObtenerIterador();
            int indice = 0;

            while (iterador.Siguiente())
            {
                var termino = iterador.Current;

                // Buscar si el término aparece en la consulta
                int frecuenciaEnConsulta = ObtenerFrecuenciaTermino(frecuenciasConsulta, termino.Palabra);

                if (frecuenciaEnConsulta > 0)
                {
                    // TF-IDF = TF * IDF
                    double tfIdf = frecuenciaEnConsulta * termino.Idf;
                    vector[indice] = tfIdf;
                }
                else
                {
                    vector[indice] = 0.0;
                }

                indice++;
            }

            return vector;
        }

        /// <summary>
        /// Crear vector TF-IDF para un documento específico
        /// </summary>
        private Vector CrearVectorDocumento(Documento documento)
        {
            if (indiceTerminos.Count == 0)
                return null;

            var vector = new Vector(indiceTerminos.Count);
            var iterador = indiceTerminos.ObtenerIterador();
            int indice = 0;

            while (iterador.Siguiente())
            {
                var termino = iterador.Current;

                // Obtener TF-IDF del término para este documento
                double tfIdf = termino.ObtenerTfIdf(documento.Id);
                vector[indice] = tfIdf;

                indice++;
            }

            return vector;
        }

        /// <summary>
        /// Obtener frecuencia de un término específico en el array
        /// </summary>
        private int ObtenerFrecuenciaTermino(TerminoFrecuencia[] frecuencias, string termino)
        {
            for (int i = 0; i < frecuencias.Length; i++)
            {
                if (string.Equals(frecuencias[i].Token, termino, StringComparison.OrdinalIgnoreCase))
                    return frecuencias[i].Frecuencia;
            }

            return 0;
        }

        /// <summary>
        /// Calcular métricas TF-IDF para todo el corpus
        /// </summary>
        public void CalcularMetricasTfIdf()
        {
            int totalDocumentos = documentos.Count;
            if (totalDocumentos == 0) return;

            Console.WriteLine($"📊 Calculando IDF para {indiceTerminos.Count} términos...");

            var iterador = indiceTerminos.ObtenerIterador();
            while (iterador.Siguiente())
            {
                iterador.Current.CalcularIdf(totalDocumentos);
            }

            Console.WriteLine("✅ Métricas TF-IDF calculadas");
        }

        /// <summary>
        /// Búsqueda tradicional TF-IDF (método alternativo)
        /// </summary>
        public ListaDobleEnlazada<ResultadoBusqueda> BuscarTfIdf(string consulta)
        {
            var resultados = new ListaDobleEnlazada<ResultadoBusqueda>();

            if (string.IsNullOrWhiteSpace(consulta))
                return resultados;

            var tokensConsulta = procesador.ProcesarTextoCompleto(consulta);
            if (tokensConsulta.Count == 0)
                return resultados;

            // Obtener términos únicos de la consulta
            var terminosConsulta = new ListaDobleEnlazada<Termino>();
            var tokensUnicos = EliminarDuplicados(tokensConsulta);

            foreach (var token in tokensUnicos)
            {
                var termino = BuscarTermino(token);
                if (termino != null)
                {
                    terminosConsulta.Agregar(termino);
                }
            }

            if (terminosConsulta.Count == 0)
                return resultados;

            // Calcular puntuación para cada documento
            var iteradorDocs = new Iterador<Documento>(documentos);
            while (iteradorDocs.Siguiente())
            {
                var doc = iteradorDocs.Current;
                double puntuacion = 0;

                var iteradorTerminos = new Iterador<Termino>(terminosConsulta);
                while (iteradorTerminos.Siguiente())
                {
                    puntuacion += iteradorTerminos.Current.ObtenerTfIdf(doc.Id);
                }

                if (puntuacion > 0)
                    resultados.Agregar(new ResultadoBusqueda(doc, puntuacion));
            }

            resultados.OrdenarDescendente(r => r.Score);
            return resultados;
        }

        /// <summary>
        /// Actualizar índice con nuevos documentos
        /// </summary>
        public async Task ActualizarIndice(string rutaDirectorio)
        {
            Console.WriteLine("🔄 Actualizando índice...");

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
                // Reordenar con RadixSort
                indiceTerminos.OrdenarRadix();
                CalcularMetricasTfIdf();
                Console.WriteLine($"✅ Agregados {agregados} documentos");
            }
            else
            {
                Console.WriteLine("ℹ️ No hay documentos nuevos");
            }
        }

        /// <summary>
        /// Eliminar duplicados sin usar genéricos prohibidos
        /// </summary>
        private string[] EliminarDuplicados(List<string> tokens)
        {
            var tokensTemp = new string[tokens.Count];
            int cantidadUnicos = 0;

            foreach (var token in tokens)
            {
                if (string.IsNullOrWhiteSpace(token)) continue;

                string tokenNormalizado = token.ToLowerInvariant();
                bool existe = false;

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

            var resultado = new string[cantidadUnicos];
            Array.Copy(tokensTemp, resultado, cantidadUnicos);
            return resultado;
        }

        /// <summary>
        /// Guardar índice en archivo binario
        /// </summary>
        public void GuardarEnArchivoBinario(string rutaArchivo)
        {
            try
            {
                Console.WriteLine("💾 Guardando índice con vector ordenado...");

                // Convertir vector ordenado a lista para serialización
                var listaTerminos = new ListaDobleEnlazada<Termino>();
                var iterador = indiceTerminos.ObtenerIterador();
                while (iterador.Siguiente())
                {
                    listaTerminos.Agregar(iterador.Current);
                }

                serializador.GuardarIndice(rutaArchivo, listaTerminos, documentos);
                Console.WriteLine($"✅ Índice guardado en {rutaArchivo}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error guardando: {ex.Message}");
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

                // Convertir lista a vector ordenado
                indiceTerminos = new VectorOrdenado<Termino>();
                var iterador = new Iterador<Termino>(indiceNuevo);
                while (iterador.Siguiente())
                {
                    indiceTerminos.Agregar(iterador.Current);
                }

                // Aplicar RadixSort al cargar
                indiceTerminos.OrdenarRadix();

                documentos = documentosNuevos;

                // Recalcular contador
                contadorDocumentos = 0;
                var iteradorDocs = new Iterador<Documento>(documentos);
                while (iteradorDocs.Siguiente())
                {
                    if (iteradorDocs.Current.Id > contadorDocumentos)
                        contadorDocumentos = iteradorDocs.Current.Id;
                }

                Console.WriteLine($"✅ Índice cargado: {documentos.Count} docs, {indiceTerminos.Count} términos");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error cargando: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Limpiar índice
        /// </summary>
        public void Limpiar()
        {
            indiceTerminos.Limpiar();
            documentos.Limpiar();
            contadorDocumentos = 0;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Console.WriteLine("🧹 Índice limpiado");
        }

        /// <summary>
        /// Obtener estadísticas del índice
        /// </summary>
        public EstadisticasIndice ObtenerEstadisticas()
        {
            return new EstadisticasIndice
            {
                CantidadDocumentos = documentos.Count,
                CantidadTerminos = indiceTerminos.Count,
                IndiceOrdenado = indiceTerminos.EstaOrdenado,
                MemoriaEstimadaKB = EstimarUsoMemoria(),
                PromedioTerminosPorDocumento =
                    documentos.Count > 0 ? (double)indiceTerminos.Count / documentos.Count : 0.0
            };
        }

        /// <summary>
        /// Estimar uso de memoria
        /// </summary>
        private int EstimarUsoMemoria()
        {
            int memoria = 0;
            memoria += indiceTerminos.Count * 128; // Términos
            memoria += documentos.Count * 256; // Documentos
            return memoria / 1024; // KB
        }

        // Getters
        public int GetCantidadDocumentos() => documentos.Count;
        public ListaDobleEnlazada<Documento> GetDocumentos() => documentos;
        public VectorOrdenado<Termino> GetIndiceTerminos() => indiceTerminos;
    }

    /// <summary>
    /// Estadísticas del índice
    /// </summary>
    public class EstadisticasIndice
    {
        public int CantidadDocumentos { get; set; }
        public int CantidadTerminos { get; set; }
        public bool IndiceOrdenado { get; set; }
        public int MemoriaEstimadaKB { get; set; }
        public double PromedioTerminosPorDocumento { get; set; }

        public override string ToString()
        {
            return $"📊 Docs: {CantidadDocumentos} | Términos: {CantidadTerminos} | " +
                   $"Ordenado: {(IndiceOrdenado ? "✅" : "❌")} | RAM: {MemoriaEstimadaKB} KB";
        }
    }
}