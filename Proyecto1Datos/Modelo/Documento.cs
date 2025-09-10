using System.IO;
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
        
        public Documento(int id, string textoOriginal, string ruta)
        {
            this.id = id;
            this.textoOriginal = textoOriginal ?? throw new ArgumentNullException(nameof(textoOriginal));
            this.ruta = ruta ?? throw new ArgumentNullException(nameof(ruta));
            this.tokens = "";
            this.frecuencias = new ListaDobleEnlazada<TerminoFrecuencia>();
        }

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
        
        public void CalcularFrecuenciasArray(string[] tokens)
        {
            frecuencias.Limpiar();

            if (tokens == null || tokens.Length == 0)
                return;

            var contadoresArray = new ContadorTerminoOptimizado[tokens.Length];
            int cantidadUnicos = 0;
            
            foreach (var token in tokens)
            {
                if (string.IsNullOrWhiteSpace(token)) continue;

                string tokenNormalizado = token.ToLowerInvariant();
                bool encontrado = false;

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

            for (int i = 0; i < cantidadUnicos; i++)
            {
                var contador = contadoresArray[i];
                frecuencias.Agregar(new TerminoFrecuencia(contador.Token, contador.Frecuencia));
            }
        }
        
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