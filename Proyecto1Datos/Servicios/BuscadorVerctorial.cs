using System.IO;
using PruebaRider.Estructura.Nodo;
using PruebaRider.Estructura.Vector;
using PruebaRider.Modelo;

namespace PruebaRider.Servicios
{
    /// <summary>
    /// Buscador vectorial OPTIMIZADO
    /// - Sin dependencias de genéricos prohibidos
    /// - Enfocado únicamente en similitud coseno
    /// - Código limpio y eficiente
    /// </summary>
    public class BuscadorVectorial
    {
        private readonly IndiceInvertido indice;

        public BuscadorVectorial(IndiceInvertido indice)
        {
            this.indice = indice ?? throw new ArgumentNullException(nameof(indice));
        }

        /// <summary>
        /// BÚSQUEDA VECTORIAL PRINCIPAL - Similitud Coseno
        /// </summary>
        public ListaDobleEnlazada<ResultadoBusquedaVectorial> Buscar(string consulta)
        {
            var resultados = new ListaDobleEnlazada<ResultadoBusquedaVectorial>();

            if (string.IsNullOrWhiteSpace(consulta))
                return resultados;

            // 1. CREAR VECTOR DE CONSULTA
            var vectorConsulta = CrearVectorConsulta(consulta);
            if (vectorConsulta == null || !vectorConsulta.TieneValoresSignificativos())
                return resultados;

            // 2. COMPARAR CON TODOS LOS DOCUMENTOS
            var documentos = indice.GetDocumentos();
            var iteradorDocs = new Iterador<Documento>(documentos);

            while (iteradorDocs.Siguiente())
            {
                var documento = iteradorDocs.Current;
                var vectorDoc = CrearVectorDocumento(documento);
                
                if (vectorDoc == null || !vectorDoc.TieneValoresSignificativos())
                    continue;

                // CALCULAR SIMILITUD COSENO
                double similitud = vectorConsulta.SimilitudCoseno(vectorDoc);

                if (similitud > 0.001) // Umbral mínimo
                {
                    var resultado = new ResultadoBusquedaVectorial(documento, similitud);
                    resultados.Agregar(resultado);
                }
            }

            // 3. ORDENAR POR SIMILITUD DESCENDENTE
            if (resultados.Count > 0)
            {
                resultados.OrdenarDescendente(r => r.SimilitudCoseno);
            }

            return resultados;
        }

        /// <summary>
        /// Crear vector TF-IDF para la consulta
        /// </summary>
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

        /// <summary>
        /// Crear vector TF-IDF para un documento
        /// </summary>
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

        /// <summary>
        /// Contar frecuencias usando ArrayDinamico
        /// </summary>
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

        /// <summary>
        /// Obtener frecuencia de un token específico
        /// </summary>
        private int ObtenerFrecuencia(TokenConteo[] conteos, string token)
        {
            for (int i = 0; i < conteos.Length; i++)
            {
                if (string.Equals(conteos[i].Token, token, StringComparison.OrdinalIgnoreCase))
                    return conteos[i].Frecuencia;
            }
            return 0;
        }

        /// <summary>
        /// Estructura para conteo de tokens
        /// </summary>
        private struct TokenConteo
        {
            public string Token { get; set; }
            public int Frecuencia { get; set; }
        }
    }
}