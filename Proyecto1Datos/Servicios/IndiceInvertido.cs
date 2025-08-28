using PruebaRider.Modelo;
using PruebaRider.Estructura.Nodo;
using PruebaRider.Strategy;
using PruebaRider.Persistencia;

namespace PruebaRider.Servicios
{
    /// <summary>
    /// Índice Invertido simplificado - Solo funciones esenciales
    /// Mantiene búsqueda binaria O(log n) con código directo
    /// </summary>
    public class IndiceInvertido
    {
        private ListaDobleEnlazada<Termino> indice;
        private ListaDobleEnlazada<Documento> documentos;
        private ProcesadorDeTexto procesador;
        private SerializadorBinario serializador;
        private BuscadorVectorial buscadorVectorial;
        private int contadorDocumentos;

        public IndiceInvertido()
        {
            indice = new ListaDobleEnlazada<Termino>();
            documentos = new ListaDobleEnlazada<Documento>();
            procesador = new ProcesadorDeTexto();
            serializador = new SerializadorBinario();
            contadorDocumentos = 0;
        }

        /// <summary>
        /// Crear índice desde directorio - Método principal
        /// </summary>
        public async Task CrearDesdeRuta(string rutaDirectorio)
        {
            Console.WriteLine("🚀 Creando índice...");
            
            Limpiar();
            await CargarDirectorio(rutaDirectorio);
            OrdenarIndice();
            CalcularIdfGlobal();
            
            buscadorVectorial = new BuscadorVectorial(this);
            
            Console.WriteLine($"✅ Índice creado: {documentos.Count} documentos, {indice.Count} términos");
        }

        /// <summary>
        /// Cargar todos los archivos del directorio
        /// </summary>
        public async Task CargarDirectorio(string rutaDirectorio)
        {
            if (!Directory.Exists(rutaDirectorio))
                throw new DirectoryNotFoundException($"Directorio no encontrado: {rutaDirectorio}");
                
            var archivos = Directory.GetFiles(rutaDirectorio, "*.txt");
            
            if (archivos.Length == 0)
                throw new InvalidOperationException("No se encontraron archivos .txt");
                
            foreach (var archivo in archivos)
            {
                await AgregarDocumento(archivo);
            }
        }

        /// <summary>
        /// Agregar un documento al índice
        /// </summary>
        public async Task AgregarDocumento(string rutaArchivo)
        {
            string contenido = await File.ReadAllTextAsync(rutaArchivo);
            
            if (string.IsNullOrWhiteSpace(contenido)) return;
            
            var tokens = procesador.ProcesarTextoCompleto(contenido);
            if (tokens.Count == 0) return;
            
            // Crear documento
            var documento = new Documento(++contadorDocumentos, contenido, rutaArchivo);
            documento.CalcularFrecuencias(tokens);
            documentos.Agregar(documento);
            
            // Procesar términos únicos
            var tokensUnicos = EliminarDuplicados(tokens);
            foreach (var token in tokensUnicos)
            {
                AgregarTermino(token, documento);
            }
        }

        /// <summary>
        /// Agregar término al índice
        /// </summary>
        private void AgregarTermino(string palabra, Documento documento)
        {
            var terminoExistente = BuscarTermino(palabra);
            
            if (terminoExistente == null)
            {
                var nuevoTermino = new Termino(palabra);
                nuevoTermino.AgregarDocumento(documento);
                indice.Agregar(nuevoTermino);
            }
            else
            {
                terminoExistente.AgregarDocumento(documento);
            }
        }

        /// <summary>
        /// Buscar término en el índice
        /// </summary>
        public Termino BuscarTermino(string palabra)
        {
            if (indice.EstaOrdenada && indice.Count > 10)
            {
                // Búsqueda binaria O(log n)
                var dummy = new Termino(palabra);
                return indice.BuscarBinario(dummy, CompararTerminos);
            }
            else
            {
                // Búsqueda lineal O(n)
                var iterador = new Iterador<Termino>(indice);
                while (iterador.Siguiente())
                {
                    if (iterador.Current.Palabra.Equals(palabra, StringComparison.OrdinalIgnoreCase))
                        return iterador.Current;
                }
                return null;
            }
        }

        /// <summary>
        /// Búsqueda de documentos con TF-IDF
        /// </summary>
        public ListaDobleEnlazada<ResultadoBusqueda> Buscar(string consulta)
        {
            var resultados = new ListaDobleEnlazada<ResultadoBusqueda>();
            var tokensConsulta = procesador.ProcesarTextoCompleto(consulta);
            
            if (tokensConsulta.Count == 0) return resultados;
            
            // Buscar términos de la consulta
            var terminosConsulta = new ListaDobleEnlazada<Termino>();
            var tokensUnicos = EliminarDuplicados(tokensConsulta);
            
            foreach (var token in tokensUnicos)
            {
                var termino = BuscarTermino(token);
                if (termino != null)
                    terminosConsulta.Agregar(termino);
            }
            
            if (terminosConsulta.Count == 0) return resultados;
            
            // Calcular puntuaciones para cada documento
            var iteradorDocs = new Iterador<Documento>(documentos);
            while (iteradorDocs.Siguiente())
            {
                var doc = iteradorDocs.Current;
                double puntuacion = 0;
                
                var iteradorTerminos = new Iterador<Termino>(terminosConsulta);
                while (iteradorTerminos.Siguiente())
                {
                    puntuacion += iteradorTerminos.Current.GetTfIdf(doc);
                }
                
                if (puntuacion > 0)
                    resultados.Agregar(new ResultadoBusqueda(doc, puntuacion));
            }
            
            // Ordenar por puntuación
            resultados.OrdenarDescendente(r => r.Score);
            return resultados;
        }

        /// <summary>
        /// Búsqueda vectorial con similitud coseno
        /// </summary>
        public ListaDobleEnlazada<ResultadoBusquedaVectorial> BuscarConSimilitudCoseno(string consulta)
        {
            if (buscadorVectorial == null)
                buscadorVectorial = new BuscadorVectorial(this);
            
            return buscadorVectorial.BuscarConSimilitudCoseno(consulta);
        }

        /// <summary>
        /// Aplicar Ley de Zipf
        /// </summary>
        public void AplicarLeyZipf(int percentil, bool eliminarFrecuentes = true)
        {
            if (percentil <= 0 || percentil >= 100)
                throw new ArgumentException("Percentil debe estar entre 1 y 99");
                
            var contexto = new ContextoZipf();
            
            if (eliminarFrecuentes)
                contexto.EstablecerEstrategia(new EliminarTerminosFrecuentes(indice));
            else
                contexto.EstablecerEstrategia(new EliminarTerminosRaros(indice));
                
            contexto.AplicarLeyZipf(percentil);
            
            OrdenarIndice();
            CalcularIdfGlobal();
            buscadorVectorial = new BuscadorVectorial(this);
        }

        /// <summary>
        /// Actualizar índice con nuevos documentos
        /// </summary>
        public async Task ActualizarIndice(string rutaDirectorio)
        {
            var archivosExistentes = new List<string>();
            var iterador = new Iterador<Documento>(documentos);
            while (iterador.Siguiente())
            {
                archivosExistentes.Add(iterador.Current.Ruta);
            }
            
            var archivos = Directory.GetFiles(rutaDirectorio, "*.txt");
            int agregados = 0;
            
            foreach (var archivo in archivos)
            {
                if (!archivosExistentes.Contains(archivo))
                {
                    await AgregarDocumento(archivo);
                    agregados++;
                }
            }
            
            if (agregados > 0)
            {
                OrdenarIndice();
                CalcularIdfGlobal();
                buscadorVectorial = new BuscadorVectorial(this);
                Console.WriteLine($"✅ Agregados {agregados} documentos nuevos");
            }
        }

        /// <summary>
        /// Guardar índice en archivo binario
        /// </summary>
        public void GuardarEnArchivoBinario(string rutaArchivo)
        {
            serializador.GuardarIndice(rutaArchivo, indice, documentos);
        }

        /// <summary>
        /// Cargar índice desde archivo binario
        /// </summary>
        public void CargarDesdeArchivoBinario(string rutaArchivo)
        {
            var (indiceNuevo, documentosNuevos) = serializador.CargarIndice(rutaArchivo);
            
            indice = indiceNuevo;
            documentos = documentosNuevos;
            
            // Recalcular contador
            contadorDocumentos = 0;
            var iterador = new Iterador<Documento>(documentos);
            while (iterador.Siguiente())
            {
                if (iterador.Current.Id > contadorDocumentos)
                    contadorDocumentos = iterador.Current.Id;
            }
            
            OrdenarIndice();
            buscadorVectorial = new BuscadorVectorial(this);
        }

        /// <summary>
        /// Ordenar índice alfabéticamente para búsqueda binaria
        /// </summary>
        private void OrdenarIndice()
        {
            indice.OrdenarCon(CompararTerminos);
        }

        /// <summary>
        /// Calcular IDF para todos los términos
        /// </summary>
        public void CalcularIdfGlobal()
        {
            int totalDocumentos = documentos.Count;
            if (totalDocumentos == 0) return;
            
            var iterador = new Iterador<Termino>(indice);
            while (iterador.Siguiente())
            {
                iterador.Current.CalcularIdf(totalDocumentos);
            }
        }

        /// <summary>
        /// Eliminar duplicados de lista de tokens
        /// </summary>
        private List<string> EliminarDuplicados(List<string> tokens)
        {
            var unicos = new List<string>();
            
            foreach (var token in tokens)
            {
                if (string.IsNullOrWhiteSpace(token)) continue;
                
                bool existe = false;
                foreach (var existente in unicos)
                {
                    if (existente.Equals(token, StringComparison.OrdinalIgnoreCase))
                    {
                        existe = true;
                        break;
                    }
                }
                
                if (!existe)
                    unicos.Add(token.ToLowerInvariant());
            }
            
            return unicos;
        }

        /// <summary>
        /// Comparador para ordenar términos alfabéticamente
        /// </summary>
        private int CompararTerminos(Termino t1, Termino t2)
        {
            return string.Compare(t1.Palabra, t2.Palabra, StringComparison.OrdinalIgnoreCase);
        }

        public void Limpiar()
        {
            indice.Limpiar();
            documentos.Limpiar();
            contadorDocumentos = 0;
        }

        public int GetCantidadDocumentos() => documentos.Count;
        public ListaDobleEnlazada<Documento> GetDocumentos() => documentos;
        public ListaDobleEnlazada<Termino> GetIndice() => indice;

        /// <summary>
        /// Estadísticas básicas del índice
        /// </summary>
        public EstadisticasIndice ObtenerEstadisticas()
        {
            return new EstadisticasIndice
            {
                CantidadDocumentos = documentos.Count,
                CantidadTerminos = indice.Count,
                IndiceOrdenado = indice.EstaOrdenada
            };
        }
    }

    /// <summary>
    /// Estadísticas básicas del índice
    /// </summary>
    public class EstadisticasIndice
    {
        public int CantidadDocumentos { get; set; }
        public int CantidadTerminos { get; set; }
        public bool IndiceOrdenado { get; set; }

        public override string ToString()
        {
            return $"📊 Documentos: {CantidadDocumentos}, Términos: {CantidadTerminos}, " +
                   $"Ordenado: {(IndiceOrdenado ? "Sí" : "No")}";
        }
    }
}