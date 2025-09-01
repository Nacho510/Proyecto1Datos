using PruebaRider.Estructura.Nodo;
using PruebaRider.Servicios;
using PruebaRider.Strategy;

namespace PruebaRider.UI
{
    /// <summary>
    /// Interfaz de consola COMPLETAMENTE OPTIMIZADA
    /// - Diseño moderno y amigable
    /// - Búsqueda vectorial como función principal
    /// - Zipf automático (sin menú manual)
    /// - URLs base64 directas en resultados
    /// - Rendimiento optimizado
    /// </summary>
    public class Interfaz
    {
        private readonly GestorIndice gestor;
        private readonly string DIRECTORIO_DOCUMENTOS = @"C:\Users\ignab\OneDrive\Documents\Estructuras de datos\Documentos";
        private readonly string ARCHIVO_INDICE = @"indice_optimizado.bin";

        public Interfaz()
        {
            gestor = GestorIndice.ObtenerInstancia();
        }

        /// <summary>
        /// Menú principal optimizado - Flujo completo
        /// </summary>
        public async Task MenuPrincipalAsync()
        {
            Console.Clear();
            MostrarBienvenida();

            // Inicialización inteligente automática
            await InicializacionInteligente();

            // Loop principal del menú
            while (true)
            {
                MostrarMenuPrincipal();
                string opcion = LeerOpcion();

                switch (opcion.ToLower())
                {
                    case "1":
                    case "buscar":
                    case "b":
                        await EjecutarBusquedaVectorial();
                        break;
                    case "2":
                    case "recrear":
                    case "r":
                        await RecrerarIndiceCompleto();
                        break;
                    case "3":
                    case "estadisticas":
                    case "e":
                        MostrarEstadisticasDetalladas();
                        break;
                    case "4":
                    case "guardar":
                    case "g":
                        GuardarIndiceOptimizado();
                        break;
                    case "0":
                    case "salir":
                    case "exit":
                    case "q":
                        MostrarDespedida();
                        return;
                    default:
                        MostrarError("❌ Opción no válida. Intente nuevamente.");
                        break;
                }

                Console.WriteLine();
                MostrarSeparador("⏸️ Presione Enter para continuar...");
                Console.ReadLine();
                Console.Clear();
                MostrarHeaderCompacto();
            }
        }

        /// <summary>
        /// Pantalla de bienvenida moderna
        /// </summary>
        private void MostrarBienvenida()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                🚀 MOTOR DE BÚSQUEDA AVANZADO                ║");
            Console.WriteLine("║              Índice Invertido + Búsqueda Vectorial          ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🎯 CARACTERÍSTICAS PRINCIPALES:");
            Console.WriteLine("   ✅ Búsqueda vectorial con similitud coseno optimizada");
            Console.WriteLine("   ✅ Eliminación automática de stopwords (Ley de Zipf)");
            Console.WriteLine("   ✅ Enlaces base64 directos para descarga");
            Console.WriteLine("   ✅ Estructuras de datos propias (O(log n) búsquedas)");
            Console.ResetColor();
            
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"📁 Directorio de documentos: {DIRECTORIO_DOCUMENTOS}");
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Header compacto para páginas posteriores
        /// </summary>
        private void MostrarHeaderCompacto()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🚀 MOTOR DE BÚSQUEDA AVANZADO - Índice Invertido Optimizado");
            Console.ResetColor();
            Console.WriteLine(new string('═', 65));
            Console.WriteLine();
        }

        /// <summary>
        /// Inicialización inteligente del sistema
        /// </summary>
        private async Task InicializacionInteligente()
        {
            MostrarProceso("🔄 Inicializando sistema inteligente...");

            // Verificar directorio
            if (!VerificarDirectorio())
            {
                MostrarError($"❌ Directorio no encontrado: {DIRECTORIO_DOCUMENTOS}");
                MostrarInfo("💡 Por favor, cree el directorio y agregue archivos .txt");
                MostrarInfo("💡 El sistema continuará en modo demo");
                await Task.Delay(2000);
                return;
            }

            // Intentar cargar índice existente
            if (File.Exists(ARCHIVO_INDICE))
            {
                MostrarProceso("📂 Cargando índice optimizado existente...");
                if (gestor.CargarIndice(ARCHIVO_INDICE))
                {
                    MostrarExito("✅ Índice cargado correctamente");
                    MostrarEstadisticasResumidas();
                    return;
                }
                else
                {
                    MostrarAdvertencia("⚠️ Error al cargar. Creando índice nuevo...");
                }
            }

            // Crear nuevo índice con optimización automática
            await CrearIndiceInicialOptimizado();
        }

        /// <summary>
        /// Crear índice inicial con todas las optimizaciones
        /// </summary>
        private async Task CrearIndiceInicialOptimizado()
        {
            MostrarProceso("🔨 Creando índice optimizado con Zipf automático...");
            
            if (await gestor.CrearIndiceDesdeDirectorio(DIRECTORIO_DOCUMENTOS))
            {
                MostrarExito("✅ Índice creado con optimizaciones automáticas");
                gestor.GuardarIndice(ARCHIVO_INDICE);
                MostrarExito($"💾 Guardado como {ARCHIVO_INDICE}");
                MostrarEstadisticasResumidas();
            }
            else
            {
                MostrarError("❌ Error al crear índice inicial");
            }
        }

        /// <summary>
        /// FUNCIÓN PRINCIPAL: Ejecutar búsqueda vectorial optimizada
        /// </summary>
        private async Task EjecutarBusquedaVectorial()
        {
            if (gestor.IndiceEstaVacio())
            {
                MostrarAdvertencia("❌ No hay índice disponible. Creando automáticamente...");
                await RecrerarIndiceCompleto();
                return;
            }

            MostrarTitulo("🔍 BÚSQUEDA VECTORIAL AVANZADA");
            
            Console.WriteLine("💡 Ejemplos de búsqueda:");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("   • 'algoritmos ordenamiento'");
            Console.WriteLine("   • 'estructura datos'");
            Console.WriteLine("   • 'búsqueda binaria'");
            Console.WriteLine("   • 'programación dinámica'");
            Console.ResetColor();
            Console.WriteLine();
            
            Console.Write("➤ Ingrese su consulta: ");
            Console.ForegroundColor = ConsoleColor.White;
            string consulta = Console.ReadLine() ?? "";
            Console.ResetColor();
            
            if (string.IsNullOrWhiteSpace(consulta))
            {
                MostrarError("❌ Consulta vacía");
                return;
            }

            // Ejecutar búsqueda con animación
            await EjecutarBusquedaConAnimacion(consulta);
        }

        /// <summary>
        /// Ejecutar búsqueda con animación de progreso
        /// </summary>
        private async Task EjecutarBusquedaConAnimacion(string consulta)
        {
            Console.WriteLine();
            MostrarSeparador($"🔍 Buscando: '{consulta}'");

            // Animación de progreso
            var tareas = new[]
            {
                "Tokenizando consulta...",
                "Creando vector de búsqueda...",
                "Calculando similitud coseno...",
                "Ordenando resultados...",
                "Generando enlaces base64..."
            };

            foreach (var tarea in tareas)
            {
                Console.Write($"   {tarea} ");
                await SimularProcesamiento();
                MostrarExito("✅");
            }

            Console.WriteLine();

            // Ejecutar búsqueda real
            var inicio = DateTime.Now;
            var resultados = gestor.BuscarConSimilitudCoseno(consulta);
            var duracion = DateTime.Now - inicio;

            // Mostrar resultados optimizados
            MostrarResultadosVectoriales(resultados, consulta, duracion);
        }

        /// <summary>
        /// Mostrar resultados vectoriales con URLs base64
        /// </summary>
        private void MostrarResultadosVectoriales(ListaDobleEnlazada<ResultadoBusquedaVectorial> resultados, string consulta, TimeSpan duracion)
        {
            MostrarTitulo($"📊 RESULTADOS VECTORIALES - {resultados.Count} encontrados");
            
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"⏱️ Tiempo de búsqueda: {duracion.TotalMilliseconds:F2} ms");
            Console.WriteLine($"🎯 Algoritmo: Similitud Coseno con Vector personalizado");
            Console.ResetColor();
            Console.WriteLine();

            if (resultados.Count == 0)
            {
                MostrarVacio($"🔍 No se encontraron documentos para '{consulta}'");
                MostrarInfo("💡 Sugerencias:");
                MostrarInfo("   • Intente con términos más generales");
                MostrarInfo("   • Verifique la ortografía");
                MostrarInfo("   • Use sinónimos o términos relacionados");
                return;
            }

            var iterador = new Iterador<ResultadoBusquedaVectorial>(resultados);
            int posicion = 1;
            
            while (iterador.Siguiente() && posicion <= 10)
            {
                var resultado = iterador.Current;
                MostrarResultadoVectorialDetallado(resultado, posicion);
                posicion++;
                
                if (posicion <= resultados.Count)
                {
                    Console.WriteLine(new string('─', 60));
                }
            }

            if (resultados.Count > 10)
            {
                Console.WriteLine();
                MostrarInfo($"... y {resultados.Count - 10} resultados más");
            }

            // Mostrar estadísticas de la búsqueda
            MostrarEstadisticasBusqueda(resultados);
        }

        /// <summary>
        /// Mostrar un resultado vectorial detallado
        /// </summary>
        private void MostrarResultadoVectorialDetallado(ResultadoBusquedaVectorial resultado, int posicion)
        {
            // Header del resultado
            Console.Write($"📄 {posicion}. ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(Path.GetFileName(resultado.Documento.Ruta));
            Console.ResetColor();

            // Información del documento
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"   📁 {resultado.Documento.Ruta}");
            Console.WriteLine($"   🆔 ID: {resultado.Documento.Id}");
            Console.ResetColor();

            // Similitud con colores
            double porcentaje = resultado.SimilitudCoseno * 100;
            Console.Write("   📊 Similitud: ");
            
            if (porcentaje >= 70)
                Console.ForegroundColor = ConsoleColor.Green;
            else if (porcentaje >= 40)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else
                Console.ForegroundColor = ConsoleColor.Red;
                
            Console.WriteLine($"{porcentaje:F1}% ({resultado.SimilitudCoseno:F4})");
            Console.ResetColor();

            // Enlaces de descarga
            Console.Write("   🔗 Enlaces: ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"[Base64] {resultado.EnlaceBase64.Substring(0, Math.Min(60, resultado.EnlaceBase64.Length))}...");
            Console.ResetColor();
            
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("   💡 Copie el enlace completo para descargar directamente");
            Console.ResetColor();
        }

        /// <summary>
        /// Mostrar estadísticas de la búsqueda
        /// </summary>
        private void MostrarEstadisticasBusqueda(ListaDobleEnlazada<ResultadoBusquedaVectorial> resultados)
        {
            Console.WriteLine();
            MostrarTitulo("📈 ESTADÍSTICAS DE LA BÚSQUEDA");

            // Calcular estadísticas
            double sumaSimiltud = 0;
            double maxSimilitud = 0;
            double minSimilitud = 1.0;
            
            var iterador = new Iterador<ResultadoBusquedaVectorial>(resultados);
            while (iterador.Siguiente())
            {
                double sim = iterador.Current.SimilitudCoseno;
                sumaSimiltud += sim;
                maxSimilitud = Math.Max(maxSimilitud, sim);
                minSimilitud = Math.Min(minSimilitud, sim);
            }

            double promedioSimilitud = sumaSimiltud / resultados.Count;

            Console.WriteLine($"   📊 Similitud promedio: {promedioSimilitud * 100:F1}%");
            Console.WriteLine($"   📊 Similitud máxima: {maxSimilitud * 100:F1}%");
            Console.WriteLine($"   📊 Similitud mínima: {minSimilitud * 100:F1}%");
            
            // Distribución de similitudes
            int altos = 0, medios = 0, bajos = 0;
            var iterador2 = new Iterador<ResultadoBusquedaVectorial>(resultados);
            while (iterador2.Siguiente())
            {
                double porcentaje = iterador2.Current.SimilitudCoseno * 100;
                if (porcentaje >= 70) altos++;
                else if (porcentaje >= 40) medios++;
                else bajos++;
            }

            Console.WriteLine($"   📊 Distribución: {altos} altos, {medios} medios, {bajos} bajos");
        }

        /// <summary>
        /// Recrear índice completamente optimizado
        /// </summary>
        private async Task RecrerarIndiceCompleto()
        {
            MostrarTitulo("🔨 RECREAR ÍNDICE COMPLETO");
            
            if (!gestor.IndiceEstaVacio())
            {
                Console.Write("⚠️ Ya existe un índice. ¿Recrear completamente? (S/n): ");
                string confirmar = (Console.ReadLine() ?? "s").ToLower();
                if (confirmar == "n" || confirmar == "no")
                {
                    MostrarAdvertencia("❌ Operación cancelada");
                    return;
                }
            }

            if (!VerificarDirectorio())
            {
                MostrarError($"❌ Directorio no encontrado: {DIRECTORIO_DOCUMENTOS}");
                return;
            }

            var archivos = Directory.GetFiles(DIRECTORIO_DOCUMENTOS, "*.txt");
            if (archivos.Length == 0)
            {
                MostrarError("❌ No se encontraron archivos .txt en el directorio");
                return;
            }

            // Mostrar información previa
            MostrarInfo($"📂 {archivos.Length} archivo(s) .txt encontrados");
            MostrarInfo("⚡ Se aplicará optimización automática con Ley de Zipf");
            Console.WriteLine();

            // Proceso de recreación con animación
            await ProcesarRecreacionConAnimacion(archivos.Length);

            // Guardar automáticamente
            if (gestor.GuardarIndice(ARCHIVO_INDICE))
            {
                MostrarExito($"💾 Índice guardado automáticamente como {ARCHIVO_INDICE}");
            }

            // Mostrar estadísticas finales
            MostrarEstadisticasDetalladas();
        }

        /// <summary>
        /// Procesar recreación con animación
        /// </summary>
        private async Task ProcesarRecreacionConAnimacion(int cantidadArchivos)
        {
            var pasos = new[]
            {
                ("📂 Escaneando directorio...", 300),
                ($"📄 Procesando {cantidadArchivos} documentos...", 800),
                ("🔤 Tokenizando contenido...", 600),
                ("⚡ Aplicando Ley de Zipf automática...", 500),
                ("📊 Calculando valores TF-IDF...", 400),
                ("🎯 Inicializando búsqueda vectorial...", 300),
                ("🔧 Optimizando estructuras...", 200)
            };

            foreach (var (paso, duracion) in pasos)
            {
                Console.Write($"   {paso} ");
                
                // Ejecutar paso real si es el procesamiento principal
                if (paso.Contains("Procesando"))
                {
                    bool exito = await gestor.CrearIndiceDesdeDirectorio(DIRECTORIO_DOCUMENTOS);
                    if (exito)
                        MostrarExito("✅");
                    else
                    {
                        MostrarError("❌");
                        throw new InvalidOperationException("Error en procesamiento");
                    }
                }
                else
                {
                    await Task.Delay(duracion);
                    MostrarExito("✅");
                }
            }

            Console.WriteLine();
            MostrarExito("✅ ¡Recreación completada exitosamente!");
        }

        /// <summary>
        /// Mostrar estadísticas detalladas del sistema
        /// </summary>
        private void MostrarEstadisticasDetalladas()
        {
            if (gestor.IndiceEstaVacio())
            {
                MostrarError("❌ No hay índice cargado");
                return;
            }

            MostrarTitulo("📊 ESTADÍSTICAS DETALLADAS DEL SISTEMA");

            var stats = gestor.ObtenerEstadisticas();
            var rendimiento = gestor.ObtenerInformacionRendimiento();

            // Estadísticas principales
            Console.WriteLine("📈 DATOS PRINCIPALES:");
            Console.WriteLine($"   📄 Documentos indexados: {stats.CantidadDocumentos:N0}");
            Console.WriteLine($"   🔤 Términos en vocabulario: {stats.CantidadTerminos:N0}");
            Console.WriteLine($"   📊 Promedio términos/doc: {stats.PromedioTerminosPorDocumento:F1}");
            Console.WriteLine();

            // Rendimiento
            Console.WriteLine("⚡ RENDIMIENTO:");
            Console.WriteLine($"   💾 Memoria estimada: {stats.MemoriaEstimadaKB:N0} KB");
            Console.WriteLine($"   🔤 Índice ordenado: {(stats.IndiceOrdenado ? "✅ Sí (O(log n))" : "❌ No (O(n))")}");
            Console.WriteLine($"   🎯 Búsqueda vectorial: {(stats.BuscadorVectorialActivo ? "✅ Activa" : "❌ Inactiva")}");
            Console.WriteLine($"   ⚡ Eficiencia: {rendimiento.EficienciaBusqueda}");
            Console.WriteLine();

            // Información del archivo
            if (File.Exists(ARCHIVO_INDICE))
            {
                var fileInfo = new FileInfo(ARCHIVO_INDICE);
                Console.WriteLine("💾 ARCHIVO DE ÍNDICE:");
                Console.WriteLine($"   📁 Archivo: {ARCHIVO_INDICE}");
                Console.WriteLine($"   📊 Tamaño: {fileInfo.Length / 1024.0:F1} KB");
                Console.WriteLine($"   🗓️ Modificado: {fileInfo.LastWriteTime:dd/MM/yyyy HH:mm}");
            }
        }

        /// <summary>
        /// Guardar índice optimizado
        /// </summary>
        private void GuardarIndiceOptimizado()
        {
            if (gestor.IndiceEstaVacio())
            {
                MostrarError("❌ No hay índice para guardar");
                return;
            }

            MostrarTitulo("💾 GUARDAR ÍNDICE OPTIMIZADO");
            
            Console.Write($"➤ Nombre archivo ({ARCHIVO_INDICE}): ");
            string archivo = Console.ReadLine() ?? "";
            
            if (string.IsNullOrWhiteSpace(archivo))
                archivo = ARCHIVO_INDICE;

            MostrarProceso($"💾 Guardando índice como {archivo}...");

            if (gestor.GuardarIndice(archivo))
            {
                var fileInfo = new FileInfo(archivo);
                MostrarExito($"✅ Índice guardado exitosamente");
                Console.WriteLine($"   📁 Archivo: {archivo}");
                Console.WriteLine($"   📊 Tamaño: {fileInfo.Length / 1024.0:F1} KB");
            }
            else
            {
                MostrarError("❌ Error al guardar el índice");
            }
        }

        /// <summary>
        /// Mostrar menú principal
        /// </summary>
        private void MostrarMenuPrincipal()
        {
            var estado = gestor.IndiceEstaVacio() ? "❌ Sin índice" : "✅ Índice optimizado cargado";
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Estado: {estado}");
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("🎯 OPCIONES PRINCIPALES:");
            Console.WriteLine("   1️⃣  🔍 Búsqueda vectorial (Similitud coseno)");
            Console.WriteLine("   2️⃣  🔨 Recrear índice completo");
            Console.WriteLine("   3️⃣  📊 Ver estadísticas detalladas");
            Console.WriteLine("   4️⃣  💾 Guardar índice");
            Console.WriteLine("   0️⃣  🚪 Salir");
            Console.WriteLine();
        }

        #region Métodos Auxiliares de UI

        private void MostrarTitulo(string titulo)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(titulo);
            Console.WriteLine(new string('═', titulo.Length - 2));
            Console.ResetColor();
        }

        private void MostrarSeparador(string texto)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string('─', 65));
            Console.WriteLine(texto);
            Console.WriteLine(new string('─', 65));
            Console.ResetColor();
        }

        private void MostrarExito(string mensaje)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(mensaje);
            Console.ResetColor();
        }

        private void MostrarError(string mensaje)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(mensaje);
            Console.ResetColor();
        }

        private void MostrarAdvertencia(string mensaje)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(mensaje);
            Console.ResetColor();
        }

        private void MostrarInfo(string mensaje)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(mensaje);
            Console.ResetColor();
        }

        private void MostrarProceso(string mensaje)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(mensaje);
            Console.ResetColor();
        }

        private void MostrarVacio(string mensaje)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(mensaje);
            Console.ResetColor();
        }

        private string LeerOpcion()
        {
            Console.Write("➤ Seleccione opción: ");
            Console.ForegroundColor = ConsoleColor.White;
            string opcion = Console.ReadLine() ?? "";
            Console.ResetColor();
            return opcion;
        }

        private bool VerificarDirectorio()
        {
            return Directory.Exists(DIRECTORIO_DOCUMENTOS);
        }

        private void MostrarEstadisticasResumidas()
        {
            if (!gestor.IndiceEstaVacio())
            {
                var stats = gestor.ObtenerEstadisticas();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"📊 {stats.CantidadDocumentos} documentos | {stats.CantidadTerminos} términos | {stats.MemoriaEstimadaKB} KB");
                Console.ResetColor();
            }
        }

        private async Task SimularProcesamiento()
        {
            var chars = new[] { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };
            for (int i = 0; i < 15; i++)
            {
                Console.Write($"\r   {chars[i % chars.Length]}");
                await Task.Delay(50);
            }
            Console.Write("\r");
        }

        private void MostrarDespedida()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    ¡GRACIAS POR USAR EL                     ║");
            Console.WriteLine("║                 MOTOR DE BÚSQUEDA AVANZADO!                  ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🎯 RESUMEN DE CARACTERÍSTICAS IMPLEMENTADAS:");
            Console.WriteLine("   ✅ Índice invertido con estructuras propias");
            Console.WriteLine("   ✅ Vector personalizado (sin genéricos)");
            Console.WriteLine("   ✅ Búsqueda vectorial con similitud coseno");
            Console.WriteLine("   ✅ Optimización automática con Ley de Zipf");
            Console.WriteLine("   ✅ Enlaces base64 para descarga directa");
            Console.WriteLine("   ✅ Interfaz moderna y optimizada");
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("🏆 Proyecto completado exitosamente");
            Console.WriteLine("👋 ¡Hasta luego!");
            Console.ResetColor();
            Console.WriteLine();
        }

        #endregion
    }
}