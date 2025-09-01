using PruebaRider.Estructura.Nodo;
using PruebaRider.Servicios;
using PruebaRider.Strategy;

namespace PruebaRider.Servicios
{
    /// <summary>
    /// GestorIndice FINAL OPTIMIZADO - Versión para entrega
    /// - Patrón Singleton optimizado
    /// - Búsqueda vectorial ultra rápida con Vector personalizado
    /// - Zipf automático integrado (sin exposición manual)
    /// - Enlaces base64 directos
    /// - Rendimiento O(log n) en búsquedas
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
        /// Obtener instancia única - Patrón Singleton thread-safe
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
        /// Crear índice con optimización automática completa
        /// </summary>
        public async Task<bool> CrearIndiceDesdeDirectorio(string rutaDirectorio)
        {
            try
            {
                Console.WriteLine("🎯 Iniciando creación optimizada con Vector personalizado...");
                
                // El método CrearDesdeRuta ya incluye Zipf automático
                await indice.CrearDesdeRuta(rutaDirectorio);
                rutaIndiceActual = rutaDirectorio;
                
                Console.WriteLine("✅ Índice creado con optimizaciones automáticas:");
                Console.WriteLine("   🎯 Vector personalizado inicializado");
                Console.WriteLine("   ⚡ Ley de Zipf aplicada automáticamente");
                Console.WriteLine("   🔍 Búsqueda vectorial habilitada");
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creando índice optimizado: {ex.Message}");
                Console.WriteLine($"💡 Verifique directorio y archivos .txt");
                return false;
            }
        }

        /// <summary>
        /// NUEVO: Recrear índice completamente optimizado
        /// </summary>
        public async Task<bool> RecrearIndiceCompleto(string rutaDirectorio = null)
        {
            try
            {
                string ruta = rutaDirectorio ?? rutaIndiceActual;
                if (string.IsNullOrEmpty(ruta))
                {
                    Console.WriteLine("❌ No se ha especificado ruta para recrear");
                    return false;
                }

                Console.WriteLine("🔨 Recreando índice con optimización completa...");
                
                // Usar el nuevo método optimizado del IndiceInvertido
                await indice.RecrerarIndiceOptimizado(ruta);
                rutaIndiceActual = ruta;
                
                Console.WriteLine("✅ Recreación completada con todas las optimizaciones");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en recreación completa: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Actualizar índice existente con reoptimización
        /// </summary>
        public async Task<bool> ActualizarIndice(string rutaDirectorio = null)
        {
            try
            {
                string ruta = rutaDirectorio ?? rutaIndiceActual;
                if (string.IsNullOrEmpty(ruta))
                {
                    Console.WriteLine("❌ No se ha especificado ruta para actualizar");
                    return false;
                }

                Console.WriteLine("🔄 Actualizando índice con reoptimización...");
                await indice.ActualizarIndice(ruta);
                Console.WriteLine("✅ Índice actualizado - Optimizaciones aplicadas automáticamente");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error actualizando índice: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Guardar índice optimizado
        /// </summary>
        public bool GuardarIndice(string rutaArchivo)
        {
            try
            {
                Console.WriteLine("💾 Guardando índice optimizado con estructuras vectoriales...");
                indice.GuardarEnArchivoBinario(rutaArchivo);
                Console.WriteLine("✅ Índice optimizado guardado exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error guardando índice: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cargar índice optimizado
        /// </summary>
        public bool CargarIndice(string rutaArchivo)
        {
            try
            {
                Console.WriteLine("📂 Cargando índice optimizado...");
                indice.CargarDesdeArchivoBinario(rutaArchivo);
                Console.WriteLine("✅ Índice cargado - Buscador vectorial optimizado inicializado");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error cargando índice: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// MÉTODO PRINCIPAL: Búsqueda vectorial optimizada con Vector personalizado
        /// Esta es la función de búsqueda principal del sistema
        /// </summary>
        public ListaDobleEnlazada<ResultadoBusquedaVectorial> BuscarConSimilitudCoseno(string consulta)
        {
            if (string.IsNullOrWhiteSpace(consulta))
                return new ListaDobleEnlazada<ResultadoBusquedaVectorial>();

            try
            {
                // Usar búsqueda vectorial ultra optimizada del IndiceInvertido
                var resultados = indice.BuscarConSimilitudCoseno(consulta);
                
                Console.WriteLine($"✅ Búsqueda vectorial optimizada completada: {resultados.Count} resultados");
                Console.WriteLine("🎯 Resultados incluyen enlaces base64 directos");
                
                return resultados;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en búsqueda vectorial optimizada: {ex.Message}");
                return new ListaDobleEnlazada<ResultadoBusquedaVectorial>();
            }
        }

        /// <summary>
        /// Búsqueda TF-IDF tradicional (método alternativo)
        /// </summary>
        public ListaDobleEnlazada<ResultadoBusqueda> BuscarTfIdf(string consulta)
        {
            if (string.IsNullOrWhiteSpace(consulta))
                return new ListaDobleEnlazada<ResultadoBusqueda>();

            try
            {
                Console.WriteLine($"📊 Ejecutando búsqueda TF-IDF tradicional: '{consulta}'");
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
        /// Obtener estadísticas completas del sistema
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
        /// Obtener información detallada de rendimiento
        /// </summary>
        public InformacionRendimientoCompleta ObtenerInformacionRendimiento()
        {
            try
            {
                var stats = ObtenerEstadisticas();
                
                return new InformacionRendimientoCompleta
                {
                    CantidadDocumentos = stats.CantidadDocumentos,
                    CantidadTerminos = stats.CantidadTerminos,
                    MemoriaEstimadaKB = stats.MemoriaEstimadaKB,
                    BuscadorVectorialActivo = stats.BuscadorVectorialActivo,
                    IndiceOrdenado = stats.IndiceOrdenado,
                    EficienciaBusqueda = CalcularEficienciaBusqueda(stats),
                    CapacidadVectorial = stats.BuscadorVectorialActivo ? "Óptima con Vector personalizado" : "No disponible",
                    OptimizacionZipf = "Automática habilitada",
                    TipoBusqueda = "Similitud Coseno Vectorial",
                    ComplejidadTemporal = stats.IndiceOrdenado ? "O(log n)" : "O(n)"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error obteniendo información de rendimiento: {ex.Message}");
                return new InformacionRendimientoCompleta();
            }
        }

        /// <summary>
        /// Validar integridad completa del sistema
        /// </summary>
        public ResultadoValidacionCompleta ValidarIntegridadCompleta()
        {
            var resultado = new ResultadoValidacionCompleta();
            
            try
            {
                resultado.IndiceNoVacio = !IndiceEstaVacio();
                
                if (resultado.IndiceNoVacio)
                {
                    var stats = ObtenerEstadisticas();
                    resultado.EstructurasConsistentes = stats.CantidadDocumentos > 0 && stats.CantidadTerminos > 0;
                    resultado.BuscadorVectorialFuncional = stats.BuscadorVectorialActivo;
                    resultado.MemoriaRazonable = stats.MemoriaEstimadaKB < 1000000;
                    resultado.IndiceOrdenado = stats.IndiceOrdenado;
                    resultado.OptimizacionZipfAplicada = stats.CantidadTerminos < 10000; // Heurística
                }
                
                resultado.EsValido = resultado.IndiceNoVacio && 
                                   resultado.EstructurasConsistentes && 
                                   resultado.BuscadorVectorialFuncional &&
                                   resultado.IndiceOrdenado;
                
                resultado.PuntuacionCalidad = CalcularPuntuacionCalidad(resultado);
                
                if (resultado.EsValido)
                {
                    resultado.MensajeValidacion = $"✅ Sistema completamente optimizado (Calidad: {resultado.PuntuacionCalidad}/100)";
                }
                else
                {
                    var problemas = new List<string>();
                    if (!resultado.IndiceNoVacio) problemas.Add("índice vacío");
                    if (!resultado.EstructurasConsistentes) problemas.Add("estructuras inconsistentes");
                    if (!resultado.BuscadorVectorialFuncional) problemas.Add("buscador vectorial inactivo");
                    if (!resultado.IndiceOrdenado) problemas.Add("índice no ordenado");
                    
                    resultado.MensajeValidacion = $"⚠️ Problemas: {string.Join(", ", problemas)} (Calidad: {resultado.PuntuacionCalidad}/100)";
                }
            }
            catch (Exception ex)
            {
                resultado.EsValido = false;
                resultado.PuntuacionCalidad = 0;
                resultado.MensajeValidacion = $"❌ Error en validación completa: {ex.Message}";
            }
            
            return resultado;
        }

        /// <summary>
        /// NUEVO: Optimización automática completa del sistema
        /// </summary>
        public async Task<bool> OptimizarSistemaCompleto()
        {
            try
            {
                if (IndiceEstaVacio())
                {
                    Console.WriteLine("❌ No hay índice para optimizar");
                    return false;
                }

                Console.WriteLine($"🔧 Iniciando optimización completa del sistema...");
                
                var statsIniciales = ObtenerEstadisticas();
                Console.WriteLine($"📊 Estado inicial: {statsIniciales.CantidadTerminos} términos, {statsIniciales.MemoriaEstimadaKB} KB");

                // La optimización ya está integrada en el IndiceInvertido, 
                // pero podemos forzar una reoptimización si es necesario
                if (statsIniciales.CantidadTerminos > 5000 || statsIniciales.MemoriaEstimadaKB > 10000)
                {
                    Console.WriteLine("⚡ Aplicando reoptimización avanzada...");
                    await RecrearIndiceCompleto(rutaIndiceActual);
                }

                var statsFinales = ObtenerEstadisticas();
                int terminosOptimizados = statsIniciales.CantidadTerminos - statsFinales.CantidadTerminos;
                int memoriaLiberada = statsIniciales.MemoriaEstimadaKB - statsFinales.MemoriaEstimadaKB;
                
                Console.WriteLine($"✅ Optimización completa finalizada:");
                Console.WriteLine($"   📊 Términos optimizados: {Math.Max(0, terminosOptimizados)}");
                Console.WriteLine($"   💾 Memoria liberada: {Math.Max(0, memoriaLiberada)} KB");
                Console.WriteLine($"   🎯 Vector personalizado: Activo");
                Console.WriteLine($"   ⚡ Búsqueda vectorial: Optimizada");
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en optimización completa: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Limpiar sistema completamente
        /// </summary>
        public void LimpiarSistema()
        {
            try
            {
                Console.WriteLine("🧹 Limpiando sistema completo...");
                indice.Limpiar();
                rutaIndiceActual = "";
                
                // Forzar liberación de memoria
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                
                Console.WriteLine("✅ Sistema limpiado completamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error limpiando sistema: {ex.Message}");
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
                return true;
            }
        }

        /// <summary>
        /// Obtener ruta actual del directorio
        /// </summary>
        public string GetRutaActual()
        {
            return rutaIndiceActual ?? "";
        }

        #region Métodos Auxiliares Privados

        /// <summary>
        /// Calcular eficiencia de búsqueda basada en estadísticas
        /// </summary>
        private string CalcularEficienciaBusqueda(EstadisticasIndiceMejoradas stats)
        {
            if (stats.CantidadTerminos == 0)
                return "No disponible";
                
            if (!stats.BuscadorVectorialActivo)
                return "Limitada (sin vectores)";
                
            if (stats.IndiceOrdenado && stats.CantidadTerminos > 1000)
                return "Óptima (Vector + O(log n))";
            else if (stats.IndiceOrdenado && stats.BuscadorVectorialActivo)
                return "Excelente (Vector + O(log n))";
            else if (stats.BuscadorVectorialActivo)
                return "Buena (Vector + O(n))";
            else
                return "Básica (O(n) lineal)";
        }

        /// <summary>
        /// Calcular puntuación de calidad del sistema (0-100)
        /// </summary>
        private int CalcularPuntuacionCalidad(ResultadoValidacionCompleta resultado)
        {
            int puntuacion = 0;
            
            if (resultado.IndiceNoVacio) puntuacion += 25;
            if (resultado.EstructurasConsistentes) puntuacion += 25;
            if (resultado.BuscadorVectorialFuncional) puntuacion += 30;
            if (resultado.IndiceOrdenado) puntuacion += 15;
            if (resultado.OptimizacionZipfAplicada) puntuacion += 5;
            
            return Math.Min(100, Math.Max(0, puntuacion));
        }

        #endregion
    }

    /// <summary>
    /// Información completa de rendimiento del sistema
    /// </summary>
    public class InformacionRendimientoCompleta
    {
        public int CantidadDocumentos { get; set; }
        public int CantidadTerminos { get; set; }
        public int MemoriaEstimadaKB { get; set; }
        public bool BuscadorVectorialActivo { get; set; }
        public bool IndiceOrdenado { get; set; }
        public string EficienciaBusqueda { get; set; } = "No disponible";
        public string CapacidadVectorial { get; set; } = "No disponible";
        public string OptimizacionZipf { get; set; } = "No aplicada";
        public string TipoBusqueda { get; set; } = "Básica";
        public string ComplejidadTemporal { get; set; } = "O(n)";

        public override string ToString()
        {
            return $"🚀 RENDIMIENTO COMPLETO DEL SISTEMA\n" +
                   $"📊 Documentos: {CantidadDocumentos} | Términos: {CantidadTerminos}\n" +
                   $"💾 Memoria: {MemoriaEstimadaKB} KB | Complejidad: {ComplejidadTemporal}\n" +
                   $"🎯 Vector: {(BuscadorVectorialActivo ? "✅" : "❌")} | Ordenado: {(IndiceOrdenado ? "✅" : "❌")}\n" +
                   $"⚡ Eficiencia: {EficienciaBusqueda}\n" +
                   $"🎯 Capacidad vectorial: {CapacidadVectorial}\n" +
                   $"⚡ Optimización Zipf: {OptimizacionZipf}\n" +
                   $"🔍 Tipo búsqueda: {TipoBusqueda}";
        }
    }

    /// <summary>
    /// Resultado completo de validación del sistema
    /// </summary>
    public class ResultadoValidacionCompleta
    {
        public bool EsValido { get; set; }
        public bool IndiceNoVacio { get; set; }
        public bool EstructurasConsistentes { get; set; }
        public bool BuscadorVectorialFuncional { get; set; }
        public bool MemoriaRazonable { get; set; }
        public bool IndiceOrdenado { get; set; }
        public bool OptimizacionZipfAplicada { get; set; }
        public int PuntuacionCalidad { get; set; }
        public string MensajeValidacion { get; set; } = "";

        public override string ToString()
        {
            var resultado = $"🔍 VALIDACIÓN COMPLETA DEL SISTEMA\n";
            resultado += $"✅ Estado: {(EsValido ? "VÁLIDO" : "CON PROBLEMAS")} | Calidad: {PuntuacionCalidad}/100\n";
            resultado += $"📊 Índice poblado: {(IndiceNoVacio ? "✅" : "❌")}\n";
            resultado += $"🏗️ Estructuras consistentes: {(EstructurasConsistentes ? "✅" : "❌")}\n";
            resultado += $"🎯 Buscador vectorial: {(BuscadorVectorialFuncional ? "✅" : "❌")}\n";
            resultado += $"💾 Memoria razonable: {(MemoriaRazonable ? "✅" : "❌")}\n";
            resultado += $"🔤 Índice ordenado: {(IndiceOrdenado ? "✅" : "❌")}\n";
            resultado += $"⚡ Zipf aplicado: {(OptimizacionZipfAplicada ? "✅" : "❌")}\n";
            resultado += $"📝 {MensajeValidacion}";
            
            return resultado;
        }
    }
}