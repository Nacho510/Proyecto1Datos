using PruebaRider.Modelo;
using PruebaRider.Estructura.Nodo;
using PruebaRider.Strategy;
using PruebaRider.Persistencia;

namespace PruebaRider.Servicios
{
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

        // Crear desde una ruta (primera vez)
        public async Task CrearDesdeRuta(string rutaDirectorio)
        {
            Limpiar();
            await CargarDirectorio(rutaDirectorio);
            CalcularIdfGlobal();
            
            // CRÍTICO: Inicializar el buscador vectorial
            buscadorVectorial = new BuscadorVectorial(this);
            
            Console.WriteLine($"Índice creado exitosamente con {documentos.Count} documentos y {indice.Count} términos únicos.");
        }

        public async Task ActualizarIndice(string rutaDirectorio)
        {
            var archivosExistentes = new ListaDobleEnlazada<string>();
            
            var iteradorDocs = new Iterador<Documento>(documentos);
            while (iteradorDocs.Siguiente())
            {
                archivosExistentes.Agregar(iteradorDocs.Current.Ruta);
            }
            
            if (!Directory.Exists(rutaDirectorio))
                throw new DirectoryNotFoundException($"El directorio {rutaDirectorio} no existe");
                
            var archivos = Directory.GetFiles(rutaDirectorio, "*.txt");
            int archivosNuevos = 0;
            
            foreach (var archivo in archivos)
            {
                bool existe = false;
                var iteradorRutas = new Iterador<string>(archivosExistentes);
                while (iteradorRutas.Siguiente())
                {
                    if (iteradorRutas.Current == archivo)
                    {
                        existe = true;
                        break;
                    }
                }
                
                if (!existe)
                {
                    await AgregarDocumentoDesdeArchivo(archivo);
                    archivosNuevos++;
                }
            }
            
            if (archivosNuevos > 0)
            {
                CalcularIdfGlobal();
                buscadorVectorial = new BuscadorVectorial(this);
                Console.WriteLine($"Se agregaron {archivosNuevos} documentos nuevos al índice.");
            }
            else
            {
                Console.WriteLine("No se encontraron documentos nuevos para agregar.");
            }
        }

        public void AplicarLeyZipf(int percentil, bool eliminarFrecuentes = true)
        {
            if (percentil <= 0 || percentil >= 100)
                throw new ArgumentException("El percentil debe estar entre 1 y 99");
                
            if (indice.Count == 0)
            {
                Console.WriteLine("No hay términos en el índice para aplicar Ley de Zipf");
                return;
            }
            
            int terminosAntes = indice.Count;
            
            var contexto = new ContextoZipf();
            
            if (eliminarFrecuentes)
            {
                contexto.EstablecerEstrategia(new EliminarTerminosFrecuentes(indice));
            }
            else
            {
                contexto.EstablecerEstrategia(new EliminarTerminosRaros(indice));
            }
            
            contexto.AplicarLeyZipf(percentil);
            
            CalcularIdfGlobal();
            
            // Reinicializar buscador vectorial
            buscadorVectorial = new BuscadorVectorial(this);
            
            Console.WriteLine($"Ley de Zipf aplicada. Términos antes: {terminosAntes}, después: {indice.Count}");
        }

        public void GuardarEnArchivoBinario(string rutaArchivo)
        {
            serializador.GuardarIndice(rutaArchivo, indice, documentos);
        }

        public void CargarDesdeArchivoBinario(string rutaArchivo)
        {
            var (indiceNuevo, documentosNuevos) = serializador.CargarIndice(rutaArchivo);
            
            indice = indiceNuevo;
            documentos = documentosNuevos;
            
            // Recalcular contador de documentos
            contadorDocumentos = 0;
            var iterador = new Iterador<Documento>(documentos);
            while (iterador.Siguiente())
            {
                if (iterador.Current.Id > contadorDocumentos)
                    contadorDocumentos = iterador.Current.Id;
            }
            
            buscadorVectorial = new BuscadorVectorial(this);
        }

        public ListaDobleEnlazada<ResultadoBusquedaVectorial> BuscarConSimilitudCoseno(string consulta)
        {
            if (buscadorVectorial == null)
                buscadorVectorial = new BuscadorVectorial(this);
            
            return buscadorVectorial.BuscarConSimilitudCoseno(consulta);
        }

        public async Task CargarDirectorio(string rutaDirectorio)
        {
            if (!Directory.Exists(rutaDirectorio))
                throw new DirectoryNotFoundException($"El directorio {rutaDirectorio} no existe");
                
            var archivos = Directory.GetFiles(rutaDirectorio, "*.txt");
            
            if (archivos.Length == 0)
                throw new InvalidOperationException("No se encontraron archivos .txt en el directorio");
            
            Console.WriteLine($"Cargando {archivos.Length} archivos...");
            
            foreach (var archivo in archivos)
            {
                await AgregarDocumentoDesdeArchivo(archivo);
            }
        }

        public async Task AgregarDocumentoDesdeArchivo(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
                throw new FileNotFoundException($"El archivo {rutaArchivo} no existe");
                
            try
            {
                string contenido = await File.ReadAllTextAsync(rutaArchivo);
                
                if (string.IsNullOrWhiteSpace(contenido))
                {
                    Console.WriteLine($"Advertencia: El archivo {rutaArchivo} está vacío");
                    return;
                }
                
                var tokens = procesador.ProcesarTextoCompleto(contenido);
                
                if (tokens.Count == 0)
                {
                    Console.WriteLine($"Advertencia: No se encontraron tokens válidos en {rutaArchivo}");
                    return;
                }
                
                var documento = new Documento(++contadorDocumentos, contenido, rutaArchivo);
                documento.Tokens = string.Join(" ", tokens);
                documento.CalcularFrecuencias(tokens);
                documentos.Agregar(documento);

                // Procesar solo tokens únicos para evitar duplicados
                var tokensUnicos = tokens.Distinct().ToList();
                
                foreach (var token in tokensUnicos)
                {
                    Termino terminoExistente = BuscarTermino(token);
                    
                    if (terminoExistente == null)
                    {
                        var nuevoTermino = new Termino(token);
                        nuevoTermino.AgregarDocumento(documento);
                        indice.Agregar(nuevoTermino);
                    }
                    else
                    {
                        terminoExistente.AgregarDocumento(documento);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando archivo {rutaArchivo}: {ex.Message}");
                throw;
            }
        }

        private Termino BuscarTermino(string token) // O(n)
        {
            if (indice.Root == null) return null;
            
            var iterador = new Iterador<Termino>(indice);
            while (iterador.Siguiente())
            {
                if (iterador.Current.Palabra.Equals(token, StringComparison.OrdinalIgnoreCase))
                {
                    return iterador.Current;
                }
            }
            
            return null;
        }

        public void CalcularIdfGlobal() // O(t) donde t = número de términos
        {
            if (indice.Root == null) return;
            
            int totalDocumentos = GetCantidadDocumentos();
            if (totalDocumentos == 0) return;
            
            var iterador = new Iterador<Termino>(indice);
            while (iterador.Siguiente())
            {
                iterador.Current.CalcularIdf(totalDocumentos);
            }
        }

        public ListaDobleEnlazada<ResultadoBusqueda> Buscar(string consulta) // O(d*t) donde d=docs, t=términos
        {
            var tokensConsulta = procesador.ProcesarTextoCompleto(consulta);
            var resultados = new ListaDobleEnlazada<ResultadoBusqueda>();
            
            if (documentos.Root == null || tokensConsulta.Count == 0) return resultados;
            
            var iteradorDocs = new Iterador<Documento>(documentos);
            while (iteradorDocs.Siguiente())
            {
                var doc = iteradorDocs.Current;
                double puntuacion = 0;
                
                foreach (var token in tokensConsulta)
                {
                    Termino termino = BuscarTermino(token);
                    if (termino != null)
                    {
                        puntuacion += termino.GetTfIdf(doc);
                    }
                }
                
                if (puntuacion > 0)
                    resultados.Agregar(new ResultadoBusqueda(doc, puntuacion));
            }
            
            resultados.OrdenarDescendente(r => r.Score);
            return resultados;
        }

        public void Limpiar()
        {
            indice.Limpiar(); 
            documentos.Limpiar();
            contadorDocumentos = 0;
        }

        public int GetCantidadDocumentos()
        {
            return documentos.Count;
        }
        
        public ListaDobleEnlazada<Documento> GetDocumentos()
        {
            return documentos;
        }
        
        public ListaDobleEnlazada<Termino> GetIndice()
        {
            return indice;
        }

        public EstadisticasIndice ObtenerEstadisticas()
        {
            return new EstadisticasIndice
            {
                CantidadDocumentos = documentos.Count,
                CantidadTerminos = indice.Count,
                PromedioTerminosPorDocumento = CalcularPromedioTerminosPorDocumento()
            };
        }

        private double CalcularPromedioTerminosPorDocumento() // O(d)
        {
            if (documentos.Count == 0) return 0;
            
            int totalTerminos = 0;
            var iterador = new Iterador<Documento>(documentos);
            while (iterador.Siguiente())
            {
                totalTerminos += iterador.Current.Frecuencias.Count;
            }
            
            return (double)totalTerminos / documentos.Count;
        }

        public bool DocumentoExiste(string rutaArchivo)
        {
            var iterador = new Iterador<Documento>(documentos);
            while (iterador.Siguiente())
            {
                if (iterador.Current.Ruta == rutaArchivo)
                    return true;
            }
            return false;
        }

        public Documento ObtenerDocumentoPorId(int id)
        {
            var iterador = new Iterador<Documento>(documentos);
            while (iterador.Siguiente())
            {
                if (iterador.Current.Id == id)
                    return iterador.Current;
            }
            return null;
        }
    }

    public class EstadisticasIndice
    {
        public int CantidadDocumentos { get; set; }
        public int CantidadTerminos { get; set; }
        public double PromedioTerminosPorDocumento { get; set; }

        public override string ToString()
        {
            return $"Documentos: {CantidadDocumentos}, Términos: {CantidadTerminos}, Promedio términos/doc: {PromedioTerminosPorDocumento:F2}";
        }
    }
}