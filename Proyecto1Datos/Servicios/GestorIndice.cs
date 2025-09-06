using PruebaRider.Estructura.Nodo;
using PruebaRider.Strategy;

namespace PruebaRider.Servicios
{
    /// <summary>
    /// Gestor de índice CON LEY DE ZIPF INTEGRADA - Versión completa
    /// - Patrón Singleton implementado correctamente
    /// - Configuración de Zipf obligatoria según enunciado
    /// - Diferentes estrategias de eliminación de términos
    /// - Gestión completa del ciclo de vida del índice
    /// </summary>
    public sealed class GestorIndice
    {
        private static GestorIndice instancia = null;
        private static readonly object lockObject = new object();

        private IndiceInvertido indice;
        private BuscadorVectorial buscador;
        private string rutaIndiceActual;

        // Constructor privado para Singleton
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
        /// CREAR ÍNDICE CON LEY DE ZIPF OBLIGATORIA - Versión automática
        /// </summary>
        public async Task<bool> CrearIndiceDesdeDirectorio(string rutaDirectorio, 
            int percentilZipf = 15, 
            FabricaEstrategias.TipoEstrategia estrategiaZipf = FabricaEstrategias.TipoEstrategia.FrecuentesConservador)
        {
            try
            {
                Console.WriteLine("🎯 Creando índice con Vector + RadixSort + Ley de Zipf...");
                Console.WriteLine($"📊 Configuración Zipf: {percentilZipf}% - {estrategiaZipf}");

                await indice.CrearDesdeRuta(rutaDirectorio, percentilZipf, estrategiaZipf);
                rutaIndiceActual = rutaDirectorio;

                // Inicializar buscador vectorial
                buscador = new BuscadorVectorial(indice);

                Console.WriteLine("✅ Índice creado exitosamente:");
                Console.WriteLine("   🎯 Vector ordenado con RadixSort: ✅");
                Console.WriteLine($"   🔥 Ley de Zipf aplicada ({percentilZipf}%): ✅");
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
        /// Crear índice con configuración de Zipf personalizada por el usuario
        /// </summary>
        public async Task<bool> CrearIndiceConZipfPersonalizado(string rutaDirectorio)
        {
            try
            {
                Console.WriteLine("\n🔥 CONFIGURACIÓN DE LEY DE ZIPF");
                Console.WriteLine("================================");
                
                // Mostrar estrategias disponibles
                var estrategiasDisponibles = FabricaEstrategias.ObtenerEstrategiasDisponibles();
                Console.WriteLine("Estrategias disponibles:");
                for (int i = 0; i < estrategiasDisponibles.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {estrategiasDisponibles[i]}");
                }
                
                // Seleccionar estrategia
                Console.Write("\nSeleccione estrategia (1-4) [2]: ");
                string inputEstrategia = Console.ReadLine()?.Trim();
                int seleccionEstrategia = string.IsNullOrEmpty(inputEstrategia) ? 2 : 
                    (int.TryParse(inputEstrategia, out int s) ? Math.Max(1, Math.Min(4, s)) : 2);
                
                var tipoEstrategia = (FabricaEstrategias.TipoEstrategia)(seleccionEstrategia - 1);
                
                // Seleccionar percentil
                Console.Write("Ingrese percentil para Zipf (1-30%) [15]: ");
                string inputPercentil = Console.ReadLine()?.Trim();
                int percentil = string.IsNullOrEmpty(inputPercentil) ? 15 : 
                    (int.TryParse(inputPercentil, out int p) ? Math.Max(1, Math.Min(30, p)) : 15);
                
                Console.WriteLine($"🎯 Configuración seleccionada:");
                Console.WriteLine($"   📊 Percentil: {percentil}%");
                Console.WriteLine($"   🔧 Estrategia: {tipoEstrategia}");
                Console.WriteLine();
                
                return await CrearIndiceDesdeDirectorio(rutaDirectorio, percentil, tipoEstrategia);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en configuración personalizada: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Modificar configuración de Zipf en índice existente
        /// </summary>
        public async Task<bool> ModificarZipf(int nuevoPercentil, FabricaEstrategias.TipoEstrategia nuevaEstrategia)
        {
            try
            {
                if (IndiceEstaVacio())
                {
                    Console.WriteLine("❌ No hay índice cargado para modificar");
                    return false;
                }

                Console.WriteLine($"🔄 Modificando configuración de Zipf...");
                await indice.ModificarZipf(nuevoPercentil, nuevaEstrategia);

                // Recrear buscador
                buscador = new BuscadorVectorial(indice);

                Console.WriteLine("✅ Configuración de Zipf modificada");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error modificando Zipf: {ex.Message}");
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
        /// Guardar índice en archivo
        /// </summary>
        public bool GuardarIndice(string rutaArchivo)
        {
            try
            {
                Console.WriteLine("💾 Guardando índice con configuración Zipf...");
                indice.GuardarEnArchivoBinario(rutaArchivo);
                
                var stats = ObtenerEstadisticas();
                Console.WriteLine($"✅ Índice guardado exitosamente");
                Console.WriteLine($"📊 Configuración Zipf preservada: {stats.PercentilZipf}%");
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

                buscador = new BuscadorVectorial(indice);

                var stats = ObtenerEstadisticas();
                Console.WriteLine("✅ Índice cargado - Vector ordenado restaurado");
                if (stats.ZipfAplicado)
                {
                    Console.WriteLine($"🔥 Configuración Zipf restaurada: {stats.PercentilZipf}%");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error cargando índice: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtener estadísticas con información de Zipf
        /// </summary>
        public EstadisticasIndiceConZipf ObtenerEstadisticas()
        {
            try
            {
                return indice.ObtenerEstadisticas();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error obteniendo estadísticas: {ex.Message}");
                return new EstadisticasIndiceConZipf
                {
                    CantidadDocumentos = 0,
                    CantidadTerminos = 0,
                    IndiceOrdenado = false,
                    MemoriaEstimadaKB = 0,
                    PromedioTerminosPorDocumento = 0.0,
                    ZipfAplicado = false,
                    PercentilZipf = 0,
                    EstrategiaZipf = "No aplicada"
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
        /// Validar integridad del sistema incluyendo Zipf
        /// </summary>
        public ResultadoValidacionConZipf ValidarIntegridad()
        {
            var resultado = new ResultadoValidacionConZipf();

            try
            {
                var stats = ObtenerEstadisticas();

                resultado.IndiceNoVacio = stats.CantidadDocumentos > 0;
                resultado.Vector = stats.IndiceOrdenado;
                resultado.BuscadorFuncional = buscador != null;
                resultado.EstructurasConsistentes = stats.CantidadDocumentos > 0 && stats.CantidadTerminos > 0;
                resultado.ZipfAplicado = stats.ZipfAplicado;
                resultado.PercentilZipf = stats.PercentilZipf;

                resultado.EsValido = resultado.IndiceNoVacio &&
                                     resultado.Vector &&
                                     resultado.BuscadorFuncional &&
                                     resultado.EstructurasConsistentes &&
                                     resultado.ZipfAplicado; // Zipf es obligatorio

                if (resultado.EsValido)
                {
                    resultado.Mensaje =
                        $"✅ Sistema válido: RadixSort + Zipf({stats.PercentilZipf}%) activos, {stats.CantidadTerminos} términos optimizados";
                }
                else
                {
                    var problemas = new List<string>();
                    if (!resultado.IndiceNoVacio) problemas.Add("índice vacío");
                    if (!resultado.Vector) problemas.Add("vector no ordenado");
                    if (!resultado.BuscadorFuncional) problemas.Add("buscador no funcional");
                    if (!resultado.EstructurasConsistentes) problemas.Add("estructuras inconsistentes");
                    if (!resultado.ZipfAplicado) problemas.Add("Ley de Zipf no aplicada");

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

        /// <summary>
        /// Aplicar Zipf a índice existente (método directo)
        /// </summary>
        public async Task<bool> AplicarZipfDirecto(int percentil, FabricaEstrategias.TipoEstrategia estrategia)
        {
            try
            {
                if (IndiceEstaVacio())
                {
                    Console.WriteLine("❌ No hay índice para aplicar Zipf");
                    return false;
                }

                Console.WriteLine($"🔥 Aplicando Ley de Zipf: {percentil}% - {estrategia}");
                await indice.AplicarLeyDeZipf(estrategia);
                
                // Recrear buscador
                buscador = new BuscadorVectorial(indice);
                
                Console.WriteLine("✅ Ley de Zipf aplicada exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error aplicando Zipf: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtener información técnica del sistema
        /// </summary>
        public string ObtenerInformacionTecnica()
        {
            try
            {
                var stats = ObtenerEstadisticas();
                return $@"
🏗️ INFORMACIÓN TÉCNICA DEL SISTEMA
===================================
📊 Arquitectura: Índice Invertido con Vector personalizado
⚡ Algoritmo de ordenamiento: RadixSort O(n×k)
🔍 Búsqueda de términos: Binaria O(log n)
📈 Cálculo de relevancia: TF-IDF vectorial
🎯 Similitud de documentos: Coseno vectorial
🔥 Optimización: Ley de Zipf {(stats.ZipfAplicado ? $"({stats.PercentilZipf}%)" : "No aplicada")}
💾 Almacenamiento: Serialización binaria optimizada
🔄 Patrones de diseño: Singleton, Strategy, Iterator
📐 Complejidad de búsqueda: O(log n + k) donde k = documentos relevantes
⚡ Complejidad de creación: O(n×m×log m) donde n=docs, m=términos
";
            }
            catch (Exception ex)
            {
                return $"❌ Error obteniendo información técnica: {ex.Message}";
            }
        }
    }

    /// <summary>
    /// Resultado de validación del sistema con información de Zipf
    /// </summary>
    public class ResultadoValidacionConZipf
    {
        public bool EsValido { get; set; }
        public bool IndiceNoVacio { get; set; }
        public bool Vector { get; set; }
        public bool BuscadorFuncional { get; set; }
        public bool EstructurasConsistentes { get; set; }
        public bool ZipfAplicado { get; set; }
        public int PercentilZipf { get; set; }
        public string Mensaje { get; set; } = "";

        public override string ToString()
        {
            var estado = EsValido ? "VÁLIDO" : "INVÁLIDO";
            return $"🔍 Sistema: {estado}\n" +
                   $"   📊 Índice poblado: {(IndiceNoVacio ? "✅" : "❌")}\n" +
                   $"   🔤 Vector ordenado: {(Vector ? "✅" : "❌")}\n" +
                   $"   🎯 Buscador activo: {(BuscadorFuncional ? "✅" : "❌")}\n" +
                   $"   🏗️ Estructuras OK: {(EstructurasConsistentes ? "✅" : "❌")}\n" +
                   $"   🔥 Ley de Zipf: {(ZipfAplicado ? $"✅({PercentilZipf}%)" : "❌")}\n" +
                   $"📝 {Mensaje}";
        }
    }
}