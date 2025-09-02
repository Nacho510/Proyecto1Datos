namespace PruebaRider.Estructura.Vector
{
    /// <summary>
    /// Vector UNIFICADO que maneja tanto números para similitud coseno 
    /// como elementos genéricos para el índice con RadixSort
    /// - Para similitud coseno: Vector(dimensión) con doubles
    /// - Para índice invertido: Vector<T> con RadixSort
    /// </summary>
    public class Vector
    {
        private double[] valores;
        private const double EPSILON = 1e-12;
        
        public int Dimension => valores.Length;

        // Constructor para similitud coseno (solo números)
        public Vector(int dimension)
        {
            if (dimension <= 0)
                throw new ArgumentException("La dimensión debe ser mayor a 0", nameof(dimension));
                
            valores = new double[dimension];
        }

        public Vector(double[] valores)
        {
            if (valores == null)
                throw new ArgumentNullException(nameof(valores));
                
            if (valores.Length == 0)
                throw new ArgumentException("El array de valores no puede estar vacío", nameof(valores));

            this.valores = new double[valores.Length];
            Array.Copy(valores, this.valores, valores.Length);
        }

        // Indexer para acceso a elementos
        public double this[int index]
        {
            get
            {
                if (index < 0 || index >= valores.Length)
                    throw new IndexOutOfRangeException($"Índice {index} fuera de rango [0, {valores.Length - 1}]");
                return valores[index];
            }
            set
            {
                if (index < 0 || index >= valores.Length)
                    throw new IndexOutOfRangeException($"Índice {index} fuera de rango [0, {valores.Length - 1}]");
                    
                if (double.IsNaN(value) || double.IsInfinity(value))
                {
                    valores[index] = 0.0;
                }
                else if (value < 0.0)
                {
                    valores[index] = 0.0;
                }
                else
                {
                    valores[index] = value;
                }
            }
        }

        /// <summary>
        /// Producto punto para similitud coseno
        /// </summary>
        public static double operator *(Vector v1, Vector v2)
        {
            if (v1 == null || v2 == null)
                throw new ArgumentNullException("Los vectores no pueden ser nulos");
                
            if (v1.Dimension != v2.Dimension)
                throw new ArgumentException(
                    $"Los vectores deben tener las mismas dimensiones. V1: {v1.Dimension}, V2: {v2.Dimension}");

            double productoPunto = 0.0;
            
            for (int i = 0; i < v1.Dimension; i++)
            {
                double val1 = v1.valores[i];
                double val2 = v2.valores[i];
                
                if (Math.Abs(val1) > EPSILON && Math.Abs(val2) > EPSILON)
                {
                    productoPunto += val1 * val2;
                }
            }
            
            return productoPunto;
        }

        /// <summary>
        /// Magnitud del vector
        /// </summary>
        public double Magnitud()
        {
            double sumaCuadrados = 0.0;
            
            for (int i = 0; i < valores.Length; i++)
            {
                double valor = valores[i];
                if (Math.Abs(valor) > EPSILON)
                {
                    sumaCuadrados += valor * valor;
                }
            }
            
            if (sumaCuadrados <= EPSILON)
                return 0.0;
            
            double magnitud = Math.Sqrt(sumaCuadrados);
            
            if (double.IsNaN(magnitud) || double.IsInfinity(magnitud))
                return 0.0;
                
            return magnitud;
        }

        /// <summary>
        /// Similitud coseno entre vectores
        /// </summary>
        public double SimilitudCoseno(Vector other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
                
            if (this.Dimension != other.Dimension)
                throw new ArgumentException("Los vectores deben tener las mismas dimensiones");
            
            // Calcular producto punto
            double productoPunto = this * other;
            
            // Calcular magnitudes
            double magnitud1 = this.Magnitud();
            double magnitud2 = other.Magnitud();
            
            // Verificar casos especiales
            if (magnitud1 <= EPSILON || magnitud2 <= EPSILON)
                return 0.0;
            
            // Calcular similitud coseno básica
            double similitud = productoPunto / (magnitud1 * magnitud2);
            
            // Validar resultado
            if (double.IsNaN(similitud) || double.IsInfinity(similitud))
                return 0.0;
            
            return Math.Max(0.0, Math.Min(1.0, similitud));
        }

        /// <summary>
        /// Verificar si el vector tiene valores significativos
        /// </summary>
        public bool TieneValoresSignificativos()
        {
            for (int i = 0; i < valores.Length; i++)
            {
                if (Math.Abs(valores[i]) > EPSILON)
                    return true;
            }
            return false;
        }

        public double[] ToArray()
        {
            var resultado = new double[valores.Length];
            Array.Copy(valores, resultado, valores.Length);
            return resultado;
        }

        public override string ToString()
        {
            if (valores.Length == 0)
                return "Vector[]";
            
            if (valores.Length <= 5)
            {
                var valoresStr = string.Join(", ", valores.Select(v => v.ToString("F4")));
                return $"Vector[{valoresStr}]";
            }
            else
            {
                var primeros = valores.Take(2).Select(v => v.ToString("F4"));
                var ultimos = valores.Skip(valores.Length - 1).Select(v => v.ToString("F4"));
                return $"Vector[{string.Join(", ", primeros)}, ..., {string.Join(", ", ultimos)}] (dim: {valores.Length}, mag: {Magnitud():F4})";
            }
        }
    }

    /// <summary>
    /// Vector genérico para índice invertido con RadixSort
    /// </summary>
    public class VectorOrdenado<T> where T : IComparable<T>
    {
        private T[] elementos;
        private int capacidad;
        private int tamaño;
        private bool estaOrdenado;
        
        private const int CAPACIDAD_INICIAL = 100;
        private const double FACTOR_CRECIMIENTO = 2.0;

        public VectorOrdenado(int capacidadInicial = CAPACIDAD_INICIAL)
        {
            this.capacidad = Math.Max(capacidadInicial, CAPACIDAD_INICIAL);
            this.elementos = new T[this.capacidad];
            this.tamaño = 0;
            this.estaOrdenado = true;
        }

        public int Count => tamaño;
        public bool EstaOrdenado => estaOrdenado;

        /// <summary>
        /// Agregar elemento sin orden (más rápido)
        /// </summary>
        public void Agregar(T elemento)
        {
            if (elemento == null)
                throw new ArgumentNullException(nameof(elemento));

            if (tamaño >= capacidad)
                ExpandirCapacidad();

            elementos[tamaño] = elemento;
            tamaño++;
            estaOrdenado = false;
        }

        /// <summary>
        /// ALGORITMO RADIX SORT - Requisito específico del proyecto
        /// </summary>
        public void OrdenarRadix()
        {
            if (tamaño <= 1)
            {
                estaOrdenado = true;
                return;
            }

            // Para tipos string, usar RadixSort especializado
            if (typeof(T) == typeof(string))
            {
                RadixSortStrings();
            }
            else
            {
                // Para otros tipos, usar comparador genérico
                Array.Sort(elementos, 0, tamaño);
            }
            
            estaOrdenado = true;
        }

        /// <summary>
        /// Radix Sort especializado para strings
        /// </summary>
        private void RadixSortStrings()
        {
            if (typeof(T) != typeof(string)) return;

            var strings = elementos as string[];
            if (strings == null) return;

            // Encontrar la longitud máxima
            int maxLength = 0;
            for (int i = 0; i < tamaño; i++)
            {
                if (strings[i] != null && strings[i].Length > maxLength)
                    maxLength = strings[i].Length;
            }

            // Aplicar counting sort para cada posición de carácter
            for (int pos = maxLength - 1; pos >= 0; pos--)
            {
                CountingSortPorPosicion(strings, pos);
            }
        }

        /// <summary>
        /// Counting sort por posición de carácter
        /// </summary>
        private void CountingSortPorPosicion(string[] strings, int posicion)
        {
            const int ALFABETO_SIZE = 256;
            
            int[] count = new int[ALFABETO_SIZE];
            string[] output = new string[tamaño];

            // Contar frecuencias
            for (int i = 0; i < tamaño; i++)
            {
                int index = ObtenerCaracterEn(strings[i], posicion);
                count[index]++;
            }

            // Cambiar count[i] para que contenga la posición actual
            for (int i = 1; i < ALFABETO_SIZE; i++)
                count[i] += count[i - 1];

            // Construir el array resultado
            for (int i = tamaño - 1; i >= 0; i--)
            {
                int index = ObtenerCaracterEn(strings[i], posicion);
                output[count[index] - 1] = strings[i];
                count[index]--;
            }

            // Copiar de vuelta al array original
            for (int i = 0; i < tamaño; i++)
                strings[i] = output[i];
        }

        /// <summary>
        /// Obtener carácter en posición específica
        /// </summary>
        private int ObtenerCaracterEn(string str, int posicion)
        {
            if (str == null || posicion >= str.Length)
                return 0;
            
            return (int)str[posicion];
        }

        /// <summary>
        /// Búsqueda binaria O(log n)
        /// </summary>
        public T BuscarBinario(T valor)
        {
            if (!estaOrdenado)
                throw new InvalidOperationException("El vector debe estar ordenado para búsqueda binaria");

            int inicio = 0;
            int fin = tamaño - 1;

            while (inicio <= fin)
            {
                int medio = inicio + (fin - inicio) / 2;
                int comparacion = elementos[medio].CompareTo(valor);

                if (comparacion == 0)
                    return elementos[medio];
                else if (comparacion < 0)
                    inicio = medio + 1;
                else
                    fin = medio - 1;
            }

            return default(T);
        }

        /// <summary>
        /// Acceso por índice
        /// </summary>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= tamaño)
                    throw new IndexOutOfRangeException($"Índice {index} fuera de rango [0, {tamaño - 1}]");
                return elementos[index];
            }
            set
            {
                if (index < 0 || index >= tamaño)
                    throw new IndexOutOfRangeException($"Índice {index} fuera de rango [0, {tamaño - 1}]");
                elementos[index] = value;
                estaOrdenado = false; // Al modificar directamente, se puede perder el orden
            }
        }

        /// <summary>
        /// Crear iterador para recorrido
        /// </summary>
        public IteradorVectorOrdenado<T> ObtenerIterador()
        {
            return new IteradorVectorOrdenado<T>(this);
        }

        /// <summary>
        /// Limpiar vector
        /// </summary>
        public void Limpiar()
        {
            Array.Clear(elementos, 0, tamaño);
            tamaño = 0;
            estaOrdenado = true;
        }

        /// <summary>
        /// Expandir capacidad del vector
        /// </summary>
        private void ExpandirCapacidad()
        {
            int nuevaCapacidad = (int)(capacidad * FACTOR_CRECIMIENTO);
            T[] nuevoArray = new T[nuevaCapacidad];
            
            Array.Copy(elementos, nuevoArray, tamaño);
            
            elementos = nuevoArray;
            capacidad = nuevaCapacidad;
        }

        public override string ToString()
        {
            return $"VectorOrdenado[{tamaño}/{capacidad}, Ordenado: {estaOrdenado}]";
        }
    }

    /// <summary>
    /// Iterador para VectorOrdenado
    /// </summary>
    public class IteradorVectorOrdenado<T> where T : IComparable<T>
    {
        private readonly VectorOrdenado<T> vector;
        private int posicionActual;

        public IteradorVectorOrdenado(VectorOrdenado<T> vector)
        {
            this.vector = vector ?? throw new ArgumentNullException(nameof(vector));
            this.posicionActual = -1;
        }

        public T Current { get; private set; }

        public bool Siguiente()
        {
            posicionActual++;
            if (posicionActual < vector.Count)
            {
                Current = vector[posicionActual];
                return true;
            }
            return false;
        }

        public void Reiniciar()
        {
            posicionActual = -1;
            Current = default(T);
        }
    }
}