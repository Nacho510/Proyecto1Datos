using PruebaRider.Estructura.Nodo;
using PruebaRider.Strategy;

namespace PruebaRider.Servicios
{
    /// Gestor manteniendo Singleton
    public sealed class GestorIndice
    {
        private static GestorIndice instancia = null;
        private static readonly object lockObject = new object();

        private IndiceInvertido indice;
        private BuscadorVectorial buscador;

        private GestorIndice()
        {
            indice = new IndiceInvertido();
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

        public async Task<bool> CrearIndiceDesdeDirectorio(string rutaDirectorio, int percentilZipf = 15)
        {
            try
            {
                Console.WriteLine("🎯 Creando índice con Ley de Zipf obligatoria...");
                
                await indice.CrearDesdeRuta(rutaDirectorio, percentilZipf);
                buscador = new BuscadorVectorial(indice);
                
                Console.WriteLine("✅ Índice creado exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                return false;
            }
        }

        public ListaDobleEnlazada<ResultadoBusquedaVectorial> BuscarConSimilitudCoseno(string consulta)
        {
            if (buscador == null)
                buscador = new BuscadorVectorial(indice);
                
            return buscador.Buscar(consulta);
        }

        public bool GuardarIndice(string rutaArchivo)
        {
            try
            {
                indice.GuardarEnArchivoBinario(rutaArchivo);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CargarIndice(string rutaArchivo)
        {
            try
            {
                indice.CargarDesdeArchivoBinario(rutaArchivo);
                buscador = new BuscadorVectorial(indice);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public EstadisticasIndice ObtenerEstadisticas()
        {
            return new EstadisticasIndice
            {
                CantidadDocumentos = indice.GetCantidadDocumentos(),
                CantidadTerminos = indice.GetIndiceTerminos().Count,
                ZipfAplicado = indice.ZipfAplicado
            };
        }

        public bool IndiceEstaVacio() => indice.GetCantidadDocumentos() == 0;

        public void LimpiarSistema()
        {
            indice.Limpiar();
            buscador = null;
        }
    }

    public class EstadisticasIndice
    {
        public int CantidadDocumentos { get; set; }
        public int CantidadTerminos { get; set; }
        public bool ZipfAplicado { get; set; }
    }
}