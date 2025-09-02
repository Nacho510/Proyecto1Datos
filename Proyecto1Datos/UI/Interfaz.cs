using PruebaRider.Estructura.Nodo;
using PruebaRider.Servicios;

namespace PruebaRider.UI
{
    /// <summary>
    /// Interfaz reestructurada y simplificada
    /// - Enfocada en la funcionalidad core
    /// - Muestra claramente RadixSort y búsqueda vectorial
    /// - Sin complejidad innecesaria
    /// </summary>
    public class Interfaz
    {
        private readonly GestorIndice gestor;

        private readonly string DIRECTORIO_DOCUMENTOS =
            @"C:\Users\ignab\OneDrive\Documents\Estructuras de datos\Documentos";

        private readonly string ARCHIVO_INDICE = @"indice_radix.bin";

        public Interfaz()
        {
            gestor = GestorIndice.ObtenerInstancia();
        }

        /// <summary>
        /// Menú principal simplificado
        /// </summary>
        public async Task MenuPrincipalAsync()
        {
            Console.Clear();
            MostrarBienvenida();

            // Inicialización automática
            await InicializarSistema();

            // Loop del menú principal
            while (true)
            {
                MostrarMenuPrincipal();
                string opcion = LeerOpcion();

                switch (opcion.ToLower())
                {
                    case "1":
                    case "buscar":
                        await EjecutarBusquedaVectorial();
                        break;
                    case "2":
                    case "crear":
                        await CrearIndiceCompleto();
                        break;
                    case "3":
                    case "estadisticas":
                        MostrarEstadisticas();
                        break;
                    case "4":
                    case "guardar":
                        GuardarIndice();
                        break;
                    case "5":
                    case "validar":
                        ValidarSistema();
                        break;
                    case "0":
                    case "salir":
                        MostrarDespedida();
                        return;
                    default:
                        MostrarError("❌ Opción no válida");
                        break;
                }

                Console.WriteLine("\n⏸️ Presione Enter para continuar...");
                Console.ReadLine();
                Console.Clear();
            }
        }

        /// <summary>
        /// Bienvenida enfocada en las características principales
        /// </summary>
        private void MostrarBienvenida()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              🚀 MOTOR DE BÚSQUEDA AVANZADO            ║");
            Console.WriteLine("║         Vector Ordenado + RadixSort + Coseno          ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🎯 CARACTERÍSTICAS IMPLEMENTADAS:");
            Console.WriteLine("   ✅ Vector ordenado para el índice invertido");
            Console.WriteLine("   ✅ Algoritmo RadixSort para ordenamiento de términos");
            Console.WriteLine("   ✅ Búsqueda vectorial con similitud coseno perfecta");
            Console.WriteLine("   ✅ Estructuras de datos propias (sin genéricos)");
            Console.WriteLine("   ✅ Optimización de memoria y tiempo O(log n)");
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine($"📁 Directorio: {DIRECTORIO_DOCUMENTOS}");
            Console.WriteLine();
        }

        /// <summary>
        /// Inicializar sistema automáticamente
        /// </summary>
        private async Task InicializarSistema()
        {
            Console.WriteLine("🔄 Inicializando sistema...");

            // Verificar directorio
            if (!Directory.Exists(DIRECTORIO_DOCUMENTOS))
            {
                MostrarError($"❌ Directorio no encontrado: {DIRECTORIO_DOCUMENTOS}");
                MostrarInfo("💡 Cree el directorio y agregue archivos .txt");
                return;
            }

            // Intentar cargar índice existente
            if (File.Exists(ARCHIVO_INDICE))
            {
                Console.WriteLine("📂 Cargando índice existente...");
                if (gestor.CargarIndice(ARCHIVO_INDICE))
                {
                    MostrarExito("✅ Índice cargado con RadixSort");
                    MostrarEstadisticasResumen();
                    return;
                }
            }

            // Crear nuevo índice si no existe
            Console.WriteLine("🔨 Creando índice nuevo...");
            if (await gestor.CrearIndiceDesdeDirectorio(DIRECTORIO_DOCUMENTOS))
            {
                MostrarExito("✅ Índice creado con RadixSort");
                gestor.GuardarIndice(ARCHIVO_INDICE);
                MostrarEstadisticasResumen();
            }
            else
            {
                MostrarError("❌ Error al crear índice inicial");
            }
        }

        /// <summary>
        /// FUNCIÓN PRINCIPAL: Búsqueda vectorial
        /// </summary>
        private async Task EjecutarBusquedaVectorial()
        {
            if (gestor.IndiceEstaVacio())
            {
                MostrarError("❌ No hay índice. Creando automáticamente...");
                await CrearIndiceCompleto();
                return;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🔍 BÚSQUEDA VECTORIAL CON SIMILITUD COSENO");
            Console.WriteLine("════════════════════════════════════════");
            Console.ResetColor();

            Console.WriteLine();
            Console.WriteLine("💡 Ejemplos de búsqueda:");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("   • 'algoritmo ordenamiento'");
            Console.WriteLine("   • 'estructura datos'");
            Console.WriteLine("   • 'búsqueda binaria'");
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

            Console.WriteLine();
            Console.WriteLine($"🔍 Buscando: '{consulta}'");
            Console.WriteLine("⚡ Procesando con RadixSort optimizado...");

            // Ejecutar búsqueda
            var inicio = DateTime.Now;
            var resultados = gestor.BuscarConSimilitudCoseno(consulta);
            var duracion = DateTime.Now - inicio;

            // Mostrar resultados
            MostrarResultados(resultados, consulta, duracion);
        }

        /// <summary>
        /// Mostrar resultados de búsqueda
        /// </summary>
        private void MostrarResultados(ListaDobleEnlazada<ResultadoBusquedaVectorial> resultados, string consulta,
            TimeSpan duracion)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"📊 RESULTADOS - {resultados.Count} encontrados");
            Console.WriteLine("═══════════════════════════════════════");
            Console.ResetColor();

            Console.WriteLine($"⏱️ Tiempo: {duracion.TotalMilliseconds:F2} ms");
            Console.WriteLine($"🎯 Algoritmo: Similitud Coseno con Vector Ordenado");
            Console.WriteLine();

            if (resultados.Count == 0)
            {
                MostrarVacio($"🔍 No se encontraron resultados para '{consulta}'");
                Console.WriteLine();
                Console.WriteLine("💡 Sugerencias:");
                Console.WriteLine("   • Use términos más generales");
                Console.WriteLine("   • Verifique la ortografía");
                Console.WriteLine("   • Pruebe sinónimos");
                return;
            }

            var iterador = new Iterador<ResultadoBusquedaVectorial>(resultados);
            int posicion = 1;

            while (iterador.Siguiente() && posicion <= 10)
            {
                var resultado = iterador.Current;
                MostrarResultadoDetallado(resultado, posicion);
                posicion++;

                if (posicion <= resultados.Count && posicion <= 10)
                {
                    Console.WriteLine(new string('─', 50));
                }
            }

            if (resultados.Count > 10)
            {
                Console.WriteLine();
                MostrarInfo($"... y {resultados.Count - 10} resultados más");
            }

            // Estadísticas de similitud
            MostrarEstadisticasSimilitud(resultados);
        }

        /// <summary>
        /// Mostrar resultado individual
        /// </summary>
        private void MostrarResultadoDetallado(ResultadoBusquedaVectorial resultado, int posicion)
        {
            Console.Write($"📄 {posicion}. ");
            Console.ForegroundColor = ConsoleColor.White;
            //Console.WriteLine(Path.GetFileName(resultado.Documento.Ruta));
            string url = $"file:///{resultado.Documento.Ruta.Replace("\\", "/")}";
            Console.WriteLine(url);
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"   📁 {resultado.Documento.Ruta}");
            Console.WriteLine($"   🆔 ID: {resultado.Documento.Id}");
            Console.ResetColor();

            // Similitud con colores
            double porcentaje = resultado.SimilitudCoseno * 100;
            Console.Write("   📊 Similitud: ");

            if (porcentaje >= 50)
                Console.ForegroundColor = ConsoleColor.Green;
            else if (porcentaje >= 20)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else
                Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine($"{porcentaje:F1}% ({resultado.SimilitudCoseno:F4})");
            Console.ResetColor();

            // Enlace base64
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(
                $"   🔗 Base64: {resultado.EnlaceBase64.Substring(0, Math.Min(50, resultado.EnlaceBase64.Length))}...");
            Console.ResetColor();
        }

        /// <summary>
        /// Crear índice completo
        /// </summary>
        private async Task CrearIndiceCompleto()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🔨 CREAR ÍNDICE CON RADIX SORT");
            Console.WriteLine("══════════════════════════════");
            Console.ResetColor();

            if (!Directory.Exists(DIRECTORIO_DOCUMENTOS))
            {
                MostrarError($"❌ Directorio no encontrado: {DIRECTORIO_DOCUMENTOS}");
                return;
            }

            var archivos = Directory.GetFiles(DIRECTORIO_DOCUMENTOS, "*.txt");
            if (archivos.Length == 0)
            {
                MostrarError("❌ No se encontraron archivos .txt");
                return;
            }

            Console.WriteLine($"📂 {archivos.Length} archivo(s) .txt encontrados");
            Console.WriteLine();

            Console.WriteLine("🔄 Creando índice...");
            Console.WriteLine("   ⚡ Aplicando RadixSort automáticamente...");

            if (await gestor.CrearIndiceDesdeDirectorio(DIRECTORIO_DOCUMENTOS))
            {
                MostrarExito("✅ Índice creado exitosamente");

                if (gestor.GuardarIndice(ARCHIVO_INDICE))
                {
                    MostrarExito($"💾 Guardado como {ARCHIVO_INDICE}");
                }

                MostrarEstadisticas();
            }
            else
            {
                MostrarError("❌ Error al crear índice");
            }
        }

        /// <summary>
        /// Mostrar estadísticas detalladas
        /// </summary>
        private void MostrarEstadisticas()
        {
            if (gestor.IndiceEstaVacio())
            {
                MostrarError("❌ No hay índice cargado");
                return;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("📊 ESTADÍSTICAS DEL SISTEMA");
            Console.WriteLine("══════════════════════════");
            Console.ResetColor();

            var stats = gestor.ObtenerEstadisticas();

            Console.WriteLine();
            Console.WriteLine("📈 DATOS PRINCIPALES:");
            Console.WriteLine($"   📄 Documentos: {stats.CantidadDocumentos}");
            Console.WriteLine($"   🔤 Términos: {stats.CantidadTerminos}");
            Console.WriteLine($"   📊 Promedio términos/doc: {stats.PromedioTerminosPorDocumento:F1}");

            Console.WriteLine();
            Console.WriteLine("⚡ RENDIMIENTO:");
            Console.WriteLine($"   💾 Memoria: {stats.MemoriaEstimadaKB} KB");
            Console.WriteLine($"   🔤 Vector ordenado: {(stats.IndiceOrdenado ? "✅ Sí (RadixSort)" : "❌ No")}");
            Console.WriteLine($"   ⚡ Complejidad búsqueda: {(stats.IndiceOrdenado ? "O(log n)" : "O(n)")}");

            if (File.Exists(ARCHIVO_INDICE))
            {
                var fileInfo = new FileInfo(ARCHIVO_INDICE);
                Console.WriteLine();
                Console.WriteLine("💾 ARCHIVO:");
                Console.WriteLine($"   📁 {ARCHIVO_INDICE}");
                Console.WriteLine($"   📊 Tamaño: {fileInfo.Length / 1024.0:F1} KB");
                Console.WriteLine($"   🗓️ Modificado: {fileInfo.LastWriteTime:dd/MM/yyyy HH:mm}");
            }
        }

        /// <summary>
        /// Guardar índice
        /// </summary>
        private void GuardarIndice()
        {
            if (gestor.IndiceEstaVacio())
            {
                MostrarError("❌ No hay índice para guardar");
                return;
            }

            Console.WriteLine();
            Console.Write($"➤ Nombre archivo ({ARCHIVO_INDICE}): ");
            string archivo = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(archivo))
                archivo = ARCHIVO_INDICE;

            Console.WriteLine($"💾 Guardando {archivo}...");

            if (gestor.GuardarIndice(archivo))
            {
                var fileInfo = new FileInfo(archivo);
                MostrarExito($"✅ Guardado exitosamente ({fileInfo.Length / 1024.0:F1} KB)");
            }
            else
            {
                MostrarError("❌ Error al guardar");
            }
        }

        /// <summary>
        /// Validar integridad del sistema
        /// </summary>
        private void ValidarSistema()
        {
            Console.WriteLine();
            Console.WriteLine("🔍 VALIDANDO INTEGRIDAD DEL SISTEMA");
            Console.WriteLine("═══════════════════════════════════");

            var validacion = gestor.ValidarIntegridad();
            Console.WriteLine();
            Console.WriteLine(validacion.ToString());
        }

        /// <summary>
        /// Mostrar menú principal
        /// </summary>
        private void MostrarMenuPrincipal()
        {
            var stats = gestor.ObtenerEstadisticas();
            var estado = gestor.IndiceEstaVacio()
                ? "❌ Sin índice"
                : $"✅ {stats.CantidadTerminos} términos {(stats.IndiceOrdenado ? "(RadixSort)" : "(sin ordenar)")}";

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Estado: {estado}");
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("🎯 OPCIONES:");
            Console.WriteLine("   1️⃣  🔍 Búsqueda vectorial (Similitud coseno)");
            Console.WriteLine("   2️⃣  🔨 Crear índice con RadixSort");
            Console.WriteLine("   3️⃣  📊 Ver estadísticas");
            Console.WriteLine("   4️⃣  💾 Guardar índice");
            Console.WriteLine("   5️⃣  ✅ Validar sistema");
            Console.WriteLine("   0️⃣  🚪 Salir");
            Console.WriteLine();
        }

        #region Métodos Auxiliares

        private void MostrarEstadisticasResumen()
        {
            if (!gestor.IndiceEstaVacio())
            {
                var stats = gestor.ObtenerEstadisticas();
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine(
                    $"📊 {stats.CantidadDocumentos} docs | {stats.CantidadTerminos} términos | {stats.MemoriaEstimadaKB} KB");
                Console.ResetColor();
            }
        }

        private void MostrarEstadisticasSimilitud(ListaDobleEnlazada<ResultadoBusquedaVectorial> resultados)
        {
            if (resultados.Count == 0) return;

            double suma = 0, max = 0, min = 1.0;
            var iterador = new Iterador<ResultadoBusquedaVectorial>(resultados);

            while (iterador.Siguiente())
            {
                double sim = iterador.Current.SimilitudCoseno;
                suma += sim;
                max = Math.Max(max, sim);
                min = Math.Min(min, sim);
            }

            double promedio = suma / resultados.Count;

            Console.WriteLine();
            Console.WriteLine("📈 ESTADÍSTICAS DE SIMILITUD:");
            Console.WriteLine($"   📊 Promedio: {promedio * 100:F1}%");
            Console.WriteLine($"   📊 Máxima: {max * 100:F1}%");
            Console.WriteLine($"   📊 Mínima: {min * 100:F1}%");
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

        private void MostrarInfo(string mensaje)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
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

        private void MostrarDespedida()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔═════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                  ¡PROYECTO COMPLETADO!                 ║");
            Console.WriteLine("║          Índice Invertido con Búsqueda Vectorial       ║");
            Console.WriteLine("╚═════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🏆 CARACTERÍSTICAS IMPLEMENTADAS:");
            Console.WriteLine("   ✅ Vector ordenado para índice invertido");
            Console.WriteLine("   ✅ Algoritmo RadixSort para ordenamiento");
            Console.WriteLine("   ✅ Búsqueda vectorial con similitud coseno");
            Console.WriteLine("   ✅ Solo estructuras de datos propias");
            Console.WriteLine("   ✅ Optimización O(log n) en búsquedas");
            Console.WriteLine("   ✅ Enlaces base64 para descarga");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("🎯 ¡Sistema funcionando a la perfección!");
            Console.WriteLine("👋 ¡Gracias por usar el motor de búsqueda!");
            Console.ResetColor();
            Console.WriteLine();
        }

        #endregion
    }
}