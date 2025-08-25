using PruebaRider.Estructura.Nodo;
using PruebaRider.Strategy;

namespace PruebaRider.Servicios
{
    // Patrón Singleton para gestionar el índice invertido
    public sealed class GestorIndice
    {
        private static GestorIndice instancia = null;
        private static readonly object lockObject = new object();
        
        private IndiceInvertido indice;
        private string rutaIndiceActual;

        private GestorIndice()
        {
            indice = new IndiceInvertido();
            rutaIndiceActual = "";
        }

        public static GestorIndice ObtenerInstancia()
        {
            if (instancia == null)
            {
                lock (lockObject)
                {
                    if (instancia == null)
                        instancia = new GestorIndice();
                }
            }
            return instancia;
        }

        public async Task<bool> CrearIndiceDesdeDirectorio(string rutaDirectorio)
        {
            try
            {
                await indice.CrearDesdeRuta(rutaDirectorio);
                rutaIndiceActual = rutaDirectorio;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creando índice: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ActualizarIndice(string rutaDirectorio = null)
        {
            try
            {
                string ruta = rutaDirectorio ?? rutaIndiceActual;
                if (string.IsNullOrEmpty(ruta))
                {
                    Console.WriteLine("No se ha especificado una ruta para actualizar");
                    return false;
                }

                await indice.ActualizarIndice(ruta);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error actualizando índice: {ex.Message}");
                return false;
            }
        }

        public bool GuardarIndice(string rutaArchivo)
        {
            try
            {
                indice.GuardarEnArchivoBinario(rutaArchivo);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guardando índice: {ex.Message}");
                return false;
            }
        }

        public bool CargarIndice(string rutaArchivo)
        {
            try
            {
                indice.CargarDesdeArchivoBinario(rutaArchivo);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando índice: {ex.Message}");
                return false;
            }
        }

        public void AplicarLeyZipf(int percentil, bool eliminarFrecuentes = true)
        {
            try
            {
                indice.AplicarLeyZipf(percentil, eliminarFrecuentes);
                Console.WriteLine($"Ley de Zipf aplicada. Percentil: {percentil}%, Eliminar frecuentes: {eliminarFrecuentes}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error aplicando Ley de Zipf: {ex.Message}");
            }
        }

        public ListaDobleEnlazada<ResultadoBusquedaVectorial> BuscarConSimilitudCoseno(string consulta)
        {
            if (string.IsNullOrWhiteSpace(consulta))
                return new ListaDobleEnlazada<ResultadoBusquedaVectorial>();

            try
            {
                return indice.BuscarConSimilitudCoseno(consulta);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en búsqueda vectorial: {ex.Message}");
                return new ListaDobleEnlazada<ResultadoBusquedaVectorial>();
            }
        }

        public ListaDobleEnlazada<ResultadoBusqueda> BuscarTfIdf(string consulta)
        {
            if (string.IsNullOrWhiteSpace(consulta))
                return new ListaDobleEnlazada<ResultadoBusqueda>();

            try
            {
                return indice.Buscar(consulta);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en búsqueda TF-IDF: {ex.Message}");
                return new ListaDobleEnlazada<ResultadoBusqueda>();
            }
        }

        public EstadisticasIndice ObtenerEstadisticas()
        {
            return indice.ObtenerEstadisticas();
        }

        public void LimpiarIndice()
        {
            indice.Limpiar();
            rutaIndiceActual = "";
        }

        public bool IndiceEstaVacio()
        {
            return indice.GetCantidadDocumentos() == 0;
        }

        public string GetRutaActual()
        {
            return rutaIndiceActual;
        }
    }
}