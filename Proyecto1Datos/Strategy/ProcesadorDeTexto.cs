using System.IO;
using System.Text.RegularExpressions;
using System.Text;

namespace PruebaRider.Servicios
{
    public class ProcesadorDeTexto
    {
        private static readonly string[] StopWords = {
            "el", "la", "los", "las", "un", "una", "uno", "unos", "unas",
            "de", "del", "da", "en", "a", "al", "ante", "bajo", "con", "contra", 
            "desde", "durante", "entre", "hacia", "hasta", "para", "por", "según", 
            "sin", "sobre", "tras", "y", "e", "o", "u", "pero", "sino", "aunque", 
            "porque", "que", "si", "como", "yo", "tú", "él", "ella", "nosotros", 
            "vosotros", "ellos", "ellas", "me", "te", "se", "nos", "os", "le", 
            "les", "lo", "los", "mi", "tu", "su", "nuestro", "vuestro", "es", 
            "son", "está", "están", "ser", "estar", "tener", "haber", "hacer",
            "no", "sí", "más", "menos", "muy", "mucho", "poco", "bastante", 
            "demasiado", "ya", "aún", "todavía", "siempre", "nunca", "también", 
            "tampoco", "este", "esta", "estos", "estas", "ese", "esa", "esos", 
            "esas", "aquel", "aquella", "aquellos", "aquellas"
        };

        private static readonly Regex TokenRegex = new Regex(@"\b[a-záéíóúüñ]+\b", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        public string[] ProcesarTextoCompleto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return new string[0]; // Array vacío

            int cantidadTokensValidos = ContarTokensValidos(texto);
            
            if (cantidadTokensValidos == 0)
                return new string[0];

            string[] resultado = new string[cantidadTokensValidos];
            LlenarArrayConTokens(texto, resultado);

            return resultado;
        }
        
        private int ContarTokensValidos(string texto)
        {
            int contador = 0;
            
            foreach (Match match in TokenRegex.Matches(texto))
            {
                string token = match.Value.ToLowerInvariant();
                
                // Aplicar mismos filtros que en la segunda pasada
                if (token.Length >= 3 && !EsStopWord(token))
                {
                    contador++;
                }
            }
            
            return contador;
        }
        
        private void LlenarArrayConTokens(string texto, string[] resultado)
        {
            int indice = 0;
            
            foreach (Match match in TokenRegex.Matches(texto))
            {
                string token = match.Value.ToLowerInvariant();
                
                if (token.Length >= 3 && !EsStopWord(token))
                {
                    resultado[indice] = token;
                    indice++;
                }
            }
        }
        
        private bool EsStopWord(string palabra)
        {
            for (int i = 0; i < StopWords.Length; i++)
            {
                if (StopWords[i] == palabra)
                    return true;
            }
            return false;
        }
    }
}