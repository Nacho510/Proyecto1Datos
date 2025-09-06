using System.IO;
using PruebaRider.Modelo;
using PruebaRider.Estructura.Nodo;
using PruebaRider.Estructura.Vector;
using PruebaRider.Persistencia;
using PruebaRider.Strategy;

namespace PruebaRider.Servicios
{
    /// <summary>
    /// Índice invertido CON LEY DE ZIPF INTEGRADA
    /// - Aplicación obligatoria de Zipf durante construcción
    /// - Percentil configurable por usuario
    /// - Patrón Strategy para diferentes enfoques de Zipf
    /// </summary>
    public class IndiceInvertido
    {
        private VectorOrdenado<Termino> indiceTerminos;
        private ListaDobleEnlazada<Documento> documentos;
        private ProcesadorDeTexto procesador;
        private SerializadorBinario serializador;
        private int contadorDocumentos;
        
        // LEY DE ZIPF - Requisito del enunciado
        private ContextoZipf contextoZipf;
        private int percentilZipf;
        private bool zipfAplicado;

        public IndiceInvertido()
        {
            indiceTerminos = new VectorOrdenado<Termino>();
            documentos = new ListaDobleEnlazada<Documento>();
            procesador = new ProcesadorDeTexto();
            serializador = new SerializadorBinario();
            contadorDocumentos = 0;
            
            // Configuración por defecto de Zipf
            contextoZipf = new ContextoZipf();
            percentilZipf = 15; // 15% por defecto (conservador)
            zipfAplicado = false;
        }

        /// <summary>
        /// CREAR ÍNDICE CON LEY DE ZIPF OBLIGATORIA
        /// </summary>
        public async Task CrearDesdeRuta(string rutaDirectorio, int percentilZipf = 15, 
            FabricaEstrategias.TipoEstrategia tipoZipf = FabricaEstrategias.TipoEstrategia.FrecuentesConservador)
        {
            Console.WriteLine("🚀 Creando índice invertido con Ley de Zipf...");
            
            Limpiar();
            this.percentilZipf = Math.Max(1, Math.Min(30, percentilZipf)); // Límite de seguridad
            
            // FASE 1: Cargar documentos
            await CargarDirectorio(rutaDirectorio);
            Console.WriteLine($"📄 {documentos.Count} documentos cargados");

            // FASE 2: Aplicar RadixSort ANTES de Zipf
            Console.WriteLine("⚡ Aplicando RadixSort...");
            indiceTerminos.OrdenarRadix();
            
            // FASE 3: APLICAR LEY DE ZIPF (OBLIGATORIO según enunciado)
            Console.WriteLine($"🔥 Aplicando Ley de Zipf ({this.percentilZipf}%)...");
            await AplicarLeyDeZipf(tipoZipf);
            
            // FASE 4: Calcular TF-IDF después de Zipf
            Console.WriteLine("📊 Calculando TF-IDF...");
            CalcularMetricasTfIdf();
            
            // FASE 5: Reordenar después de filtrado
            indiceTerminos.OrdenarRadix();
            
            Console.WriteLine($"✅ Índice creado con Zipf aplicado: {documentos.Count} docs, {indiceTerminos.Count} términos");
        }

        /// <summary>
        /// APLICAR LEY DE ZIPF - Método principal (OBLIGATORIO)
        /// </summary>
        public async Task AplicarLeyDeZipf(FabricaEstrategias.TipoEstrategia tipoEstrategia)
        {
            if (indiceTerminos.Count == 0)
            {
                Console.WriteLine("⚠️ No hay términos para aplicar Zipf");
                return;
            }

            // Convertir vector a lista para Strategy
            var listaTerminos = ConvertirVectorALista();
            
            // Crear estrategia usando Factory
            var estrategia = FabricaEstrategias.CrearEstrategia(tipoEstrategia, listaTerminos, documentos.Count);
            contextoZipf.EstablecerEstrategia(estrategia);
            
            Console.WriteLine($"🎯 Estrategia: {estrategia.NombreEstrategia}");
            Console.WriteLine($"📊 Términos antes de Zipf: {indiceTerminos.Count}");
            
            // APLICAR ZIPF
            contextoZipf.AplicarLeyZipf(percentilZipf);
            
            // Convertir lista filtrada de vuelta a vector
            ConvertirListaAVector(listaTerminos);
            
            zipfAplicado = true;
            Console.WriteLine($"📊 Términos después de Zipf: {indiceTerminos.Count}");
            Console.WriteLine($"✅ Ley de Zipf aplicada exitosamente");
        }

        /// <summary>
        /// Método para cambiar percentil de Zipf después de creación
        /// </summary>
        public async Task ModificarZipf(int nuevoPercentil, FabricaEstrategias.TipoEstrategia tipoEstrategia)
        {
            if (!zipfAplicado)
            {
                Console.WriteLine("⚠️ Zipf no ha sido aplicado previamente");
                return;
            }
            
            Console.WriteLine($"🔄 Modificando Zipf: {percentilZipf}% → {nuevoPercentil}%");
            
            // Necesitaríamos reconstruir desde archivos originales para cambiar Zipf
            // Por ahora, aplicar nuevo filtro sobre el existente
            percentilZipf = Math.Max(1, Math.Min(30, nuevoPercentil));
            await AplicarLeyDeZipf(tipoEstrategia);
            CalcularMetricasTfIdf();
        }

        /// <summary>
        /// Convertir vector ordenado a lista para Strategy pattern
        /// </summary>
        private ListaDobleEnlazada<Termino> ConvertirVectorALista()
        {
            var lista = new ListaDobleEnlazada<Termino>();
            var iterador = indiceTerminos.ObtenerIterador();
            
            while (iterador.Siguiente())
            {
                lista.Agregar(iterador.Current);
            }
            
            return lista;
        }

        /// <summary>
        /// Convertir lista filtrada de vuelta a vector ordenado
        /// </summary>
        private void ConvertirListaAVector(ListaDobleEnlazada<Termino> lista)
        {
            indiceTerminos.Limpiar();
            var iterador = new Iterador<Termino>(lista);
            
            while (iterador.Siguiente())
            {
                indiceTerminos.Agregar(iterador.Current);
            }
        }

        /// <summary>
        /// Cargar directorio de documentos
        /// </summary>
        private async Task CargarDirectorio(string rutaDirectorio)
        {
            if (!Directory.Exists(rutaDirectorio))
                throw new DirectoryNotFoundException($"Directorio no encontrado: {rutaDirectorio}");

            var archivos = Directory.GetFiles(rutaDirectorio, "*.txt");
            if (archivos.Length == 0)
                throw new InvalidOperationException("No se encontraron archivos .txt");

            foreach (var archivo in archivos)
            {
                await AgregarDocumento(archivo);
            }
        }

        /// <summary>
        /// Agregar documento individual
        /// </summary>
        private async Task AgregarDocumento(string rutaArchivo)
        {
            try
            {
                string contenido = await File.ReadAllTextAsync(rutaArchivo);
                if (string.IsNullOrWhiteSpace(contenido)) return;

                var tokens = procesador.ProcesarTextoCompleto(contenido);
                if (tokens.Count == 0) return;

                var documento = new Documento(++contadorDocumentos, contenido, rutaArchivo);
                documento.CalcularFrecuenciasArray(tokens.ToArray());
                documentos.Agregar(documento);

                ProcesarTerminosDelDocumento(documento, tokens);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error procesando {Path.GetFileName(rutaArchivo)}: {ex.Message}");
            }
        }

        /// <summary>
        /// Procesar términos del documento
        /// </summary>
        private void ProcesarTerminosDelDocumento(Documento documento, ArrayDinamico tokens)
        {
            var frecuenciasLocales = ContarFrecuenciasLocales(tokens);

            for (int i = 0; i < frecuenciasLocales.Length; i++)
            {
                var tf = frecuenciasLocales[i];
                AgregarTerminoAlIndice(tf.Token, documento, tf.Frecuencia);
            }
        }

        /// <summary>
        /// Contar frecuencias locales sin genéricos
        /// </summary>
        private TerminoFrecuencia[] ContarFrecuenciasLocales(ArrayDinamico tokens)
        {
            var resultado = new TerminoFrecuencia[tokens.Count];
            int cantidadUnicos = 0;

            var iterador = tokens.ObtenerIterador();
            while (iterador.Siguiente())
            {
                string token = iterador.Current.ToLowerInvariant();
                bool encontrado = false;

                for (int i = 0; i < cantidadUnicos; i++)
                {
                    if (resultado[i].Token == token)
                    {
                        resultado[i].Frecuencia++;
                        encontrado = true;
                        break;
                    }
                }

                if (!encontrado)
                {
                    resultado[cantidadUnicos] = new TerminoFrecuencia(token, 1);
                    cantidadUnicos++;
                }
            }

            var final = new TerminoFrecuencia[cantidadUnicos];
            Array.Copy(resultado, final, cantidadUnicos);
            return final;
        }

        /// <summary>
        /// Agregar término al índice
        /// </summary>
        private void AgregarTerminoAlIndice(string palabra, Documento documento, int frecuencia)
        {
            var terminoExistente = BuscarTermino(palabra);

            if (terminoExistente == null)
            {
                var nuevoTermino = new Termino(palabra);
                nuevoTermino.AgregarDocumento(documento, frecuencia);
                indiceTerminos.Agregar(nuevoTermino);
            }
            else
            {
                terminoExistente.AgregarDocumento(documento, frecuencia);
            }
        }

        /// <summary>
        /// BÚSQUEDA PRINCIPAL - Similitud coseno
        /// </summary>
        public ListaDobleEnlazada<ResultadoBusquedaVectorial> BuscarConSimilitudCoseno(string consulta)
        {
            var resultados = new ListaDobleEnlazada<ResultadoBusquedaVectorial>();

            if (string.IsNullOrWhiteSpace(consulta))
                return resultados;

            var tokensConsulta = procesador.ProcesarTextoCompleto(consulta);
            if (tokensConsulta.Count == 0)
                return resultados;

            var vectorConsulta = CrearVectorConsulta(tokensConsulta);
            if (vectorConsulta == null || !vectorConsulta.TieneValoresSignificativos())
                return resultados;

            // Calcular similitud para cada documento
            var iteradorDocs = new Iterador<Documento>(documentos);
            while (iteradorDocs.Siguiente())
            {
                var documento = iteradorDocs.Current;
                var vectorDoc = CrearVectorDocumento(documento);
                
                if (vectorDoc == null || !vectorDoc.TieneValoresSignificativos())
                    continue;

                double similitud = vectorConsulta.SimilitudCoseno(vectorDoc);

                if (similitud > 0.001)
                {
                    var resultado = new ResultadoBusquedaVectorial(documento, similitud);
                    resultados.Agregar(resultado);
                }
            }

            if (resultados.Count > 0)
            {
                resultados.OrdenarDescendente(r => r.SimilitudCoseno);
            }

            return resultados;
        }

        /// <summary>
        /// Crear vector de consulta
        /// </summary>
        private Vector CrearVectorConsulta(ArrayDinamico tokens)
        {
            if (indiceTerminos.Count == 0) return null;

            var vector = new Vector(indiceTerminos.Count);
            var frecuenciasConsulta = ContarFrecuenciasLocales(tokens);

            var iterador = indiceTerminos.ObtenerIterador();
            int indice = 0;

            while (iterador.Siguiente())
            {
                var termino = iterador.Current;
                int frecuenciaEnConsulta = ObtenerFrecuenciaTermino(frecuenciasConsulta, termino.Palabra);

                if (frecuenciaEnConsulta > 0)
                {
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
        /// Crear vector TF-IDF para documento
        /// </summary>
        private Vector CrearVectorDocumento(Documento documento)
        {
            if (indiceTerminos.Count == 0) return null;

            var vector = new Vector(indiceTerminos.Count);
            var iterador = indiceTerminos.ObtenerIterador();
            int indice = 0;

            while (iterador.Siguiente())
            {
                var termino = iterador.Current;
                double tfIdf = termino.ObtenerTfIdf(documento.Id);
                vector[indice] = tfIdf;
                indice++;
            }

            return vector;
        }

        /// <summary>
        /// Buscar término con búsqueda binaria
        /// </summary>
        public Termino BuscarTermino(string palabra)
        {
            if (string.IsNullOrWhiteSpace(palabra)) return null;

            string palabraNormalizada = palabra.ToLowerInvariant();

            if (indiceTerminos.EstaOrdenado)
            {
                var terminoBusqueda = new Termino(palabraNormalizada);
                return indiceTerminos.BuscarBinario(terminoBusqueda);
            }
            else
            {
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
        /// Calcular TF-IDF para todo el corpus
        /// </summary>
        private void CalcularMetricasTfIdf()
        {
            int totalDocumentos = documentos.Count;
            if (totalDocumentos == 0) return;

            var iterador = indiceTerminos.ObtenerIterador();
            while (iterador.Siguiente())
            {
                iterador.Current.CalcularIdf(totalDocumentos);
            }
        }

        /// <summary>
        /// Obtener frecuencia de término específico
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

        // MÉTODOS DE PERSISTENCIA
        public void GuardarEnArchivoBinario(string rutaArchivo)
        {
            var listaTerminos = ConvertirVectorALista();
            serializador.GuardarIndice(rutaArchivo, listaTerminos, documentos);
        }

        public void CargarDesdeArchivoBinario(string rutaArchivo)
        {
            var (indiceNuevo, documentosNuevos) = serializador.CargarIndice(rutaArchivo);

            indiceTerminos = new VectorOrdenado<Termino>();
            var iterador = new Iterador<Termino>(indiceNuevo);
            while (iterador.Siguiente())
            {
                indiceTerminos.Agregar(iterador.Current);
            }

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
            
            zipfAplicado = true; // Asumir que índice cargado ya tiene Zipf aplicado
        }

        public void Limpiar()
        {
            indiceTerminos.Limpiar();
            documentos.Limpiar();
            contadorDocumentos = 0;
            zipfAplicado = false;
        }

        public EstadisticasIndiceConZipf ObtenerEstadisticas()
        {
            return new EstadisticasIndiceConZipf
            {
                CantidadDocumentos = documentos.Count,
                CantidadTerminos = indiceTerminos.Count,
                IndiceOrdenado = indiceTerminos.EstaOrdenado,
                MemoriaEstimadaKB = EstimarUsoMemoria(),
                PromedioTerminosPorDocumento = documentos.Count > 0 ? (double)indiceTerminos.Count / documentos.Count : 0.0,
                ZipfAplicado = zipfAplicado,
                PercentilZipf = percentilZipf,
                EstrategiaZipf = contextoZipf.ObtenerInformacionEstrategia()
            };
        }

        private int EstimarUsoMemoria()
        {
            int memoria = 0;
            memoria += indiceTerminos.Count * 128;
            memoria += documentos.Count * 256;
            return memoria / 1024;
        }

        // Getters
        public int GetCantidadDocumentos() => documentos.Count;
        public ListaDobleEnlazada<Documento> GetDocumentos() => documentos;
        public VectorOrdenado<Termino> GetIndiceTerminos() => indiceTerminos;
        public bool ZipfAplicado => zipfAplicado;
        public int PercentilZipf => percentilZipf;
    }

    /// <summary>
    /// Estadísticas extendidas con información de Zipf
    /// </summary>
    public class EstadisticasIndiceConZipf
    {
        public int CantidadDocumentos { get; set; }
        public int CantidadTerminos { get; set; }
        public bool IndiceOrdenado { get; set; }
        public int MemoriaEstimadaKB { get; set; }
        public double PromedioTerminosPorDocumento { get; set; }
        
        // Información de Zipf
        public bool ZipfAplicado { get; set; }
        public int PercentilZipf { get; set; }
        public string EstrategiaZipf { get; set; }

        public override string ToString()
        {
            return $"📊 Docs: {CantidadDocumentos} | Términos: {CantidadTerminos} | " +
                   $"Ordenado: {(IndiceOrdenado ? "✅" : "❌")} | " +
                   $"Zipf: {(ZipfAplicado ? $"✅({PercentilZipf}%)" : "❌")} | " +
                   $"RAM: {MemoriaEstimadaKB} KB";
        }
    }
}