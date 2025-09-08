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
        

        /// <summary>
        /// Ordena la lista descendentemente usando Bubble Sort optimizado
        /// </summary>
        public void OrdenarDescendente(Func<T, double> criterio)
        {
            if (count < 2)
            {
                estaOrdenada = true;
                return;
            }

            // Usar solo Bubble Sort optimizado para todos los casos
            BubbleSortOptimizado(criterio, false); // false = descendente
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
        

        /// <summary>
        /// Bubble Sort optimizado con detección temprana de ordenamiento
        /// Complejidad: O(n²) peor caso, O(n) mejor caso
        /// </summary>
        private void BubbleSortOptimizado(Func<T, double> criterio, bool ascendente)
        {
            if (count < 2) return;

            bool huboIntercambio;
            int pasadas = 0;

            do
            {
                huboIntercambio = false;
                var actual = root;

                // Recorrer la lista comparando elementos adyacentes
                for (int i = 0; i < count - 1 - pasadas; i++)
                {
                    var siguiente = actual.Sig;
                    double valorActual = criterio(actual.Data);
                    double valorSiguiente = criterio(siguiente.Data);

                    // Determinar si se debe intercambiar según el orden deseado
                    bool debeIntercambiar = ascendente ? valorActual > valorSiguiente : valorActual < valorSiguiente;

                    if (debeIntercambiar)
                    {
                        // Intercambiar los datos (no los nodos)
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
    }
}