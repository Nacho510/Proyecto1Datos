using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using PruebaRider.Servicios;
using PruebaRider.Estructura.Nodo;
using PruebaRider.Modelo;
using PruebaRider.Strategy;

namespace PruebaRider.UI
{
    /// <summary>
    /// Interfaz de usuario completamente nueva desde cero
    /// - Compatible 100% con el nuevo GestorIndice
    /// - Integración completa de Ley de Zipf
    /// - Enlaces HTML decodificados para navegador
    /// - Sin errores de compilación
    /// </summary>
    public class InterfazSimple
    {
        private readonly GestorIndice gestor;
        private readonly string directorioDocumentos;
        private readonly string archivoIndice;

        public InterfazSimple()
        {
            gestor = GestorIndice.ObtenerInstancia();
            directorioDocumentos = @"C:\Users\ignab\OneDrive\Documents\Estructuras de datos\Documentos";
            archivoIndice = "indice_zipf.bin";
        }

        /// <summary>
        /// Método principal para iniciar la aplicación
        /// </summary>
        public async Task IniciarAsync()
        {
            ConfigurarConsola();
            MostrarBienvenida();
            await InicializarSistema();
            
            await EjecutarMenuPrincipal();
            
            MostrarDespedida();
        }

        /// <summary>
        /// Configurar consola para mejor experiencia
        /// </summary>
        private void ConfigurarConsola()
        {
            Console.Title = "Motor de Búsqueda Vectorial - Índice Invertido + RadixSort + Ley de Zipf";
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    Console.WindowWidth = Math.Min(120, Console.LargestWindowWidth);
                    Console.WindowHeight = Math.Min(40, Console.LargestWindowHeight);
                }
            }
            catch
            {
                // Ignorar errores de configuración de consola
            }
        }

        /// <summary>
        /// Mostrar pantalla de bienvenida
        /// </summary>
        private void MostrarBienvenida()
        {
            Console.Clear();
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                 MOTOR DE BÚSQUEDA VECTORIAL                   ║");
            Console.WriteLine("║          Índice Invertido + RadixSort + TF-IDF + Zipf        ║");
            Console.WriteLine("║                      Similitud Coseno                        ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("🔥 Características implementadas:");
            Console.WriteLine("   ✅ Ley de Zipf aplicada obligatoriamente");
            Console.WriteLine("   ✅ Algoritmo RadixSort para ordenamiento óptimo");
            Console.WriteLine("   ✅ Búsqueda vectorial con similitud coseno exacta");
            Console.WriteLine("   ✅ Enlaces HTML decodificados para navegador");
            Console.WriteLine("   ✅ Estructuras de datos personalizadas (sin genéricos)");
            Console.WriteLine();
        }

        /// <summary>
        /// Inicializar sistema al arranque
        /// </summary>
        private async Task InicializarSistema()
        {
            Console.WriteLine("🚀 Inicializando sistema...");
            
            // Crear directorio si no existe
            if (!Directory.Exists(directorioDocumentos))
            {
                Console.WriteLine($"📁 Creando directorio: {directorioDocumentos}");
                try
                {
                    Directory.CreateDirectory(directorioDocumentos);
                    Console.WriteLine("💡 Agregue archivos .txt al directorio para indexar");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error creando directorio: {ex.Message}");
                }
            }

            // Intentar cargar índice existente
            if (File.Exists(archivoIndice))
            {
                Console.WriteLine("📂 Detectado índice existente, cargando...");
                if (gestor.CargarIndice(archivoIndice))
                {
                    var stats = gestor.ObtenerEstadisticas();
                    Console.WriteLine("✅ Índice cargado correctamente");
                    Console.WriteLine($"📊 {stats.CantidadDocumentos} documentos, {stats.CantidadTerminos} términos");
                    
                    if (stats.ZipfAplicado)
                    {
                        Console.WriteLine($"🔥 Ley de Zipf: {stats.PercentilZipf}% aplicado");
                    }
                    else
                    {
                        Console.WriteLine("⚠️ Índice sin Zipf - se recomienda recrear");
                    }
                }
                else
                {
                    Console.WriteLine("⚠️ Error al cargar índice existente");
                }
            }
            else
            {
                Console.WriteLine("💡 No se encontró índice existente");
            }
            
            Console.WriteLine();
        }

        /// <summary>
        /// Ejecutar menú principal
        /// </summary>
        private async Task EjecutarMenuPrincipal()
        {
            while (true)
            {
                MostrarMenu();
                string opcion = LeerOpcion();
                
                bool salir = await ProcesarOpcion(opcion);
                if (salir) break;
                
                PausarYLimpiar();
            }
        }

        /// <summary>
        /// Mostrar menú principal
        /// </summary>
        private void MostrarMenu()
        {
            var stats = gestor.ObtenerEstadisticas();
            string estado = gestor.IndiceEstaVacio() ? "❌ Sin índice" : 
                $"✅ {stats.CantidadTerminos} términos {(stats.ZipfAplicado ? $"(Zipf {stats.PercentilZipf}%)" : "(Sin Zipf)")}";
            
            Console.WriteLine($"Estado del sistema: {estado}");
            Console.WriteLine();
            Console.WriteLine("══════════════════ MENÚ PRINCIPAL ══════════════════");
            Console.WriteLine("1. 🔍 Buscar documentos (Similitud Coseno)");
            Console.WriteLine("2. 🔨 Crear índice nuevo (Zipf automático 15%)");
            Console.WriteLine("3. ⚙️ Crear índice personalizado (Configurar Zipf)");
            Console.WriteLine("4. 📊 Ver estadísticas detalladas del sistema");
            Console.WriteLine("5. 💾 Guardar índice en archivo");
            Console.WriteLine("6. 📂 Cargar índice desde archivo");
            Console.WriteLine("7. 🔥 Modificar configuración de Ley de Zipf");
            Console.WriteLine("8. 🧹 Limpiar sistema y memoria");
            Console.WriteLine("0. 🚪 Salir del programa");
            Console.WriteLine("════════════════════════════════════════════════════");
            Console.Write("Seleccione una opción (0-8): ");
        }

        /// <summary>
        /// Leer opción del usuario
        /// </summary>
        private string LeerOpcion()
        {
            return Console.ReadLine()?.Trim() ?? "";
        }

        /// <summary>
        /// Procesar opción seleccionada
        /// </summary>
        private async Task<bool> ProcesarOpcion(string opcion)
        {
            Console.WriteLine();
            
            switch (opcion)
            {
                case "1":
                    await RealizarBusquedaVectorial();
                    break;
                    
                case "2":
                    await CrearIndiceAutomatico();
                    break;
                    
                case "3":
                    await CrearIndicePersonalizado();
                    break;
                    
                case "4":
                    MostrarEstadisticasDetalladas();
                    break;
                    
                case "5":
                    GuardarIndiceEnArchivo();
                    break;
                    
                case "6":
                    await CargarIndiceDesdeArchivo();
                    break;
                    
                case "7":
                    await ModificarConfiguracionZipf();
                    break;
                    
                case "8":
                    LimpiarSistemaCompleto();
                    break;
                    
                case "0":
                    return true; // Salir
                    
                default:
                    Console.WriteLine("❌ Opción no válida. Por favor seleccione un número del 0 al 8.");
                    break;
            }
            
            return false; // Continuar
        }

        /// <summary>
        /// OPCIÓN 1: Realizar búsqueda vectorial con similitud coseno
        /// </summary>
        private async Task RealizarBusquedaVectorial()
        {
            Console.WriteLine("🔍 BÚSQUEDA VECTORIAL - SIMILITUD COSENO");
            Console.WriteLine("==========================================");
            
            if (gestor.IndiceEstaVacio())
            {
                Console.WriteLine("❌ No hay índice cargado.");
                Console.WriteLine("💡 Use la opción 2 o 3 para crear un nuevo índice primero.");
                return;
            }

            var stats = gestor.ObtenerEstadisticas();
            if (!stats.ZipfAplicado)
            {
                Console.WriteLine("⚠️ ADVERTENCIA: El índice no tiene Ley de Zipf aplicada");
                Console.WriteLine("   Esto no cumple con el enunciado del proyecto.");
                Console.WriteLine("   Se recomienda recrear el índice con la opción 2 o 3.");
                Console.WriteLine();
            }

            Console.Write("Ingrese los términos de búsqueda: ");
            string consulta = Console.ReadLine()?.Trim();
            
            if (string.IsNullOrWhiteSpace(consulta))
            {
                Console.WriteLine("❌ Consulta vacía. Intente nuevamente.");
                return;
            }

            await EjecutarBusquedaYMostrarResultados(consulta, stats);
        }

        /// <summary>
        /// Ejecutar búsqueda y mostrar resultados formateados
        /// </summary>
        private async Task EjecutarBusquedaYMostrarResultados(string consulta, EstadisticasIndiceConZipf stats)
        {
            Console.WriteLine($"\n🎯 Buscando: '{consulta}'");
            if (stats.ZipfAplicado)
            {
                Console.WriteLine($"🔥 Usando índice optimizado con Zipf ({stats.PercentilZipf}%)");
            }
            Console.WriteLine("⏳ Calculando similitud coseno...");
            
            var inicioTiempo = DateTime.Now;
            var resultados = gestor.BuscarConSimilitudCoseno(consulta);
            var duracion = DateTime.Now - inicioTiempo;

            Console.WriteLine($"\n📊 RESULTADOS DE BÚSQUEDA ({duracion.TotalMilliseconds:F2} ms)");
            Console.WriteLine("═════════════════════════════════════════════════════");

            if (resultados.Count == 0)
            {
                Console.WriteLine("❌ No se encontraron documentos relevantes para la consulta.");
                Console.WriteLine("💡 Sugerencias:");
                Console.WriteLine("   • Intente con otros términos de búsqueda");
                Console.WriteLine("   • Verifique que los documentos contengan palabras relacionadas");
                Console.WriteLine("   • Use términos más generales o sinónimos");
                return;
            }

            Console.WriteLine($"✅ Encontrados {resultados.Count} documento(s) relevante(s)\n");

            MostrarResultadosBusqueda(resultados);
            MostrarInformacionAdicionalBusqueda(stats, duracion);
        }

        /// <summary>
        /// Mostrar resultados de búsqueda formateados
        /// </summary>
        private void MostrarResultadosBusqueda(ListaDobleEnlazada<ResultadoBusquedaVectorial> resultados)
        {
            var iterador = new Iterador<ResultadoBusquedaVectorial>(resultados);
            int posicion = 1;
            
            while (iterador.Siguiente() && posicion <= 10)
            {
                var resultado = iterador.Current;
                string nombreArchivo = Path.GetFileName(resultado.Documento.Ruta);
                double porcentajeSimilitud = resultado.SimilitudCoseno * 100;
                
                Console.WriteLine($"📄 {posicion}. {nombreArchivo}");
                Console.WriteLine($"   🎯 Similitud: {porcentajeSimilitud:F1}% ({GetBarraProgreso(porcentajeSimilitud)})");
                Console.WriteLine($"   🔗 Enlace Web: {GenerarEnlaceWebCompleto(resultado.Documento)}");
                Console.WriteLine($"   📁 Ubicación: {resultado.Documento.Ruta}");
                
                // string preview = ObtenerVistaPrevia(resultado.Documento);
                //Console.WriteLine($"   📝 Contenido: {preview}");
               // Console.WriteLine();
                
                posicion++;
            }

            if (resultados.Count > 10)
            {
                Console.WriteLine($"   ... y {resultados.Count - 10} resultado(s) adicional(es)");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Mostrar información adicional de la búsqueda
        /// </summary>
        private void MostrarInformacionAdicionalBusqueda(EstadisticasIndiceConZipf stats, TimeSpan duracion)
        {
            Console.WriteLine("💡 INFORMACIÓN DE LA BÚSQUEDA:");
            Console.WriteLine($"   ⚡ Tiempo de respuesta: {duracion.TotalMilliseconds:F2} ms");
            Console.WriteLine($"   🔤 Términos en índice: {stats.CantidadTerminos:N0}");
            Console.WriteLine($"   📄 Documentos indexados: {stats.CantidadDocumentos:N0}");
            Console.WriteLine($"   🔥 Optimización Zipf: {(stats.ZipfAplicado ? $"✅ ({stats.PercentilZipf}%)" : "❌")}");
            Console.WriteLine($"   🎯 Algoritmo: Similitud Coseno con TF-IDF");
            Console.WriteLine();
            Console.WriteLine("🌐 Para abrir un documento, copie el enlace web y péguelo en su navegador.");
        }

        /// <summary>
        /// OPCIÓN 2: Crear índice automático con Zipf
        /// </summary>
        private async Task CrearIndiceAutomatico()
        {
            Console.WriteLine("🔨 CREAR ÍNDICE AUTOMÁTICO CON LEY DE ZIPF");
            Console.WriteLine("==========================================");
            Console.WriteLine("📊 Configuración automática:");
            Console.WriteLine("   • Percentil Zipf: 15% (conservador)");
            Console.WriteLine("   • Estrategia: Eliminar términos muy frecuentes");
            Console.WriteLine("   • Algoritmo: RadixSort + TF-IDF + Similitud Coseno");
            Console.WriteLine();

            if (!await ValidarDirectorioDocumentos()) return;

            if (!gestor.IndiceEstaVacio())
            {
                Console.Write("⚠️ Ya existe un índice. ¿Desea sobrescribirlo? (s/N): ");
                string respuesta = Console.ReadLine()?.Trim().ToLower();
                if (respuesta != "s" && respuesta != "si" && respuesta != "sí")
                {
                    Console.WriteLine("❌ Operación cancelada.");
                    return;
                }
            }

            await EjecutarCreacionIndice(false);
        }

        /// <summary>
        /// OPCIÓN 3: Crear índice personalizado
        /// </summary>
        private async Task CrearIndicePersonalizado()
        {
            Console.WriteLine("⚙️ CREAR ÍNDICE CON CONFIGURACIÓN PERSONALIZADA");
            Console.WriteLine("===============================================");
            
            if (!await ValidarDirectorioDocumentos()) return;

            if (!gestor.IndiceEstaVacio())
            {
                Console.Write("⚠️ Ya existe un índice. ¿Desea sobrescribirlo? (s/N): ");
                string respuesta = Console.ReadLine()?.Trim().ToLower();
                if (respuesta != "s" && respuesta != "si" && respuesta != "sí")
                {
                    Console.WriteLine("❌ Operación cancelada.");
                    return;
                }
            }

            await EjecutarCreacionIndice(true);
        }

        /// <summary>
        /// Ejecutar creación de índice
        /// </summary>
        private async Task EjecutarCreacionIndice(bool personalizado)
        {
            Console.WriteLine("\n🚀 Iniciando creación del índice...");
            Console.WriteLine("📊 Proceso:");
            Console.WriteLine("   1️⃣ Cargar y procesar documentos");
            Console.WriteLine("   2️⃣ Aplicar algoritmo RadixSort");
            Console.WriteLine("   3️⃣ Aplicar Ley de Zipf (OBLIGATORIO)");
            Console.WriteLine("   4️⃣ Calcular métricas TF-IDF");
            Console.WriteLine("   5️⃣ Preparar búsqueda vectorial");
            Console.WriteLine();

            var inicioTiempo = DateTime.Now;
            bool exito;

            if (personalizado)
            {
                exito = await gestor.CrearIndiceConZipfPersonalizado(directorioDocumentos);
            }
            else
            {
                exito = await gestor.CrearIndiceDesdeDirectorio(directorioDocumentos);
            }

            var duracion = DateTime.Now - inicioTiempo;

            if (exito)
            {
                await MostrarResultadosCreacionExitosa(duracion);
            }
            else
            {
                Console.WriteLine("❌ Error durante la creación del índice.");
                Console.WriteLine("💡 Verifique que los archivos .txt sean accesibles y tengan contenido válido.");
            }
        }

        /// <summary>
        /// Mostrar resultados de creación exitosa
        /// </summary>
        private async Task MostrarResultadosCreacionExitosa(TimeSpan duracion)
        {
            var stats = gestor.ObtenerEstadisticas();
            
            Console.WriteLine($"✅ ÍNDICE CREADO EXITOSAMENTE en {duracion.TotalSeconds:F2} segundos");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine($"📄 Documentos procesados: {stats.CantidadDocumentos}");
            Console.WriteLine($"🔤 Términos únicos indexados: {stats.CantidadTerminos:N0}");
            Console.WriteLine($"📈 Promedio términos/documento: {stats.PromedioTerminosPorDocumento:F1}");
            Console.WriteLine($"⚡ RadixSort aplicado: {(stats.IndiceOrdenado ? "✅ Sí" : "❌ No")}");
            Console.WriteLine($"🔥 Ley de Zipf aplicada: {(stats.ZipfAplicado ? $"✅ Sí ({stats.PercentilZipf}%)" : "❌ No")}");
            Console.WriteLine($"💾 Memoria estimada: {stats.MemoriaEstimadaKB:N0} KB");
            Console.WriteLine();

            // Guardar automáticamente
            Console.WriteLine("💾 Guardando índice automáticamente...");
            if (gestor.GuardarIndice(archivoIndice))
            {
                var fileInfo = new FileInfo(archivoIndice);
                Console.WriteLine($"✅ Índice guardado: {archivoIndice} ({fileInfo.Length / 1024.0:F1} KB)");
            }
            else
            {
                Console.WriteLine("⚠️ Advertencia: No se pudo guardar el índice automáticamente.");
            }
        }

        /// <summary>
        /// OPCIÓN 4: Mostrar estadísticas detalladas
        /// </summary>
        private void MostrarEstadisticasDetalladas()
        {
            Console.WriteLine("📊 ESTADÍSTICAS DETALLADAS DEL SISTEMA");
            Console.WriteLine("=======================================");
            
            if (gestor.IndiceEstaVacio())
            {
                Console.WriteLine("❌ No hay índice cargado en el sistema.");
                Console.WriteLine("💡 Use la opción 2 o 3 para crear un nuevo índice.");
                return;
            }

            var stats = gestor.ObtenerEstadisticas();
            var validacion = gestor.ValidarIntegridad();

            // Información básica
            Console.WriteLine("📋 INFORMACIÓN BÁSICA:");
            Console.WriteLine($"   📄 Documentos indexados: {stats.CantidadDocumentos:N0}");
            Console.WriteLine($"   🔤 Términos únicos: {stats.CantidadTerminos:N0}");
            Console.WriteLine($"   📈 Promedio términos/documento: {stats.PromedioTerminosPorDocumento:F1}");
            Console.WriteLine($"   💾 Memoria estimada: {stats.MemoriaEstimadaKB:N0} KB");
            Console.WriteLine();

            // Información técnica
            Console.WriteLine("⚡ ALGORITMOS Y OPTIMIZACIONES:");
            Console.WriteLine($"   🔄 RadixSort aplicado: {(stats.IndiceOrdenado ? "✅ Sí - O(n×k)" : "❌ No")}");
            Console.WriteLine($"   🔍 Búsqueda de términos: {(stats.IndiceOrdenado ? "✅ Binaria O(log n)" : "❌ Lineal O(n)")}");
            Console.WriteLine($"   📊 Cálculo TF-IDF: ✅ Implementado");
            Console.WriteLine($"   🎯 Similitud coseno: ✅ Vectorial");
            Console.WriteLine();

            // Ley de Zipf (CRÍTICO)
            Console.WriteLine("🔥 LEY DE ZIPF (REQUISITO OBLIGATORIO):");
            Console.WriteLine($"   Estado: {(stats.ZipfAplicado ? "✅ APLICADA" : "❌ NO APLICADA")}");
            if (stats.ZipfAplicado)
            {
                Console.WriteLine($"   Percentil configurado: {stats.PercentilZipf}%");
                Console.WriteLine($"   Estrategia utilizada: {stats.EstrategiaZipf}");
                Console.WriteLine($"   Términos optimizados: ✅ Sí");
            }
            else
            {
                Console.WriteLine($"   ⚠️ ATENCIÓN: Zipf es OBLIGATORIO según el enunciado");
                Console.WriteLine($"   🔧 Solución: Recrear índice con opción 2 o 3");
            }
            Console.WriteLine();

            // Información de archivos
            Console.WriteLine("📁 INFORMACIÓN DE PERSISTENCIA:");
            if (File.Exists(archivoIndice))
            {
                var fileInfo = new FileInfo(archivoIndice);
                Console.WriteLine($"   📄 Archivo: {archivoIndice}");
                Console.WriteLine($"   📏 Tamaño: {fileInfo.Length / 1024.0:F1} KB");
                Console.WriteLine($"   📅 Última modificación: {fileInfo.LastWriteTime:dd/MM/yyyy HH:mm:ss}");
            }
            else
            {
                Console.WriteLine($"   ❌ No existe archivo de índice guardado");
            }
            Console.WriteLine();

            // Validación del sistema
            Console.WriteLine("🔍 VALIDACIÓN DEL SISTEMA:");
            Console.WriteLine($"   Estado general: {(validacion.EsValido ? "✅ SISTEMA VÁLIDO" : "❌ SISTEMA INVÁLIDO")}");
            if (!validacion.EsValido)
            {
                Console.WriteLine($"   ⚠️ Problemas detectados: {validacion.Mensaje}");
            }
            else
            {
                Console.WriteLine($"   ✅ {validacion.Mensaje}");
            }
        }

        /// <summary>
        /// OPCIÓN 5: Guardar índice
        /// </summary>
        private void GuardarIndiceEnArchivo()
        {
            Console.WriteLine("💾 GUARDAR ÍNDICE EN ARCHIVO");
            Console.WriteLine("============================");
            
            if (gestor.IndiceEstaVacio())
            {
                Console.WriteLine("❌ No hay índice para guardar.");
                Console.WriteLine("💡 Cree un índice primero usando la opción 2 o 3.");
                return;
            }

            Console.WriteLine($"📄 Guardando en: {archivoIndice}");
            Console.WriteLine("⏳ Serializando estructuras de datos...");
            
            if (gestor.GuardarIndice(archivoIndice))
            {
                var fileInfo = new FileInfo(archivoIndice);
                var stats = gestor.ObtenerEstadisticas();
                
                Console.WriteLine("✅ Índice guardado exitosamente");
                Console.WriteLine($"📏 Tamaño del archivo: {fileInfo.Length / 1024.0:F1} KB");
                Console.WriteLine($"🔄 Estructura vectorial preservada: ✅");
                
                if (stats.ZipfAplicado)
                {
                    Console.WriteLine($"🔥 Configuración Zipf preservada: ✅ ({stats.PercentilZipf}%)");
                }
                
                Console.WriteLine($"📅 Guardado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            }
            else
            {
                Console.WriteLine("❌ Error al guardar el índice.");
                Console.WriteLine("💡 Verifique permisos de escritura en el directorio.");
            }
        }

        /// <summary>
        /// OPCIÓN 6: Cargar índice
        /// </summary>
        private async Task CargarIndiceDesdeArchivo()
        {
            Console.WriteLine("📂 CARGAR ÍNDICE DESDE ARCHIVO");
            Console.WriteLine("===============================");
            
            if (!File.Exists(archivoIndice))
            {
                Console.WriteLine($"❌ No se encontró el archivo: {archivoIndice}");
                Console.WriteLine("💡 Cree un nuevo índice usando la opción 2 o 3.");
                return;
            }

            if (!gestor.IndiceEstaVacio())
            {
                Console.Write("⚠️ Ya existe un índice en memoria. ¿Desea sobrescribirlo? (s/N): ");
                string respuesta = Console.ReadLine()?.Trim().ToLower();
                if (respuesta != "s" && respuesta != "si" && respuesta != "sí")
                {
                    Console.WriteLine("❌ Operación cancelada.");
                    return;
                }
            }

            var fileInfo = new FileInfo(archivoIndice);
            Console.WriteLine($"📄 Archivo: {archivoIndice} ({fileInfo.Length / 1024.0:F1} KB)");
            Console.WriteLine($"📅 Modificado: {fileInfo.LastWriteTime:dd/MM/yyyy HH:mm:ss}");
            Console.WriteLine("⏳ Cargando y deserializando...");

            if (gestor.CargarIndice(archivoIndice))
            {
                var stats = gestor.ObtenerEstadisticas();
                
                Console.WriteLine("✅ Índice cargado exitosamente");
                Console.WriteLine($"📊 {stats.CantidadDocumentos} documentos, {stats.CantidadTerminos:N0} términos");
                Console.WriteLine($"⚡ RadixSort restaurado: {(stats.IndiceOrdenado ? "✅" : "❌")}");
                
                if (stats.ZipfAplicado)
                {
                    Console.WriteLine($"🔥 Ley de Zipf restaurada: ✅ ({stats.PercentilZipf}%)");
                    Console.WriteLine($"🔧 Estrategia: {stats.EstrategiaZipf}");
                }
                else
                {
                    Console.WriteLine("⚠️ ADVERTENCIA: Índice sin Ley de Zipf");
                    Console.WriteLine("   Esto no cumple con el enunciado del proyecto.");
                }
                
                Console.WriteLine("🎯 Sistema listo para búsquedas");
            }
            else
            {
                Console.WriteLine("❌ Error al cargar el índice.");
                Console.WriteLine("💡 El archivo podría estar corrupto o ser incompatible.");
            }
        }

        /// <summary>
        /// OPCIÓN 7: Modificar configuración de Zipf
        /// </summary>
        private async Task ModificarConfiguracionZipf()
        {
            Console.WriteLine("🔥 MODIFICAR CONFIGURACIÓN DE LEY DE ZIPF");
            Console.WriteLine("==========================================");
            
            if (gestor.IndiceEstaVacio())
            {
                Console.WriteLine("❌ No hay índice cargado para modificar.");
                Console.WriteLine("💡 Cree un índice primero usando la opción 2 o 3.");
                return;
            }

            var stats = gestor.ObtenerEstadisticas();
            Console.WriteLine($"Configuración actual:");
            Console.WriteLine($"   🔥 Zipf aplicado: {(stats.ZipfAplicado ? $"✅ Sí ({stats.PercentilZipf}%)" : "❌ No")}");
            if (stats.ZipfAplicado)
            {
                Console.WriteLine($"   🔧 Estrategia: {stats.EstrategiaZipf}");
            }
            Console.WriteLine();

            // Mostrar estrategias disponibles
            var estrategias = FabricaEstrategias.ObtenerEstrategiasDisponibles();
            Console.WriteLine("Estrategias disponibles:");
            for (int i = 0; i < estrategias.Count; i++)
            {
                Console.WriteLine($"   {i + 1}. {estrategias[i]}");
            }
            Console.WriteLine();

            // Seleccionar nueva estrategia
            Console.Write("Seleccione nueva estrategia (1-4): ");
            string inputEstrategia = Console.ReadLine()?.Trim();
            if (!int.TryParse(inputEstrategia, out int seleccion) || seleccion < 1 || seleccion > 4)
            {
                Console.WriteLine("❌ Selección inválida. Operación cancelada.");
                return;
            }

            var nuevaEstrategia = (FabricaEstrategias.TipoEstrategia)(seleccion - 1);

            // Seleccionar nuevo percentil
            Console.Write("Ingrese nuevo percentil (1-30%): ");
            string inputPercentil = Console.ReadLine()?.Trim();
            if (!int.TryParse(inputPercentil, out int nuevoPercentil) || nuevoPercentil < 1 || nuevoPercentil > 30)
            {
                Console.WriteLine("❌ Percentil inválido. Operación cancelada.");
                return;
            }

            Console.WriteLine($"\n🔄 Aplicando nueva configuración:");
            Console.WriteLine($"   📊 Percentil: {nuevoPercentil}%");
            Console.WriteLine($"   🔧 Estrategia: {nuevaEstrategia}");
            Console.WriteLine("⏳ Recalculando índice...");

            bool exito = await gestor.ModificarZipf(nuevoPercentil, nuevaEstrategia);

            if (exito)
            {
                var nuevasStats = gestor.ObtenerEstadisticas();
                Console.WriteLine("✅ Configuración de Zipf modificada exitosamente");
                Console.WriteLine($"📊 Términos resultantes: {nuevasStats.CantidadTerminos:N0}");
                Console.WriteLine($"🎯 Sistema listo para búsquedas optimizadas");
            }
            else
            {
                Console.WriteLine("❌ Error al modificar configuración de Zipf.");
            }
        }

        /// <summary>
        /// OPCIÓN 8: Limpiar sistema
        /// </summary>
        private void LimpiarSistemaCompleto()
        {
            Console.WriteLine("🧹 LIMPIAR SISTEMA Y MEMORIA");
            Console.WriteLine("=============================");
            
            if (gestor.IndiceEstaVacio())
            {
                Console.WriteLine("ℹ️ El sistema ya está limpio (sin índice cargado).");
                return;
            }

            Console.Write("⚠️ Esto eliminará el índice de la memoria. ¿Continuar? (s/N): ");
            string respuesta = Console.ReadLine()?.Trim().ToLower();
            
            if (respuesta != "s" && respuesta != "si" && respuesta != "sí")
            {
                Console.WriteLine("❌ Operación cancelada.");
                return;
            }

            Console.WriteLine("🧹 Limpiando estructuras de datos...");
            Console.WriteLine("🗑️ Liberando memoria...");
            
            gestor.LimpiarSistema();
            
            Console.WriteLine("✅ Sistema limpiado exitosamente");
            Console.WriteLine("💡 El archivo guardado no se vio afectado");
        }

        /// <summary>
        /// Pausar y limpiar pantalla
        /// </summary>
        private void PausarYLimpiar()
        {
            Console.WriteLine("\n" + new string('═', 60));
            Console.Write("Presione Enter para continuar...");
            Console.ReadLine();
            Console.Clear();
        }

        /// <summary>
        /// Mostrar mensaje de despedida
        /// </summary>
        private void MostrarDespedida()
        {
            Console.Clear();
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                          ¡HASTA PRONTO!                      ║");
            Console.WriteLine("║                 Gracias por usar el Motor de Búsqueda        ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("🎯 Sistema desarrollado con:");
            Console.WriteLine("   ✅ Índice Invertido + Vector personalizado + RadixSort");
            Console.WriteLine("   ✅ Ley de Zipf aplicada según enunciado");
            Console.WriteLine("   ✅ Búsqueda vectorial con similitud coseno");
            Console.WriteLine("   ✅ Estructuras de datos implementadas desde cero");
            Console.WriteLine();
            Console.WriteLine("Presione Enter para salir...");
            Console.ReadLine();
        }

        #region Métodos de Utilidad

        /// <summary>
        /// Validar que existe el directorio y tiene archivos
        /// </summary>
        private async Task<bool> ValidarDirectorioDocumentos()
        {
            if (!Directory.Exists(directorioDocumentos))
            {
                Console.WriteLine($"❌ Directorio no encontrado: {directorioDocumentos}");
                Console.WriteLine("💡 Cree el directorio y agregue archivos .txt para indexar.");
                return false;
            }

            var archivos = Directory.GetFiles(directorioDocumentos, "*.txt");
            if (archivos.Length == 0)
            {
                Console.WriteLine($"❌ No se encontraron archivos .txt en: {directorioDocumentos}");
                Console.WriteLine("💡 Agregue archivos de texto (.txt) al directorio para crear el índice.");
                return false;
            }

            Console.WriteLine($"✅ Directorio válido: {archivos.Length} archivo(s) .txt encontrado(s)");
            return true;
        }

        /// <summary>
        /// Generar enlace web completo y estilizado
        /// </summary>
        private string GenerarEnlaceWebCompleto(Documento documento)
        {
            try
            {
                string contenido = File.Exists(documento.Ruta) 
                    ? File.ReadAllText(documento.Ruta)
                    : documento.TextoOriginal ?? "Contenido no disponible";
                
                string contenidoLimpio = LimpiarContenidoParaHtml(contenido);
                string nombreArchivo = Path.GetFileName(documento.Ruta);
                string html = CrearHtmlProfesional(contenidoLimpio, nombreArchivo);
                
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(html);
                string base64 = Convert.ToBase64String(bytes);
                
                return $"data:text/html;charset=utf-8;base64,{base64}";
            }
            catch (Exception ex)
            {
                string errorHtml = CrearHtmlError(documento.Ruta, ex.Message);
                byte[] errorBytes = System.Text.Encoding.UTF8.GetBytes(errorHtml);
                string errorBase64 = Convert.ToBase64String(errorBytes);
                return $"data:text/html;charset=utf-8;base64,{errorBase64}";
            }
        }

        /// <summary>
        /// Crear HTML profesional y estilizado
        /// </summary>
        private string CrearHtmlProfesional(string contenido, string nombreArchivo)
        {
            return $@"<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{nombreArchivo} - Motor de Búsqueda Vectorial</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            line-height: 1.7;
            color: #2c3e50;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            padding: 20px;
        }}
        .container {{
            max-width: 900px;
            margin: 0 auto;
            background: white;
            border-radius: 20px;
            box-shadow: 0 20px 40px rgba(0,0,0,0.1);
            overflow: hidden;
            animation: slideIn 0.6s ease-out;
        }}
        @keyframes slideIn {{
            from {{ opacity: 0; transform: translateY(30px); }}
            to {{ opacity: 1; transform: translateY(0); }}
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 40px 30px;
            text-align: center;
            position: relative;
        }}
        .header::before {{
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: url('data:image/svg+xml,<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 100 100""><defs><pattern id=""grain"" width=""100"" height=""100"" patternUnits=""userSpaceOnUse""><circle cx=""50"" cy=""50"" r=""1"" fill=""white"" opacity=""0.1""/></pattern></defs><rect width=""100"" height=""100"" fill=""url(%23grain)""/></svg>');
        }}
        .header-content {{
            position: relative;
            z-index: 1;
        }}
        h1 {{
            font-size: 2.5em;
            margin-bottom: 10px;
            text-shadow: 2px 2px 4px rgba(0,0,0,0.3);
            font-weight: 700;
        }}
        .subtitle {{
            font-size: 1.1em;
            opacity: 0.9;
            margin-bottom: 20px;
        }}
        .tech-badges {{
            display: flex;
            justify-content: center;
            flex-wrap: wrap;
            gap: 10px;
            margin-top: 20px;
        }}
        .badge {{
            background: rgba(255,255,255,0.2);
            padding: 8px 16px;
            border-radius: 25px;
            font-size: 0.9em;
            font-weight: 500;
            backdrop-filter: blur(10px);
            border: 1px solid rgba(255,255,255,0.1);
        }}
        .content {{
            padding: 40px;
            font-size: 1.1em;
            line-height: 1.8;
            font-family: 'Georgia', 'Times New Roman', serif;
        }}
        .content pre {{
            white-space: pre-wrap;
            word-wrap: break-word;
            background: #f8f9fa;
            padding: 20px;
            border-radius: 10px;
            border-left: 4px solid #667eea;
            font-family: 'Georgia', serif;
            font-size: 1em;
        }}
        .footer {{
            background: #f8f9fa;
            padding: 30px;
            text-align: center;
            border-top: 1px solid #e9ecef;
        }}
        .footer h3 {{
            color: #495057;
            margin-bottom: 15px;
            font-size: 1.2em;
        }}
        .tech-info {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 15px;
            margin: 20px 0;
            text-align: left;
        }}
        .tech-item {{
            background: white;
            padding: 15px;
            border-radius: 10px;
            border: 1px solid #e9ecef;
            box-shadow: 0 2px 5px rgba(0,0,0,0.05);
        }}
        .credits {{
            font-size: 0.9em;
            color: #6c757d;
            margin-top: 20px;
            padding-top: 20px;
            border-top: 1px solid #e9ecef;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='header-content'>
                <h1>📄 {nombreArchivo}</h1>
                <p class='subtitle'>Motor de Búsqueda Vectorial con IA</p>
                <div class='tech-badges'>
                    <span class='badge'>🔥 Ley de Zipf</span>
                    <span class='badge'>⚡ RadixSort</span>
                    <span class='badge'>🎯 TF-IDF</span>
                    <span class='badge'>📊 Similitud Coseno</span>
                    <span class='badge'>🔍 Búsqueda Vectorial</span>
                </div>
            </div>
        </div>
        
        <div class='content'>
            <pre>{contenido}</pre>
        </div>
        
        <div class='footer'>
            <h3>🔬 Tecnologías Implementadas</h3>
            <div class='tech-info'>
                <div class='tech-item'>
                    <strong>🏗️ Estructura de Datos</strong><br>
                    Índice Invertido con Vector personalizado
                </div>
                <div class='tech-item'>
                    <strong>⚡ Algoritmos</strong><br>
                    RadixSort O(n×k) + Búsqueda Binaria O(log n)
                </div>
                <div class='tech-item'>
                    <strong>🔥 Optimización</strong><br>
                    Ley de Zipf para filtrado inteligente
                </div>
                <div class='tech-item'>
                    <strong>🎯 Recuperación</strong><br>
                    TF-IDF + Similitud Coseno Vectorial
                </div>
                <div class='tech-item'>
                    <strong>🔄 Patrones</strong><br>
                    Singleton, Strategy, Iterator
                </div>
                <div class='tech-item'>
                    <strong>💾 Persistencia</strong><br>
                    Serialización binaria optimizada
                </div>
            </div>
            
            <div class='credits'>
                <strong>🎓 Proyecto Académico - Estructuras de Datos</strong><br>
                Implementación completa sin librerías externas<br>
                C# .NET 8.0 • Vector personalizado • RadixSort • Ley de Zipf<br>
                <em>Generado automáticamente por el Motor de Búsqueda</em>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// Limpiar contenido para HTML
        /// </summary>
        private string LimpiarContenidoParaHtml(string contenido)
        {
            if (string.IsNullOrEmpty(contenido))
                return "📄 Contenido no disponible";

            // Escapar caracteres HTML
            contenido = contenido
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");

            // Limitar longitud para performance
            if (contenido.Length > 15000)
            {
                contenido = contenido.Substring(0, 15000) + 
                    "\n\n📎 [Contenido truncado para optimizar la visualización web]";
            }

            return contenido;
        }

        /// <summary>
        /// Crear HTML de error
        /// </summary>
        private string CrearHtmlError(string ruta, string error)
        {
            return $@"<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <title>Error - Motor de Búsqueda</title>
    <style>
        body {{ 
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background: linear-gradient(135deg, #ff6b6b, #ee5a24);
            margin: 0;
            padding: 40px;
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
        }}
        .error-container {{ 
            background: white;
            padding: 40px;
            border-radius: 15px;
            box-shadow: 0 10px 30px rgba(0,0,0,0.2);
            max-width: 500px;
            text-align: center;
        }}
        h2 {{ color: #e74c3c; margin-bottom: 20px; }}
        .error-details {{ 
            background: #f8f9fa;
            padding: 20px;
            border-radius: 10px;
            margin: 20px 0;
            text-align: left;
        }}
        .suggestions {{ 
            background: #e8f5e8;
            padding: 15px;
            border-radius: 8px;
            border-left: 4px solid #28a745;
        }}
    </style>
</head>
<body>
    <div class='error-container'>
        <h2>❌ Error al cargar documento</h2>
        <div class='error-details'>
            <p><strong>📄 Archivo:</strong> {Path.GetFileName(ruta)}</p>
            <p><strong>🔧 Error:</strong> {error}</p>
            <p><strong>📁 Ruta:</strong> {ruta}</p>
        </div>
        <div class='suggestions'>
            <h4>💡 Posibles soluciones:</h4>
            <ul style='text-align: left;'>
                <li>Verificar que el archivo no haya sido movido o eliminado</li>
                <li>Comprobar permisos de acceso al archivo</li>
                <li>Recrear el índice si el problema persiste</li>
                <li>Verificar que el archivo tenga contenido válido</li>
            </ul>
        </div>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// Obtener vista previa del contenido
        /// </summary>
        private string ObtenerVistaPrevia(Documento documento)
        {
            try
            {
                string contenido = File.Exists(documento.Ruta) 
                    ? File.ReadAllText(documento.Ruta)
                    : documento.TextoOriginal ?? "";

                if (string.IsNullOrWhiteSpace(contenido))
                    return "Sin contenido disponible";

                // Limpiar y obtener primeras líneas
                var lineas = contenido.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                var lineasSignificativas = lineas.Where(l => !string.IsNullOrWhiteSpace(l)).Take(2);
                
                string preview = string.Join(" ", lineasSignificativas);
                
                if (preview.Length > 120)
                    preview = preview.Substring(0, 120) + "...";

                return preview.Trim();
            }
            catch
            {
                return "Error al obtener vista previa";
            }
        }

        /// <summary>
        /// Generar barra de progreso visual para similitud
        /// </summary>
        private string GetBarraProgreso(double porcentaje)
        {
            int barras = (int)(porcentaje / 10);
            string lleno = new string('█', barras);
            string vacio = new string('░', 10 - barras);
            return lleno + vacio;
        }

        #endregion
    }
}