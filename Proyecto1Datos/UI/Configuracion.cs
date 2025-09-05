namespace PruebaRider.Configuracion
{
    /// <summary>
    /// Configuración centralizada del sistema
    /// - Constantes globales
    /// - Configuraciones por defecto
    /// - Parámetros del sistema
    /// </summary>
    public static class ConfiguracionSistema
    {
        #region Configuración de Archivos y Directorios

        /// <summary>
        /// Nombre por defecto del archivo de índice
        /// </summary>
        public const string ARCHIVO_INDICE_DEFECTO = "indice_radix.bin";

        /// <summary>
        /// Extensión de archivos de texto a procesar
        /// </summary>
        public const string EXTENSION_DOCUMENTOS = "*.txt";

        /// <summary>
        /// Directorio por defecto para documentos
        /// </summary>
        public const string DIRECTORIO_DOCUMENTOS_DEFECTO = "Documentos";

        /// <summary>
        /// Rutas posibles para buscar documentos
        /// </summary>
        public static readonly string[] RUTAS_DOCUMENTOS_POSIBLES = {
            @"C:\Users\ignab\OneDrive\Documents\Estructuras de datos\Documentos",
            @".\Documentos",
            @".\docs",
            @"C:\Documentos",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Documentos"),
            Path.Combine(Environment.CurrentDirectory, "Documentos")
        };

        #endregion

        #region Configuración del Vector y RadixSort

        /// <summary>
        /// Capacidad inicial por defecto del vector ordenado
        /// </summary>
        public const int CAPACIDAD_INICIAL_VECTOR = 100;

        /// <summary>
        /// Factor de crecimiento del vector
        /// </summary>
        public const double FACTOR_CRECIMIENTO_VECTOR = 2.0;

        /// <summary>
        /// Tamaño máximo del alfabeto para RadixSort (ASCII extendido)
        /// </summary>
        public const int TAMAÑO_ALFABETO_RADIX = 256;

        #endregion

        #region Configuración de Búsqueda

        /// <summary>
        /// Umbral mínimo para similitud coseno
        /// </summary>
        public const double UMBRAL_SIMILITUD_MINIMO = 0.0001;

        /// <summary>
        /// Número máximo de resultados a mostrar por defecto
        /// </summary>
        public const int MAX_RESULTADOS_DEFECTO = 10;

        /// <summary>
        /// Longitud mínima de tokens para procesar
        /// </summary>
        public const int LONGITUD_MINIMA_TOKEN = 3;

        /// <summary>
        /// Umbral para cambiar de bubble sort a array sort
        /// </summary>
        public const int UMBRAL_BUBBLE_SORT = 20;

        #endregion

        #region Configuración de Optimización

        /// <summary>
        /// Epsilon para cálculos de punto flotante
        /// </summary>
        public const double EPSILON = 1e-12;

        /// <summary>
        /// Umbral para usar búsqueda binaria vs lineal
        /// </summary>
        public const int UMBRAL_BUSQUEDA_BINARIA = 15;

        /// <summary>
        /// Número máximo de términos antes de aplicar optimizaciones
        /// </summary>
        public const int MAX_TERMINOS_SIN_OPTIMIZACION = 1000;

        #endregion

        #region Configuración de Memoria

        /// <summary>
        /// Límite de memoria estimada en KB antes de mostrar advertencia
        /// </summary>
        public const int LIMITE_MEMORIA_ADVERTENCIA_KB = 100 * 1024; // 100 MB

        /// <summary>
        /// Intervalo para forzar recolección de basura (número de operaciones)
        /// </summary>
        public const int INTERVALO_GC = 1000;

        #endregion

        #region Configuración de Interfaz

        /// <summary>
        /// Ancho preferido de la consola
        /// </summary>
        public const int ANCHO_CONSOLA_PREFERIDO = 120;

        /// <summary>
        /// Alto preferido de la consola
        /// </summary>
        public const int ALTO_CONSOLA_PREFERIDO = 40;

        /// <summary>
        /// Longitud máxima para vista previa de contenido
        /// </summary>
        public const int LONGITUD_MAXIMA_PREVIEW = 150;

        /// <summary>
        /// Número de líneas de stack trace a mostrar en errores
        /// </summary>
        public const int LINEAS_STACK_TRACE_MOSTRAR = 10;

        #endregion

        #region Configuración de Ley de Zipf

        /// <summary>
        /// Porcentaje máximo permitido para eliminación de términos
        /// </summary>
        public const int MAX_PORCENTAJE_ELIMINACION_ZIPF = 30;

        /// <summary>
        /// Umbral conservador para eliminación de términos frecuentes (85% de documentos)
        /// </summary>
        public const double UMBRAL_FRECUENCIA_CONSERVADOR = 0.85;

        /// <summary>
        /// Límite de seguridad para eliminación automática (20% máximo)
        /// </summary>
        public const int LIMITE_SEGURIDAD_ELIMINACION = 20;

        #endregion

        #region Métodos de Configuración

        /// <summary>
        /// Obtener directorio de documentos válido
        /// </summary>
        public static string ObtenerDirectorioDocumentosValido()
        {
            foreach (var ruta in RUTAS_DOCUMENTOS_POSIBLES)
            {
                try
                {
                    if (Directory.Exists(ruta))
                    {
                        var archivos = Directory.GetFiles(ruta, EXTENSION_DOCUMENTOS);
                        if (archivos.Length > 0)
                        {
                            return ruta;
                        }
                    }
                }
                catch
                {
                    // Ignorar errores de acceso y continuar con la siguiente ruta
                    continue;
                }
            }

            // Si no se encuentra ninguna ruta válida, devolver la primera como defecto
            return RUTAS_DOCUMENTOS_POSIBLES[0];
        }

        /// <summary>
        /// Validar configuración de memoria
        /// </summary>
        public static bool ValidarConfiguracionMemoria()
        {
            try
            {
                GC.Collect();
                long memoriaActual = GC.GetTotalMemory(false);
                return memoriaActual < LIMITE_MEMORIA_ADVERTENCIA_KB * 1024;
            }
            catch
            {
                return true; // Asumir que está bien si no se puede verificar
            }
        }

        /// <summary>
        /// Obtener configuración de consola segura
        /// </summary>
        public static (int ancho, int alto) ObtenerConfiguracionConsola()
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    int anchoMax = Console.LargestWindowWidth;
                    int altoMax = Console.LargestWindowHeight;

                    int ancho = Math.Min(ANCHO_CONSOLA_PREFERIDO, anchoMax);
                    int alto = Math.Min(ALTO_CONSOLA_PREFERIDO, altoMax);

                    return (ancho, alto);
                }
            }
            catch
            {
                // Si falla, usar valores por defecto conservadores
            }

            return (80, 25); // Valores seguros por defecto
        }

        /// <summary>
        /// Verificar si el sistema soporta características avanzadas
        /// </summary>
        public static bool SoportaCaracteristicasAvanzadas()
        {
            try
            {
                // Verificar soporte UTF-8
                var testString = "áéíóúñü🔍📄💾";
                var bytes = System.Text.Encoding.UTF8.GetBytes(testString);
                var restored = System.Text.Encoding.UTF8.GetString(bytes);

                if (testString != restored)
                    return false;

                // Verificar operaciones de archivo
                string tempFile = Path.GetTempFileName();
                File.WriteAllText(tempFile, "test");
                File.Delete(tempFile);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Información del Sistema

        /// <summary>
        /// Versión del sistema
        /// </summary>
        public const string VERSION_SISTEMA = "2.0.0";

        /// <summary>
        /// Nombre del sistema
        /// </summary>
        public const string NOMBRE_SISTEMA = "Motor de Búsqueda - Índice Invertido";

        /// <summary>
        /// Descripción del sistema
        /// </summary>
        public const string DESCRIPCION_SISTEMA = "Sistema avanzado de búsqueda con vector personalizado, RadixSort y similitud coseno";

        /// <summary>
        /// Tecnologías utilizadas
        /// </summary>
        public static readonly string[] TECNOLOGIAS = {
            "C# .NET 9.0",
            "Estructuras de datos personalizadas",
            "Algoritmo RadixSort",
            "Búsqueda vectorial TF-IDF",
            "Similitud coseno",
            "Serialización binaria",
            "Patrón Strategy (Ley de Zipf)",
            "Patrón Singleton"
        };

        /// <summary>
        /// Características implementadas
        /// </summary>
        public static readonly string[] CARACTERISTICAS = {
            "Vector ordenado personalizado para índice invertido",
            "Algoritmo RadixSort para ordenamiento óptimo",
            "Búsqueda vectorial con similitud coseno exacta",
            "Estructuras de datos propias (sin genéricos)",
            "Optimización O(log n) en búsquedas binarias",
            "Gestión completa de índices (crear/cargar/guardar)",
            "Aplicación de Ley de Zipf para optimización",
            "Interfaz de usuario completa e intuitiva",
            "Manejo robusto de errores",
            "Generación automática de enlaces base64"
        };

        #endregion
    }
}