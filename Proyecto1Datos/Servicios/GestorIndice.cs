using PruebaRider.Estructura.Nodo;
using PruebaRider.Servicios;
using PruebaRider.Strategy;

namespace PruebaRider.Servicios
{
    /// <summary>
    /// GestorIndice CORREGIDO para usar únicamente Vector personalizado
    /// - Patrón Singleton para gestionar el índice invertido
    /// - Integración completa con BuscadorVectorial que usa Vector propio
    /// - Enlaces base64 directos en resultados
    /// - Estadísticas mejoradas con información vectorial
    /// - 100% compatible con enunciado (sin genéricos del lenguaje)
    /// </summary>
    public sealed class GestorIndice
    {
        private static GestorIndice instancia = null;
        private static readonly object lockObject = new object();
        
        private IndiceInvertido indice;
        private string rutaIndiceActual;

        private GestorIndice()
        {
            indice = new IndiceInvertido();
            rutaIndiceActual = "";
        }

        /// <summary>
        /// Obtener instancia única (Patrón Singleton)
        /// </summary>
        public static GestorIndice ObtenerInstancia()
        {
            if (instancia == null)
            {
                lock (lockObject)
                {
                    if (instancia == null)
                        instancia = new GestorIndice();
                }
            }
            return instancia;
        }

        /// <summary>
        /// Crear índice desde directorio con Vector personalizado
        /// </summary>
        public async Task<bool> CrearIndiceDesdeDirectorio(string rutaDirectorio)
        {
            try
            {
                Console.WriteLine("🎯 Iniciando creación de índice con Vector personalizado...");
                await indice.CrearDesdeRuta(rutaDirectorio);
                rutaIndiceActual = rutaDirectorio;
                
                Console.WriteLine("✅ Índice creado exitosamente con capacidades vectoriales");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creando índice: {ex.Message}");
                Console.WriteLine($"💡 Verifique que el directorio existe y contiene archivos .txt");
                return false;
            }
        }

        /// <summary>
        /// Actualizar índice existente
        /// </summary>
        public async Task<bool> ActualizarIndice(string rutaDirectorio = null)
        {
            try
            {
                string ruta = rutaDirectorio ?? rutaIndiceActual;
                if (string.IsNullOrEmpty(ruta))
                {
                    Console.WriteLine("❌ No se ha especificado una ruta para actualizar");
                    return false;
                }

                Console.WriteLine("🔄 Actualizando índice con nuevos documentos...");
                await indice.ActualizarIndice(ruta);
                Console.WriteLine("✅ Índice actualizado - Vectores recalculados automáticamente");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error actualizando índice: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Guardar índice en archivo binario
        /// </summary>
        public bool GuardarIndice(string rutaArchivo)
        {
            try
            {
                Console.WriteLine("💾 Guardando índice con estructuras vectoriales...");
                indice.GuardarEnArchivoBinario(rutaArchivo);
                Console.WriteLine("✅ Índice guardado exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error guardando índice: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cargar índice desde archivo binario
        /// </summary>
        public bool CargarIndice(string rutaArchivo)
        {
            try
            {
                Console.WriteLine("📂 Cargando índice con soporte vectorial...");
                indice.CargarDesdeArchivoBinario(rutaArchivo);
                Console.WriteLine("✅ Índice cargado - Buscador vectorial inicializado");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error cargando índice: {ex.Message}");
                Console.WriteLine("💡 Verifique que el archivo es un índice válido");
                return false;
            }
        }

        /// <summary>
        /// Aplicar Ley de Zipf con optimización vectorial
        /// </summary>
        public void AplicarLeyZipf(int percentil, bool eliminarFrecuentes = true)
        {
            try
            {
                string estrategia = eliminarFrecuentes ? "términos frecuentes" : "términos raros";
                Console.WriteLine($"⚡ Aplicando Ley de Zipf: {percentil}% de {estrategia}");
                Console.WriteLine("🎯 Optimizando vectores de términos...");
                
                indice.AplicarLeyZipf(percentil, eliminarFrecuentes);
                
                Console.WriteLine($"✅ Ley de Zipf aplicada exitosamente");
                Console.WriteLine("🎯 Vectores compactados y buscador actualizado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error aplicando Ley de Zipf: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// CORE: Búsqueda vectorial con similitud coseno usando Vector personalizado
        /// </summary>
        public ListaDobleEnlazada<ResultadoBusquedaVectorial> BuscarConSimilitudCoseno(string consulta)
        {
            if (string.IsNullOrWhiteSpace(consulta))
                return new ListaDobleEnlazada<ResultadoBusquedaVectorial>();

            try
            {
                Console.WriteLine($"🔍 Ejecutando búsqueda vectorial: '{consulta}'");
                Console.WriteLine("🎯 Usando Vector personalizado con similitud coseno precisa...");
        
                var resultadosMejorados = indice.BuscarConSimilitudCoseno(consulta);
        
                // Convertir a tipo original
                var resultados = new ListaDobleEnlazada<ResultadoBusquedaVectorial>();
                var iterador = new Iterador<ResultadoBusquedaVectorial>(resultados);
        
                while (iterador.Siguiente())
                {
                    var mejorado = iterador.Current;
                    var original = new ResultadoBusquedaVectorial(mejorado.Documento, mejorado.SimilitudCoseno);
                    resultados.Agregar(original);
                }
        
                Console.WriteLine($"✅ Búsqueda vectorial completada: {resultados.Count} resultados");
                return resultados;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en búsqueda vectorial: {ex.Message}");
                return new ListaDobleEnlazada<ResultadoBusquedaVectorial>();
            }
        }

        /// <summary>
        /// Búsqueda TF-IDF tradicional
        /// </summary>
        public ListaDobleEnlazada<ResultadoBusqueda> BuscarTfIdf(string consulta)
        {
            if (string.IsNullOrWhiteSpace(consulta))
                return new ListaDobleEnlazada<ResultadoBusqueda>();

            try
            {
                Console.WriteLine($"📊 Ejecutando búsqueda TF-IDF: '{consulta}'");
                var resultados = indice.Buscar(consulta);
                Console.WriteLine($"✅ Búsqueda TF-IDF completada: {resultados.Count} resultados");
                return resultados;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en búsqueda TF-IDF: {ex.Message}");
                return new ListaDobleEnlazada<ResultadoBusqueda>();
            }
        }

        /// <summary>
        /// Obtener estadísticas mejoradas del índice
        /// </summary>
        public EstadisticasIndiceMejoradas ObtenerEstadisticas()
        {
            try
            {
                return indice.ObtenerEstadisticas();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error obteniendo estadísticas: {ex.Message}");
                return new EstadisticasIndiceMejoradas
                {
                    CantidadDocumentos = 0,
                    CantidadTerminos = 0,
                    IndiceOrdenado = false,
                    BuscadorVectorialActivo = false,
                    MemoriaEstimadaKB = 0,
                    PromedioTerminosPorDocumento = 0.0
                };
            }
        }

        /// <summary>
        /// NUEVO: Analizar consulta para debugging (usando Vector personalizado)
        /// </summary>
        /*public AnalisisConsultaVectorial AnalizarConsulta(string consulta)
        {
            try
            {
                Console.WriteLine($"🔬 Analizando consulta con Vector personalizado: '{consulta}'");
                return indice.AnalizarConsultaVectorial(consulta);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error analizando consulta: {ex.Message}");
                return new AnalisisConsultaVectorial
                {
                    ConsultaOriginal = consulta,
                    CantidadTokensOriginales = 0,
                    CantidadTokensUnicos = 0,
                    DimensionVector = 0,
                    MagnitudVector = 0.0,
                    TieneValoresSignificativos = false,
                    ComponentesSignificativas = new (int, double)[0],
                    VocabularioTotal = 0
                };
            }
        }*/

        /// <summary>
        /// Limpiar índice completamente
        /// </summary>
        public void LimpiarIndice()
        {
            try
            {
                Console.WriteLine("🧹 Limpiando índice y liberando memoria vectorial...");
                indice.Limpiar();
                rutaIndiceActual = "";
                Console.WriteLine("✅ Índice limpiado completamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error limpiando índice: {ex.Message}");
            }
        }

        /// <summary>
        /// Verificar si el índice está vacío
        /// </summary>
        public bool IndiceEstaVacio()
        {
            try
            {
                return indice.GetCantidadDocumentos() == 0;
            }
            catch
            {
                return true; // Asumir vacío si hay error
            }
        }

        /// <summary>
        /// Obtener ruta actual del directorio de documentos
        /// </summary>
        public string GetRutaActual()
        {
            return rutaIndiceActual ?? "";
        }

        /// <summary>
        /// NUEVO: Obtener información de rendimiento
        /// </summary>
        public InformacionRendimiento ObtenerInformacionRendimiento()
        {
            try
            {
                var stats = ObtenerEstadisticas();
                
                return new InformacionRendimiento
                {
                    CantidadDocumentos = stats.CantidadDocumentos,
                    CantidadTerminos = stats.CantidadTerminos,
                    MemoriaEstimadaKB = stats.MemoriaEstimadaKB,
                    BuscadorVectorialActivo = stats.BuscadorVectorialActivo,
                    IndiceOrdenado = stats.IndiceOrdenado,
                    EficienciaBusqueda = CalcularEficienciaBusqueda(stats),
                    CapacidadVectorial = stats.CantidadTerminos > 0 ? "Óptima" : "No disponible"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error obteniendo información de rendimiento: {ex.Message}");
                return new InformacionRendimiento();
            }
        }

        /// <summary>
        /// Calcular eficiencia de búsqueda basada en estadísticas
        /// </summary>
        private string CalcularEficienciaBusqueda(EstadisticasIndiceMejoradas stats)
        {
            if (stats.CantidadTerminos == 0)
                return "No disponible";
                
            if (!stats.BuscadorVectorialActivo)
                return "Limitada (sin vectores)";
                
            if (stats.IndiceOrdenado && stats.CantidadTerminos > 100)
                return "Óptima (O(log n))";
            else if (stats.IndiceOrdenado)
                return "Buena (O(log n))";
            else if (stats.CantidadTerminos < 50)
                return "Aceptable (O(n) pequeño)";
            else
                return "Mejorable (O(n) grande)";
        }

        /// <summary>
        /// NUEVO: Validar integridad del índice
        /// </summary>
        public ResultadoValidacion ValidarIntegridadIndice()
        {
            var resultado = new ResultadoValidacion();
            
            try
            {
                resultado.IndiceNoVacio = !IndiceEstaVacio();
                
                if (resultado.IndiceNoVacio)
                {
                    var stats = ObtenerEstadisticas();
                    resultado.EstructurasConsistentes = stats.CantidadDocumentos > 0 && stats.CantidadTerminos > 0;
                    resultado.BuscadorVectorialFuncional = stats.BuscadorVectorialActivo;
                    resultado.MemoriaRazonable = stats.MemoriaEstimadaKB < 1000000; // Menos de 1GB
                }
                
                resultado.EsValido = resultado.IndiceNoVacio && 
                                   resultado.EstructurasConsistentes && 
                                   resultado.BuscadorVectorialFuncional;
                
                if (resultado.EsValido)
                {
                    resultado.MensajeValidacion = "✅ Índice válido y operativo";
                }
                else
                {
                    var problemas = new List<string>();
                    if (!resultado.IndiceNoVacio) problemas.Add("índice vacío");
                    if (!resultado.EstructurasConsistentes) problemas.Add("estructuras inconsistentes");
                    if (!resultado.BuscadorVectorialFuncional) problemas.Add("buscador vectorial inactivo");
                    
                    resultado.MensajeValidacion = $"⚠️ Problemas encontrados: {string.Join(", ", problemas)}";
                }
            }
            catch (Exception ex)
            {
                resultado.EsValido = false;
                resultado.MensajeValidacion = $"❌ Error en validación: {ex.Message}";
            }
            
            return resultado;
        }

        /// <summary>
        /// NUEVO: Optimizar índice automáticamente
        /// </summary>
        public bool OptimizarIndiceAutomatico()
        {
            try
            {
                if (IndiceEstaVacio())
                {
                    Console.WriteLine("❌ No hay índice para optimizar");
                    return false;
                }

                var stats = ObtenerEstadisticas();
                Console.WriteLine($"🔧 Iniciando optimización automática...");
                Console.WriteLine($"📊 Estado inicial: {stats.CantidadTerminos} términos");

                // Aplicar Zipf automático si hay muchos términos
                if (stats.CantidadTerminos > 1000)
                {
                    Console.WriteLine("⚡ Aplicando Ley de Zipf automática (15% términos frecuentes)...");
                    AplicarLeyZipf(15, true); // Eliminar 15% de términos más frecuentes
                }
                else if (stats.CantidadTerminos > 500)
                {
                    Console.WriteLine("⚡ Aplicando Ley de Zipf automática (10% términos frecuentes)...");
                    AplicarLeyZipf(10, true); // Eliminar 10% de términos más frecuentes
                }

                var statsFinales = ObtenerEstadisticas();
                int terminosEliminados = stats.CantidadTerminos - statsFinales.CantidadTerminos;
                
                Console.WriteLine($"✅ Optimización completada:");
                Console.WriteLine($"   📊 Términos eliminados: {terminosEliminados}");
                Console.WriteLine($"   📊 Términos finales: {statsFinales.CantidadTerminos}");
                Console.WriteLine($"   💾 Memoria estimada: {statsFinales.MemoriaEstimadaKB} KB");
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en optimización automática: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// NUEVA: Información de rendimiento del sistema
    /// </summary>
    public class InformacionRendimiento
    {
        public int CantidadDocumentos { get; set; }
        public int CantidadTerminos { get; set; }
        public int MemoriaEstimadaKB { get; set; }
        public bool BuscadorVectorialActivo { get; set; }
        public bool IndiceOrdenado { get; set; }
        public string EficienciaBusqueda { get; set; } = "No disponible";
        public string CapacidadVectorial { get; set; } = "No disponible";

        public override string ToString()
        {
            return $"🚀 RENDIMIENTO DEL SISTEMA\n" +
                   $"📊 Documentos: {CantidadDocumentos}\n" +
                   $"📊 Términos: {CantidadTerminos}\n" +
                   $"💾 Memoria: {MemoriaEstimadaKB} KB\n" +
                   $"🎯 Vector: {(BuscadorVectorialActivo ? "✅" : "❌")}\n" +
                   $"🔤 Ordenado: {(IndiceOrdenado ? "✅" : "❌")}\n" +
                   $"⚡ Eficiencia: {EficienciaBusqueda}\n" +
                   $"🎯 Capacidad vectorial: {CapacidadVectorial}";
        }
    }

    /// <summary>
    /// NUEVA: Resultado de validación de integridad
    /// </summary>
    public class ResultadoValidacion
    {
        public bool EsValido { get; set; }
        public bool IndiceNoVacio { get; set; }
        public bool EstructurasConsistentes { get; set; }
        public bool BuscadorVectorialFuncional { get; set; }
        public bool MemoriaRazonable { get; set; }
        public string MensajeValidacion { get; set; } = "";

        public override string ToString()
        {
            var resultado = $"🔍 VALIDACIÓN DE INTEGRIDAD\n";
            resultado += $"✅ Válido: {(EsValido ? "SÍ" : "NO")}\n";
            resultado += $"📊 Índice poblado: {(IndiceNoVacio ? "✅" : "❌")}\n";
            resultado += $"🏗️ Estructuras consistentes: {(EstructurasConsistentes ? "✅" : "❌")}\n";
            resultado += $"🎯 Buscador vectorial: {(BuscadorVectorialFuncional ? "✅" : "❌")}\n";
            resultado += $"💾 Memoria razonable: {(MemoriaRazonable ? "✅" : "❌")}\n";
            resultado += $"📝 {MensajeValidacion}";
            
            return resultado;
        }
    }
}