namespace PruebaRider.Estructura.ColeccionOrdenada
{
    //Iterador para VectorOrdenado
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