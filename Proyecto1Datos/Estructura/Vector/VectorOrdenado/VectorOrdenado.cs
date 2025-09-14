namespace PruebaRider.Estructura.ColeccionOrdenada
{
    // Vector genérico que mantiene sus elementos en orden. En este caso los terminos
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
        public void OrdenarRadix()
        {
            if (tamaño <= 1)
            {
                estaOrdenado = true;
                return;
            }
            
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
        private void RadixSortStrings()
        {
            if (typeof(T) != typeof(string)) return;

            var strings = elementos as string[];
            if (strings == null) return;
            
            int maxLength = 0;
            for (int i = 0; i < tamaño; i++)
            {
                if (strings[i] != null && strings[i].Length > maxLength)
                    maxLength = strings[i].Length;
            }
            
            for (int pos = maxLength - 1; pos >= 0; pos--)
            {
                CountingSortPorPosicion(strings, pos);
            }
        }
        
        private void CountingSortPorPosicion(string[] strings, int posicion)
        {
            const int ALFABETO_SIZE = 256;

            int[] count = new int[ALFABETO_SIZE];
            string[] output = new string[tamaño];
            
            for (int i = 0; i < tamaño; i++)
            {
                int index = ObtenerCaracterEn(strings[i], posicion);
                count[index]++;
            }
            
            for (int i = 1; i < ALFABETO_SIZE; i++)
                count[i] += count[i - 1];
            
            for (int i = tamaño - 1; i >= 0; i--)
            {
                int index = ObtenerCaracterEn(strings[i], posicion);
                output[count[index] - 1] = strings[i];
                count[index]--;
            }

            for (int i = 0; i < tamaño; i++)
                strings[i] = output[i];
        }
        
        private int ObtenerCaracterEn(string str, int posicion)
        {
            if (str == null || posicion >= str.Length)
                return 0;

            return (int)str[posicion];
        }
        
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
                estaOrdenado = false;
            }
        }
        
        public IteradorVectorOrdenado<T> ObtenerIterador()
        {
            return new IteradorVectorOrdenado<T>(this);
        }
        
        public void Limpiar()
        {
            Array.Clear(elementos, 0, tamaño);
            tamaño = 0;
            estaOrdenado = true;
        }
        
        private void ExpandirCapacidad()
        {
            int nuevaCapacidad = (int)(capacidad * FACTOR_CRECIMIENTO);
            T[] nuevoArray = new T[nuevaCapacidad];

            Array.Copy(elementos, nuevoArray, tamaño);

            elementos = nuevoArray;
            capacidad = nuevaCapacidad;
        }
    }
}