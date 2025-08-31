using PruebaRider.UI;

namespace PruebaRider;

class Program
{
    public static async Task Main(string[] args)
    {
        Console.Title = "Sistema de Búsqueda - Índice Invertido con Vector Personalizado";
        
        // Configurar encoding para caracteres especiales
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        try
        {
            Console.WriteLine("🚀 Iniciando sistema con Vector personalizado...");
            Console.WriteLine("🎯 Similitud coseno precisa habilitada");
            Console.WriteLine("🔗 Enlaces base64 directos incluidos");
            Console.WriteLine();
            
            // Crear y ejecutar la interfaz corregida
            var interfaz = new Interfaz();
            await interfaz.MenuPrincipalAsync();
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine("❌ ERROR FATAL EN LA APLICACIÓN");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine($"Mensaje: {ex.Message}");
            Console.WriteLine($"\nDetalles técnicos:");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine(new string('=', 60));
            Console.WriteLine("\n⚠️  DIAGNÓSTICO AUTOMÁTICO:");
            Console.WriteLine("   • Verifique que el directorio de documentos existe");
            Console.WriteLine("   • Confirme que hay archivos .txt en el directorio");
            Console.WriteLine("   • Verifique permisos de lectura/escritura");
            Console.WriteLine("   • Asegúrese de tener .NET 9.0 instalado");
            Console.WriteLine("   • Revise que todas las clases Vector estén compiladas");
            Console.WriteLine("\n🎯 CARACTERÍSTICAS DEL SISTEMA:");
            Console.WriteLine("   • Vector personalizado (sin genéricos del lenguaje)");
            Console.WriteLine("   • Similitud coseno precisa y realista");
            Console.WriteLine("   • Enlaces base64 directos en resultados");
            Console.WriteLine("   • Estructuras de datos propias (ListaDobleEnlazada)");
            Console.WriteLine("\n⏸️  Presione cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}