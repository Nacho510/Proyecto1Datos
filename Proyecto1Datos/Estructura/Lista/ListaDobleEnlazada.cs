namespace PruebaRider.Estructura.Nodo
{
    public class ListaDobleEnlazada<T>
    {
        private NodoDoble<T> root;
        private int count;

        public int Count => count;
        
        public NodoDoble<T> Root => root;

        public bool IsReadOnly => false;

        public void Agregar(T item) // O(1)
        {
            var newNode = new NodoDoble<T>(item);

            if (root == null)
            {
                root = newNode;
                // El nodo ya se apunta a sí mismo en el constructor
            }
            else
            {
                var last = root.Ant; // Último nodo (anterior al root)
                
                // Conectar el nuevo nodo
                newNode.Ant = last;
                newNode.Sig = root;
                
                // Actualizar conexiones existentes
                last.Sig = newNode;
                root.Ant = newNode;
            }

            count++;
        }

        public bool Eliminar(T item) // O(n)
        {
            if (root == null) return false;

            var current = root;
            do
            {
                if (EqualityComparer<T>.Default.Equals(current.Data, item))
                {
                    if (count == 1)
                    {
                        root = null;
                    }
                    else
                    {
                        current.Ant.Sig = current.Sig;
                        current.Sig.Ant = current.Ant;

                        if (current == root)
                            root = current.Sig;
                    }

                    count--;
                    return true;
                }

                current = current.Sig;
            } while (current != root);

            return false;
        }

        public bool Existe(T item) // O(n)
        {
            if (root == null) return false;

            var current = root;
            do
            {
                if (EqualityComparer<T>.Default.Equals(current.Data, item))
                    return true;

                current = current.Sig;
            } while (current != root);

            return false;
        }

        public void Limpiar()
        {
            root = null;
            count = 0;
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
        
        public NodoDoble<T> ObtenerInicio()
        {
            return root;
        }
        
        public override string ToString()
        {
            if (root == null)
                return "[]";

            string resultado = "[";
            var current = root;

            for (int i = 0; i < count; i++)
            {
                resultado += current.Data;

                if (i < count - 1)
                    resultado += " <-> ";

                current = current.Sig;
            }

            resultado += "]";
            return resultado;
        }
        
        public void OrdenarDescendente(Func<T, double> criterio) // O(n^2)
        {
            if (count < 2) return; // No necesita ordenarse

            bool huboIntercambio;

            // Bubble Sort - O(n^2)
            do
            {
                huboIntercambio = false;
                var actual = root;

                for (int i = 0; i < count - 1; i++)
                {
                    var siguiente = actual.Sig;
                    if (criterio(actual.Data) < criterio(siguiente.Data))
                    {
                        // Intercambiar los datos de los nodos
                        var temp = actual.Data;
                        actual.Data = siguiente.Data;
                        siguiente.Data = temp;

                        huboIntercambio = true;
                    }
                    actual = actual.Sig;
                }
            } while (huboIntercambio);
        }
    }
}