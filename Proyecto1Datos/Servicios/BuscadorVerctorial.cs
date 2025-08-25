using PruebaRider.Estructura.Nodo;
using PruebaRider.Estructura.Vector;
using PruebaRider.Modelo;
using PruebaRider.Servicios;

namespace PruebaRider.Servicios
{
    public class BuscadorVectorial
    {
        private readonly IndiceInvertido indiceInvertido;
        private readonly ProcesadorDeTexto procesador;

        public BuscadorVectorial(IndiceInvertido indiceInvertido)
        {
            this.indiceInvertido = indiceInvertido;
            this.procesador = new ProcesadorDeTexto();
        }

        public ListaDobleEnlazada<ResultadoBusquedaVectorial> BuscarConSimilitudCoseno(string consulta)
        {
            var tokensConsulta = procesador.ProcesarTextoCompleto(consulta);
            if (tokensConsulta.Count == 0)
                return new ListaDobleEnlazada<ResultadoBusquedaVectorial>();

            // Obtener todos los términos únicos del índice
            var terminos = ObtenerTerminosUnicos();
            
            // Crear vector de consulta
            var vectorConsulta = CrearVectorConsulta(tokensConsulta, terminos);
            
            // Calcular similitud con cada documento
            var resultados = new ListaDobleEnlazada<ResultadoBusquedaVectorial>();
            
            var iteradorDocs = new Iterador<Documento>(indiceInvertido.GetDocumentos());
            while (iteradorDocs.Siguiente())
            {
                var documento = iteradorDocs.Current;
                var vectorDocumento = CrearVectorDocumento(documento, terminos);
                
                double similitud = vectorConsulta.SimilitudCoseno(vectorDocumento);
                
                if (similitud > 0)
                {
                    resultados.Agregar(new ResultadoBusquedaVectorial(documento, similitud));
                }
            }
            
            // Ordenar resultados por similitud descendente
            resultados.OrdenarDescendente(r => r.SimilitudCoseno);
            return resultados;
        }

        private ListaDobleEnlazada<string> ObtenerTerminosUnicos()
        {
            var terminos = new ListaDobleEnlazada<string>();
            
            var iterador = new Iterador<Termino>(indiceInvertido.GetIndice());
            while (iterador.Siguiente())
            {
                terminos.Agregar(iterador.Current.Palabra);
            }
            
            return terminos;
        }

        private Vector CrearVectorConsulta(List<string> tokensConsulta, ListaDobleEnlazada<string> terminos)
        {
            var vector = new Vector(terminos.Count);
            
            // Contar frecuencias de términos en la consulta
            var frecuenciasConsulta = ContarFrecuencias(tokensConsulta);
            
            var iteradorTerminos = new Iterador<string>(terminos);
            int indice = 0;
            
            while (iteradorTerminos.Siguiente())
            {
                string termino = iteradorTerminos.Current;
                
                // Buscar frecuencia del término en la consulta
                int tf = 0;
                foreach (var kvp in frecuenciasConsulta)
                {
                    if (kvp.Key == termino)
                    {
                        tf = kvp.Value;
                        break;
                    }
                }
                
                if (tf > 0)
                {
                    // Buscar el término en el índice para obtener su IDF
                    var terminoIndice = BuscarTerminoEnIndice(termino);
                    if (terminoIndice != null)
                    {
                        vector[indice] = tf * terminoIndice.Idf;
                    }
                }
                else
                {
                    vector[indice] = 0;
                }
                
                indice++;
            }
            
            return vector;
        }

        private Vector CrearVectorDocumento(Documento documento, ListaDobleEnlazada<string> terminos)
        {
            var vector = new Vector(terminos.Count);
            
            var iteradorTerminos = new Iterador<string>(terminos);
            int indice = 0;
            
            while (iteradorTerminos.Siguiente())
            {
                string termino = iteradorTerminos.Current;
                
                // Obtener TF del término en el documento
                int tf = documento.GetFrecuencia(termino);
                
                if (tf > 0)
                {
                    // Buscar el término en el índice para obtener su IDF
                    var terminoIndice = BuscarTerminoEnIndice(termino);
                    if (terminoIndice != null)
                    {
                        vector[indice] = tf * terminoIndice.Idf;
                    }
                }
                else
                {
                    vector[indice] = 0;
                }
                
                indice++;
            }
            
            return vector;
        }

        private Termino BuscarTerminoEnIndice(string palabra)
        {
            var iterador = new Iterador<Termino>(indiceInvertido.GetIndice());
            while (iterador.Siguiente())
            {
                if (iterador.Current.Palabra.Equals(palabra, StringComparison.OrdinalIgnoreCase))
                {
                    return iterador.Current;
                }
            }
            return null;
        }

        private List<KeyValuePair<string, int>> ContarFrecuencias(List<string> tokens)
        {
            var frecuencias = new List<KeyValuePair<string, int>>();
            
            foreach (var token in tokens)
            {
                bool encontrado = false;
                
                for (int i = 0; i < frecuencias.Count; i++)
                {
                    if (frecuencias[i].Key == token)
                    {
                        frecuencias[i] = new KeyValuePair<string, int>(token, frecuencias[i].Value + 1);
                        encontrado = true;
                        break;
                    }
                }
                
                if (!encontrado)
                {
                    frecuencias.Add(new KeyValuePair<string, int>(token, 1));
                }
            }
            
            return frecuencias;
        }
    }

    public class ResultadoBusquedaVectorial
    {
        public Documento Documento { get; set; }
        public double SimilitudCoseno { get; set; }

        public ResultadoBusquedaVectorial(Documento documento, double similitudCoseno)
        {
            Documento = documento;
            SimilitudCoseno = similitudCoseno;
        }

        public override string ToString()
        {
            return $"Documento: {Documento.Ruta} | Similitud: {SimilitudCoseno:F4}";
        }
    }
}