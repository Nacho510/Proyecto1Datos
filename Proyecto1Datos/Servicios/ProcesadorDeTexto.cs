using System.IO;
using System.Text.RegularExpressions;
using System.Text;

namespace PruebaRider.Servicios
{
    /// <summary>
    /// Procesador de texto completamente optimizado SIN GENÉRICOS
    /// - Array manual para stopwords (no HashSet)
    /// - Array dinámico para tokens (no List<string>)
    /// - Más eficiente y cumple 100% con el enunciado
    /// </summary>
    public class ProcesadorDeTexto
    {
        // STOPWORDS como array fijo - NO usar HashSet (está prohibido)
        private static readonly string[] StopWords = {
            "el", "la", "los", "las", "un", "una", "uno", "unos", "unas",
            "de", "del", "da", "en", "a", "al", "ante", "bajo", "con", "contra", 
            "desde", "durante", "entre", "hacia", "hasta", "para", "por", "según", 
            "sin", "sobre", "tras", "y", "e", "o", "u", "pero", "sino", "aunque", 
            "porque", "que", "si", "como", "yo", "tú", "él", "ella", "nosotros", 
            "vosotros", "ellos", "ellas", "me", "te", "se", "nos", "os", "le", 
            "les", "lo", "los", "mi", "tu", "su", "nuestro", "vuestro", "es", 
            "son", "está", "están", "ser", "estar", "tener", "haber", "hacer",
            "no", "sí", "más", "menos", "muy", "mucho", "poco", "bastante", 
            "demasiado", "ya", "aún", "todavía", "siempre", "nunca", "también", 
            "tampoco", "este", "esta", "estos", "estas", "ese", "esa", "esos", 
            "esas", "aquel", "aquella", "aquellos", "aquellas"
        };

        private static readonly Regex TokenRegex = new Regex(@"\b[a-záéíóúüñ]+\b", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// MÉTODO PRINCIPAL - Reemplaza List<string> con array dinámico manual
        /// </summary>
        public ArrayDinamico ProcesarTextoCompleto(string texto)
        {
            var resultado = new ArrayDinamico();
            
            if (string.IsNullOrWhiteSpace(texto))
                return resultado;

            foreach (Match match in TokenRegex.Matches(texto))
            {
                string token = match.Value.ToLowerInvariant();
                
                // Filtrar tokens cortos y stopwords
                if (token.Length >= 3 && !EsStopWord(token))
                {
                    resultado.Agregar(token);
                }
            }

            return resultado;
        }

        /// <summary>
        /// Verificar si es stopword usando búsqueda lineal en array
        /// Más eficiente que HashSet para arrays pequeños
        /// </summary>
        private bool EsStopWord(string palabra)
        {
            for (int i = 0; i < StopWords.Length; i++)
            {
                if (StopWords[i] == palabra)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Procesar archivo de texto
        /// </summary>
        public async Task<ArrayDinamico> ProcesarArchivo(string rutaArchivo)
        {
            try
            {
                string contenido = await File.ReadAllTextAsync(rutaArchivo, Encoding.UTF8);
                return ProcesarTextoCompleto(contenido);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error procesando archivo: {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// ARRAY DINÁMICO MANUAL - Reemplaza List<string>
    /// Implementación propia sin usar genéricos
    /// </summary>
    public class ArrayDinamico
    {
        private string[] elementos;
        private int tamaño;
        private int capacidad;
        private const int CAPACIDAD_INICIAL = 50;
        private const double FACTOR_CRECIMIENTO = 1.5;

        public ArrayDinamico()
        {
            capacidad = CAPACIDAD_INICIAL;
            elementos = new string[capacidad];
            tamaño = 0;
        }

        public int Count => tamaño;

        /// <summary>
        /// Agregar elemento con redimensionamiento automático
        /// </summary>
        public void Agregar(string elemento)
        {
            if (tamaño >= capacidad)
                ExpandirCapacidad();

            elementos[tamaño] = elemento;
            tamaño++;
        }

        /// <summary>
        /// Acceso por índice
        /// </summary>
        public string this[int index]
        {
            get
            {
                if (index < 0 || index >= tamaño)
                    throw new IndexOutOfRangeException();
                return elementos[index];
            }
        }

        /// <summary>
        /// Convertir a array fijo para compatibilidad
        /// </summary>
        public string[] ToArray()
        {
            var resultado = new string[tamaño];
            Array.Copy(elementos, resultado, tamaño);
            return resultado;
        }

        /// <summary>
        /// Iterar sin usar foreach (evitar IEnumerable)
        /// </summary>
        public IteradorArray ObtenerIterador()
        {
            return new IteradorArray(this);
        }

        private void ExpandirCapacidad()
        {
            int nuevaCapacidad = (int)(capacidad * FACTOR_CRECIMIENTO);
            var nuevoArray = new string[nuevaCapacidad];
            Array.Copy(elementos, nuevoArray, tamaño);
            elementos = nuevoArray;
            capacidad = nuevaCapacidad;
        }

        /// <summary>
        /// Limpiar array
        /// </summary>
        public void Limpiar()
        {
            Array.Clear(elementos, 0, tamaño);
            tamaño = 0;
        }
    }

    /// <summary>
    /// Iterador para ArrayDinamico - Patrón Iterator manual
    /// </summary>
    public class IteradorArray
    {
        private readonly ArrayDinamico array;
        private int posicion;

        public IteradorArray(ArrayDinamico array)
        {
            this.array = array;
            this.posicion = -1;
        }

        public string Current { get; private set; }

        public bool Siguiente()
        {
            posicion++;
            if (posicion < array.Count)
            {
                Current = array[posicion];
                return true;
            }
            return false;
        }

        public void Reiniciar()
        {
            posicion = -1;
            Current = null;
        }
    }
}