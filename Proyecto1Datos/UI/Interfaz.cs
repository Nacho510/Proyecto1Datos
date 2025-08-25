using PruebaRider.Estructura.Nodo;
using PruebaRider.Servicios;
using PruebaRider.Strategy;

namespace PruebaRider.UI
{
    public class Interfaz
    {
        private readonly GestorIndice gestorIndice;
        private readonly string DIRECTORIO_DOCUMENTOS = @"C:\Users\ignab\OneDrive\Documents\Estructuras de datos\Documentos"; // Directorio fijo
        private readonly string ARCHIVO_INDICE = @"indice_invertido.bin"; // Archivo de índice por defecto

        public Interfaz()
        {
            gestorIndice = GestorIndice.ObtenerInstancia();
        }

        public async Task MenuPrincipalAsync()
        {
            Console.Clear();
            Console.WriteLine("=== SISTEMA DE BÚSQUEDA CON ÍNDICE INVERTIDO ===");
            Console.WriteLine($"Directorio de trabajo: {DIRECTORIO_DOCUMENTOS}");
            Console.WriteLine(new string('=', 60));

            // Intentar cargar índice existente automáticamente
            await InicializarIndice();

            while (true)
            {
                Console.WriteLine("\n📋 MENÚ PRINCIPAL:");
                Console.WriteLine("1. 🔍 Buscar documentos");
                Console.WriteLine("2. 🔄 Actualizar índice (agregar nuevos documentos)");
                Console.WriteLine("3. ⚡ Aplicar Ley de Zipf");
                Console.WriteLine("4. 💾 Guardar índice");
                Console.WriteLine("5. 📊 Ver estadísticas del índice");
                Console.WriteLine("6. 🗑️  Recrear índice desde cero");
                Console.WriteLine("0. 🚪 Salir");
                
                MostrarEstadoIndice();
                
                Console.Write("\n➤ Seleccione una opción: ");
                string opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        await RealizarBusqueda();
                        break;
                    case "2":
                        await ActualizarIndice();
                        break;
                    case "3":
                        AplicarLeyZipf();
                        break;
                    case "4":
                        GuardarIndice();
                        break;
                    case "5":
                        MostrarEstadisticas();
                        break;
                    case "6":
                        await RecrearIndice();
                        break;
                    case "0":
                        Console.WriteLine("\n👋 ¡Hasta luego!");
                        return;
                    default:
                        Console.WriteLine("\n❌ Opción no válida. Intente de nuevo.");
                        break;
                }

                Console.WriteLine("\n⏸️  Presione cualquier tecla para continuar...");
                Console.ReadKey();
                Console.Clear();
                Console.WriteLine("=== SISTEMA DE BÚSQUEDA CON ÍNDICE INVERTIDO ===");
                Console.WriteLine($"Directorio de trabajo: {DIRECTORIO_DOCUMENTOS}");
                Console.WriteLine(new string('=', 60));
            }
        }

        private async Task InicializarIndice()
        {
            Console.WriteLine("\n🔄 Inicializando sistema...");

            // Verificar si existe el directorio
            if (!Directory.Exists(DIRECTORIO_DOCUMENTOS))
            {
                Console.WriteLine($"❌ El directorio {DIRECTORIO_DOCUMENTOS} no existe.");
                Console.WriteLine("📝 Por favor, cree el directorio y agregue archivos .txt");
                Console.WriteLine("⏸️  Presione cualquier tecla para continuar...");
                Console.ReadKey();
                return;
            }

            // Intentar cargar índice existente
            if (File.Exists(ARCHIVO_INDICE))
            {
                Console.WriteLine("📂 Cargando índice existente...");
                bool cargado = gestorIndice.CargarIndice(ARCHIVO_INDICE);
                
                if (cargado)
                {
                    Console.WriteLine("✅ Índice cargado exitosamente!");
                    MostrarEstadisticas();
                    return;
                }
                else
                {
                    Console.WriteLine("⚠️  Error al cargar índice existente. Creando nuevo...");
                }
            }

            // Crear nuevo índice
            Console.WriteLine("🔨 Creando nuevo índice...");
            bool creado = await gestorIndice.CrearIndiceDesdeDirectorio(DIRECTORIO_DOCUMENTOS);
            
            if (creado)
            {
                Console.WriteLine("✅ ¡Índice creado exitosamente!");
                // Guardar automáticamente
                gestorIndice.GuardarIndice(ARCHIVO_INDICE);
                Console.WriteLine($"💾 Índice guardado en {ARCHIVO_INDICE}");
                MostrarEstadisticas();
            }
            else
            {
                Console.WriteLine("❌ Error al crear el índice.");
                var archivos = Directory.GetFiles(DIRECTORIO_DOCUMENTOS, "*.txt");
                if (archivos.Length == 0)
                {
                    Console.WriteLine("📝 No se encontraron archivos .txt en el directorio.");
                    Console.WriteLine("   Agregue algunos archivos .txt y reinicie la aplicación.");
                }
            }
        }

        private async Task RealizarBusqueda()
        {
            if (gestorIndice.IndiceEstaVacio())
            {
                Console.WriteLine("\n❌ No hay índice disponible.");
                Console.WriteLine("   El sistema intentará crear uno automáticamente.");
                await InicializarIndice();
                return;
            }

            Console.WriteLine("\n🔍 === BÚSQUEDA DE DOCUMENTOS ===");
            Console.WriteLine("💡 Ejemplos de búsqueda:");
            Console.WriteLine("   • Palabras simples: 'algoritmo'");
            Console.WriteLine("   • Frases: 'inteligencia artificial'");
            Console.WriteLine("   • Múltiples términos: 'datos estructura búsqueda'");
            
            Console.Write("\n➤ Ingrese su consulta: ");
            string consulta = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(consulta))
            {
                Console.WriteLine("❌ Consulta vacía.");
                return;
            }

            Console.WriteLine($"\n🔍 Buscando: '{consulta}'");
            Console.WriteLine(new string('-', 50));

            // Búsqueda Vectorial (Similitud Coseno)
            Console.WriteLine("\n📊 RESULTADOS VECTORIALES (Similitud Coseno):");
            var resultadosVectoriales = gestorIndice.BuscarConSimilitudCoseno(consulta);
            MostrarResultadosVectoriales(resultadosVectoriales);

            // Separador visual
            Console.WriteLine(new string('-', 50));

            // Búsqueda TF-IDF Tradicional
            Console.WriteLine("\n📊 RESULTADOS TF-IDF (Puntuación Clásica):");
            var resultadosTfIdf = gestorIndice.BuscarTfIdf(consulta);
            MostrarResultadosTfIdf(resultadosTfIdf);
        }

        private void MostrarResultadosVectoriales(ListaDobleEnlazada<ResultadoBusquedaVectorial> resultados)
        {
            if (resultados.Count == 0)
            {
                Console.WriteLine("🔍 No se encontraron documentos relevantes.");
                return;
            }

            Console.WriteLine($"📁 Se encontraron {resultados.Count} documento(s) relevante(s):");
            
            var iterador = new Iterador<ResultadoBusquedaVectorial>(resultados);
            int posicion = 1;
            
            while (iterador.Siguiente())
            {
                var resultado = iterador.Current;
                string emoji = posicion <= 3 ? "🥇" : "📄";
                
                Console.WriteLine($"\n{emoji} {posicion}. {Path.GetFileName(resultado.Documento.Ruta)}");
                Console.WriteLine($"   📊 Similitud: {resultado.SimilitudCoseno:F4} ({(resultado.SimilitudCoseno * 100):F1}%)");
                Console.WriteLine($"   📁 Archivo: {resultado.Documento.Ruta}");
                
                posicion++;
                
                if (posicion > 8) // Limitar resultados mostrados
                {
                    Console.WriteLine($"\n   ... y {resultados.Count - 8} resultado(s) más");
                    break;
                }
            }
        }

        private void MostrarResultadosTfIdf(ListaDobleEnlazada<ResultadoBusqueda> resultados)
        {
            if (resultados.Count == 0)
            {
                Console.WriteLine("🔍 No se encontraron documentos relevantes.");
                return;
            }

            Console.WriteLine($"📁 Se encontraron {resultados.Count} documento(s) relevante(s):");
            
            var iterador = new Iterador<ResultadoBusqueda>(resultados);
            int posicion = 1;
            
            while (iterador.Siguiente())
            {
                var resultado = iterador.Current;
                string emoji = posicion <= 3 ? "🥇" : "📄";
                
                Console.WriteLine($"\n{emoji} {posicion}. {Path.GetFileName(resultado.Documento.Ruta)}");
                Console.WriteLine($"   📊 Puntuación TF-IDF: {resultado.Score:F4}");
                Console.WriteLine($"   📁 Archivo: {resultado.Documento.Ruta}");
                
                posicion++;
                
                if (posicion > 8) // Limitar resultados mostrados
                {
                    Console.WriteLine($"\n   ... y {resultados.Count - 8} resultado(s) más");
                    break;
                }
            }
        }

        private async Task ActualizarIndice()
        {
            if (gestorIndice.IndiceEstaVacio())
            {
                Console.WriteLine("\n❌ No hay índice base. Creando índice desde cero...");
                await RecrearIndice();
                return;
            }

            Console.WriteLine("\n🔄 === ACTUALIZAR ÍNDICE ===");
            Console.WriteLine("🔍 Buscando documentos nuevos...");
            
            try
            {
                bool actualizado = await gestorIndice.ActualizarIndice(DIRECTORIO_DOCUMENTOS);
                
                if (actualizado)
                {
                    Console.WriteLine("✅ ¡Índice actualizado exitosamente!");
                    // Guardar automáticamente
                    gestorIndice.GuardarIndice(ARCHIVO_INDICE);
                    Console.WriteLine($"💾 Cambios guardados en {ARCHIVO_INDICE}");
                    MostrarEstadisticas();
                }
                else
                {
                    Console.WriteLine("⚠️  Error al actualizar el índice.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        private async Task RecrearIndice()
        {
            Console.WriteLine("\n🔄 === RECREAR ÍNDICE DESDE CERO ===");
            Console.WriteLine("⚠️  Esto eliminará el índice actual y creará uno nuevo.");
            Console.Write("¿Está seguro? (s/N): ");
            
            string confirmacion = Console.ReadLine()?.ToLower();
            if (confirmacion != "s" && confirmacion != "si")
            {
                Console.WriteLine("❌ Operación cancelada.");
                return;
            }

            Console.WriteLine("🔨 Recreando índice...");
            
            try
            {
                bool creado = await gestorIndice.CrearIndiceDesdeDirectorio(DIRECTORIO_DOCUMENTOS);
                
                if (creado)
                {
                    Console.WriteLine("✅ ¡Índice recreado exitosamente!");
                    // Guardar automáticamente
                    gestorIndice.GuardarIndice(ARCHIVO_INDICE);
                    Console.WriteLine($"💾 Índice guardado en {ARCHIVO_INDICE}");
                    MostrarEstadisticas();
                }
                else
                {
                    Console.WriteLine("❌ Error al recrear el índice.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        private void AplicarLeyZipf()
        {
            if (gestorIndice.IndiceEstaVacio())
            {
                Console.WriteLine("\n❌ No hay índice disponible para aplicar Ley de Zipf.");
                return;
            }

            Console.WriteLine("\n⚡ === APLICAR LEY DE ZIPF ===");
            Console.WriteLine("📊 Seleccione el tipo de optimización:");
            Console.WriteLine("1. 🧹 Eliminar términos MUY FRECUENTES (stopwords, palabras comunes)");
            Console.WriteLine("2. 🗑️  Eliminar términos MUY RAROS (ruido, errores tipográficos)");
            Console.Write("\n➤ Seleccione opción (1 o 2): ");
            
            string opcion = Console.ReadLine();
            bool eliminarFrecuentes = opcion == "1";

            string tipoEliminacion = eliminarFrecuentes ? "términos frecuentes" : "términos raros";
            Console.WriteLine($"\n📊 Aplicando eliminación de {tipoEliminacion}");

            Console.Write("➤ Ingrese el percentil a eliminar (recomendado 10-30): ");
            if (!int.TryParse(Console.ReadLine(), out int percentil) || percentil < 1 || percentil > 99)
            {
                Console.WriteLine("❌ Percentil no válido. Debe ser un número entre 1 y 99.");
                return;
            }

            var statsAntes = gestorIndice.ObtenerEstadisticas();
            Console.WriteLine($"\n📊 Términos antes de aplicar Zipf: {statsAntes.CantidadTerminos}");

            try
            {
                gestorIndice.AplicarLeyZipf(percentil, eliminarFrecuentes);
                
                var statsDespues = gestorIndice.ObtenerEstadisticas();
                int terminosEliminados = statsAntes.CantidadTerminos - statsDespues.CantidadTerminos;
                
                Console.WriteLine("✅ ¡Ley de Zipf aplicada exitosamente!");
                Console.WriteLine($"📊 Términos eliminados: {terminosEliminados}");
                Console.WriteLine($"📊 Términos restantes: {statsDespues.CantidadTerminos}");
                Console.WriteLine($"📈 Reducción: {((double)terminosEliminados / statsAntes.CantidadTerminos * 100):F1}%");
                
                // Guardar automáticamente
                gestorIndice.GuardarIndice(ARCHIVO_INDICE);
                Console.WriteLine($"💾 Cambios guardados en {ARCHIVO_INDICE}");
                
                MostrarEstadisticas();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error aplicando Ley de Zipf: {ex.Message}");
            }
        }

        private void GuardarIndice()
        {
            if (gestorIndice.IndiceEstaVacio())
            {
                Console.WriteLine("\n❌ No hay índice para guardar.");
                return;
            }

            Console.WriteLine("\n💾 === GUARDAR ÍNDICE ===");
            
            bool exito = gestorIndice.GuardarIndice(ARCHIVO_INDICE);
            
            if (exito)
            {
                Console.WriteLine($"✅ ¡Índice guardado exitosamente!");
                Console.WriteLine($"📁 Ubicación: {Path.GetFullPath(ARCHIVO_INDICE)}");
                Console.WriteLine($"📊 Tamaño: {new FileInfo(ARCHIVO_INDICE).Length / 1024.0:F1} KB");
            }
            else
            {
                Console.WriteLine("❌ Error al guardar el índice.");
            }
        }

        private void MostrarEstadisticas()
        {
            var stats = gestorIndice.ObtenerEstadisticas();
            
            Console.WriteLine("\n📊 === ESTADÍSTICAS DEL ÍNDICE ===");
            Console.WriteLine($"📁 Documentos indexados: {stats.CantidadDocumentos}");
            Console.WriteLine($"🔤 Términos únicos: {stats.CantidadTerminos:N0}");
            Console.WriteLine($"📈 Promedio términos/documento: {stats.PromedioTerminosPorDocumento:F1}");
            
            if (File.Exists(ARCHIVO_INDICE))
            {
                var fileInfo = new FileInfo(ARCHIVO_INDICE);
                Console.WriteLine($"💾 Tamaño del índice: {fileInfo.Length / 1024.0:F1} KB");
                Console.WriteLine($"🕐 Última modificación: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm}");
            }
        }

        private void MostrarEstadoIndice()
        {
            if (gestorIndice.IndiceEstaVacio())
            {
                Console.WriteLine("\n⚠️  Estado: Índice no disponible");
            }
            else
            {
                var stats = gestorIndice.ObtenerEstadisticas();
                Console.WriteLine($"\n✅ Estado: Índice cargado ({stats.CantidadDocumentos} docs, {stats.CantidadTerminos} términos)");
            }
        }
    }
}