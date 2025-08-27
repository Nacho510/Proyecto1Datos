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
        public bool IsReadOnly => false;
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
        /// Inserción manteniendo orden - O(n) pero garantiza O(log n) en búsquedas futuras
        /// Trade-off: inserción más lenta por búsquedas consistentemente rápidas
        /// </summary>
        public void AgregarOrdenado(T item, Func<T, T, int> comparador)
        {
            if (root == null)
            {
                Agregar(item);
                estaOrdenada = true;
                return;
            }

            var newNode = new NodoDoble<T>(item);

            // Buscar posición correcta - O(n) pero necesario para mantener orden
            var current = root;
            do
            {
                if (comparador(item, current.Data) <= 0)
                {
                    // Insertar antes de current
                    InsertarAntesDe(newNode, current);
                    
                    if (current == root)
                        root = newNode;
                        
                    count++;
                    estaOrdenada = true;
                    return;
                }
                current = current.Sig;
            } while (current != root);

            // Insertar al final si es el mayor elemento
            InsertarAlFinal(newNode);
            count++;
            estaOrdenada = true;
        }

        /// <summary>
        /// Búsqueda optimizada - usa binaria si está ordenada y es grande, lineal si no
        /// </summary>
        public bool Existe(T item, Func<T, T, int> comparador = null)
        {
            if (root == null) return false;

            // Si tiene comparador y está ordenada, usar búsqueda binaria para listas grandes
            if (comparador != null && estaOrdenada && count > 10)
            {
                var resultado = BuscarBinario(item, comparador);
                return !EqualityComparer<T>.Default.Equals(resultado, default(T));
            }

            // Búsqueda lineal para listas pequeñas o desordenadas
            return ExisteBusquedaLineal(item, comparador);
        }

        /// <summary>
        /// Búsqueda binaria nativa - O(log n) solo si está ordenada
        /// </summary>
        public T BuscarBinario(T valorBuscado, Func<T, T, int> comparador)
        {
            if (!estaOrdenada)
                throw new InvalidOperationException("La lista debe estar ordenada para búsqueda binaria");

            if (count == 0) return default(T);

            // Para listas muy pequeñas, la búsqueda lineal es más eficiente
            if (count <= 5)
                return BusquedaLineal(valorBuscado, comparador);

            // Conversión a array para acceso O(1) por índice
            T[] elementos = new T[count];
            CopiarA(elementos, 0);

            // Búsqueda binaria clásica
            return BusquedaBinariaEnArray(elementos, valorBuscado, comparador);
        }

        /// <summary>
        /// Eliminación optimizada
        /// </summary>
        public bool Eliminar(T item)
        {
            if (root == null) return false;

            var current = root;
            do
            {
                if (EqualityComparer<T>.Default.Equals(current.Data, item))
                {
                    EliminarNodo(current);
                    return true;
                }
                current = current.Sig;
            } while (current != root);

            return false;
        }

        /// <summary>
        /// Elimina nodo específico - O(1) si se tiene referencia directa al nodo
        /// </summary>
        public void EliminarNodo(NodoDoble<T> nodo)
        {
            if (nodo == null || count == 0) return;

            if (count == 1)
            {
                root = null;
                cola = null;
            }
            else
            {
                nodo.Ant.Sig = nodo.Sig;
                nodo.Sig.Ant = nodo.Ant;

                if (nodo == root)
                    root = nodo.Sig;
                if (nodo == cola)
                    cola = nodo.Ant;
            }

            count--;
            // No marcar como desordenada por eliminación
        }

        /// <summary>
        /// Ordenamiento híbrido - algoritmo óptimo según tamaño
        /// </summary>
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

       
        public void OrdenarAscendente(Func<T, double> criterio)
        {
            if (count < 2) 
            {
                estaOrdenada = true;
                return;
            }

            if (count <= 20)
            {
                BubbleSortOptimizado(criterio, true); // true = ascendente
            }
            else
            {
                OrdenarConArraySort(criterio, true);
            }
            
            estaOrdenada = true;
        }

      
        public void OrdenarCon(Func<T, T, int> comparador)
        {
            if (count < 2) 
            {
                estaOrdenada = true;
                return;
            }

            if (count <= 20)
            {
                BubbleSortConComparador(comparador);
            }
            else
            {
                OrdenarConArraySortComparador(comparador);
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

        public override string ToString()
        {
            if (root == null)
                return "[]";

            var resultado = new System.Text.StringBuilder("[");
            var current = root;

            for (int i = 0; i < count; i++)
            {
                resultado.Append(current.Data);

                if (i < count - 1)
                    resultado.Append(" <-> ");

                current = current.Sig;
            }

            resultado.Append("]");
            return resultado.ToString();
        }

        #region Métodos Privados de Optimización
        
        private bool ExisteBusquedaLineal(T item, Func<T, T, int> comparador)
        {
            var current = root;
            do
            {
                if (comparador != null)
                {
                    if (comparador(current.Data, item) == 0)
                        return true;
                }
                else
                {
                    if (EqualityComparer<T>.Default.Equals(current.Data, item))
                        return true;
                }

                current = current.Sig;
            } while (current != root);

            return false;
        }
        
        private T BusquedaLineal(T valorBuscado, Func<T, T, int> comparador)
        {
            var current = root;
            do
            {
                if (comparador(current.Data, valorBuscado) == 0)
                    return current.Data;

                current = current.Sig;
            } while (current != root);

            return default(T);
        }
        
        private T BusquedaBinariaEnArray(T[] elementos, T valorBuscado, Func<T, T, int> comparador)
        {
            int inicio = 0;
            int fin = elementos.Length - 1;

            while (inicio <= fin)
            {
                int medio = inicio + (fin - inicio) / 2;
                int comparacion = comparador(elementos[medio], valorBuscado);

                if (comparacion == 0)
                    return elementos[medio];
                else if (comparacion < 0)
                    inicio = medio + 1;
                else
                    fin = medio - 1;
            }

            return default(T);
        }
        
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

                    bool debeIntercambiar = ascendente ? 
                        valorActual > valorSiguiente : 
                        valorActual < valorSiguiente;

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
        
        private void BubbleSortConComparador(Func<T, T, int> comparador)
        {
            if (count < 2) return;

            bool huboIntercambio;
            do
            {
                huboIntercambio = false;
                var actual = root;

                for (int i = 0; i < count - 1; i++)
                {
                    var siguiente = actual.Sig;
                    
                    if (comparador(actual.Data, siguiente.Data) > 0)
                    {
                        var temp = actual.Data;
                        actual.Data = siguiente.Data;
                        siguiente.Data = temp;
                        huboIntercambio = true;
                    }
                    actual = actual.Sig;
                }
            } while (huboIntercambio);
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
        private void OrdenarConArraySortComparador(Func<T, T, int> comparador)
        {
            var elementos = new T[count];
            CopiarA(elementos, 0);
            
            Array.Sort(elementos, (a, b) => comparador(a, b));
            
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
        
        private void InsertarAntesDe(NodoDoble<T> nuevoNodo, NodoDoble<T> nodoExistente)
        {
            nuevoNodo.Sig = nodoExistente;
            nuevoNodo.Ant = nodoExistente.Ant;
            
            nodoExistente.Ant.Sig = nuevoNodo;
            nodoExistente.Ant = nuevoNodo;
        }
        
        private void InsertarAlFinal(NodoDoble<T> nuevoNodo)
        {
            nuevoNodo.Ant = cola;
            nuevoNodo.Sig = root;
            
            cola.Sig = nuevoNodo;
            root.Ant = nuevoNodo;
            
            cola = nuevoNodo;
        }

        #endregion
    }
}