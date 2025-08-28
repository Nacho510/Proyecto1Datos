using PruebaRider.Estructura.Nodo;

namespace PruebaRider.Modelo
{
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
            this.textoOriginal = textoOriginal ?? "";
            this.ruta = ruta ?? "";
            this.tokens = "";
            this.frecuencias = new ListaDobleEnlazada<TerminoFrecuencia>();
        }

        // Propiedades básicas
        public int Id
        {
            get => id;
            set => id = value;
        }

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

        public ListaDobleEnlazada<TerminoFrecuencia> Frecuencias
        {
            get => frecuencias;
            set => frecuencias = value ?? new ListaDobleEnlazada<TerminoFrecuencia>();
        }

        /// <summary>
        /// Calcular frecuencias de términos - Método principal
        /// </summary>
        public void CalcularFrecuencias(List<string> tokens)
        {
            frecuencias.Limpiar();

            if (tokens == null || tokens.Count == 0) return;

            // Contar frecuencias
            var contadores = new List<ContadorTermino>();

            foreach (var token in tokens)
            {
                if (string.IsNullOrWhiteSpace(token)) continue;

                string tokenLimpio = token.ToLowerInvariant();
                bool encontrado = false;

                // Buscar si ya existe
                foreach (var contador in contadores)
                {
                    if (contador.Token.Equals(tokenLimpio, StringComparison.OrdinalIgnoreCase))
                    {
                        contador.Incrementar();
                        encontrado = true;
                        break;
                    }
                }

                if (!encontrado)
                {
                    contadores.Add(new ContadorTermino(tokenLimpio));
                }
            }

            // Convertir a TerminoFrecuencia ordenado alfabéticamente
            foreach (var contador in contadores)
            {
                var tf = new TerminoFrecuencia(contador.Token, contador.Frecuencia);
                frecuencias.AgregarOrdenado(tf, CompararTerminosAlfabeticamente);
            }
        }

        /// <summary>
        /// Obtener frecuencia de un término específico
        /// </summary>
        public int GetFrecuencia(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino)) return 0;

            // Búsqueda binaria si está ordenado y es grande
            if (frecuencias.EstaOrdenada && frecuencias.Count > 5)
            {
                var dummy = new TerminoFrecuencia(termino.ToLowerInvariant(), 0);
                var encontrado = frecuencias.BuscarBinario(dummy, CompararTerminosAlfabeticamente);
                return encontrado?.Frecuencia ?? 0;
            }
            
            // Búsqueda lineal para listas pequeñas
            var iterador = new Iterador<TerminoFrecuencia>(frecuencias);
            while (iterador.Siguiente())
            {
                if (iterador.Current.Token.Equals(termino, StringComparison.OrdinalIgnoreCase))
                    return iterador.Current.Frecuencia;
            }
            
            return 0;
        }

        /// <summary>
        /// Verificar si contiene un término
        /// </summary>
        public bool ContieneTerm(string termino)
        {
            return GetFrecuencia(termino) > 0;
        }

        /// <summary>
        /// Obtener términos más frecuentes
        /// </summary>
        public ListaDobleEnlazada<TerminoFrecuencia> ObtenerTerminosMasFrecuentes(int cantidad = 10)
        {
            var resultado = new ListaDobleEnlazada<TerminoFrecuencia>();
            
            if (frecuencias.Count == 0) return resultado;

            // Crear copia ordenada por frecuencia
            var terminosOrdenados = new ListaDobleEnlazada<TerminoFrecuencia>();
            var iterador = new Iterador<TerminoFrecuencia>(frecuencias);
            
            while (iterador.Siguiente())
            {
                terminosOrdenados.Agregar(new TerminoFrecuencia(
                    iterador.Current.Token, 
                    iterador.Current.Frecuencia
                ));
            }

            // Ordenar por frecuencia descendente
            terminosOrdenados.OrdenarDescendente(tf => tf.Frecuencia);

            // Tomar solo la cantidad solicitada
            var iteradorOrdenado = new Iterador<TerminoFrecuencia>(terminosOrdenados);
            int contador = 0;
            
            while (iteradorOrdenado.Siguiente() && contador < cantidad)
            {
                resultado.Agregar(iteradorOrdenado.Current);
                contador++;
            }

            return resultado;
        }

        /// <summary>
        /// Obtener estadísticas básicas del documento
        /// </summary>
        public EstadisticasDocumento ObtenerEstadisticas()
        {
            int totalTokens = 0;
            int maxFrecuencia = 0;
            string terminoMasFrecuente = "";

            var iterador = new Iterador<TerminoFrecuencia>(frecuencias);
            while (iterador.Siguiente())
            {
                totalTokens += iterador.Current.Frecuencia;
                
                if (iterador.Current.Frecuencia > maxFrecuencia)
                {
                    maxFrecuencia = iterador.Current.Frecuencia;
                    terminoMasFrecuente = iterador.Current.Token;
                }
            }

            return new EstadisticasDocumento
            {
                DocumentoId = id,
                NombreArchivo = Path.GetFileName(ruta),
                TerminosUnicos = frecuencias.Count,
                TotalTokens = totalTokens,
                TerminoMasFrecuente = terminoMasFrecuente,
                FrecuenciaMaxima = maxFrecuencia
            };
        }

        /// <summary>
        /// Comparador alfabético para TerminoFrecuencia
        /// </summary>
        private int CompararTerminosAlfabeticamente(TerminoFrecuencia tf1, TerminoFrecuencia tf2)
        {
            return string.Compare(tf1.Token, tf2.Token, StringComparison.OrdinalIgnoreCase);
        }

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
            return $"Doc[ID:{Id}, Archivo:{Path.GetFileName(ruta)}, Términos:{frecuencias.Count}]";
        }

        /// <summary>
        /// Clase auxiliar para conteo de frecuencias
        /// </summary>
        private class ContadorTermino
        {
            public string Token { get; }
            public int Frecuencia { get; private set; }

            public ContadorTermino(string token)
            {
                Token = token;
                Frecuencia = 1;
            }

            public void Incrementar()
            {
                Frecuencia++;
            }
        }
    }

    /// <summary>
    /// Estadísticas básicas de un documento
    /// </summary>
    public class EstadisticasDocumento
    {
        public int DocumentoId { get; set; }
        public string NombreArchivo { get; set; }
        public int TerminosUnicos { get; set; }
        public int TotalTokens { get; set; }
        public string TerminoMasFrecuente { get; set; }
        public int FrecuenciaMaxima { get; set; }

        public override string ToString()
        {
            return $"📄 {NombreArchivo}: {TerminosUnicos} términos únicos, " +
                   $"{TotalTokens} tokens totales, más frecuente: '{TerminoMasFrecuente}' ({FrecuenciaMaxima}x)";
        }
    }
}