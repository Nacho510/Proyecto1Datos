using PruebaRider.Modelo;
using PruebaRider.Estructura.Nodo;
using PruebaRider.Strategy;
using PruebaRider.Persistencia;

namespace PruebaRider.Servicios
{
    /// <summary>
    /// IndiceInvertido CORREGIDO - Búsqueda funcionando
    /// - Errores críticos corregidos
    /// - BuscarConSimilitudCoseno ahora funciona
    /// - Zipf automático corregido
    /// </summary>
    public class IndiceInvertido
    {
        private ListaDobleEnlazada<Termino> indice;
        private ListaDobleEnlazada<Documento> documentos;
        private ProcesadorDeTexto procesador;
        private SerializadorBinario serializador;
        private BuscadorVectorial buscadorVectorial;
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
        /// CREAR DESDE RUTA - CORREGIDO
        /// </summary>
        public async Task CrearDesdeRuta(string rutaDirectorio)
        {
            Console.WriteLine("🚀 Creando índice invertido...");

            Limpiar();
            await CargarDirectorio(rutaDirectorio);

            Console.WriteLine($"📊 Documentos cargados: {documentos.Count}");
            Console.WriteLine($"📊 Términos únicos: {indice.Count}");

            // APLICAR ZIPF AUTOMÁTICO SOLO SI HAY MUCHOS TÉRMINOS
            if (indice.Count > 100)
            {
                await AplicarOptimizacionAutomatica();
            }
            else
            {
                Console.WriteLine("📊 Vocabulario pequeño - No se aplica Zipf");
            }

            OrdenarIndice();
            CalcularIdfGlobal();

            // INICIALIZAR BUSCADOR VECTORIAL
            buscadorVectorial = new BuscadorVectorial(this);

            Console.WriteLine($"✅ Índice creado: {documentos.Count} documentos, {indice.Count} términos");
        }

        /// <summary>
        /// CARGAR DIRECTORIO - SIN CAMBIOS
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
        /// AGREGAR DOCUMENTO - SIN CAMBIOS
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

                Console.WriteLine($"📄 Procesado: {Path.GetFileName(rutaArchivo)} ({tokens.Count} tokens)");

                // Procesar términos únicos
                var tokensUnicos = EliminarDuplicadosSinGenericos(tokens);
                foreach (var token in tokensUnicos)
                {
                    AgregarTermino(token, documento);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error procesando {Path.GetFileName(rutaArchivo)}: {ex.Message}");
            }
        }

        /// <summary>
        /// BÚSQUEDA VECTORIAL CORREGIDA - MÉTODO PRINCIPAL
        /// </summary>
        public ListaDobleEnlazada<ResultadoBusquedaVectorial> BuscarConSimilitudCoseno(string consulta)
        {
            Console.WriteLine($"🔍 === INICIANDO BÚSQUEDA VECTORIAL ===");
            Console.WriteLine($"📝 Consulta: '{consulta}'");
            Console.WriteLine($"📚 Índice: {indice.Count} términos, {documentos.Count} documentos");

            if (buscadorVectorial == null)
            {
                Console.WriteLine("🎯 Inicializando buscador vectorial...");
                buscadorVectorial = new BuscadorVectorial(this);
            }

            var resultados = buscadorVectorial.BuscarConSimilitudCoseno(consulta);

            Console.WriteLine($"🔍 === FIN BÚSQUEDA VECTORIAL ===");
            return resultados;
        }

        /// <summary>
        /// BUSCAR TERMINO - CORREGIDO para debugging
        /// </summary>
        public Termino BuscarTermino(string palabra)
        {
            if (string.IsNullOrWhiteSpace(palabra))
                return null;

            string palabraNormalizada = palabra.ToLowerInvariant();

            if (indice.EstaOrdenada && indice.Count > 10)
            {
                // Búsqueda binaria
                var dummy = new Termino(palabraNormalizada);
                return indice.BuscarBinario(dummy, CompararTerminos);
            }
            else
            {
                // Búsqueda lineal
                var iterador = new Iterador<Termino>(indice);
                while (iterador.Siguiente())
                {
                    if (string.Equals(iterador.Current.Palabra, palabraNormalizada, StringComparison.OrdinalIgnoreCase))
                        return iterador.Current;
                }

                return null;
            }
        }

        /// <summary>
        /// APLICAR OPTIMIZACIÓN AUTOMÁTICA - CORREGIDO
        /// </summary>
        private async Task AplicarOptimizacionAutomatica()
        {
            if (indice.Count == 0) return;

            Console.WriteLine("⚡ Aplicando optimización automática...");

            int terminosOriginales = indice.Count;

            // Estrategia más conservadora para no eliminar demasiado
            if (terminosOriginales > 1000)
            {
                Console.WriteLine("📊 Aplicando Zipf moderado (10% términos frecuentes)");
                AplicarZipfInterno(10, true);
            }
            else if (terminosOriginales > 500)
            {
                Console.WriteLine("📊 Aplicando Zipf suave (5% términos frecuentes)");
                AplicarZipfInterno(5, true);
            }

            int terminosFinales = indice.Count;
            int terminosEliminados = terminosOriginales - terminosFinales;

            if (terminosEliminados > 0)
            {
                Console.WriteLine($"✅ Optimización aplicada:");
                Console.WriteLine($"   📊 Términos eliminados: {terminosEliminados}");
                Console.WriteLine($"   📊 Términos restantes: {terminosFinales}");
            }
        }

        /// <summary>
        /// APLICAR ZIPF INTERNO - CORREGIDO
        /// </summary>
        private void AplicarZipfInterno(int percentil, bool eliminarFrecuentes)
        {
            if (percentil <= 0 || percentil >= 100) return;

            var contexto = new ContextoZipf();

            if (eliminarFrecuentes)
                contexto.EstablecerEstrategia(new EliminarTerminosFrecuentes(indice));
            else
                contexto.EstablecerEstrategia(new EliminarTerminosRaros(indice));

            contexto.AplicarLeyZipf(percentil);
        }

        /// <summary>
        /// MOSTRAR ESTADÍSTICAS DE DEBUGGING
        /// </summary>
        public void MostrarEstadisticasDebug()
        {
            Console.WriteLine($"\n=== ESTADÍSTICAS DEL ÍNDICE ===");
            Console.WriteLine($"📊 Documentos: {documentos.Count}");
            Console.WriteLine($"📊 Términos únicos: {indice.Count}");
            Console.WriteLine($"📊 Índice ordenado: {indice.EstaOrdenada}");

            if (indice.Count > 0)
            {
                Console.WriteLine($"\n=== PRIMEROS 10 TÉRMINOS ===");
                var iterador = new Iterador<Termino>(indice);
                int contador = 0;
                while (iterador.Siguiente() && contador < 10)
                {
                    var termino = iterador.Current;
                    Console.WriteLine(
                        $"   {contador + 1}. '{termino.Palabra}' -> {termino.ListaDocumentos.Count} docs, IDF: {termino.Idf:F3}");
                    contador++;
                }
            }

            if (documentos.Count > 0)
            {
                Console.WriteLine($"\n=== DOCUMENTOS CARGADOS ===");
                var iteradorDocs = new Iterador<Documento>(documentos);
                while (iteradorDocs.Siguiente())
                {
                    var doc = iteradorDocs.Current;
                    Console.WriteLine(
                        $"   📄 {Path.GetFileName(doc.Ruta)} (ID: {doc.Id}, Términos: {doc.Frecuencias.Count})");
                }
            }

            Console.WriteLine($"=== FIN ESTADÍSTICAS ===\n");
        }

        /// <summary>
        /// BÚSQUEDA TRADICIONAL TF-IDF - CORREGIDA
        /// </summary>
        public ListaDobleEnlazada<ResultadoBusqueda> Buscar(string consulta)
        {
            var resultados = new ListaDobleEnlazada<ResultadoBusqueda>();

            if (string.IsNullOrWhiteSpace(consulta))
                return resultados;

            var tokensConsulta = procesador.ProcesarTextoCompleto(consulta);
            if (tokensConsulta.Count == 0)
                return resultados;

            Console.WriteLine($"🔍 Búsqueda TF-IDF para tokens: {string.Join(", ", tokensConsulta)}");

            var terminosConsulta = new ListaDobleEnlazada<Termino>();
            var tokensUnicos = EliminarDuplicadosSinGenericos(tokensConsulta);

            foreach (var token in tokensUnicos)
            {
                var termino = BuscarTermino(token);
                if (termino != null)
                {
                    terminosConsulta.Agregar(termino);
                    Console.WriteLine(
                        $"   ✅ Término encontrado: '{termino.Palabra}' en {termino.ListaDocumentos.Count} documentos");
                }
                else
                {
                    Console.WriteLine($"   ❌ Término NO encontrado: '{token}'");
                }
            }

            if (terminosConsulta.Count == 0)
            {
                Console.WriteLine("❌ Ningún término de la consulta fue encontrado en el índice");
                return resultados;
            }

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

            resultados.OrdenarDescendente(r => r.Score);
            Console.WriteLine($"✅ Búsqueda TF-IDF completada: {resultados.Count} resultados");
            return resultados;
        }

        /// <summary>
        /// ELIMINAR DUPLICADOS - SIN CAMBIOS
        /// </summary>
        private string[] EliminarDuplicadosSinGenericos(List<string> tokens)
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
        /// AGREGAR TÉRMINO - SIN CAMBIOS
        /// </summary>
        private void AgregarTermino(string palabra, Documento documento)
        {
            var terminoExistente = BuscarTermino(palabra);

            if (terminoExistente == null)
            {
                var nuevoTermino = new Termino(palabra);
                nuevoTermino.AgregarDocumento(documento);
                indice.Agregar(nuevoTermino);
            }
            else
            {
                terminoExistente.AgregarDocumento(documento);
            }
        }

        /// <summary>
        /// ACTUALIZAR ÍNDICE - CORREGIDO
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
                OrdenarIndice();
                CalcularIdfGlobal();

                if (buscadorVectorial != null)
                {
                    buscadorVectorial = new BuscadorVectorial(this);
                }

                Console.WriteLine($"✅ Agregados {agregados} documentos");
            }
            else
            {
                Console.WriteLine("ℹ️ No hay documentos nuevos");
            }
        }

        /// <summary>
        /// GUARDAR EN ARCHIVO BINARIO - SIN CAMBIOS
        /// </summary>
        public void GuardarEnArchivoBinario(string rutaArchivo)
        {
            try
            {
                Console.WriteLine("💾 Guardando índice...");
                serializador.GuardarIndice(rutaArchivo, indice, documentos);
                Console.WriteLine($"✅ Índice guardado en {rutaArchivo}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error guardando: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// CARGAR DESDE ARCHIVO BINARIO - SIN CAMBIOS
        /// </summary>
        public void CargarDesdeArchivoBinario(string rutaArchivo)
        {
            try
            {
                Console.WriteLine($"📂 Cargando índice desde {rutaArchivo}...");
                var (indiceNuevo, documentosNuevos) = serializador.CargarIndice(rutaArchivo);

                indice = indiceNuevo;
                documentos = documentosNuevos;

                contadorDocumentos = 0;
                var iterador = new Iterador<Documento>(documentos);
                while (iterador.Siguiente())
                {
                    if (iterador.Current.Id > contadorDocumentos)
                        contadorDocumentos = iterador.Current.Id;
                }

                OrdenarIndice();
                buscadorVectorial = new BuscadorVectorial(this);

                Console.WriteLine($"✅ Índice cargado: {documentos.Count} docs, {indice.Count} términos");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error cargando: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// ORDENAR ÍNDICE - SIN CAMBIOS
        /// </summary>
        private void OrdenarIndice()
        {
            if (indice.Count > 1)
            {
                Console.WriteLine($"🔤 Ordenando {indice.Count} términos...");
                indice.OrdenarCon(CompararTerminos);
                Console.WriteLine("✅ Índice ordenado");
            }
        }

        /// <summary>
        /// CALCULAR IDF GLOBAL - SIN CAMBIOS
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
        /// COMPARAR TÉRMINOS - SIN CAMBIOS
        /// </summary>
        private int CompararTerminos(Termino t1, Termino t2)
        {
            if (t1 == null && t2 == null) return 0;
            if (t1 == null) return -1;
            if (t2 == null) return 1;

            return string.Compare(t1.Palabra, t2.Palabra, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// LIMPIAR - SIN CAMBIOS
        /// </summary>
        public void Limpiar()
        {
            indice.Limpiar();
            documentos.Limpiar();
            contadorDocumentos = 0;
            buscadorVectorial = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Console.WriteLine("🧹 Índice limpiado");
        }

        // GETTERS - SIN CAMBIOS
        public int GetCantidadDocumentos() => documentos.Count;
        public ListaDobleEnlazada<Documento> GetDocumentos() => documentos;
        public ListaDobleEnlazada<Termino> GetIndice() => indice;

        /// <summary>
        /// OBTENER ESTADÍSTICAS - SIN CAMBIOS
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
        /// ESTIMAR USO MEMORIA - SIN CAMBIOS
        /// </summary>
        private int EstimarUsoMemoria()
        {
            int memoria = 0;
            memoria += indice.Count * 64;
            memoria += documentos.Count * 256;

            var iterador = new Iterador<Termino>(indice);
            while (iterador.Siguiente())
            {
                var termino = iterador.Current;
                memoria += termino.VectorDocumentosIds.Dimension * 8;
            }

            return memoria / 1024;
        }

        /// <summary>
        /// RECREAR ÍNDICE OPTIMIZADO - NUEVO MÉTODO
        /// </summary>
        public async Task RecrerarIndiceOptimizado(string rutaDirectorio)
        {
            Console.WriteLine("🔨 Recreando índice completo...");

            try
            {
                await CrearDesdeRuta(rutaDirectorio);

                var stats = ObtenerEstadisticas();
                Console.WriteLine("✅ Recreación completada:");
                Console.WriteLine($"   📊 Documentos: {stats.CantidadDocumentos}");
                Console.WriteLine($"   📊 Términos: {stats.CantidadTerminos}");
                Console.WriteLine($"   💾 Memoria: {stats.MemoriaEstimadaKB} KB");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en recreación: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>
    /// ESTADÍSTICAS MEJORADAS - SIN CAMBIOS
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