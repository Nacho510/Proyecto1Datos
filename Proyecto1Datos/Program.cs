using System;
using System.Windows.Forms;
using PruebaRider.UI;

namespace PruebaRider
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                // Configurar la aplicación para usar estilos visuales
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                // Configurar DPI awareness para mejor escalado
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    Application.SetHighDpiMode(HighDpiMode.SystemAware);
                }

                // Crear y ejecutar el formulario principal
                using (var form = new FormPrincipal())
                {
                    Application.Run(form);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ ERROR FATAL\n\n" +
                    $"Mensaje: {ex.Message}\n\n" +
                    $"Detalles técnicos:\n{ex}",
                    "Error de Aplicación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}