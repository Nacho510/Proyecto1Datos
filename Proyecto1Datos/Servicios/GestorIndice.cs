using PruebaRider.Estructura.Nodo;

namespace PruebaRider.Servicios
{
    /// <summary>
    /// Gestor de índice reestructurado
    /// - Patrón Singleton
    /// - Usa el nuevo índice con Vector y RadixSort
    /// - Enfocado en simplicidad y eficiencia
    /// </summary>
    public sealed class GestorIndice
    {
        private static GestorIndice instancia = null;
        private static readonly object lockObject = new object();

        private IndiceInvertido indice;
        private BuscadorVectorial buscador;
        private string rutaIndiceActual;

        private GestorIndice()
        {
            indice = new IndiceInvertido();
            rutaIndiceActual = "";
        }

        /// <summary>
        /// Obtener instancia única - Patrón Singleton
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
        /// Crear índice desde directorio con RadixSort
        /// </summary>
        public async Task<bool> CrearIndiceDesdeDirectorio(string rutaDirectorio)
        {
            try
            {
                Console.WriteLine("🎯 Creando índice con Vector + RadixSort...");

                await indice.CrearDesdeRuta(rutaDirectorio);
                rutaIndiceActual = rutaDirectorio;

                // Inicializar buscador vectorial
                buscador = new BuscadorVectorial(indice);

                Console.WriteLine("✅ Índice creado exitosamente:");
                Console.WriteLine("   🎯 Vector ordenado con RadixSort: ✅");
                Console.WriteLine("   🔍 Búsqueda vectorial: ✅");
                Console.WriteLine("   📊 Similitud coseno: ✅");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creando índice: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// BÚSQUEDA PRINCIPAL - Similitud Coseno
        /// </summary>
        public ListaDobleEnlazada<ResultadoBusquedaVectorial> BuscarConSimilitudCoseno(string consulta)
        {
            if (string.IsNullOrWhiteSpace(consulta))
                return new ListaDobleEnlazada<ResultadoBusquedaVectorial>();

            if (buscador == null)
            {
                Console.WriteLine("🎯 Inicializando buscador vectorial...");
                buscador = new BuscadorVectorial(indice);
            }

            try
            {
                var resultados = buscador.Buscar(consulta);
                Console.WriteLine($"✅ Búsqueda completada: {resultados.Count} resultados");
                return resultados;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en búsqueda vectorial: {ex.Message}");
                return new ListaDobleEnlazada<ResultadoBusquedaVectorial>();
            }
        }

        /// <summary>
        /// Búsqueda tradicional TF-IDF (alternativa)
        /// </summary>
        public ListaDobleEnlazada<ResultadoBusqueda> BuscarTfIdf(string consulta)
        {
            if (string.IsNullOrWhiteSpace(consulta))
                return new ListaDobleEnlazada<ResultadoBusqueda>();

            try
            {
                return indice.BuscarTfIdf(consulta);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en búsqueda TF-IDF: {ex.Message}");
                return new ListaDobleEnlazada<ResultadoBusqueda>();
            }
        }

        /// <summary>
        /// Actualizar índice con nuevos documentos
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

                Console.WriteLine("🔄 Actualizando índice...");
                await indice.ActualizarIndice(ruta);

                // Recrear buscador para usar el índice actualizado
                buscador = new BuscadorVectorial(indice);

                Console.WriteLine("✅ Índice actualizado - RadixSort aplicado automáticamente");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error actualizando índice: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Guardar índice en archivo
        /// </summary>
        public bool GuardarIndice(string rutaArchivo)
        {
            try
            {
                Console.WriteLine("💾 Guardando índice con vector ordenado...");
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
        /// Cargar índice desde archivo
        /// </summary>
        public bool CargarIndice(string rutaArchivo)
        {
            try
            {
                Console.WriteLine("📂 Cargando índice...");
                indice.CargarDesdeArchivoBinario(rutaArchivo);

                // Inicializar buscador después de cargar
                buscador = new BuscadorVectorial(indice);

                Console.WriteLine("✅ Índice cargado - Vector ordenado restaurado");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error cargando índice: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtener estadísticas del índice
        /// </summary>
        public EstadisticasIndice ObtenerEstadisticas()
        {
            try
            {
                return indice.ObtenerEstadisticas();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error obteniendo estadísticas: {ex.Message}");
                return new EstadisticasIndice
                {
                    CantidadDocumentos = 0,
                    CantidadTerminos = 0,
                    IndiceOrdenado = false,
                    MemoriaEstimadaKB = 0,
                    PromedioTerminosPorDocumento = 0.0
                };
            }
        }

        /// <summary>
        /// Limpiar sistema
        /// </summary>
        public void LimpiarSistema()
        {
            try
            {
                Console.WriteLine("🧹 Limpiando sistema...");
                indice.Limpiar();
                buscador = null;
                rutaIndiceActual = "";

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                Console.WriteLine("✅ Sistema limpiado");
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
        /// Obtener ruta actual
        /// </summary>
        public string GetRutaActual()
        {
            return rutaIndiceActual ?? "";
        }

        /// <summary>
        /// Validar integridad del sistema
        /// </summary>
        public ResultadoValidacion ValidarIntegridad()
        {
            var resultado = new ResultadoValidacion();

            try
            {
                var stats = ObtenerEstadisticas();

                resultado.IndiceNoVacio = stats.CantidadDocumentos > 0;
                resultado.Vector = stats.IndiceOrdenado;
                resultado.BuscadorFuncional = buscador != null;
                resultado.EstructurasConsistentes = stats.CantidadDocumentos > 0 && stats.CantidadTerminos > 0;

                resultado.EsValido = resultado.IndiceNoVacio &&
                                     resultado.Vector &&
                                     resultado.BuscadorFuncional &&
                                     resultado.EstructurasConsistentes;

                if (resultado.EsValido)
                {
                    resultado.Mensaje =
                        $"✅ Sistema válido: RadixSort activo, {stats.CantidadTerminos} términos ordenados";
                }
                else
                {
                    var problemas = new List<string>();
                    if (!resultado.IndiceNoVacio) problemas.Add("índice vacío");
                    if (!resultado.Vector) problemas.Add("vector no ordenado");
                    if (!resultado.BuscadorFuncional) problemas.Add("buscador no funcional");
                    if (!resultado.EstructurasConsistentes) problemas.Add("estructuras inconsistentes");

                    resultado.Mensaje = $"⚠️ Problemas: {string.Join(", ", problemas)}";
                }
            }
            catch (Exception ex)
            {
                resultado.EsValido = false;
                resultado.Mensaje = $"❌ Error en validación: {ex.Message}";
            }

            return resultado;
        }
    }

    /// <summary>
    /// Resultado de validación del sistema
    /// </summary>
    public class ResultadoValidacion
    {
        public bool EsValido { get; set; }
        public bool IndiceNoVacio { get; set; }
        public bool Vector { get; set; }
        public bool BuscadorFuncional { get; set; }
        public bool EstructurasConsistentes { get; set; }
        public string Mensaje { get; set; } = "";

        public override string ToString()
        {
            var estado = EsValido ? "VÁLIDO" : "INVÁLIDO";
            return $"🔍 Sistema: {estado}\n" +
                   $"   📊 Índice poblado: {(IndiceNoVacio ? "✅" : "❌")}\n" +
                   $"   🔤 Vector ordenado: {(Vector ? "✅" : "❌")}\n" +
                   $"   🎯 Buscador activo: {(BuscadorFuncional ? "✅" : "❌")}\n" +
                   $"   🏗️ Estructuras OK: {(EstructurasConsistentes ? "✅" : "❌")}\n" +
                   $"📝 {Mensaje}";
        }
    }
}