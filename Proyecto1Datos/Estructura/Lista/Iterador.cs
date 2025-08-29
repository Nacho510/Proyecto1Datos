using System.Collections;

namespace PruebaRider.Estructura.Nodo
{
    public class Iterador<T>
    {
        private readonly ListaDobleEnlazada<T> lista;
        private NodoDoble<T> currentNode;
        private bool isFirstIteration;
        private int currentPosition;

        public Iterador(ListaDobleEnlazada<T> lista)
        {
            this.lista = lista;
            this.isFirstIteration = true;
            this.currentPosition = -1;
        }

        public T Current { get; private set; }
        
        public bool Siguiente()
        {
            if (lista.Count == 0) return false;

            if (isFirstIteration)
            {
                currentNode = lista.Root;
                Current = currentNode.Data;
                isFirstIteration = false;
                currentPosition = 0;
                return true;
            }

            currentPosition++;
            if (currentPosition >= lista.Count)
            {
                return false; // Ya recorrimos todos los elementos
            }

            currentNode = currentNode.Sig;
            Current = currentNode.Data;
            return true;
        }

        public void Limpiar()
        {
            currentNode = null;
            Current = default(T);
            isFirstIteration = true;
            currentPosition = -1;
        }
        
    }
}