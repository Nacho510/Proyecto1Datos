using PruebaRider.Estructura.Nodo;
using PruebaRider.Servicios;
namespace PruebaRider.UI;

public class Interfaz
{
    public string menu()
    {
        IndiceInvertido indiceInvertido = new IndiceInvertido();
        indiceInvertido.CargarDirectorio("C:\\Users\\ignab\\OneDrive\\Documents\\Estructuras de datos\\Documentos");
        System.Console.WriteLine("\tMenu Principal\n");
        System.Console.WriteLine("Porfavor Ingrese la frase que desea Buscar");
        System.Console.Write("Buscar:");
        string frase = Console.ReadLine();
         indiceInvertido.Buscar(frase);
         return frase;
    }
    
    public void mostrarResultados(ListaDobleEnlazada<string> resultado)
    {
        System.Console.WriteLine("\tResultados\n");
        System.Console.WriteLine(resultado);
    }
}