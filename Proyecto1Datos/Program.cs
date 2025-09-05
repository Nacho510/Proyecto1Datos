using PruebaRider.UI;

namespace PruebaRider
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var interfaz = new InterfazSimple();
                await interfaz.IniciarAsync();
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine("❌ ERROR FATAL");
                Console.WriteLine("================");
                Console.WriteLine($"Mensaje: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("Detalles técnicos:");
                Console.WriteLine(ex.ToString());
                Console.WriteLine();
                Console.WriteLine("Presione cualquier tecla para salir...");
                Console.ReadKey();
            }
        }
    }
}