namespace PruebaRider.Estructura.Nodo
{

    public class ListaDobleEnlazada<T>
    {
        private NodoDoble<T> root;
        private NodoDoble<T> cola; // Cache estructural (no de datos) para inserción O(1)
        private int count;
        private bool estaOrdenada; // Flag para optimizaciones de búsqueda

        public int Count => count;
        public NodoDoble<T> Root => root;
        public bool EstaOrdenada => estaOrdenada;

        public ListaDobleEnlazada()
        {
            root = null;
            cola = null;
            count = 0;
            estaOrdenada = true; // Lista vacía está ordenada
        }

        /// <summary>
        /// Agrega elemento al final - O(1) con cola pointer
        /// </summary>
        public void Agregar(T item)
        {
            var newNode = new NodoDoble<T>(item);

            if (root == null)
            {
                root = newNode;
                cola = newNode;
                // El nodo ya se apunta a sí mismo en el constructor
            }
            else
            {
                // Usar cola pointer para inserción O(1)
                newNode.Ant = cola;
                newNode.Sig = root;

                cola.Sig = newNode;
                root.Ant = newNode;

                cola = newNode; // Actualizar cola pointer
            }

            count++;
            estaOrdenada = false; // Asumir que se desordena al agregar arbitrariamente
        }
        

        public void OrdenarDescendente(Func<T, double> criterio)
        {
            if (count < 2)
            {
                estaOrdenada = true;
                return;
            }

            if (count <= 20)
            {
                // Bubble sort optimizado para listas pequeñas
                BubbleSortOptimizado(criterio, false); // false = descendente
            }
            else
            {
                // Array.Sort para listas grandes - O(n log n)
                OrdenarConArraySort(criterio, false);
            }

            estaOrdenada = true;
        }
        
        public void CopiarA(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            if (array.Length - arrayIndex < count)
                throw new ArgumentException("El array destino no tiene suficiente espacio.");

            if (root == null) return;

            var current = root;
            for (int i = 0; i < count; i++)
            {
                array[arrayIndex + i] = current.Data;
                current = current.Sig;
            }
        }

        public void Limpiar()
        {
            root = null;
            cola = null;
            count = 0;
            estaOrdenada = true;
        }

        public NodoDoble<T> ObtenerInicio() => root;
        

        private void BubbleSortOptimizado(Func<T, double> criterio, bool ascendente)
        {
            if (count < 2) return;

            bool huboIntercambio;
            int pasadas = 0;

            do
            {
                huboIntercambio = false;
                var actual = root;

                for (int i = 0; i < count - 1 - pasadas; i++)
                {
                    var siguiente = actual.Sig;
                    double valorActual = criterio(actual.Data);
                    double valorSiguiente = criterio(siguiente.Data);

                    bool debeIntercambiar = ascendente ? valorActual > valorSiguiente : valorActual < valorSiguiente;

                    if (debeIntercambiar)
                    {
                        var temp = actual.Data;
                        actual.Data = siguiente.Data;
                        siguiente.Data = temp;
                        huboIntercambio = true;
                    }

                    actual = actual.Sig;
                }

                pasadas++;
            } while (huboIntercambio && pasadas < count);
        }
        

        private void OrdenarConArraySort(Func<T, double> criterio, bool ascendente)
        {
            var elementos = new T[count];
            CopiarA(elementos, 0);

            if (ascendente)
            {
                Array.Sort(elementos, (a, b) => criterio(a).CompareTo(criterio(b)));
            }
            else
            {
                Array.Sort(elementos, (a, b) => criterio(b).CompareTo(criterio(a)));
            }

            ReconstruirDesdeArray(elementos);
        }
        
        private void ReconstruirDesdeArray(T[] elementos)
        {
            Limpiar();
            foreach (var elemento in elementos)
            {
                Agregar(elemento);
            }
        }
    }
}