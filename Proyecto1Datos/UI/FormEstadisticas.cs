using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using PruebaRider.Servicios;
using PruebaRider.Estructura.Nodo;

namespace PruebaRider.UI
{
    /// <summary>
    /// Formulario para mostrar estadísticas detalladas del índice
    /// </summary>
    public partial class FormEstadisticas : Form
    {
        private readonly GestorIndice gestor;
        private RichTextBox txtEstadisticas;
        private Button btnCerrar;
        private Button btnExportar;

        public FormEstadisticas()
        {
            gestor = GestorIndice.ObtenerInstancia();
            InitializeComponent();
            CargarEstadisticas();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Configuración del formulario
            this.Text = "📊 Estadísticas Detalladas del Sistema";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(600, 400);
            this.MaximizeBox = false;
            this.Icon = SystemIcons.Information;

            // Panel superior con título
            var panelTitulo = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(0, 120, 215),
                Padding = new Padding(20, 10, 20, 10)
            };

            var lblTitulo = new Label
            {
                Text = "📊 Análisis Completo del Índice Invertido",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 15)
            };

            var lblSubtitulo = new Label
            {
                Text = "Métricas detalladas de rendimiento y estructura de datos",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.LightGray,
                AutoSize = true,
                Location = new Point(20, 35)
            };

            panelTitulo.Controls.AddRange(new Control[] { lblTitulo, lblSubtitulo });

            // Área de texto para estadísticas
            txtEstadisticas = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(248, 248, 248),
                Margin = new Padding(10)
            };

            // Panel inferior con botones
            var panelBotones = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(10)
            };

            btnExportar = new Button
            {
                Text = "📄 Exportar Reporte",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(140, 35),
                Location = new Point(10, 12),
                BackColor = Color.FromArgb(0, 150, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnCerrar = new Button
            {
                Text = "✖️ Cerrar",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(100, 35),
                Location = new Point(670, 12),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            panelBotones.Controls.AddRange(new Control[] { btnExportar, btnCerrar });

            // Agregar controles al formulario
            this.Controls.Add(txtEstadisticas);
            this.Controls.Add(panelBotones);
            this.Controls.Add(panelTitulo);

            // Eventos
            btnCerrar.Click += (s, e) => this.Close();
            btnExportar.Click += BtnExportar_Click;

            this.ResumeLayout(false);
        }

        private void CargarEstadisticas()
        {
            if (gestor.IndiceEstaVacio())
            {
                txtEstadisticas.Text = "❌ No hay índice cargado para mostrar estadísticas.";
                return;
            }

            var stats = gestor.ObtenerEstadisticas();
            var texto = GenerarReporteCompleto(stats);
            
            txtEstadisticas.Clear();
            txtEstadisticas.SelectionFont = new Font("Consolas", 9);
            txtEstadisticas.AppendText(texto);
            
            // Aplicar formato de colores
            AplicarFormatoColores();
        }

        private string GenerarReporteCompleto(EstadisticasIndice stats)
        {
            var reporte = new System.Text.StringBuilder();
            var fechaActual = DateTime.Now;

            reporte.AppendLine("╔══════════════════════════════════════════════════════════════════════╗");
            reporte.AppendLine("║                    REPORTE DE ESTADÍSTICAS DETALLADO                ║");
            reporte.AppendLine("║                     Motor de Búsqueda - Índice Invertido            ║");
            reporte.AppendLine("╚══════════════════════════════════════════════════════════════════════╝");
            reporte.AppendLine();
            reporte.AppendLine($"📅 Fecha de generación: {fechaActual:dd/MM/yyyy HH:mm:ss}");
            reporte.AppendLine($"🖥️  Sistema operativo: {Environment.OSVersion}");
            reporte.AppendLine($"⚡ Versión .NET: {Environment.Version}");
            reporte.AppendLine();

            // Estadísticas principales
            reporte.AppendLine("═══════════════════════════════════════════════════════════════════════");
            reporte.AppendLine("                           MÉTRICAS PRINCIPALES");
            reporte.AppendLine("═══════════════════════════════════════════════════════════════════════");
            reporte.AppendLine();
            reporte.AppendLine($"📄 Total de documentos indexados:      {stats.CantidadDocumentos:N0}");
            reporte.AppendLine($"🔤 Total de términos únicos:            {stats.CantidadTerminos:N0}");
            reporte.AppendLine($"🔥 Ley de Zipf aplicada:                {(stats.ZipfAplicado ? "✅ SÍ" : "❌ NO")}");
            reporte.AppendLine($"⚡ Algoritmo de ordenamiento:           ✅ RadixSort");
            reporte.AppendLine($"🎯 Método de similitud:                 ✅ Coseno Vectorial");
            reporte.AppendLine();

            // Calcular métricas adicionales
            if (stats.CantidadDocumentos > 0)
            {
                double terminosPorDocumento = (double)stats.CantidadTerminos / stats.CantidadDocumentos;
                reporte.AppendLine($"📊 Promedio términos por documento:     {terminosPorDocumento:F2}");
            }

            // Información del archivo
            reporte.AppendLine();
            reporte.AppendLine("═══════════════════════════════════════════════════════════════════════");
            reporte.AppendLine("                        INFORMACIÓN DE PERSISTENCIA");
            reporte.AppendLine("═══════════════════════════════════════════════════════════════════════");
            reporte.AppendLine();

            string archivoIndice = "indice_zipf.bin";
            if (File.Exists(archivoIndice))
            {
                var info = new FileInfo(archivoIndice);
                reporte.AppendLine($"💾 Archivo de índice:                   {archivoIndice}");
                reporte.AppendLine($"📁 Tamaño del archivo:                  {info.Length / 1024.0:F2} KB");
                reporte.AppendLine($"📅 Fecha de creación:                   {info.CreationTime:dd/MM/yyyy HH:mm:ss}");
                reporte.AppendLine($"🔄 Última modificación:                 {info.LastWriteTime:dd/MM/yyyy HH:mm:ss}");
                
                // Calcular compresión aproximada
                if (stats.CantidadTerminos > 0)
                {
                    double bytesEstimadosSinComprimir = stats.CantidadTerminos * 50 + stats.CantidadDocumentos * 200;
                    double ratioCompresion = info.Length / bytesEstimadosSinComprimir;
                    reporte.AppendLine($"🗜️  Ratio de compresión estimado:       {ratioCompresion:P1}");
                }
            }
            else
            {
                reporte.AppendLine("💾 Archivo de índice:                   ❌ No guardado");
            }

            // Estructuras de datos utilizadas
            reporte.AppendLine();
            reporte.AppendLine("═══════════════════════════════════════════════════════════════════════");
            reporte.AppendLine("                      ESTRUCTURAS DE DATOS IMPLEMENTADAS");
            reporte.AppendLine("═══════════════════════════════════════════════════════════════════════");
            reporte.AppendLine();
            reporte.AppendLine("✅ Lista Doblemente Enlazada Circular");
            reporte.AppendLine("   • Almacenamiento de documentos y resultados");
            reporte.AppendLine("   • Navegación bidireccional eficiente");
            reporte.AppendLine("   • Iterador personalizado implementado");
            reporte.AppendLine();
            reporte.AppendLine("✅ Vector con Operador * Sobrecargado");
            reporte.AppendLine("   • Cálculo de producto punto para similitud coseno");
            reporte.AppendLine("   • Validación de dimensiones automática");
            reporte.AppendLine("   • Manejo de valores nulos y overflow");
            reporte.AppendLine();
            reporte.AppendLine("✅ Vector Ordenado con RadixSort");
            reporte.AppendLine("   • Ordenamiento de términos alfabéticamente");
            reporte.AppendLine("   • Búsqueda binaria O(log n)");
            reporte.AppendLine("   • Optimizado para strings");
            reporte.AppendLine();
            reporte.AppendLine("✅ Índice Invertido");
            reporte.AppendLine("   • Mapeo término → documentos");
            reporte.AppendLine("   • Cálculo de TF-IDF automático");
            reporte.AppendLine("   • Aplicación de Ley de Zipf");

            // Patrones de diseño
            reporte.AppendLine();
            reporte.AppendLine("═══════════════════════════════════════════════════════════════════════");
            reporte.AppendLine("                        PATRONES DE DISEÑO APLICADOS");
            reporte.AppendLine("═══════════════════════════════════════════════════════════════════════");
            reporte.AppendLine();
            reporte.AppendLine("🏗️  Singleton Pattern:");
            reporte.AppendLine("   • GestorIndice como instancia única del sistema");
            reporte.AppendLine("   • Thread-safe con double-checked locking");
            reporte.AppendLine();
            reporte.AppendLine("🔄 Iterator Pattern:");
            reporte.AppendLine("   • Iteradores para Lista Doblemente Enlazada");
            reporte.AppendLine("   • Iteradores para Vector Ordenado");
            reporte.AppendLine("   • Navegación segura sin exposer estructura interna");
            reporte.AppendLine();
            reporte.AppendLine("⚡ Strategy Pattern:");
            reporte.AppendLine("   • EstrategiaZipfConservadora para reducción de términos");
            reporte.AppendLine("   • ContextoZipf para cambio dinámico de estrategias");
            reporte.AppendLine("   • Extensible para nuevas estrategias de filtrado");

            // Complejidad algorítmica
            reporte.AppendLine();
            reporte.AppendLine("═══════════════════════════════════════════════════════════════════════");
            reporte.AppendLine("                         ANÁLISIS DE COMPLEJIDAD O(n)");
            reporte.AppendLine("═══════════════════════════════════════════════════════════════════════");
            reporte.AppendLine();
            reporte.AppendLine("🔍 Operaciones de Búsqueda:");
            reporte.AppendLine($"   • Búsqueda de término:                  O(log n) - n = {stats.CantidadTerminos}");
            reporte.AppendLine($"   • Cálculo similitud coseno:             O(m) - m = términos en consulta");
            reporte.AppendLine($"   • Ordenamiento de resultados:           O(k log k) - k = documentos relevantes");
            reporte.AppendLine();
            reporte.AppendLine("📊 Operaciones de Construcción:");
            reporte.AppendLine("   • Tokenización de documento:            O(n) - n = caracteres");
            reporte.AppendLine("   • Inserción en índice:                  O(log t) - t = términos únicos");
            reporte.AppendLine("   • RadixSort de términos:                O(d × n) - d = longitud máxima palabra");
            reporte.AppendLine("   • Aplicación Ley de Zipf:               O(t) - t = términos totales");
            reporte.AppendLine();
            reporte.AppendLine("💾 Operaciones de Persistencia:");
            reporte.AppendLine("   • Serialización binaria:                O(t + d) - lineal");
            reporte.AppendLine("   • Deserialización:                      O(t + d) - lineal");
            reporte.AppendLine("   • Compresión básica:                     O(n) - n = bytes de datos");

            // Métricas de rendimiento estimadas
            reporte.AppendLine();
            reporte.AppendLine("═══════════════════════════════════════════════════════════════════════");
            reporte.AppendLine("                       ESTIMACIONES DE RENDIMIENTO");
            reporte.AppendLine("═══════════════════════════════════════════════════════════════════════");
            reporte.AppendLine();
            
            if (stats.CantidadTerminos > 0)
            {
                double tiempoBusquedaEstimado = Math.Log(stats.CantidadTerminos, 2) * 0.001;
                reporte.AppendLine($"⚡ Tiempo estimado búsqueda simple:      ~{tiempoBusquedaEstimado:F2} ms");
                
                double memoriaEstimada = (stats.CantidadTerminos * 64 + stats.CantidadDocumentos * 128) / 1024.0;
                reporte.AppendLine($"🧠 Uso estimado de memoria RAM:         ~{memoriaEstimada:F1} KB");
                
                int maxResultadosEficientes = Math.Min(1000, stats.CantidadDocumentos);
                reporte.AppendLine($"📊 Máx. resultados eficientes:          {maxResultadosEficientes}");
            }

            reporte.AppendLine();
            reporte.AppendLine("═══════════════════════════════════════════════════════════════════════");
            reporte.AppendLine("                            RECOMENDACIONES");
            reporte.AppendLine("═══════════════════════════════════════════════════════════════════════");
            reporte.AppendLine();
            
            if (stats.CantidadDocumentos < 100)
            {
                reporte.AppendLine("📈 RECOMENDACIÓN: Índice pequeño detectado");
                reporte.AppendLine("   • Considere agregar más documentos para mejor efectividad");
                reporte.AppendLine("   • El Zipf será más efectivo con mayor corpus");
            }
            else if (stats.CantidadDocumentos > 10000)
            {
                reporte.AppendLine("⚡ RECOMENDACIÓN: Corpus grande detectado");
                reporte.AppendLine("   • Considere aumentar el percentil Zipf para mejor rendimiento");
                reporte.AppendLine("   • Monitor memory usage durante búsquedas complejas");
            }
            
            if (!stats.ZipfAplicado)
            {
                reporte.AppendLine("🔥 ADVERTENCIA: Ley de Zipf no aplicada");
                reporte.AppendLine("   • Regenere el índice con Zipf para mejor rendimiento");
            }

            reporte.AppendLine();
            reporte.AppendLine("═══════════════════════════════════════════════════════════════════════");
            reporte.AppendLine($"                    Reporte generado: {fechaActual:HH:mm:ss}");
            reporte.AppendLine("═══════════════════════════════════════════════════════════════════════");

            return reporte.ToString();
        }

        private void AplicarFormatoColores()
        {
            // Aplicar colores a diferentes secciones del texto
            var texto = txtEstadisticas.Text;
            
            // Títulos principales (líneas con ═)
            int inicio = 0;
            while ((inicio = texto.IndexOf("═══", inicio)) != -1)
            {
                int fin = texto.IndexOf('\n', inicio);
                if (fin == -1) fin = texto.Length;
                
                txtEstadisticas.Select(inicio, fin - inicio);
                txtEstadisticas.SelectionColor = Color.FromArgb(0, 120, 215);
                txtEstadisticas.SelectionFont = new Font("Consolas", 9, FontStyle.Bold);
                
                inicio = fin;
            }

            // Etiquetas con emojis (✅, ❌, 📄, etc.)
            string[] patrones = { "✅", "❌", "📄", "🔤", "🔥", "⚡", "🎯", "💾", "📁", "📅", "🔄", "🗜️", "🏗️", "🔍", "📊", "🧠" };
            
            foreach (var patron in patrones)
            {
                inicio = 0;
                while ((inicio = texto.IndexOf(patron, inicio)) != -1)
                {
                    int finLinea = texto.IndexOf('\n', inicio);
                    if (finLinea == -1) finLinea = texto.Length;
                    
                    txtEstadisticas.Select(inicio, finLinea - inicio);
                    txtEstadisticas.SelectionColor = Color.FromArgb(0, 150, 0);
                    
                    inicio = finLinea;
                }
            }

            // Números importantes
            inicio = 0;
            while ((inicio = texto.IndexOf("O(", inicio)) != -1)
            {
                int fin = texto.IndexOf(')', inicio) + 1;
                if (fin > inicio)
                {
                    txtEstadisticas.Select(inicio, fin - inicio);
                    txtEstadisticas.SelectionColor = Color.FromArgb(255, 140, 0);
                    txtEstadisticas.SelectionFont = new Font("Consolas", 9, FontStyle.Bold);
                }
                inicio = fin;
            }

            // Resetear selección
            txtEstadisticas.Select(0, 0);
        }

        private void BtnExportar_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Archivo de texto (*.txt)|*.txt|Archivo RTF (*.rtf)|*.rtf";
                dialog.DefaultExt = "txt";
                dialog.FileName = $"Reporte_Estadisticas_{DateTime.Now:yyyyMMdd_HHmmss}";
                dialog.Title = "Exportar Reporte de Estadísticas";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (Path.GetExtension(dialog.FileName).ToLower() == ".rtf")
                        {
                            txtEstadisticas.SaveFile(dialog.FileName, RichTextBoxStreamType.RichText);
                        }
                        else
                        {
                            File.WriteAllText(dialog.FileName, txtEstadisticas.Text);
                        }

                        MessageBox.Show($"✅ Reporte exportado exitosamente:\n{dialog.FileName}", 
                            "Exportación Completada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"❌ Error al exportar: {ex.Message}", 
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}