using System;
using System.Windows.Forms;
using PruebaRider.UI;

namespace PruebaRider
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    Application.SetHighDpiMode(HighDpiMode.SystemAware);
                }

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