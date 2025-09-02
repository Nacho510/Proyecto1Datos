using PruebaRider.Estructura.Nodo;

namespace PruebaRider.Estructura.Vector
{
    /// <summary>
    /// Vector ordenado especializado para el índice invertido
    /// Usa Radix Sort para mantener términos ordenados automáticamente
    /// Reemplaza la lista enlazada del índice por un vector más eficiente
    /// </summary>
    public class Vector<T> where T : IComparable<T>
    {
        private T[] elementos;
        private int capacidad;
        private int tamaño;
        private bool estaOrdenado;
        
        private const int CAPACIDAD_INICIAL = 100;
        private const double FACTOR_CRECIMIENTO = 2.0;

        public Vector(int capacidadInicial = CAPACIDAD_INICIAL)
        {
            this.capacidad = Math.Max(capacidadInicial, CAPACIDAD_INICIAL);
            this.elementos = new T[this.capacidad];
            this.tamaño = 0;
            this.estaOrdenado = true; // Vector vacío está ordenado
        }

        public int Count => tamaño;
        public bool EstaOrdenado => estaOrdenado;

        /// <summary>
        /// Agregar elemento manteniendo el orden usando inserción ordenada
        /// O(n) en el peor caso, pero mantiene el vector siempre ordenado
        /// </summary>
        public void AgregarOrdenado(T elemento)
        {
            if (elemento == null)
                throw new ArgumentNullException(nameof(elemento));

            // Expandir si es necesario
            if (tamaño >= capacidad)
                ExpandirCapacidad();

            if (tamaño == 0)
            {
                elementos[0] = elemento;
                tamaño = 1;
                estaOrdenado = true;
                return;
            }

            // Encontrar posición correcta usando búsqueda binaria
            int posicion = EncontrarPosicionInsercion(elemento);
            
            // Desplazar elementos hacia la derecha
            for (int i = tamaño; i > posicion; i--)
            {
                elementos[i] = elementos[i - 1];
            }
            
            // Insertar el elemento
            elementos[posicion] = elemento;
            tamaño++;
            estaOrdenado = true;
        }

        /// <summary>
        /// Agregar elemento sin orden (más rápido)
        /// Requiere llamar OrdenarRadix() después
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
        /// Optimizado para strings (términos del índice)
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
                // Para otros tipos, usar el comparador genérico con merge sort como fallback
                Array.Sort(elementos, 0, tamaño);
            }
            
            estaOrdenado = true;
        }

        /// <summary>
        /// Radix Sort especializado para strings
        /// Ordena caracteres de derecha a izquierda
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
        /// Counting sort por posición de carácter específica
        /// </summary>
        private void CountingSortPorPosicion(string[] strings, int posicion)
        {
            const int ALFABETO_SIZE = 256; // ASCII extendido
            
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
        /// Obtener carácter en posición específica, 0 si está fuera de rango
        /// </summary>
        private int ObtenerCaracterEn(string str, int posicion)
        {
            if (str == null || posicion >= str.Length)
                return 0; // Tratar como carácter nulo para strings más cortos
            
            return (int)str[posicion];
        }

        /// <summary>
        /// Búsqueda binaria optimizada - O(log n)
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
        /// Búsqueda binaria que retorna índice
        /// </summary>
        public int BuscarIndice(T valor)
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
                    return medio;
                else if (comparacion < 0)
                    inicio = medio + 1;
                else
                    fin = medio - 1;
            }

            return -1; // No encontrado
        }

        /// <summary>
        /// Acceso por índice - O(1)
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
                // Al modificar directamente, podríamos perder el orden
                if (index > 0 && elementos[index - 1].CompareTo(value) > 0)
                    estaOrdenado = false;
                if (index < tamaño - 1 && elementos[index + 1].CompareTo(value) < 0)
                    estaOrdenado = false;
            }
        }

        /// <summary>
        /// Encontrar posición para inserción ordenada
        /// </summary>
        private int EncontrarPosicionInsercion(T elemento)
        {
            int inicio = 0;
            int fin = tamaño;

            while (inicio < fin)
            {
                int medio = inicio + (fin - inicio) / 2;
                if (elementos[medio].CompareTo(elemento) < 0)
                    inicio = medio + 1;
                else
                    fin = medio;
            }

            return inicio;
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

        /// <summary>
        /// Copiar elementos a un array
        /// </summary>
        public void CopiarA(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            
            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            
            if (array.Length - arrayIndex < tamaño)
                throw new ArgumentException("Array destino muy pequeño");

            Array.Copy(elementos, 0, array, arrayIndex, tamaño);
        }

        /// <summary>
        /// Crear iterador para recorrido
        /// </summary>
        public IteradorVector<T> ObtenerIterador()
        {
            return new IteradorVector<T>(this);
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
        /// Verificar si contiene elemento
        /// </summary>
        public bool Contiene(T elemento)
        {
            if (estaOrdenado)
            {
                return BuscarIndice(elemento) >= 0;
            }
            else
            {
                for (int i = 0; i < tamaño; i++)
                {
                    if (elementos[i].CompareTo(elemento) == 0)
                        return true;
                }
                return false;
            }
        }

        public override string ToString()
        {
            return $"VectorOrdenado[{tamaño}/{capacidad}, Ordenado: {estaOrdenado}]";
        }
    }

    /// <summary>
    /// Iterador para VectorOrdenado
    /// </summary>
    public class IteradorVector<T> where T : IComparable<T>
    {
        private readonly Vector<T> vector;
        private int posicionActual;

        public IteradorVector(Vector<T> vector)
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