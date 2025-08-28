using PruebaRider.Estructura.Nodo;
using PruebaRider.Servicios;
using PruebaRider.Strategy;

namespace PruebaRider.UI
{
    /// <summary>
    /// Interfaz simplificada - Solo funciones esenciales
    /// Directorio fijo, menú claro, operaciones básicas
    /// </summary>
    public class Interfaz
    {
        private readonly GestorIndice gestor;
        private readonly string DIRECTORIO_DOCUMENTOS = @"C:\Users\ignab\OneDrive\Documents\Estructuras de datos\Documentos";
        private readonly string ARCHIVO_INDICE = @"indice.bin";

        public Interfaz()
        {
            gestor = GestorIndice.ObtenerInstancia();
        }

        /// <summary>
        /// Menú principal - Flujo completo
        /// </summary>
        public async Task MenuPrincipalAsync()
        {
            Console.Clear();
            MostrarTitulo();

            // Intentar inicializar automáticamente
            await InicializarAutomatico();

            // Menú principal
            while (true)
            {
                MostrarMenu();
                string opcion = Console.ReadLine() ?? "";

                switch (opcion.ToLower())
                {
                    case "1":
                        await RealizarBusqueda();
                        break;
                    case "2":
                        await CrearIndice();
                        break;
                    case "3":
                        AplicarZipf();
                        break;
                    case "4":
                        GuardarCargarIndice();
                        break;
                    case "5":
                        MostrarEstadisticas();
                        break;
                    case "0":
                    case "salir":
                        Console.WriteLine("\n👋 ¡Hasta luego!");
                        return;
                    default:
                        Console.WriteLine("❌ Opción no válida");
                        break;
                }

                Console.WriteLine("\n⏸️  Presione Enter para continuar...");
                Console.ReadLine();
                Console.Clear();
                MostrarTitulo();
            }
        }

        private void MostrarTitulo()
        {
            Console.WriteLine("╔══════════════════════════════════════════╗");
            Console.WriteLine("║        ÍNDICE INVERTIDO - TF-IDF         ║");
            Console.WriteLine("║      Estructura de Datos Avanzada       ║");
            Console.WriteLine("╚══════════════════════════════════════════╝");
            Console.WriteLine($"📁 Directorio: {DIRECTORIO_DOCUMENTOS}");
            Console.WriteLine();
        }

        private void MostrarMenu()
        {
            var estado = gestor.IndiceEstaVacio() ? "❌ Sin índice" : "✅ Índice cargado";
            
            Console.WriteLine($"Estado actual: {estado}");
            Console.WriteLine();
            Console.WriteLine("🔍 1. Buscar documentos");
            Console.WriteLine("🔨 2. Crear/recrear índice");
            Console.WriteLine("⚡ 3. Aplicar Ley de Zipf");
            Console.WriteLine("💾 4. Guardar/cargar índice");
            Console.WriteLine("📊 5. Ver estadísticas");
            Console.WriteLine("🚪 0. Salir");
            Console.Write("\n➤ Opción: ");
        }

        /// <summary>
        /// Inicialización automática al arrancar
        /// </summary>
        private async Task InicializarAutomatico()
        {
            Console.WriteLine("🔄 Inicializando...");

            // Verificar directorio
            if (!Directory.Exists(DIRECTORIO_DOCUMENTOS))
            {
                Console.WriteLine($"❌ Directorio no encontrado: {DIRECTORIO_DOCUMENTOS}");
                Console.WriteLine("📝 Cree el directorio y agregue archivos .txt");
                return;
            }

            // Intentar cargar índice existente
            if (File.Exists(ARCHIVO_INDICE))
            {
                Console.WriteLine("📂 Cargando índice existente...");
                if (gestor.CargarIndice(ARCHIVO_INDICE))
                {
                    Console.WriteLine("✅ Índice cargado correctamente");
                    return;
                }
                else
                {
                    Console.WriteLine("⚠️  Error al cargar. Creando nuevo...");
                }
            }

            // Crear nuevo índice
            Console.WriteLine("🔨 Creando nuevo índice...");
            if (await gestor.CrearIndiceDesdeDirectorio(DIRECTORIO_DOCUMENTOS))
            {
                Console.WriteLine("✅ Índice creado exitosamente");
                gestor.GuardarIndice(ARCHIVO_INDICE);
                Console.WriteLine($"💾 Guardado en {ARCHIVO_INDICE}");
            }
            else
            {
                Console.WriteLine("❌ Error al crear índice");
            }
        }

        /// <summary>
        /// Realizar búsqueda - Función principal
        /// </summary>
        private async Task RealizarBusqueda()
        {
            if (gestor.IndiceEstaVacio())
            {
                Console.WriteLine("❌ No hay índice. Creando automáticamente...");
                await CrearIndice();
                return;
            }

            Console.WriteLine("\n🔍 === BÚSQUEDA DE DOCUMENTOS ===");
            Console.WriteLine("💡 Ejemplos: 'algoritmo', 'estructura datos', 'búsqueda'");
            Console.Write("➤ Ingrese su consulta: ");
            
            string consulta = Console.ReadLine() ?? "";
            
            if (string.IsNullOrWhiteSpace(consulta))
            {
                Console.WriteLine("❌ Consulta vacía");
                return;
            }

            Console.WriteLine($"\n🔍 Buscando: '{consulta}'");
            Console.WriteLine(new string('─', 50));

            // Búsqueda TF-IDF
            Console.WriteLine("\n📊 RESULTADOS TF-IDF:");
            var resultadosTfIdf = gestor.BuscarTfIdf(consulta);
            MostrarResultadosTfIdf(resultadosTfIdf);

            Console.WriteLine(new string('─', 50));

            // Búsqueda Vectorial  
            Console.WriteLine("\n📊 RESULTADOS VECTORIALES (Similitud Coseno):");
            var resultadosVectoriales = gestor.BuscarConSimilitudCoseno(consulta);
            MostrarResultadosVectoriales(resultadosVectoriales);
        }

        private void MostrarResultadosTfIdf(ListaDobleEnlazada<ResultadoBusqueda> resultados)
        {
            if (resultados.Count == 0)
            {
                Console.WriteLine("🔍 No se encontraron documentos relevantes");
                return;
            }

            Console.WriteLine($"📁 {resultados.Count} documento(s) encontrado(s):");
            
            var iterador = new Iterador<ResultadoBusqueda>(resultados);
            int posicion = 1;
            
            while (iterador.Siguiente() && posicion <= 5)
            {
                var resultado = iterador.Current;
                Console.WriteLine($"{posicion}. {Path.GetFileName(resultado.Documento.Ruta)}");
                Console.WriteLine($"   📊 Puntuación: {resultado.Score:F3}");
                posicion++;
            }

            if (resultados.Count > 5)
                Console.WriteLine($"   ... y {resultados.Count - 5} más");
        }

        private void MostrarResultadosVectoriales(ListaDobleEnlazada<ResultadoBusquedaVectorial> resultados)
        {
            if (resultados.Count == 0)
            {
                Console.WriteLine("🔍 No se encontraron documentos relevantes");
                return;
            }

            Console.WriteLine($"📁 {resultados.Count} documento(s) encontrado(s):");
            
            var iterador = new Iterador<ResultadoBusquedaVectorial>(resultados);
            int posicion = 1;
            
            while (iterador.Siguiente() && posicion <= 5)
            {
                var resultado = iterador.Current;
                Console.WriteLine($"{posicion}. {Path.GetFileName(resultado.Documento.Ruta)}");
                Console.WriteLine($"   📊 Similitud: {resultado.SimilitudCoseno:F3} ({resultado.SimilitudCoseno * 100:F1}%)");
                posicion++;
            }

            if (resultados.Count > 5)
                Console.WriteLine($"   ... y {resultados.Count - 5} más");
        }

        /// <summary>
        /// Crear o recrear índice
        /// </summary>
        private async Task CrearIndice()
        {
            Console.WriteLine("\n🔨 === CREAR ÍNDICE ===");
            
            if (!gestor.IndiceEstaVacio())
            {
                Console.Write("⚠️  Ya existe un índice. ¿Recrear? (s/N): ");
                string confirmar = Console.ReadLine()?.ToLower() ?? "";
                if (confirmar != "s" && confirmar != "si")
                {
                    Console.WriteLine("❌ Operación cancelada");
                    return;
                }
            }

            if (!Directory.Exists(DIRECTORIO_DOCUMENTOS))
            {
                Console.WriteLine($"❌ Directorio no encontrado: {DIRECTORIO_DOCUMENTOS}");
                return;
            }

            var archivos = Directory.GetFiles(DIRECTORIO_DOCUMENTOS, "*.txt");
            if (archivos.Length == 0)
            {
                Console.WriteLine("❌ No se encontraron archivos .txt");
                return;
            }

            Console.WriteLine($"📂 Procesando {archivos.Length} archivo(s)...");
            
            bool exito = await gestor.CrearIndiceDesdeDirectorio(DIRECTORIO_DOCUMENTOS);
            
            if (exito)
            {
                Console.WriteLine("✅ ¡Índice creado exitosamente!");
                gestor.GuardarIndice(ARCHIVO_INDICE);
                Console.WriteLine($"💾 Guardado automáticamente");
                MostrarEstadisticas();
            }
            else
            {
                Console.WriteLine("❌ Error al crear índice");
            }
        }

        /// <summary>
        /// Aplicar Ley de Zipf
        /// </summary>
        private void AplicarZipf()
        {
            if (gestor.IndiceEstaVacio())
            {
                Console.WriteLine("❌ No hay índice para optimizar");
                return;
            }

            Console.WriteLine("\n⚡ === LEY DE ZIPF ===");
            Console.WriteLine("Eliminar términos para optimizar el índice:");
            Console.WriteLine("1. Términos muy frecuentes (stopwords)");
            Console.WriteLine("2. Términos muy raros (ruido)");
            Console.Write("➤ Opción (1 o 2): ");
            
            string opcion = Console.ReadLine() ?? "";
            bool eliminarFrecuentes = opcion == "1";

            Console.Write("➤ Porcentaje a eliminar (10-30 recomendado): ");
            if (!int.TryParse(Console.ReadLine(), out int percentil) || percentil < 1 || percentil > 99)
            {
                Console.WriteLine("❌ Porcentaje inválido");
                return;
            }

            var statsAntes = gestor.ObtenerEstadisticas();
            Console.WriteLine($"\n📊 Términos antes: {statsAntes.CantidadTerminos}");

            try
            {
                gestor.AplicarLeyZipf(percentil, eliminarFrecuentes);
                
                var statsDespues = gestor.ObtenerEstadisticas();
                int eliminados = statsAntes.CantidadTerminos - statsDespues.CantidadTerminos;
                
                Console.WriteLine("✅ Ley de Zipf aplicada");
                Console.WriteLine($"📊 Términos eliminados: {eliminados}");
                Console.WriteLine($"📊 Términos restantes: {statsDespues.CantidadTerminos}");
                
                gestor.GuardarIndice(ARCHIVO_INDICE);
                Console.WriteLine("💾 Cambios guardados automáticamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Guardar y cargar índice
        /// </summary>
        private void GuardarCargarIndice()
        {
            Console.WriteLine("\n💾 === GESTIÓN DE ARCHIVOS ===");
            Console.WriteLine("1. Guardar índice actual");
            Console.WriteLine("2. Cargar índice desde archivo");
            Console.Write("➤ Opción (1 o 2): ");
            
            string opcion = Console.ReadLine() ?? "";

            if (opcion == "1")
            {
                if (gestor.IndiceEstaVacio())
                {
                    Console.WriteLine("❌ No hay índice para guardar");
                    return;
                }

                Console.Write($"➤ Nombre archivo ({ARCHIVO_INDICE}): ");
                string archivo = Console.ReadLine() ?? "";
                
                if (string.IsNullOrWhiteSpace(archivo))
                    archivo = ARCHIVO_INDICE;

                if (gestor.GuardarIndice(archivo))
                {
                    Console.WriteLine($"✅ Índice guardado en {archivo}");
                    var fileInfo = new FileInfo(archivo);
                    Console.WriteLine($"📊 Tamaño: {fileInfo.Length / 1024.0:F1} KB");
                }
                else
                {
                    Console.WriteLine("❌ Error al guardar");
                }
            }
            else if (opcion == "2")
            {
                Console.Write($"➤ Archivo a cargar ({ARCHIVO_INDICE}): ");
                string archivo = Console.ReadLine() ?? "";
                
                if (string.IsNullOrWhiteSpace(archivo))
                    archivo = ARCHIVO_INDICE;

                if (!File.Exists(archivo))
                {
                    Console.WriteLine($"❌ Archivo no encontrado: {archivo}");
                    return;
                }

                if (gestor.CargarIndice(archivo))
                {
                    Console.WriteLine($"✅ Índice cargado desde {archivo}");
                    MostrarEstadisticas();
                }
                else
                {
                    Console.WriteLine("❌ Error al cargar");
                }
            }
            else
            {
                Console.WriteLine("❌ Opción inválida");
            }
        }

        /// <summary>
        /// Mostrar estadísticas del índice
        /// </summary>
        private void MostrarEstadisticas()
        {
            if (gestor.IndiceEstaVacio())
            {
                Console.WriteLine("❌ No hay índice cargado");
                return;
            }

            Console.WriteLine("\n📊 === ESTADÍSTICAS ===");
            var stats = gestor.ObtenerEstadisticas();
            Console.WriteLine(stats.ToString());
            
            if (File.Exists(ARCHIVO_INDICE))
            {
                var fileInfo = new FileInfo(ARCHIVO_INDICE);
                Console.WriteLine($"💾 Archivo índice: {fileInfo.Length / 1024.0:F1} KB");
            }
        }
    }
}