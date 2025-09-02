using PruebaRider.Estructura.Nodo;
using PruebaRider.Modelo;

namespace PruebaRider.Strategy
{
    // Estrategia para eliminar términos por frecuencia alta (muy comunes)
    public class EliminarTerminosFrecuentesStrategy 
    {
        private ListaDobleEnlazada<Termino> indice;

        public EliminarTerminosFrecuentesStrategy(ListaDobleEnlazada<Termino> indice)
        {
            this.indice = indice;
        }

        public void Aplicar(int percentil)
        {
            if (percentil <= 0 || percentil >= 100) return;

            // Calcular el umbral de frecuencia
            int umbralFrecuencia = CalcularUmbralFrecuencia(percentil);
            
            // Crear una nueva lista sin los términos que superen el umbral
            var nuevaLista = new ListaDobleEnlazada<Termino>();
            
            var iterador = new Iterador<Termino>(indice);
            while (iterador.Siguiente())
            {
                var termino = iterador.Current;
                if (termino.Documentos.Count < umbralFrecuencia)
                {
                    nuevaLista.Agregar(termino);
                }
            }
            
            // Limpiar el índice original y copiar los términos filtrados
            indice.Limpiar();
            var iteradorNuevo = new Iterador<Termino>(nuevaLista);
            while (iteradorNuevo.Siguiente())
            {
                indice.Agregar(iteradorNuevo.Current);
            }
        }

        private int CalcularUmbralFrecuencia(int percentil)
        {
            // Crear un array con las frecuencias
            var frecuencias = new ListaDobleEnlazada<int>();
            
            var iterador = new Iterador<Termino>(indice);
            while (iterador.Siguiente())
            {
                frecuencias.Agregar(iterador.Current.Documentos.Count);
            }
            
            if (frecuencias.Count == 0) return 0;
            
            // Ordenar frecuencias de forma descendente
            frecuencias.OrdenarDescendente(f => f);
            
            // Calcular el índice del percentil
            int indicePercentil = (frecuencias.Count * percentil) / 100;
            if (indicePercentil >= frecuencias.Count) indicePercentil = frecuencias.Count - 1;
            
            // Obtener el valor en la posición del percentil
            var iteradorFrec = new Iterador<int>(frecuencias);
            int posicion = 0;
            while (iteradorFrec.Siguiente())
            {
                if (posicion == indicePercentil)
                {
                    return iteradorFrec.Current;
                }
                posicion++;
            }
            
            return 1;
        }
    }

    // Estrategia para eliminar términos poco frecuentes (ruido)
    public class EliminarTerminosRarosStrategy
    {
        private ListaDobleEnlazada<Termino> indice;

        public EliminarTerminosRarosStrategy(ListaDobleEnlazada<Termino> indice)
        {
            this.indice = indice;
        }

        public void Aplicar(int percentil)
        {
            if (percentil <= 0 || percentil >= 100) return;

            // Crear un array con las frecuencias para ordenarlas
            var frecuencias = new ListaDobleEnlazada<int>();
            
            var iterador = new Iterador<Termino>(indice);
            while (iterador.Siguiente())
            {
                frecuencias.Agregar(iterador.Current.Documentos.Count);
            }
            
            if (frecuencias.Count == 0) return;
            
            // Ordenar frecuencias ascendente (los menos frecuentes primero)
            var listaOrdenada = new ListaDobleEnlazada<int>();
            var iteradorFrec = new Iterador<int>(frecuencias);
            while (iteradorFrec.Siguiente())
            {
                listaOrdenada.Agregar(iteradorFrec.Current);
            }
            
            // Ordenar de forma ascendente (invertir la lógica del orden descendente)
            OrdenarAscendente(listaOrdenada);
            
            // Calcular umbral (los términos con frecuencia menor al percentil)
            int indicePercentil = (listaOrdenada.Count * percentil) / 100;
            if (indicePercentil >= listaOrdenada.Count) indicePercentil = listaOrdenada.Count - 1;
            
            int umbral = ObtenerElementoEnPosicion(listaOrdenada, indicePercentil);
            
            // Eliminar términos con frecuencia menor al umbral
            var nuevaLista = new ListaDobleEnlazada<Termino>();
            
            var iteradorTerminos = new Iterador<Termino>(indice);
            while (iteradorTerminos.Siguiente())
            {
                var termino = iteradorTerminos.Current;
                if (termino.Documentos.Count > umbral)
                {
                    nuevaLista.Agregar(termino);
                }
            }
            
            // Actualizar el índice
            indice.Limpiar();
            var iteradorNuevo = new Iterador<Termino>(nuevaLista);
            while (iteradorNuevo.Siguiente())
            {
                indice.Agregar(iteradorNuevo.Current);
            }
        }

        private void OrdenarAscendente(ListaDobleEnlazada<int> lista)
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
                    if (actual.Data > siguiente.Data) // Cambio aquí para orden ascendente
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

        private int ObtenerElementoEnPosicion(ListaDobleEnlazada<int> lista, int posicion)
        {
            var iterador = new Iterador<int>(lista);
            int contador = 0;
            while (iterador.Siguiente())
            {
                if (contador == posicion)
                    return iterador.Current;
                contador++;
            }
            return 1;
        }
    }
}