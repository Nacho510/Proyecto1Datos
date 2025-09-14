namespace PruebaRider.Estructura.Vector
{
    // Vector matemático para cálculos de similitud.
    
    public class Vector
    {
        private double[] valores;
        private const double EPSILON = 1e-12;

        public int Dimension => valores.Length;
        
        public Vector(int dimension)
        {
            if (dimension <= 0)
                throw new ArgumentException("La dimensión debe ser mayor a 0", nameof(dimension));

            valores = new double[dimension];
        }

        public double this[int index]
        {
            get
            {
                if (index < 0 || index >= valores.Length)
                    throw new IndexOutOfRangeException();
                return valores[index];
            }
            set
            {
                if (index < 0 || index >= valores.Length)
                    throw new IndexOutOfRangeException();
                if (value < 0.0)
                    valores[index] = 0.0;
                else
                    valores[index] = value;
            }
        }
        
        public static double operator *(Vector v1, Vector v2)
        {
            if (v1 == null || v2 == null)
                throw new ArgumentNullException("Los vectores no pueden ser nulos");

            if (v1.Dimension != v2.Dimension)
                throw new ArgumentException(
                    $"Los vectores deben tener las mismas dimensiones. V1: {v1.Dimension}, V2: {v2.Dimension}");

            double productoPunto = 0.0;

            for (int i = 0; i < v1.Dimension; i++)
            {
                double val1 = v1.valores[i];
                double val2 = v2.valores[i];

                if (Math.Abs(val1) > EPSILON && Math.Abs(val2) > EPSILON)
                {
                    productoPunto += val1 * val2;
                }
            }

            return productoPunto;
        }
        
        public double Magnitud()
        {
            double sumaCuadrados = 0.0;

            for (int i = 0; i < valores.Length; i++)
            {
                double valor = valores[i];
                if (Math.Abs(valor) > EPSILON)
                {
                    sumaCuadrados += valor * valor;
                }
            }

            if (sumaCuadrados <= EPSILON)
                return 0.0;

            double magnitud = Math.Sqrt(sumaCuadrados);

            if (double.IsNaN(magnitud) || double.IsInfinity(magnitud))
                return 0.0;

            return magnitud;
        }

        public double SimilitudCoseno(Vector other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (this.Dimension != other.Dimension)
                throw new ArgumentException("Los vectores deben tener las mismas dimensiones");

            double productoPunto = this * other;

           
            double magnitud1 = this.Magnitud();
            double magnitud2 = other.Magnitud();

            // Evita división por cero
            if (magnitud1 <= EPSILON || magnitud2 <= EPSILON)
                return 0.0;

            double similitud = productoPunto / (magnitud1 * magnitud2);

            if (double.IsNaN(similitud) || double.IsInfinity(similitud))
                return 0.0;

            return Math.Max(0.0, Math.Min(1.0, similitud));
        }
        
        public bool TieneValoresSignificativos()
        {
            for (int i = 0; i < valores.Length; i++)
            {
                if (Math.Abs(valores[i]) > EPSILON)
                    return true;
            }

            return false;
        }
    }
}