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
            
            buscadorVectorial = new BuscadorVectorial(this);
        }

        public async Task ActualizarIndice(string rutaDirectorio)
        {
            var archivosExistentes = new ListaDobleEnlazada<string>();
            
            var iteradorDocs = new Iterador<Documento>(documentos);
            while (iteradorDocs.Siguiente())
            {
                archivosExistentes.Agregar(iteradorDocs.Current.Ruta);
            }
            
            var archivos = Directory.GetFiles(rutaDirectorio, "*.txt");
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
                }
            }
            
            CalcularIdfGlobal();
            
            buscadorVectorial = new BuscadorVectorial(this);
        }

        public void AplicarLeyZipf(int percentil, bool eliminarFrecuentes = true)
        {
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
            var archivos = Directory.GetFiles(rutaDirectorio, "*.txt");
            foreach (var archivo in archivos)
            {
                await AgregarDocumentoDesdeArchivo(archivo);
            }
        }

        public async Task AgregarDocumentoDesdeArchivo(string rutaArchivo)
        {
            string contenido = await File.ReadAllTextAsync(rutaArchivo);
            var tokens = procesador.ProcesarTextoCompleto(contenido);
            var documento = new Documento(++contadorDocumentos, contenido, rutaArchivo);
            documento.Tokens = string.Join(" ", tokens);
            documento.CalcularFrecuencias(tokens);
            documentos.Agregar(documento);

            foreach (var token in tokens.Distinct())
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

        private Termino BuscarTermino(string token)
        {
            if (indice.Root == null) return null;
            
            var nodoActual = indice.Root;
            do
            {
                if (nodoActual.Data.Palabra.Equals(token, StringComparison.OrdinalIgnoreCase))
                {
                    return nodoActual.Data;
                }
                nodoActual = nodoActual.Sig;
            } while (nodoActual != indice.Root);
            
            return null;
        }

        public void CalcularIdfGlobal()
        {
            if (indice.Root == null) return;
            
            var nodoActual = indice.Root;
            do
            {
                nodoActual.Data.CalcularIdf(GetCantidadDocumentos());
                nodoActual = nodoActual.Sig;
            } while (nodoActual != indice.Root);
        }

        public ListaDobleEnlazada<ResultadoBusqueda> Buscar(string consulta)
        {
            var tokensConsulta = procesador.ProcesarTextoCompleto(consulta);
            var resultados = new ListaDobleEnlazada<ResultadoBusqueda>();
            
            if (documentos.Root == null) return resultados;
            
            var nodoDoc = documentos.Root;
            do
            {
                var doc = nodoDoc.Data;
                double resultado = 0;
                
                foreach (var token in tokensConsulta)
                {
                    Termino termino = BuscarTermino(token);
                    if (termino != null)
                    {
                        resultado += termino.GetTfIdf(doc);
                    }
                }
                
                if (resultado > 0)
                    resultados.Agregar(new ResultadoBusqueda(doc, resultado));
                
                nodoDoc = nodoDoc.Sig;
            } while (nodoDoc != documentos.Root);
            
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
            return contadorDocumentos;
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

        private double CalcularPromedioTerminosPorDocumento()
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
            return
                $"Documentos: {CantidadDocumentos}, Términos: {CantidadTerminos}, Promedio términos/doc: {PromedioTerminosPorDocumento:F2}";
        }
    }
}