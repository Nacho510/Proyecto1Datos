
using PruebaRider.Estructura.Nodo;
using PruebaRider.Estructura.Vector;
using PruebaRider.Modelo;
using PruebaRider.Servicios;
using PruebaRider.Strategy;
using PruebaRider.UI;

namespace PruebaRider;

class Program
{
    public static async Task Main(string[] args)
    {
        Console.Title = "Sistema de Búsqueda - Índice Invertido";
        
        try
        {
            // Crear y ejecutar la interfaz
            var interfaz = new Interfaz();
            await interfaz.MenuPrincipalAsync();
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine("❌ ERROR FATAL EN LA APLICACIÓN");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"Mensaje: {ex.Message}");
            Console.WriteLine($"\nDetalles técnicos:");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine(new string('=', 50));
            Console.WriteLine("\n⚠️  Por favor, verifique:");
            Console.WriteLine("   • Que el directorio de documentos existe");
            Console.WriteLine("   • Que hay archivos .txt en el directorio");
            Console.WriteLine("   • Que tiene permisos de lectura/escritura");
            Console.WriteLine("\n⏸️  Presione cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}
/*class Program
{
    public static void Main()
    {
        Console.WriteLine("Hello, World!");
        
        Documento documento = new Documento();
        documento.Id = 1;
        Console.WriteLine(documento.Id);
        Console.WriteLine(documento.ToString());
        
        documento.TextoOriginal = "Hola";
        Console.WriteLine(documento.TextoOriginal);
        
        //documento.Frecuencias["hola"] = 3;
        //documento.Frecuencias["mundo"] = 5;

        //foreach (var par in documento.Frecuencias)
        //  Console.WriteLine($"{par.Key}: {par.Value}");
        
        
        //Prueba de similitud de coseno en clase vector (usando numeros de momento)
        var docVector = new Vector(new double[] {0.1, 0.2, 0.0, 0.4});
        var queryVector = new Vector(new double[] {0.1, 0.1, 0.0, 0.5});
        double similitud = docVector.SimilitudCoseno(queryVector);
        Console.WriteLine($"Similitud coseno: {similitud}");

        //prueba de procesador de texto
        var procesador = new ProcesadorDeTexto();
        string texto = "La inteligencia artificial es un campo de estudio de la computación.";
        var tokens = procesador.ProcesarTextoCompleto(texto);

        Console.WriteLine("Palabras procesadas:");
        foreach (var token in tokens)
        {
            Console.WriteLine(token);
        }
        
        Interfaz interfaz = new Interfaz();
        string frase = interfaz.menu();
        Console.WriteLine(frase);
        

    }
}*/