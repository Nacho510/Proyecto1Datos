using System.IO;
using PruebaRider.Modelo;
using PruebaRider.Estructura.Nodo;
using PruebaRider.Estructura.Vector;
using PruebaRider.Persistencia;
using PruebaRider.Strategy;

namespace PruebaRider.Servicios
{
    /// <summary>
    /// Índice Invertido simplificado manteniendo TUS estructuras implementadas
    /// </summary>
    public class IndiceInvertido
    {
        private VectorOrdenado<Termino> indiceTerminos;
        private ListaDobleEnlazada<Documento> documentos;
        private ProcesadorDeTexto procesador;
        private SerializadorBinario serializador;
        private int contadorDocumentos;
        private bool zipfAplicado;

        public IndiceInvertido()
        {
            indiceTerminos = new VectorOrdenado<Termino>();
            documentos = new ListaDobleEnlazada<Documento>();
            procesador = new ProcesadorDeTexto();
            serializador = new SerializadorBinario();
            contadorDocumentos = 0;
            zipfAplicado = false;
        }

        /// <summary>
        /// Crear índice CON Ley de Zipf obligatoria
        /// </summary>
        public async Task CrearDesdeRuta(string rutaDirectorio, int percentilZipf = 15)
        {
            Limpiar();
            
            // 1. Cargar documentos
            await CargarDirectorio(rutaDirectorio);
            
            // 2. RadixSort ANTES de Zipf
            indiceTerminos.OrdenarRadix();
            
            // 3. Aplicar Ley de Zipf (OBLIGATORIO)
            await AplicarLeyDeZipf(percentilZipf);
            
            // 4. Calcular TF-IDF
            CalcularMetricasTfIdf();
            
            // 5. Reordenar final
            indiceTerminos.OrdenarRadix();
        }

        private async Task CargarDirectorio(string rutaDirectorio)
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

        private async Task AgregarDocumento(string rutaArchivo)
        {
            try
            {
                string contenido = await File.ReadAllTextAsync(rutaArchivo);
                if (string.IsNullOrWhiteSpace(contenido)) return;

                var tokens = procesador.ProcesarTextoCompleto(contenido);
                if (tokens.Count == 0) return;

                var documento = new Documento(++contadorDocumentos, contenido, rutaArchivo);
                documento.CalcularFrecuenciasArray(tokens.ToArray());
                documentos.Agregar(documento);

                ProcesarTerminosDelDocumento(documento, tokens);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error procesando {Path.GetFileName(rutaArchivo)}: {ex.Message}");
            }
        }

        private void ProcesarTerminosDelDocumento(Documento documento, ArrayDinamico tokens)
        {
            var frecuenciasLocales = ContarFrecuenciasLocales(tokens);

            for (int i = 0; i < frecuenciasLocales.Length; i++)
            {
                var tf = frecuenciasLocales[i];
                AgregarTerminoAlIndice(tf.Token, documento, tf.Frecuencia);
            }
        }

        private TerminoFrecuencia[] ContarFrecuenciasLocales(ArrayDinamico tokens)
        {
            var resultado = new TerminoFrecuencia[tokens.Count];
            int cantidadUnicos = 0;

            var iterador = tokens.ObtenerIterador();
            while (iterador.Siguiente())
            {
                string token = iterador.Current.ToLowerInvariant();
                bool encontrado = false;

                for (int i = 0; i < cantidadUnicos; i++)
                {
                    if (resultado[i].Token == token)
                    {
                        resultado[i].Frecuencia++;
                        encontrado = true;
                        break;
                    }
                }

                if (!encontrado)
                {
                    resultado[cantidadUnicos] = new TerminoFrecuencia(token, 1);
                    cantidadUnicos++;
                }
            }

            var final = new TerminoFrecuencia[cantidadUnicos];
            Array.Copy(resultado, final, cantidadUnicos);
            return final;
        }

        private void AgregarTerminoAlIndice(string palabra, Documento documento, int frecuencia)
        {
            var terminoExistente = BuscarTermino(palabra);

            if (terminoExistente == null)
            {
                var nuevoTermino = new Termino(palabra);
                nuevoTermino.AgregarDocumento(documento, frecuencia);
                indiceTerminos.Agregar(nuevoTermino);
            }
            else
            {
                terminoExistente.AgregarDocumento(documento, frecuencia);
            }
        }

        /// <summary>
        /// Aplicar Ley de Zipf SIMPLIFICADA pero efectiva
        /// </summary>
        private async Task AplicarLeyDeZipf(int percentil)
        {
            if (indiceTerminos.Count == 0) return;

            Console.WriteLine($"🔥 Aplicando Ley de Zipf ({percentil}%)...");

            // Convertir a lista para trabajar
            var listaTerminos = new ListaDobleEnlazada<Termino>();
            var iterador = indiceTerminos.ObtenerIterador();
            while (iterador.Siguiente())
            {
                listaTerminos.Agregar(iterador.Current);
            }

            // Crear estrategia simple: eliminar términos más frecuentes
            var estrategia = new EliminarTerminosFrecuentesConservadora(listaTerminos, documentos.Count);
            var contexto = new ContextoZipf();
            contexto.EstablecerEstrategia(estrategia);
            contexto.AplicarLeyZipf(percentil);

            // Reconstruir vector ordenado
            indiceTerminos.Limpiar();
            var iteradorNuevo = new Iterador<Termino>(listaTerminos);
            while (iteradorNuevo.Siguiente())
            {
                indiceTerminos.Agregar(iteradorNuevo.Current);
            }

            zipfAplicado = true;
            Console.WriteLine($"✅ Zipf aplicado: {indiceTerminos.Count} términos conservados");
        }

        public Termino BuscarTermino(string palabra)
        {
            if (string.IsNullOrWhiteSpace(palabra)) return null;

            string palabraNormalizada = palabra.ToLowerInvariant();

            if (indiceTerminos.EstaOrdenado)
            {
                var terminoBusqueda = new Termino(palabraNormalizada);
                return indiceTerminos.BuscarBinario(terminoBusqueda);
            }
            else
            {
                var iterador = indiceTerminos.ObtenerIterador();
                while (iterador.Siguiente())
                {
                    if (string.Equals(iterador.Current.Palabra, palabraNormalizada, StringComparison.OrdinalIgnoreCase))
                        return iterador.Current;
                }
                return null;
            }
        }

        private void CalcularMetricasTfIdf()
        {
            int totalDocumentos = documentos.Count;
            if (totalDocumentos == 0) return;

            var iterador = indiceTerminos.ObtenerIterador();
            while (iterador.Siguiente())
            {
                iterador.Current.CalcularIdf(totalDocumentos);
            }
        }

        // PERSISTENCIA manteniendo TU SerializadorBinario
        public void GuardarEnArchivoBinario(string rutaArchivo)
        {
            var listaTerminos = new ListaDobleEnlazada<Termino>();
            var iterador = indiceTerminos.ObtenerIterador();
            while (iterador.Siguiente())
            {
                listaTerminos.Agregar(iterador.Current);
            }

            serializador.GuardarIndice(rutaArchivo, listaTerminos, documentos);
        }

        public void CargarDesdeArchivoBinario(string rutaArchivo)
        {
            var (indiceNuevo, documentosNuevos) = serializador.CargarIndice(rutaArchivo);

            indiceTerminos = new VectorOrdenado<Termino>();
            var iterador = new Iterador<Termino>(indiceNuevo);
            while (iterador.Siguiente())
            {
                indiceTerminos.Agregar(iterador.Current);
            }

            indiceTerminos.OrdenarRadix();
            documentos = documentosNuevos;

            // Recalcular contador
            contadorDocumentos = 0;
            var iteradorDocs = new Iterador<Documento>(documentos);
            while (iteradorDocs.Siguiente())
            {
                if (iteradorDocs.Current.Id > contadorDocumentos)
                    contadorDocumentos = iteradorDocs.Current.Id;
            }
            
            zipfAplicado = true;
        }

        public void Limpiar()
        {
            indiceTerminos.Limpiar();
            documentos.Limpiar();
            contadorDocumentos = 0;
            zipfAplicado = false;
        }

        // Getters manteniendo TUS estructuras
        public int GetCantidadDocumentos() => documentos.Count;
        public ListaDobleEnlazada<Documento> GetDocumentos() => documentos;
        public VectorOrdenado<Termino> GetIndiceTerminos() => indiceTerminos;
        public bool ZipfAplicado => zipfAplicado;
    }
}