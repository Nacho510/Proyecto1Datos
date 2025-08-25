using System.Text.RegularExpressions;
using System.Text;

namespace PruebaRider.Servicios
{
    public class ProcesadorDeTexto
    {
        private static readonly HashSet<string> StopWords = new HashSet<string>
        {
            // Artículos
            "el", "la", "los", "las", "un", "una", "uno", "unos", "unas",
            // Preposiciones
            "de", "del", "da", "en", "a", "al", "ante", "bajo", "con", "contra", "desde", 
            "durante", "entre", "hacia", "hasta", "para", "por", "según", "sin", "sobre", "tras",
            // Conjunciones
            "y", "e", "o", "u", "pero", "sino", "aunque", "porque", "que", "si", "como",
            // Pronombres
            "yo", "tú", "él", "ella", "nosotros", "vosotros", "ellos", "ellas", "me", "te", 
            "se", "nos", "os", "le", "les", "lo", "la", "los", "las", "mi", "tu", "su", "nuestro", "vuestro",
            // Verbos auxiliares y comunes
            "es", "son", "está", "están", "ser", "estar", "tener", "haber", "hacer",
            // Adverbios comunes
            "no", "sí", "más", "menos", "muy", "mucho", "poco", "bastante", "demasiado", 
            "ya", "aún", "todavía", "siempre", "nunca", "también", "tampoco",
            // Otros
            "este", "esta", "estos", "estas", "ese", "esa", "esos", "esas", "aquel", "aquella", "aquellos", "aquellas"
        };

        private static readonly Regex TokenRegex = new Regex(@"\b[a-záéíóúüñ]+\b", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public List<string> ProcesarTextoCompleto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return new List<string>();

            var tokens = new List<string>();
            
            foreach (Match match in TokenRegex.Matches(texto))
            {
                string token = match.Value.ToLowerInvariant();
                
                // Filtrar tokens muy cortos (1-2 caracteres) y stop words
                if (token.Length >= 3 && !StopWords.Contains(token))
                {
                    tokens.Add(token);
                }
            }
            return tokens;
        }
        
        public async Task<List<string>> ProcesarArchivo(string rutaArchivo)
        {
            try
            {
                string contenido = await File.ReadAllTextAsync(rutaArchivo, Encoding.UTF8);
                return ProcesarTextoCompleto(contenido);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error procesando archivo: {ex.Message}", ex);
            }
        }

        // Métodos basicos 
        public List<string> Tokenizar(string texto)
        {
            return TokenRegex.Matches(texto)
                .Select(m => m.Value)
                .ToList();
        }
        
        public List<string> Normalizar(List<string> tokens)
        {
            return tokens.Select(t => t.ToLowerInvariant()).ToList();
        }
        
        public List<string> EliminarStopWords(List<string> tokens)
        {
            return tokens.Where(t => !StopWords.Contains(t) && t.Length >= 3).ToList();
        }
        
    }
}