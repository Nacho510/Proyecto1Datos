using System;
using System.Windows;
using PruebaRider.UI;

namespace PruebaRider
{
    /// <summary>
    /// Punto de entrada principal para la aplicación con interfaz gráfica
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Punto de entrada principal de la aplicación
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            // Verificar si se quiere usar consola (parámetro --console)
            /*if (args.Length > 0 && args[0] == "--console")
            {
                // Usar la interfaz de consola original
                var interfazConsola = new Interfaz();
                interfazConsola.MenuPrincipalAsync().Wait();
                return;
            }*/

            // Usar la interfaz gráfica moderna
            try
            {
                var app = new Application();
                app.Run(new MainWindow());
            }
            catch (System.Exception ex)
            {
                // Si falla la interfaz gráfica, mostrar error y usar consola
                Console.WriteLine("❌ Error en interfaz gráfica, cambiando a consola...");
                Console.WriteLine($"Error: {ex.Message}");
                
                var interfazConsola = new MainWindow();
               // interfazConsola.MenuPrincipalAsync().Wait();
            }
        }
    }
}