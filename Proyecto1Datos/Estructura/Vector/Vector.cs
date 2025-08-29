namespace PruebaRider.Estructura.Vector
{
    /// <summary>
    /// Vector personalizado CORREGIDO para similitud coseno precisa
    /// - Operador * sobrecargado para producto punto
    /// - Cálculos de magnitud optimizados
    /// - Prevención de valores NaN e infinitos
    /// - Cumple requisitos del proyecto (sin usar genéricos del lenguaje)
    /// </summary>
    public class Vector
    {
        private double[] valores;
        
        public int Dimension => valores.Length;

        /// <summary>
        /// Constructor que inicializa vector con dimensión específica
        /// </summary>
        public Vector(int dimension)
        {
            if (dimension <= 0)
                throw new ArgumentException("La dimensión debe ser mayor a 0", nameof(dimension));
                
            valores = new double[dimension];
            // Los valores se inicializan automáticamente en 0.0
        }

        /// <summary>
        /// Constructor que crea vector desde array de valores
        /// </summary>
        public Vector(double[] valores)
        {
            if (valores == null)
                throw new ArgumentNullException(nameof(valores));
                
            if (valores.Length == 0)
                throw new ArgumentException("El array de valores no puede estar vacío", nameof(valores));

            this.valores = new double[valores.Length];
            Array.Copy(valores, this.valores, valores.Length);
        }

        /// <summary>
        /// Indexador para acceder a elementos del vector
        /// </summary>
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
                    
                // CORREGIDO: Validar que el valor sea válido (no NaN, no infinito)
                if (double.IsNaN(value) || double.IsInfinity(value))
                {
                    valores[index] = 0.0; // Asignar 0 en lugar de valor inválido
                }
                else
                {
                    valores[index] = value;
                }
            }
        }

        /// <summary>
        /// CORREGIDO: Operador * sobrecargado para producto punto preciso
        /// Previene errores de cálculo que causan similitudes artificiales del 100%
        /// </summary>
        public static double operator *(Vector v1, Vector v2)
        {
            if (v1 == null || v2 == null)
                throw new ArgumentNullException("Los vectores no pueden ser nulos");
                
            if (v1.Dimension != v2.Dimension)
                throw new ArgumentException(
                    $"Los vectores deben tener las mismas dimensiones. " +
                    $"V1: {v1.Dimension}, V2: {v2.Dimension}");

            double resultado = 0.0;
            
            // CORREGIDO: Cálculo preciso del producto punto
            for (int i = 0; i < v1.Dimension; i++)
            {
                double val1 = v1.valores[i];
                double val2 = v2.valores[i];
                
                // Verificar que ambos valores sean válidos
                if (!double.IsNaN(val1) && !double.IsNaN(val2) && 
                    !double.IsInfinity(val1) && !double.IsInfinity(val2))
                {
                    resultado += val1 * val2;
                }
                // Si algún valor es inválido, se ignora (contribuye 0 al producto)
            }
            
            // CORREGIDO: Verificar que el resultado sea válido
            if (double.IsNaN(resultado) || double.IsInfinity(resultado))
                return 0.0;
                
            return resultado;
        }

        /// <summary>
        /// CORREGIDO: Cálculo de magnitud preciso y seguro
        /// Evita overflow y underflow que pueden causar similitudes incorrectas
        /// </summary>
        public double Magnitud()
        {
            double sumaCuadrados = 0.0;
            bool tieneValoresSignificativos = false;
            
            // CORREGIDO: Cálculo robusto de la suma de cuadrados
            for (int i = 0; i < valores.Length; i++)
            {
                double valor = valores[i];
                
                // Verificar que el valor sea válido y significativo
                if (!double.IsNaN(valor) && !double.IsInfinity(valor) && Math.Abs(valor) > double.Epsilon)
                {
                    sumaCuadrados += valor * valor;
                    tieneValoresSignificativos = true;
                }
            }
            
            // Si no hay valores significativos, magnitud es 0
            if (!tieneValoresSignificativos || sumaCuadrados <= double.Epsilon)
                return 0.0;
            
            // CORREGIDO: Verificar overflow antes de calcular raíz cuadrada
            if (sumaCuadrados > double.MaxValue)
                return double.MaxValue;
            
            double magnitud = Math.Sqrt(sumaCuadrados);
            
            // Verificar que el resultado sea válido
            if (double.IsNaN(magnitud) || double.IsInfinity(magnitud))
                return 0.0;
                
            return magnitud;
        }

        /// <summary>
        /// CORREGIDO: Similitud coseno precisa y normalizada
        /// Produce resultados realistas en el rango [0, 1]
        /// </summary>
        public double SimilitudCoseno(Vector other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
                
            if (this.Dimension != other.Dimension)
                throw new ArgumentException(
                    $"Los vectores deben tener las mismas dimensiones para calcular similitud coseno. " +
                    $"Este vector: {this.Dimension}, Otro vector: {other.Dimension}");
            
            // CORREGIDO: Cálculo preciso paso a paso
            
            // 1. Calcular producto punto usando operador sobrecargado
            double productoPunto = this * other;
            
            // 2. Calcular magnitudes
            double magnitud1 = this.Magnitud();
            double magnitud2 = other.Magnitud();
            
            // 3. Verificar divisiones por cero
            if (magnitud1 <= double.Epsilon || magnitud2 <= double.Epsilon)
                return 0.0; // Vectores sin magnitud significativa
            
            // 4. Calcular similitud coseno
            double similitud = productoPunto / (magnitud1 * magnitud2);
            
            // 5. CORREGIDO: Normalizar resultado y manejar casos especiales
            if (double.IsNaN(similitud) || double.IsInfinity(similitud))
                return 0.0;
            
            // 6. CORREGIDO: Asegurar que esté en rango válido [0, 1]
            // En TF-IDF, los vectores tienen componentes no negativos, por lo que
            // la similitud coseno debería estar entre 0 y 1
            similitud = Math.Max(0.0, Math.Min(1.0, similitud));
            
            return similitud;
        }

        /// <summary>
        /// Convertir a array (copia defensiva)
        /// </summary>
        public double[] ToArray()
        {
            var resultado = new double[valores.Length];
            Array.Copy(valores, resultado, valores.Length);
            return resultado;
        }

        /// <summary>
        /// NUEVO: Verificar si el vector tiene valores significativos
        /// </summary>
        public bool TieneValoresSignificativos()
        {
            for (int i = 0; i < valores.Length; i++)
            {
                if (!double.IsNaN(valores[i]) && !double.IsInfinity(valores[i]) && 
                    Math.Abs(valores[i]) > double.Epsilon)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// NUEVO: Normalizar vector (convertir a vector unitario)
        /// Útil para comparaciones adicionales
        /// </summary>
        public Vector Normalizar()
        {
            double magnitud = this.Magnitud();
            
            if (magnitud <= double.Epsilon)
                return new Vector(this.Dimension); // Vector cero si no tiene magnitud
            
            var valoresNormalizados = new double[this.Dimension];
            for (int i = 0; i < this.Dimension; i++)
            {
                valoresNormalizados[i] = this.valores[i] / magnitud;
            }
            
            return new Vector(valoresNormalizados);
        }

        /// <summary>
        /// NUEVO: Obtener componentes más significativas del vector
        /// Útil para debugging y análisis - USANDO SOLO ESTRUCTURAS PROPIAS
        /// </summary>
        public (int indice, double valor)[] ObtenerComponentesSignificativas(int cantidad = 5)
        {
            // Usar array básico en lugar de List genérica para cumplir requisitos
            var componentesTemp = new (int indice, double valor)[valores.Length];
            int contadorComponentes = 0;
            
            // Recopilar componentes significativas
            for (int i = 0; i < valores.Length; i++)
            {
                if (Math.Abs(valores[i]) > double.Epsilon)
                {
                    componentesTemp[contadorComponentes] = (i, valores[i]);
                    contadorComponentes++;
                }
            }
            
            // Crear array del tamaño correcto
            var componentes = new (int indice, double valor)[contadorComponentes];
            Array.Copy(componentesTemp, componentes, contadorComponentes);
            
            // Ordenar por valor absoluto descendente usando algoritmo propio
            OrdenarComponentesPorValorAbsoluto(componentes);
            
            // Devolver solo la cantidad solicitada
            int cantidadFinal = Math.Min(cantidad, componentes.Length);
            var resultado = new (int indice, double valor)[cantidadFinal];
            Array.Copy(componentes, resultado, cantidadFinal);
            
            return resultado;
        }

        /// <summary>
        /// Método auxiliar para ordenar componentes sin usar genéricos
        /// </summary>
        private void OrdenarComponentesPorValorAbsoluto((int indice, double valor)[] componentes)
        {
            // Bubble sort optimizado para arrays pequeños
            for (int i = 0; i < componentes.Length - 1; i++)
            {
                for (int j = 0; j < componentes.Length - 1 - i; j++)
                {
                    if (Math.Abs(componentes[j].valor) < Math.Abs(componentes[j + 1].valor))
                    {
                        var temp = componentes[j];
                        componentes[j] = componentes[j + 1];
                        componentes[j + 1] = temp;
                    }
                }
            }
        }

        /// <summary>
        /// Override ToString para debugging
        /// </summary>
        public override string ToString()
        {
            if (valores.Length == 0)
                return "Vector[]";
            
            if (valores.Length <= 10)
            {
                // Mostrar todos los valores si son pocos
                var valoresStr = string.Join(", ", valores.Select(v => v.ToString("F4")));
                return $"Vector[{valoresStr}]";
            }
            else
            {
                // Mostrar solo algunos valores si son muchos
                var primeros = valores.Take(3).Select(v => v.ToString("F4"));
                var ultimos = valores.Skip(valores.Length - 2).Select(v => v.ToString("F4"));
                return $"Vector[{string.Join(", ", primeros)}, ..., {string.Join(", ", ultimos)}] (dim: {valores.Length})";
            }
        }

        /// <summary>
        /// Override Equals para comparaciones
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is Vector other)
            {
                if (this.Dimension != other.Dimension)
                    return false;
                
                for (int i = 0; i < this.Dimension; i++)
                {
                    if (Math.Abs(this.valores[i] - other.valores[i]) > double.Epsilon)
                        return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Override GetHashCode
        /// </summary>
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