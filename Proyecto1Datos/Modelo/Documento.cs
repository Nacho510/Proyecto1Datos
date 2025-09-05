using System.IO;
using PruebaRider.Estructura.Nodo;

namespace PruebaRider.Modelo
{
    /// <summary>
    /// Documento optimizado con algoritmos más eficientes y menos overhead
    /// Mantiene búsqueda binaria O(log n) cuando es posible, lineal O(n) cuando es necesario
    /// </summary>
    public class Documento
    {
        private int id;
        private string textoOriginal;
        private string tokens;
        private string ruta;
        private ListaDobleEnlazada<TerminoFrecuencia> frecuencias;

        public Documento()
        {
            this.id = 0;
            this.textoOriginal = "";
            this.tokens = "";
            this.ruta = "";
            this.frecuencias = new ListaDobleEnlazada<TerminoFrecuencia>();
        }
        
        public Documento(int id, string textoOriginal, string ruta)
        {
            this.id = id;
            this.textoOriginal = textoOriginal ?? throw new ArgumentNullException(nameof(textoOriginal));
            this.ruta = ruta ?? throw new ArgumentNullException(nameof(ruta));
            this.tokens = "";
            this.frecuencias = new ListaDobleEnlazada<TerminoFrecuencia>();
        }

        // Propiedades simplificadas sin validaciones excesivas
        public int Id
        {
            get => id;
            set => id = value;
        }

        public ListaDobleEnlazada<TerminoFrecuencia> Frecuencias => frecuencias;

        public string TextoOriginal
        {
            get => textoOriginal;
            set => textoOriginal = value ?? "";
        }

        public string Ruta
        {
            get => ruta;
            set => ruta = value ?? "";
        }
        
        public string Tokens
        {
            get => tokens;
            set => tokens = value ?? "";
        }

        /// <summary>
        /// Calcular frecuencias optimizado - O(n + m log m) donde n=tokens, m=términos únicos
        /// Usa ordenamiento solo si es beneficioso (más de 10 términos únicos)
        /// </summary>
        public void CalcularFrecuencias(List<string> tokens)
        {
            frecuencias.Limpiar();

            if (tokens == null || tokens.Count == 0)
                return;

            // Contar frecuencias usando array temporal para mejor cache locality
            var contadoresArray = new ContadorTerminoOptimizado[tokens.Count]; // Máximo posible
            int cantidadUnicos = 0;

            // Fase 1: Contar frecuencias - O(n*m) donde m crece gradualmente
            foreach (var token in tokens)
            {
                if (string.IsNullOrWhiteSpace(token)) continue;

                string tokenNormalizado = token.ToLowerInvariant();
                bool encontrado = false;

                // Búsqueda en array (mejor cache que lista enlazada para pocos elementos)
                for (int i = 0; i < cantidadUnicos; i++)
                {
                    if (contadoresArray[i].Token == tokenNormalizado)
                    {
                        contadoresArray[i].Frecuencia++;
                        encontrado = true;
                        break;
                    }
                }

                if (!encontrado)
                {
                    contadoresArray[cantidadUnicos] = new ContadorTerminoOptimizado(tokenNormalizado, 1);
                    cantidadUnicos++;
                }
            }

            // Fase 2: Transferir a lista final
            if (cantidadUnicos <= 10)
            {
                // Para listas pequeñas, inserción simple sin ordenar (búsqueda lineal es rápida)
                for (int i = 0; i < cantidadUnicos; i++)
                {
                    var contador = contadoresArray[i];
                    frecuencias.Agregar(new TerminoFrecuencia(contador.Token, contador.Frecuencia));
                }
            }
            else
            {
                // Para listas grandes, ordenar alfabéticamente para búsqueda binaria futura
                OrdenarArrayAlfabeticamente(contadoresArray, cantidadUnicos);
                
                for (int i = 0; i < cantidadUnicos; i++)
                {
                    var contador = contadoresArray[i];
                    frecuencias.Agregar(new TerminoFrecuencia(contador.Token, contador.Frecuencia));
                }
            }
        }
        
        /// <summary>
        /// Obtener estadísticas básicas sin objetos complejos
        /// </summary>
        public (int terminosUnicos, int totalTokens, string terminoMasFrecuente, int maxFrecuencia) GetEstadisticasBasicas()
        {
            if (frecuencias.Count == 0)
                return (0, 0, "", 0);

            int totalTokens = 0;
            int maxFrecuencia = 0;
            string terminoMasFrecuente = "";

            var iterador = new Iterador<TerminoFrecuencia>(frecuencias);
            while (iterador.Siguiente())
            {
                var tf = iterador.Current;
                totalTokens += tf.Frecuencia;
                
                if (tf.Frecuencia > maxFrecuencia)
                {
                    maxFrecuencia = tf.Frecuencia;
                    terminoMasFrecuente = tf.Token;
                }
            }

            return (frecuencias.Count, totalTokens, terminoMasFrecuente, maxFrecuencia);
        }

        #region Métodos Privados Optimizados

        /// <summary>
        /// Búsqueda lineal optimizada con salida temprana
        /// </summary>
        private int BusquedaLinealFrecuencia(string termino)
        {
            var iterador = new Iterador<TerminoFrecuencia>(frecuencias);
            while (iterador.Siguiente())
            {
                if (iterador.Current.Token == termino) // Ya normalizado
                    return iterador.Current.Frecuencia;
            }
            return 0;
        }

        /// <summary>
        /// Búsqueda binaria usando conversión temporal a array
        /// Solo se usa para listas grandes donde el overhead vale la pena
        /// </summary>
        private int BusquedaBinariaFrecuencia(string termino)
        {
            // Convertir a array para acceso O(1) por índice
            var elementos = new TerminoFrecuencia[frecuencias.Count];
            frecuencias.CopiarA(elementos, 0);

            // Búsqueda binaria estándar
            int inicio = 0;
            int fin = elementos.Length - 1;

            while (inicio <= fin)
            {
                int medio = inicio + (fin - inicio) / 2;
                int comparacion = string.Compare(elementos[medio].Token, termino, StringComparison.OrdinalIgnoreCase);

                if (comparacion == 0)
                    return elementos[medio].Frecuencia;
                else if (comparacion < 0)
                    inicio = medio + 1;
                else
                    fin = medio - 1;
            }

            return 0;
        }

        /// <summary>
        /// Ordenamiento in-place del array temporal - O(m log m) donde m = términos únicos
        /// </summary>
        private void OrdenarArrayAlfabeticamente(ContadorTerminoOptimizado[] array, int longitud)
        {
            // Insertion sort para arrays pequeños (< 50), Array.Sort para grandes
            if (longitud < 50)
            {
                InsertionSortContadores(array, longitud);
            }
            else
            {
                // Crear array temporal para Array.Sort
                var elementos = new ContadorTerminoOptimizado[longitud];
                Array.Copy(array, elementos, longitud);
                Array.Sort(elementos, CompararContadores);
                Array.Copy(elementos, array, longitud);
            }
        }

        /// <summary>
        /// Insertion sort optimizado para arrays pequeños
        /// </summary>
        private void InsertionSortContadores(ContadorTerminoOptimizado[] array, int longitud)
        {
            for (int i = 1; i < longitud; i++)
            {
                var elemento = array[i];
                int j = i - 1;

                while (j >= 0 && string.Compare(array[j].Token, elemento.Token, StringComparison.OrdinalIgnoreCase) > 0)
                {
                    array[j + 1] = array[j];
                    j--;
                }
                
                array[j + 1] = elemento;
            }
        }

        /// <summary>
        /// Comparador para ordenamiento
        /// </summary>
        private int CompararContadores(ContadorTerminoOptimizado a, ContadorTerminoOptimizado b)
        {
            return string.Compare(a.Token, b.Token, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Métodos de Objeto Básicos

        public override bool Equals(object obj)
        {
            return obj is Documento other && this.Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            var (terminosUnicos, totalTokens, _, _) = GetEstadisticasBasicas();
            string nombreArchivo = Path.GetFileName(ruta);
            return $"Doc[{Id}:{nombreArchivo}|{terminosUnicos}términos|{totalTokens}tokens]";
        }

        #endregion

        /// <summary>
        /// Estructura optimizada para conteo temporal - struct para mejor performance
        /// </summary>
        private struct ContadorTerminoOptimizado
        {
            public string Token;
            public int Frecuencia;

            public ContadorTerminoOptimizado(string token, int frecuencia)
            {
                Token = token;
                Frecuencia = frecuencia;
            }
        }
    }
}