using System.IO;
using PruebaRider.Estructura.Nodo;
using PruebaRider.Estructura.Vector;
using PruebaRider.Modelo;

namespace PruebaRider.Servicios
{
    /// <summary>
    /// Buscador simplificado manteniendo TU Vector con operador * sobrecargado
    /// </summary>
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

            // 1. Crear vector de consulta usando TU Vector
            var vectorConsulta = CrearVectorConsulta(consulta);
            if (vectorConsulta == null || !vectorConsulta.TieneValoresSignificativos())
                return resultados;

            // 2. Comparar con documentos usando TU similitud coseno
            var documentos = indice.GetDocumentos();
            var iteradorDocs = new Iterador<Documento>(documentos);

            while (iteradorDocs.Siguiente())
            {
                var documento = iteradorDocs.Current;
                var vectorDoc = CrearVectorDocumento(documento);

                if (vectorDoc == null || !vectorDoc.TieneValoresSignificativos())
                    continue;

                // Usar TU método SimilitudCoseno
                double similitud = vectorConsulta.SimilitudCoseno(vectorDoc);

                if (similitud > 0.001)
                {
                    var resultado = new ResultadoBusquedaVectorial(documento, similitud);
                    resultados.Agregar(resultado);
                }
            }

            // 3. Ordenar usando TU método
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

            if (tokens.Count == 0)
                return null;

            var frecuenciasConsulta = ContarFrecuencias(tokens);
            var indiceTerminos = indice.GetIndiceTerminos();

            if (indiceTerminos.Count == 0)
                return null;

            // Usar TU Vector
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

            // Usar TU Vector
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

        private TokenConteo[] ContarFrecuencias(ArrayDinamico tokens)
        {
            var conteos = new TokenConteo[tokens.Count];
            int cantidadUnicos = 0;

            var iterador = tokens.ObtenerIterador();
            while (iterador.Siguiente())
            {
                string token = iterador.Current;
                if (string.IsNullOrWhiteSpace(token)) continue;

                string tokenNorm = token.ToLowerInvariant();
                bool encontrado = false;

                for (int i = 0; i < cantidadUnicos; i++)
                {
                    if (conteos[i].Token == tokenNorm)
                    {
                        conteos[i].Frecuencia++;
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