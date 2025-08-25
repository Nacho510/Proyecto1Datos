using PruebaRider.Estructura.Nodo;
using PruebaRider.Modelo;

namespace PruebaRider.Strategy
{
    // Interfaz de estrategia para aplicar la Ley de Zipf
    public interface IReducirTerminosStrategy
    {
        void Aplicar(int percentil);
        
        string NombreEstrategia { get; }
        
        string Descripcion { get; }
    }

    public class EliminarTerminosFrecuentes : IReducirTerminosStrategy
    {
        private ListaDobleEnlazada<Termino> indice;

        public string NombreEstrategia => "Eliminar Términos Frecuentes";
        public string Descripcion => "Elimina términos que aparecen en muchos documentos (considerados muy comunes o stopwords)";

        public EliminarTerminosFrecuentes(ListaDobleEnlazada<Termino> indice)
        {
            this.indice = indice ?? throw new ArgumentNullException(nameof(indice));
        }

        public void Aplicar(int percentil)
        {
            if (percentil <= 0 || percentil >= 100) 
            {
                throw new ArgumentException("El percentil debe estar entre 1 y 99");
            }

            if (indice.Count == 0) return;

            // Crear lista de frecuencias de documentos para cada término
            var frecuenciasDocumentos = new ListaDobleEnlazada<FrecuenciaTermino>();
            
            var iterador = new Iterador<Termino>(indice);
            while (iterador.Siguiente())
            {
                var termino = iterador.Current;
                frecuenciasDocumentos.Agregar(new FrecuenciaTermino 
                { 
                    Termino = termino, 
                    FrecuenciaDocumental = termino.ListaDocumentos.Count 
                });
            }

            // Ordenar por frecuencia documental descendente
            frecuenciasDocumentos.OrdenarDescendente(ft => ft.FrecuenciaDocumental);

            // Calcular cuántos términos eliminar
            int cantidadAEliminar = (indice.Count * percentil) / 100;
            if (cantidadAEliminar == 0 && percentil > 0) cantidadAEliminar = 1;

            // Crear lista de términos a mantener (los menos frecuentes)
            var terminosAMantener = new ListaDobleEnlazada<Termino>();
            var iteradorFrec = new Iterador<FrecuenciaTermino>(frecuenciasDocumentos);
            int contador = 0;

            while (iteradorFrec.Siguiente())
            {
                if (contador >= cantidadAEliminar)
                {
                    terminosAMantener.Agregar(iteradorFrec.Current.Termino);
                }
                contador++;
            }

            // Actualizar el índice con los términos filtrados
            ActualizarIndice(terminosAMantener);
        }

        private void ActualizarIndice(ListaDobleEnlazada<Termino> nuevosTerminos)
        {
            indice.Limpiar();
            var iterador = new Iterador<Termino>(nuevosTerminos);
            while (iterador.Siguiente())
            {
                indice.Agregar(iterador.Current);
            }
        }
    }

    // Implementación para eliminar términos poco frecuentes (ruido)
    public class EliminarTerminosRaros : IReducirTerminosStrategy
    {
        private ListaDobleEnlazada<Termino> indice;

        public string NombreEstrategia => "Eliminar Términos Raros";
        public string Descripcion => "Elimina términos que aparecen en pocos documentos (considerados ruido o términos irrelevantes)";

        public EliminarTerminosRaros(ListaDobleEnlazada<Termino> indice)
        {
            this.indice = indice ?? throw new ArgumentNullException(nameof(indice));
        }

        public void Aplicar(int percentil)
        {
            if (percentil <= 0 || percentil >= 100)
            {
                throw new ArgumentException("El percentil debe estar entre 1 y 99");
            }

            if (indice.Count == 0) return;

            // Crear lista de frecuencias de documentos para cada término
            var frecuenciasDocumentos = new ListaDobleEnlazada<FrecuenciaTermino>();
            
            var iterador = new Iterador<Termino>(indice);
            while (iterador.Siguiente())
            {
                var termino = iterador.Current;
                frecuenciasDocumentos.Agregar(new FrecuenciaTermino 
                { 
                    Termino = termino, 
                    FrecuenciaDocumental = termino.ListaDocumentos.Count 
                });
            }

            // Ordenar por frecuencia documental ascendente (los menos frecuentes primero)
            OrdenarAscendentePorFrecuencia(frecuenciasDocumentos);

            // Calcular cuántos términos eliminar (los menos frecuentes)
            int cantidadAEliminar = (indice.Count * percentil) / 100;
            if (cantidadAEliminar == 0 && percentil > 0) cantidadAEliminar = 1;

            // Crear lista de términos a mantener (los más frecuentes)
            var terminosAMantener = new ListaDobleEnlazada<Termino>();
            var iteradorFrec = new Iterador<FrecuenciaTermino>(frecuenciasDocumentos);
            int contador = 0;

            while (iteradorFrec.Siguiente())
            {
                if (contador >= cantidadAEliminar)
                {
                    terminosAMantener.Agregar(iteradorFrec.Current.Termino);
                }
                contador++;
            }

            // Actualizar el índice con los términos filtrados
            ActualizarIndice(terminosAMantener);
        }

        private void OrdenarAscendentePorFrecuencia(ListaDobleEnlazada<FrecuenciaTermino> lista)
        {
            if (lista.Count < 2) return;

            bool huboIntercambio;
            do
            {
                huboIntercambio = false;
                var actual = lista.Root;

                for (int i = 0; i < lista.Count - 1; i++)
                {
                    var siguiente = actual.Sig;
                    if (actual.Data.FrecuenciaDocumental > siguiente.Data.FrecuenciaDocumental)
                    {
                        var temp = actual.Data;
                        actual.Data = siguiente.Data;
                        siguiente.Data = temp;
                        huboIntercambio = true;
                    }
                    actual = actual.Sig;
                }
            } while (huboIntercambio);
        }

        private void ActualizarIndice(ListaDobleEnlazada<Termino> nuevosTerminos)
        {
            indice.Limpiar();
            var iterador = new Iterador<Termino>(nuevosTerminos);
            while (iterador.Siguiente())
            {
                indice.Agregar(iterador.Current);
            }
        }
    }

    // Estrategia híbrida que combina ambos enfoques
    public class EliminarTerminosHibridoStrategy : IReducirTerminosStrategy
    {
        private ListaDobleEnlazada<Termino> indice;

        public string NombreEstrategia => "Eliminar Términos Híbrido";
        public string Descripcion => "Elimina tanto términos muy frecuentes como muy raros, manteniendo el rango medio";

        public EliminarTerminosHibridoStrategy(ListaDobleEnlazada<Termino> indice)
        {
            this.indice = indice ?? throw new ArgumentNullException(nameof(indice));
        }

        public void Aplicar(int percentil)
        {
            if (percentil <= 0 || percentil >= 100)
            {
                throw new ArgumentException("El percentil debe estar entre 1 y 99");
            }

            if (indice.Count == 0) return;

            // Dividir el percentil entre frecuentes y raros
            int percentilFrecuentes = percentil / 2;
            int percentilRaros = percentil - percentilFrecuentes;

            // Crear copia del índice para trabajar
            var indiceTemporal = CopiarIndice();

            // Aplicar eliminación de frecuentes
            if (percentilFrecuentes > 0)
            {
                var estrategiaFrecuentes = new EliminarTerminosFrecuentesStrategy(indiceTemporal);
                estrategiaFrecuentes.Aplicar(percentilFrecuentes);
            }

            // Aplicar eliminación de raros
            if (percentilRaros > 0)
            {
                var estrategiaRaros = new EliminarTerminosRarosStrategy(indiceTemporal);
                estrategiaRaros.Aplicar(percentilRaros);
            }

            // Actualizar índice original
            ActualizarIndiceOriginal(indiceTemporal);
        }

        private ListaDobleEnlazada<Termino> CopiarIndice()
        {
            var copia = new ListaDobleEnlazada<Termino>();
            var iterador = new Iterador<Termino>(indice);
            
            while (iterador.Siguiente())
            {
                copia.Agregar(iterador.Current);
            }
            
            return copia;
        }

        private void ActualizarIndiceOriginal(ListaDobleEnlazada<Termino> nuevoIndice)
        {
            indice.Limpiar();
            var iterador = new Iterador<Termino>(nuevoIndice);
            
            while (iterador.Siguiente())
            {
                indice.Agregar(iterador.Current);
            }
        }
    }

    // Clase auxiliar para almacenar frecuencia de términos
    internal class FrecuenciaTermino
    {
        public Termino Termino { get; set; }
        public int FrecuenciaDocumental { get; set; }
    }

    // Contexto que utiliza las estrategias (patrón Strategy)
   /* public class ContextoZipf
    {
        private IReducirTerminosStrategy estrategia;

        public void EstablecerEstrategia(IReducirTerminosStrategy estrategia)
        {
            this.estrategia = estrategia ?? throw new ArgumentNullException(nameof(estrategia));
        }

        public void AplicarLeyZipf(int percentil)
        {
            if (estrategia == null)
            {
                throw new InvalidOperationException("No se ha establecido una estrategia");
            }

            if (percentil <= 0 || percentil >= 100)
            {
                throw new ArgumentException("El percentil debe estar entre 1 y 99");
            }

            estrategia.Aplicar(percentil);
        }

        public string ObtenerInformacionEstrategia()
        {
            if (estrategia == null)
                return "No hay estrategia establecida";

            return $"{estrategia.NombreEstrategia}: {estrategia.Descripcion}";
        }
    }*/

    // Factory para crear estrategias
    public static class FabricaEstrategias
    {
        public enum TipoEstrategia
        {
            EliminarFrecuentes,
            EliminarRaros,
            Hibrido
        }

        public static IReducirTerminosStrategy CrearEstrategia(TipoEstrategia tipo, ListaDobleEnlazada<Termino> indice)
        {
            if (indice == null)
                throw new ArgumentNullException(nameof(indice));

            switch (tipo)
            {
                case TipoEstrategia.EliminarFrecuentes:
                    return new EliminarTerminosFrecuentes(indice);
                
                case TipoEstrategia.EliminarRaros:
                    return new EliminarTerminosRaros(indice);
                
                case TipoEstrategia.Hibrido:
                    return new EliminarTerminosHibridoStrategy(indice);
                
                default:
                    throw new ArgumentException("Tipo de estrategia no válido");
            }
        }

        public static List<string> ObtenerEstrategiasDisponibles()
        {
            return new List<string>
            {
                "Eliminar Términos Frecuentes - Remueve stopwords y términos muy comunes",
                "Eliminar Términos Raros - Remueve términos poco frecuentes o ruido",
                "Estrategia Híbrida - Combina ambos enfoques para rango medio óptimo"
            };
        }
    }
}