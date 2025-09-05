using PruebaRider.Servicios;
using PruebaRider.Estructura.Nodo;

namespace PruebaRider.UI
{
    /// <summary>
    /// Interfaz de consola simple para el proyecto de índice invertido
    /// </summary>
    public class InterfazSimple
    {
        private readonly GestorIndice gestor;
        private readonly string directorioDocumentos = @"C:\Users\ignab\OneDrive\Documents\Estructuras de datos\Documentos"; // Carpeta relativa
        private readonly string archivoIndice = "indice.bin";

        public InterfazSimple()
        {
            gestor = GestorIndice.ObtenerInstancia();
        }

        public async Task IniciarAsync()
        {
            Console.Title = "Sistema de Búsqueda - Índice Invertido";
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            MostrarBienvenida();
            await InicializarSistema();
            
            while (true)
            {
                MostrarMenu();
                var opcion = Console.ReadLine()?.Trim();
                
                switch (opcion)
                {
                    case "1":
                        await RealizarBusqueda();
                        break;
                    case "2":
                        await CrearIndice();
                        break;
                    case "3":
                        MostrarEstadisticas();
                        break;
                    case "4":
                        GuardarIndice();
                        break;
                    case "5":
                        await CargarIndice();
                        break;
                    case "0":
                        Console.WriteLine("\n¡Gracias por usar el sistema!");
                        return;
                    default:
                        Console.WriteLine("❌ Opción no válida. Intente nuevamente.");
                        break;
                }
                
                Console.WriteLine("\nPresione Enter para continuar...");
                Console.ReadLine();
                Console.Clear();
            }
        }

        private void MostrarBienvenida()
        {
            Console.WriteLine("╔════════════════════════════════════════════╗");
            Console.WriteLine("║        SISTEMA DE BÚSQUEDA VECTORIAL       ║");
            Console.WriteLine("║      Índice Invertido con RadixSort        ║");
            Console.WriteLine("╚════════════════════════════════════════════╝");
            Console.WriteLine();
        }

        private async Task InicializarSistema()
        {
            // Verificar si existe el directorio de documentos
            if (!Directory.Exists(directorioDocumentos))
            {
                Console.WriteLine($"📁 Creando directorio: {directorioDocumentos}");
                Directory.CreateDirectory(directorioDocumentos);
                Console.WriteLine("💡 Agregue archivos .txt al directorio 'Documentos'");
            }

            // Intentar cargar índice existente
            if (File.Exists(archivoIndice))
            {
                Console.WriteLine("📂 Cargando índice existente...");
                if (gestor.CargarIndice(archivoIndice))
                {
                    Console.WriteLine("✅ Índice cargado correctamente");
                    var stats = gestor.ObtenerEstadisticas();
                    Console.WriteLine($"📊 {stats.CantidadDocumentos} documentos, {stats.CantidadTerminos} términos");
                }
                else
                {
                    Console.WriteLine("⚠️ Error al cargar índice existente");
                }
            }
        }

        private void MostrarMenu()
        {
            var stats = gestor.ObtenerEstadisticas();
            var estado = gestor.IndiceEstaVacio() ? "Sin índice" : $"{stats.CantidadTerminos} términos";
            
            Console.WriteLine($"Estado actual: {estado}");
            Console.WriteLine();
            Console.WriteLine("=== MENÚ PRINCIPAL ===");
            Console.WriteLine("1. 🔍 Buscar documentos");
            Console.WriteLine("2. 🔨 Crear índice");
            Console.WriteLine("3. 📊 Ver estadísticas");
            Console.WriteLine("4. 💾 Guardar índice");
            Console.WriteLine("5. 📂 Cargar índice");
            Console.WriteLine("0. 🚪 Salir");
            Console.WriteLine();
            Console.Write("Seleccione una opción: ");
        }

        private async Task RealizarBusqueda()
        {
            if (gestor.IndiceEstaVacio())
            {
                Console.WriteLine("❌ No hay índice cargado. Cree uno primero.");
                return;
            }

            Console.WriteLine("\n🔍 BÚSQUEDA VECTORIAL");
            Console.WriteLine("======================");
            Console.Write("Ingrese su consulta: ");
            var consulta = Console.ReadLine()?.Trim();
            
            if (string.IsNullOrWhiteSpace(consulta))
            {
                Console.WriteLine("❌ Consulta vacía");
                return;
            }

            Console.WriteLine($"\nBuscando: '{consulta}'");
            Console.WriteLine("⏳ Procesando...");
            
            var inicio = DateTime.Now;
            var resultados = gestor.BuscarConSimilitudCoseno(consulta);
            var duracion = DateTime.Now - inicio;

            Console.WriteLine($"\n📊 RESULTADOS ({duracion.TotalMilliseconds:F2} ms)");
            Console.WriteLine("=====================================");

            if (resultados.Count == 0)
            {
                Console.WriteLine("❌ No se encontraron resultados");
                return;
            }

            var iterador = new Iterador<ResultadoBusquedaVectorial>(resultados);
            int posicion = 1;
            
            while (iterador.Siguiente() && posicion <= 10)
            {
                var resultado = iterador.Current;
                Console.WriteLine($"{posicion}. {Path.GetFileName(resultado.Documento.Ruta)}");
                Console.WriteLine($"   Similitud: {resultado.SimilitudCoseno * 100:F1}%");
                Console.WriteLine($"   Ruta: {resultado.Documento.Ruta}");
                Console.WriteLine();
                posicion++;
            }

            if (resultados.Count > 10)
            {
                Console.WriteLine($"... y {resultados.Count - 10} resultados más");
            }
        }

        private async Task CrearIndice()
        {
            Console.WriteLine("\n🔨 CREAR ÍNDICE");
            Console.WriteLine("================");
            
            if (!Directory.Exists(directorioDocumentos))
            {
                Console.WriteLine($"❌ Directorio no encontrado: {directorioDocumentos}");
                return;
            }

            var archivos = Directory.GetFiles(directorioDocumentos, "*.txt");
            if (archivos.Length == 0)
            {
                Console.WriteLine($"❌ No se encontraron archivos .txt en {directorioDocumentos}");
                return;
            }

            Console.WriteLine($"📄 Encontrados {archivos.Length} archivo(s)");
            Console.WriteLine("⏳ Creando índice con RadixSort...");
            
            var inicio = DateTime.Now;
            bool exito = await gestor.CrearIndiceDesdeDirectorio(directorioDocumentos);
            var duracion = DateTime.Now - inicio;

            if (exito)
            {
                Console.WriteLine($"✅ Índice creado en {duracion.TotalSeconds:F2} segundos");
                var stats = gestor.ObtenerEstadisticas();
                Console.WriteLine($"📊 {stats.CantidadDocumentos} documentos procesados");
                Console.WriteLine($"📊 {stats.CantidadTerminos} términos únicos");
                
                // Guardar automáticamente
                if (gestor.GuardarIndice(archivoIndice))
                {
                    Console.WriteLine($"💾 Índice guardado como {archivoIndice}");
                }
            }
            else
            {
                Console.WriteLine("❌ Error al crear el índice");
            }
        }

        private void MostrarEstadisticas()
        {
            if (gestor.IndiceEstaVacio())
            {
                Console.WriteLine("❌ No hay índice cargado");
                return;
            }

            Console.WriteLine("\n📊 ESTADÍSTICAS DEL ÍNDICE");
            Console.WriteLine("===========================");
            
            var stats = gestor.ObtenerEstadisticas();
            Console.WriteLine($"📄 Documentos: {stats.CantidadDocumentos}");
            Console.WriteLine($"🔤 Términos únicos: {stats.CantidadTerminos}");
            Console.WriteLine($"📈 Promedio términos/doc: {stats.PromedioTerminosPorDocumento:F1}");
            Console.WriteLine($"🔄 Índice ordenado: {(stats.IndiceOrdenado ? "Sí (RadixSort)" : "No")}");
            Console.WriteLine($"💾 Memoria estimada: {stats.MemoriaEstimadaKB} KB");
            
            if (File.Exists(archivoIndice))
            {
                var fileInfo = new FileInfo(archivoIndice);
                Console.WriteLine($"📁 Archivo: {archivoIndice} ({fileInfo.Length / 1024.0:F1} KB)");
            }
        }

        private void GuardarIndice()
        {
            if (gestor.IndiceEstaVacio())
            {
                Console.WriteLine("❌ No hay índice para guardar");
                return;
            }

            Console.WriteLine($"\n💾 Guardando índice como {archivoIndice}...");
            
            if (gestor.GuardarIndice(archivoIndice))
            {
                var fileInfo = new FileInfo(archivoIndice);
                Console.WriteLine($"✅ Índice guardado ({fileInfo.Length / 1024.0:F1} KB)");
            }
            else
            {
                Console.WriteLine("❌ Error al guardar el índice");
            }
        }

        private async Task CargarIndice()
        {
            Console.WriteLine($"\n📂 Cargando índice desde {archivoIndice}...");
            
            if (!File.Exists(archivoIndice))
            {
                Console.WriteLine($"❌ Archivo no encontrado: {archivoIndice}");
                return;
            }

            if (gestor.CargarIndice(archivoIndice))
            {
                Console.WriteLine("✅ Índice cargado correctamente");
                var stats = gestor.ObtenerEstadisticas();
                Console.WriteLine($"📊 {stats.CantidadDocumentos} documentos, {stats.CantidadTerminos} términos");
            }
            else
            {
                Console.WriteLine("❌ Error al cargar el índice");
            }
        }
    }
}