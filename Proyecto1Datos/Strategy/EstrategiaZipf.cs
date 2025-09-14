using PruebaRider.Estructura.Nodo;
using PruebaRider.Modelo;

namespace PruebaRider.Strategy
{
    public interface IReducirTerminosStrategy
    {
        void Aplicar(int percentil);
        string NombreEstrategia { get; }
        string Descripcion { get; }
    }


    public class EstrategiaZipfConservadora : IReducirTerminosStrategy
    {
        private ListaDobleEnlazada<Termino> indice;
        private int totalDocumentos;

        public string NombreEstrategia => "Ley de Zipf Conservadora";
        public string Descripcion => "Elimina términos que aparecen en más del 85% de documentos";

        public EstrategiaZipfConservadora(ListaDobleEnlazada<Termino> indice, int totalDocumentos)
        {
            this.indice = indice ?? throw new ArgumentNullException(nameof(indice));
            this.totalDocumentos = totalDocumentos;
        }

        public void Aplicar(int percentil)
        {
            if (percentil <= 0 || percentil >= 100) return;
            if (indice.Count == 0 || totalDocumentos == 0) return;
            
            double umbralFrecuenciaRelativa = 0.85; 
            int umbralAbsoluto = (int)(totalDocumentos * umbralFrecuenciaRelativa);

            Console.WriteLine($"🔍 Aplicando Ley de Zipf: eliminando términos en >{umbralAbsoluto} de {totalDocumentos} documentos");

            var terminosAMantener = new ListaDobleEnlazada<Termino>();
            int eliminados = 0;

            var iterador = new Iterador<Termino>(indice);
            while (iterador.Siguiente())
            {
                var termino = iterador.Current;
                int frecuenciaDocumental = termino.Documentos.Count;

                if (frecuenciaDocumental <= umbralAbsoluto)
                {
                    terminosAMantener.Agregar(termino);
                }
                else
                {
                    Console.WriteLine($"   🗑️ Eliminando: '{termino.Palabra}' ({frecuenciaDocumental} docs)");
                    eliminados++;
                }
            }

            int maxEliminar = (indice.Count * Math.Min(percentil, 20)) / 100;
            if (eliminados > maxEliminar)
            {
                Console.WriteLine($"⚠️ Límite de seguridad activado: máximo {maxEliminar} términos eliminados");
                terminosAMantener = AplicarLimiteDeSeguridad(maxEliminar);
                eliminados = maxEliminar;
            }

            ActualizarIndice(terminosAMantener);
            
            Console.WriteLine($"✅ Ley de Zipf aplicada:");
            Console.WriteLine($"   📊 Términos eliminados: {eliminados}");
            Console.WriteLine($"   📊 Términos conservados: {terminosAMantener.Count}");
            Console.WriteLine($"   📊 Porcentaje conservado: {(double)terminosAMantener.Count / (eliminados + terminosAMantener.Count) * 100:F1}%");
        }
        
        private ListaDobleEnlazada<Termino> AplicarLimiteDeSeguridad(int maxEliminar)
        {
            var terminosOrdenados = new ListaDobleEnlazada<Termino>();
            var iterador = new Iterador<Termino>(indice);
            while (iterador.Siguiente())
            {
                terminosOrdenados.Agregar(iterador.Current);
            }

            terminosOrdenados.OrdenarDescendente(t => t.Documentos.Count);

            var terminosAMantener = new ListaDobleEnlazada<Termino>();
            var iteradorOrdenado = new Iterador<Termino>(terminosOrdenados);
            int contador = 0;
            
            while (iteradorOrdenado.Siguiente())
            {
                if (contador >= maxEliminar)
                {
                    terminosAMantener.Agregar(iteradorOrdenado.Current);
                }
                else
                {
                    Console.WriteLine($"   🗑️ Eliminando (límite): '{iteradorOrdenado.Current.Palabra}' ({iteradorOrdenado.Current.Documentos.Count} docs)");
                }
                contador++;
            }

            return terminosAMantener;
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
    
    public class ContextoZipf
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
            Console.WriteLine($"🎯 Aplicando {estrategia.NombreEstrategia} ({percentil}%)");
            Console.WriteLine($"📝 {estrategia.Descripcion}");
            
            estrategia.Aplicar(percentil);
        }
    }
}