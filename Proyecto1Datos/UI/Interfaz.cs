using System.IO;
using PruebaRider.Servicios;
using PruebaRider.Estructura.Nodo;

namespace PruebaRider.UI
{
    /// <summary>
    /// Interfaz simplificada manteniendo funcionalidad completa
    /// </summary>
    public class InterfazSimple
    {
        private readonly GestorIndice gestor;
        private readonly string directorioDocumentos;
        private readonly string archivoIndice;

        public InterfazSimple()
        {
            gestor = GestorIndice.ObtenerInstancia(); // Patrón Singleton mantenido
            directorioDocumentos = @"C:\Users\ignab\OneDrive\Documents\Estructuras de datos\Documentos";
            archivoIndice = "indice_zipf.bin";
        }

        public async Task IniciarAsync()
        {
            ConfigurarConsola();
            MostrarBienvenida();
            
            // Cargar índice existente si hay
            if (File.Exists(archivoIndice))
            {
                Console.WriteLine("📂 Cargando índice existente...");
                gestor.CargarIndice(archivoIndice);
                var stats = gestor.ObtenerEstadisticas();
                Console.WriteLine($"✅ {stats.CantidadDocumentos} docs, {stats.CantidadTerminos} términos");
            }

            await EjecutarMenuPrincipal();
        }

        private void ConfigurarConsola()
        {
            Console.Title = "Motor de Búsqueda - Índice Invertido + RadixSort + Zipf";
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }

        private void MostrarBienvenida()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║         MOTOR DE BÚSQUEDA VECTORIAL          ║");
            Console.WriteLine("║    Índice Invertido + RadixSort + Zipf       ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("🎯 Características:");
            Console.WriteLine("   ✅ Lista doblemente enlazada circular");
            Console.WriteLine("   ✅ Vector con operador * sobrecargado");
            Console.WriteLine("   ✅ RadixSort para ordenamiento");
            Console.WriteLine("   ✅ Ley de Zipf obligatoria");
            Console.WriteLine("   ✅ Similitud coseno vectorial");
            Console.WriteLine();
        }

        private async Task EjecutarMenuPrincipal()
        {
            while (true)
            {
                MostrarMenu();
                string opcion = Console.ReadLine()?.Trim();
                
                bool salir = await ProcesarOpcion(opcion);
                if (salir) break;
                
                PausarYContinuar();
            }
        }

        private void MostrarMenu()
        {
            var stats = gestor.ObtenerEstadisticas();
            string estado = gestor.IndiceEstaVacio() ? "❌ Sin índice" : 
                $"✅ {stats.CantidadTerminos} términos{(stats.ZipfAplicado ? " (Zipf✓)" : "")}";
            
            Console.WriteLine($"Estado: {estado}");
            Console.WriteLine();
            Console.WriteLine("1. 🔍 Buscar documentos");
            Console.WriteLine("2. 🔨 Crear índice nuevo");
            Console.WriteLine("3. 💾 Guardar índice");
            Console.WriteLine("4. 📂 Cargar índice");
            Console.WriteLine("5. 📊 Ver estadísticas");
            Console.WriteLine("0. 🚪 Salir");
            Console.WriteLine();
            Console.Write("Opción: ");
        }

        private async Task<bool> ProcesarOpcion(string opcion)
        {
            Console.WriteLine();
            
            switch (opcion)
            {
                case "1": await RealizarBusqueda(); break;
                case "2": await CrearIndice(); break;
                case "3": GuardarIndice(); break;
                case "4": CargarIndice(); break;
                case "5": MostrarEstadisticas(); break;
                case "0": return true;
                default: Console.WriteLine("❌ Opción inválida"); break;
            }
            
            return false;
        }

       /// <summary>
        /// BÚSQUEDA CON RESULTADOS LIMPIOS - SIN contenido binario
        /// </summary>
        private async Task RealizarBusqueda()
        {
            if (gestor.IndiceEstaVacio())
            {
                Console.WriteLine("❌ No hay índice. Cree uno primero (opción 2).");
                return;
            }

            Console.Write("Términos de búsqueda: ");
            string consulta = Console.ReadLine()?.Trim();
            
            if (string.IsNullOrWhiteSpace(consulta))
            {
                Console.WriteLine("❌ Consulta vacía");
                return;
            }

            Console.WriteLine($"\n🔍 Buscando: '{consulta}'");
            var inicio = DateTime.Now;
            var resultados = gestor.BuscarConSimilitudCoseno(consulta);
            var duracion = DateTime.Now - inicio;

            if (resultados.Count == 0)
            {
                Console.WriteLine("❌ No se encontraron documentos relevantes");
                return;
            }

            Console.WriteLine($"\n📊 RESULTADOS ({duracion.TotalMilliseconds:F1} ms)");
            Console.WriteLine("═".PadRight(60, '═'));
            
            // Mostrar resultados LIMPIOS Y CLAROS
            var iterador = new Iterador<ResultadoBusquedaVectorial>(resultados);
            int posicion = 1;
            
            while (iterador.Siguiente() && posicion <= 10)
            {
                var resultado = iterador.Current;
                string archivo = Path.GetFileName(resultado.Documento.Ruta);
                double porcentaje = resultado.SimilitudCoseno * 100;
                
                Console.WriteLine($"{posicion}. 📄 {archivo}");
                Console.WriteLine($"   🎯 Similitud: {porcentaje:F1}%");
                Console.WriteLine($"   📝 Vista previa: {resultado.ObtenerVistaPrevia()}");
                Console.WriteLine($"   📁 Ruta: {resultado.Documento.Ruta}");
                
                // Opción para generar URL
                Console.WriteLine($"   🔗 Para ver contenido completo, presione 'v' + {posicion}");
                
                Console.WriteLine();
                posicion++;
            }

            if (resultados.Count > 10)
            {
                Console.WriteLine($"... y {resultados.Count - 10} resultado(s) más");
            }

            // Opción interactiva para ver documentos
            Console.WriteLine("\n💡 OPCIONES:");
            Console.WriteLine("   • Presione Enter para nueva búsqueda");
            Console.WriteLine("   • Escriba 'v1', 'v2', etc. para ver documento completo");
            Console.Write("Opción: ");
            
            string opcion = Console.ReadLine()?.Trim().ToLower();
            
            if (opcion.StartsWith("v") && opcion.Length > 1)
            {
                if (int.TryParse(opcion.Substring(1), out int num) && num >= 1 && num <= Math.Min(10, resultados.Count))
                {
                    MostrarDocumentoCompleto(resultados, num - 1);
                }
                else
                {
                    Console.WriteLine("❌ Número inválido");
                }
            }
        }

        /// <summary>
        /// Mostrar documento específico con URL
        /// </summary>
        private void MostrarDocumentoCompleto(ListaDobleEnlazada<ResultadoBusquedaVectorial> resultados, int indice)
        {
            var iterador = new Iterador<ResultadoBusquedaVectorial>(resultados);
            int contador = 0;
            
            while (iterador.Siguiente())
            {
                if (contador == indice)
                {
                    var resultado = iterador.Current;
                    
                    Console.WriteLine($"\n📄 DOCUMENTO COMPLETO");
                    Console.WriteLine("═".PadRight(40, '═'));
                    Console.WriteLine($"Archivo: {Path.GetFileName(resultado.Documento.Ruta)}");
                    Console.WriteLine($"Similitud: {resultado.SimilitudCoseno * 100:F1}%");
                    Console.WriteLine($"Ruta: {resultado.Documento.Ruta}");
                    Console.WriteLine();
                    
                    Console.WriteLine("🔗 URL para navegador:");
                    Console.WriteLine("(Copie toda la línea siguiente y péguela en su navegador)");
                    Console.WriteLine();
                    Console.WriteLine(resultado.GenerarUrlHtml());
                    Console.WriteLine();
                    
                    break;
                }
                contador++;
            }
        }

        private async Task CrearIndice()
        {
            if (!Directory.Exists(directorioDocumentos))
            {
                Console.WriteLine($"❌ Directorio no encontrado: {directorioDocumentos}");
                return;
            }

            var archivos = Directory.GetFiles(directorioDocumentos, "*.txt");
            if (archivos.Length == 0)
            {
                Console.WriteLine("❌ No hay archivos .txt");
                return;
            }

            Console.WriteLine($"📁 {archivos.Length} archivos encontrados");
            
            // Configurar Zipf
            Console.Write("Percentil Zipf (1-30) [15]: ");
            string input = Console.ReadLine()?.Trim();
            int percentil = 15;
            if (!string.IsNullOrEmpty(input) && int.TryParse(input, out int p))
            {
                percentil = Math.Max(1, Math.Min(30, p));
            }

            Console.WriteLine($"\n🚀 Creando índice con Zipf {percentil}%...");
            var inicio = DateTime.Now;
            
            bool exito = await gestor.CrearIndiceDesdeDirectorio(directorioDocumentos, percentil);
            
            if (exito)
            {
                var duracion = DateTime.Now - inicio;
                var stats = gestor.ObtenerEstadisticas();
                
                Console.WriteLine($"✅ Índice creado en {duracion.TotalSeconds:F1}s");
                Console.WriteLine($"📊 {stats.CantidadDocumentos} docs, {stats.CantidadTerminos} términos");
                
                // Guardar automáticamente
                gestor.GuardarIndice(archivoIndice);
                Console.WriteLine($"💾 Guardado en {archivoIndice}");
            }
        }

        private void GuardarIndice()
        {
            if (gestor.IndiceEstaVacio())
            {
                Console.WriteLine("❌ No hay índice para guardar");
                return;
            }

            if (gestor.GuardarIndice(archivoIndice))
            {
                var info = new FileInfo(archivoIndice);
                Console.WriteLine($"✅ Guardado: {archivoIndice} ({info.Length / 1024:F1} KB)");
            }
            else
            {
                Console.WriteLine("❌ Error al guardar");
            }
        }

        private void CargarIndice()
        {
            if (!File.Exists(archivoIndice))
            {
                Console.WriteLine($"❌ Archivo no encontrado: {archivoIndice}");
                return;
            }

            if (gestor.CargarIndice(archivoIndice))
            {
                var stats = gestor.ObtenerEstadisticas();
                Console.WriteLine($"✅ Cargado: {stats.CantidadDocumentos} docs, {stats.CantidadTerminos} términos");
                if (stats.ZipfAplicado)
                    Console.WriteLine("🔥 Ley de Zipf aplicada");
            }
            else
            {
                Console.WriteLine("❌ Error al cargar");
            }
        }

        private void MostrarEstadisticas()
        {
            if (gestor.IndiceEstaVacio())
            {
                Console.WriteLine("❌ No hay índice cargado");
                return;
            }

            var stats = gestor.ObtenerEstadisticas();
            
            Console.WriteLine("📊 ESTADÍSTICAS DEL SISTEMA");
            Console.WriteLine("═".PadRight(30, '═'));
            Console.WriteLine($"📄 Documentos: {stats.CantidadDocumentos}");
            Console.WriteLine($"🔤 Términos: {stats.CantidadTerminos}");
            Console.WriteLine($"🔥 Zipf aplicado: {(stats.ZipfAplicado ? "✅ Sí" : "❌ No")}");
            Console.WriteLine($"⚡ RadixSort: ✅ Aplicado");
            Console.WriteLine($"🎯 Similitud: ✅ Coseno vectorial");
            
            if (File.Exists(archivoIndice))
            {
                var info = new FileInfo(archivoIndice);
                Console.WriteLine($"💾 Archivo: {info.Length / 1024:F1} KB");
            }
        }

        private void PausarYContinuar()
        {
            Console.WriteLine();
            Console.Write("Presiona Enter...");
            Console.ReadLine();
            Console.Clear();
        }
    }
}