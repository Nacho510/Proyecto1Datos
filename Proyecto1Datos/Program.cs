
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
