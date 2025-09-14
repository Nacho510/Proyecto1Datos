using System.IO;
using PruebaRider.Estructura.Nodo;
using PruebaRider.Estructura.Vector;
using PruebaRider.Modelo;

namespace PruebaRider.Servicios
{
    // Se encarga de buscar documentos similares usando los vectores TF-IDF y la similitud del coseno
    public class BuscadorVectorial
    {
        private readonly IndiceInvertido indice;

        public BuscadorVectorial(IndiceInvertido indice)
        {
            this.indice = indice ?? throw new ArgumentNullException(nameof(indice));
        }

        public ListaDobleEnlazada<ResultadoBusquedaVectorial> Buscar(string consulta)
        {
            var resultados = new ListaDobleEnlazada<ResultadoBusquedaVectorial>();

            if (string.IsNullOrWhiteSpace(consulta))
                return resultados;
            
            var vectorConsulta = CrearVectorConsulta(consulta);
            if (vectorConsulta == null || !vectorConsulta.TieneValoresSignificativos())
                return resultados;

            var documentos = indice.GetDocumentos();
            var iteradorDocs = new Iterador<Documento>(documentos);

            while (iteradorDocs.Siguiente())
            {
                var documento = iteradorDocs.Current;
                var vectorDoc = CrearVectorDocumento(documento);

                if (vectorDoc == null || !vectorDoc.TieneValoresSignificativos())
                    continue;
                
                double similitud = vectorConsulta.SimilitudCoseno(vectorDoc);

                if (similitud > 0.001)
                {
                    var resultado = new ResultadoBusquedaVectorial(documento, similitud);
                    resultados.Agregar(resultado);
                }
            }
            if (resultados.Count > 0)
            {
                resultados.OrdenarDescendente(r => r.SimilitudCoseno);
            }

            return resultados;
        }

        private Vector CrearVectorConsulta(string consulta)
        {
            var procesador = new ProcesadorDeTexto();
            
            var tokens = procesador.ProcesarTextoCompleto(consulta);

            if (tokens.Length == 0)
                return null;

            var frecuenciasConsulta = ContarFrecuencias(tokens);
            var indiceTerminos = indice.GetIndiceTerminos();

            if (indiceTerminos.Count == 0)
                return null;

            var vector = new Vector(indiceTerminos.Count);
            var iterador = indiceTerminos.ObtenerIterador();
            int posicion = 0;

            while (iterador.Siguiente())
            {
                var termino = iterador.Current;
                int frecuenciaEnConsulta = ObtenerFrecuencia(frecuenciasConsulta, termino.Palabra);

                if (frecuenciaEnConsulta > 0)
                {
                    double tfIdf = frecuenciaEnConsulta * termino.Idf;
                    vector[posicion] = tfIdf;
                }
                else
                {
                    vector[posicion] = 0.0;
                }
                posicion++;
            }
            return vector;
        }

        private Vector CrearVectorDocumento(Documento documento)
        {
            var indiceTerminos = indice.GetIndiceTerminos();
            if (indiceTerminos.Count == 0)
                return null;

            var vector = new Vector(indiceTerminos.Count);
            var iterador = indiceTerminos.ObtenerIterador();
            int posicion = 0;

            while (iterador.Siguiente())
            {
                var termino = iterador.Current;
                double tfIdf = termino.ObtenerTfIdf(documento.Id);
                vector[posicion] = tfIdf;
                posicion++;
            }

            return vector;
        }

        private TokenConteo[] ContarFrecuencias(string[] tokens)
        {
            var conteos = new TokenConteo[tokens.Length];
            int cantidadUnicos = 0;

            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                if (string.IsNullOrWhiteSpace(token)) continue;

                string tokenNorm = token.ToLowerInvariant();
                bool encontrado = false;

                for (int j = 0; j < cantidadUnicos; j++)
                {
                    if (conteos[j].Token == tokenNorm)
                    {
                        conteos[j].Frecuencia++;
                        encontrado = true;
                        break;
                    }
                }
                if (!encontrado)
                {
                    conteos[cantidadUnicos] = new TokenConteo
                    {
                        Token = tokenNorm,
                        Frecuencia = 1
                    };
                    cantidadUnicos++;
                }
            }

            var resultado = new TokenConteo[cantidadUnicos];
            Array.Copy(conteos, resultado, cantidadUnicos);
            return resultado;
        }

        private int ObtenerFrecuencia(TokenConteo[] conteos, string token)
        {
            for (int i = 0; i < conteos.Length; i++)
            {
                if (string.Equals(conteos[i].Token, token, StringComparison.OrdinalIgnoreCase))
                    return conteos[i].Frecuencia;
            }

            return 0;
        }

        private struct TokenConteo
        {
            public string Token { get; set; }
            public int Frecuencia { get; set; }
        }
    }
}