namespace PruebaRider.Estructura.Nodo
{
    public class NodoDoble<T>
    {
        public T Data { get; set; }
        public NodoDoble<T> Sig { get; set; }
        public NodoDoble<T> Ant { get; set; }

        public NodoDoble(T data)
        {
            Data = data;
            Sig = this;
            Ant = this;
        }

    }
}