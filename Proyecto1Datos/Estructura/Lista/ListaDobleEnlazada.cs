namespace PruebaRider.Estructura.Nodo
{

    public class ListaDobleEnlazada<T>
    {
        private NodoDoble<T> root;
        private NodoDoble<T> cola; 
        private int count;
        private bool estaOrdenada;

        public int Count => count;
        public NodoDoble<T> Root => root;
        public bool EstaOrdenada => estaOrdenada;

        public ListaDobleEnlazada()
        {
            root = null;
            cola = null;
            count = 0;
            estaOrdenada = true; 
        }
        
        public void Agregar(T item)
        {
            var newNode = new NodoDoble<T>(item);

            if (root == null)
            {
                root = newNode;
                cola = newNode;
            }
            else
            {
                newNode.Ant = cola;
                newNode.Sig = root;

                cola.Sig = newNode;
                root.Ant = newNode;

                cola = newNode; 
            }

            count++;
            estaOrdenada = false;
        }
        
        
        public void OrdenarDescendente(Func<T, double> criterio)
        {
            if (count < 1)
            {
                estaOrdenada = true;
                return;
            }

            BubbleSortOptimizado(criterio, false);
            estaOrdenada = true;
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
    }
}