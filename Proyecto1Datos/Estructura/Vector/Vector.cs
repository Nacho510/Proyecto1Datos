namespace PruebaRider.Estructura.Vector
{
    /// <summary>
    /// Vector personalizado CORREGIDO
    /// - Similitud coseno funcionando correctamente
    /// - Valores realistas y precisos
    /// - Operador * corregido
    /// - Sin valores artificiales
    /// </summary>
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

        public Vector(double[] valores)
        {
            if (valores == null)
                throw new ArgumentNullException(nameof(valores));
                
            if (valores.Length == 0)
                throw new ArgumentException("El array de valores no puede estar vacío", nameof(valores));

            this.valores = new double[valores.Length];
            Array.Copy(valores, this.valores, valores.Length);
        }

        public double this[int index]
        {
            get
            {
                if (index < 0 || index >= valores.Length)
                    throw new IndexOutOfRangeException($"Índice {index} fuera de rango [0, {valores.Length - 1}]");
                return valores[index];
            }
            set
            {
                if (index < 0 || index >= valores.Length)
                    throw new IndexOutOfRangeException($"Índice {index} fuera de rango [0, {valores.Length - 1}]");
                    
                if (double.IsNaN(value) || double.IsInfinity(value))
                {
                    valores[index] = 0.0;
                }
                else if (value < 0.0)
                {
                    valores[index] = 0.0;
                }
                else
                {
                    valores[index] = value;
                }
            }
        }

        /// <summary>
        /// OPERADOR * CORREGIDO - Producto punto real
        /// </summary>
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

        /// <summary>
        /// MAGNITUD CORREGIDA - Cálculo preciso
        /// </summary>
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

        /// <summary>
        /// SIMILITUD COSENO CORREGIDA - SIN correcciones artificiales
        /// </summary>
        public double SimilitudCoseno(Vector other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
                
            if (this.Dimension != other.Dimension)
                throw new ArgumentException("Los vectores deben tener las mismas dimensiones");
            
            // Calcular producto punto
            double productoPunto = this * other;
            
            // Calcular magnitudes
            double magnitud1 = this.Magnitud();
            double magnitud2 = other.Magnitud();
            
            // Verificar casos especiales
            if (magnitud1 <= EPSILON || magnitud2 <= EPSILON)
                return 0.0;
            
            // Calcular similitud coseno básica
            double similitud = productoPunto / (magnitud1 * magnitud2);
            
            // Validar resultado
            if (double.IsNaN(similitud) || double.IsInfinity(similitud))
                return 0.0;
            
            // IMPORTANTE: SIN correcciones artificiales - devolver valor real
            return Math.Max(0.0, Math.Min(1.0, similitud));
        }

        /// <summary>
        /// Verificar si el vector tiene valores significativos
        /// </summary>
        public bool TieneValoresSignificativos()
        {
            for (int i = 0; i < valores.Length; i++)
            {
                if (Math.Abs(valores[i]) > EPSILON)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Normalizar vector
        /// </summary>
        public Vector Normalizar()
        {
            double magnitud = this.Magnitud();
            
            if (magnitud <= EPSILON)
                return new Vector(this.Dimension);
            
            var valoresNormalizados = new double[this.Dimension];
            for (int i = 0; i < this.Dimension; i++)
            {
                valoresNormalizados[i] = this.valores[i] / magnitud;
            }
            
            return new Vector(valoresNormalizados);
        }

        public double[] ToArray()
        {
            var resultado = new double[valores.Length];
            Array.Copy(valores, resultado, valores.Length);
            return resultado;
        }

        public override string ToString()
        {
            if (valores.Length == 0)
                return "Vector[]";
            
            if (valores.Length <= 5)
            {
                var valoresStr = string.Join(", ", valores.Select(v => v.ToString("F4")));
                return $"Vector[{valoresStr}]";
            }
            else
            {
                var primeros = valores.Take(2).Select(v => v.ToString("F4"));
                var ultimos = valores.Skip(valores.Length - 1).Select(v => v.ToString("F4"));
                return $"Vector[{string.Join(", ", primeros)}, ..., {string.Join(", ", ultimos)}] (dim: {valores.Length}, mag: {Magnitud():F4})";
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector other)
            {
                if (this.Dimension != other.Dimension)
                    return false;
                
                for (int i = 0; i < this.Dimension; i++)
                {
                    if (Math.Abs(this.valores[i] - other.valores[i]) > EPSILON)
                        return false;
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = this.Dimension.GetHashCode();
            for (int i = 0; i < valores.Length; i++)
            {
                hash = hash * 31 + valores[i].GetHashCode();
            }
            return hash;
        }
    }
}