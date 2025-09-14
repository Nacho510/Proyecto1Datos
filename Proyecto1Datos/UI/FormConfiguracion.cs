using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PruebaRider.UI
{
    public partial class FormConfiguracion : Form
    {
        public string DirectorioDocumentos { get; private set; }
        public int PercentilZipf { get; private set; }
        public bool ConfiguracionModificada { get; private set; }

        private TextBox txtDirectorio;
        private Button btnExaminar;
        private NumericUpDown numZipf;
        private CheckBox chkValidarArchivos;
        private CheckBox chkMostrarProgreso;
        private ComboBox cmbTipoOrdenamiento;
        private Button btnAceptar;
        private Button btnCancelar;
        private Button btnRestaurar;
        private Label lblInfoDirectorio;

        public FormConfiguracion(string directorioActual, int zipfActual)
        {
            DirectorioDocumentos = directorioActual;
            PercentilZipf = zipfActual;
            ConfiguracionModificada = false;
            
            InitializeComponent();
            CargarConfiguracionActual();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Configuración del formulario
            this.Text = "⚙️ Configuración Avanzada";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Icon = SystemIcons.WinLogo;

            // Panel principal
            var panelPrincipal = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Título
            var lblTitulo = new Label
            {
                Text = "⚙️ Configuración del Motor de Búsqueda",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 215),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Sección: Directorio de documentos
            var grpDirectorio = new GroupBox
            {
                Text = "📁 Directorio de Documentos",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 60),
                Size = new Size(540, 100)
            };

            txtDirectorio = new TextBox
            {
                Location = new Point(15, 25),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 9),
                ReadOnly = true
            };

            btnExaminar = new Button
            {
                Text = "📂 Examinar",
                Location = new Point(425, 25),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            lblInfoDirectorio = new Label
            {
                Location = new Point(15, 55),
                Size = new Size(510, 35),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                Text = "Seleccione el directorio que contiene los archivos .txt a indexar"
            };

            grpDirectorio.Controls.AddRange(new Control[] { txtDirectorio, btnExaminar, lblInfoDirectorio });

            // Sección: Configuración de Zipf
            var grpZipf = new GroupBox
            {
                Text = "🔥 Configuración de Ley de Zipf",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 170),
                Size = new Size(540, 120)
            };

            var lblZipf = new Label
            {
                Text = "Percentil de términos a eliminar:",
                Location = new Point(15, 25),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9)
            };

            numZipf = new NumericUpDown
            {
                Location = new Point(220, 23),
                Size = new Size(60, 25),
                Minimum = 1,
                Maximum = 50,
                Value = 15,
                Font = new Font("Segoe UI", 9)
            };

            var lblPorcentaje = new Label
            {
                Text = "%",
                Location = new Point(285, 25),
                Size = new Size(20, 20),
                Font = new Font("Segoe UI", 9)
            };

            var lblExplicacionZipf = new Label
            {
                Location = new Point(15, 50),
                Size = new Size(510, 60),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                Text = "La Ley de Zipf elimina términos muy frecuentes para mejorar la precisión.\n" +
                       "• Valores bajos (1-10%): Conserva más términos, índice más grande\n" +
                       "• Valores altos (20-50%): Elimina más términos, búsquedas más precisas\n" +
                       "• Recomendado: 15% para corpus generales"
            };

            grpZipf.Controls.AddRange(new Control[] { lblZipf, numZipf, lblPorcentaje, lblExplicacionZipf });

            // Sección: Opciones avanzadas
            var grpOpciones = new GroupBox
            {
                Text = "🔧 Opciones Avanzadas",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 300),
                Size = new Size(540, 100)
            };

            chkValidarArchivos = new CheckBox
            {
                Text = "✅ Validar archivos antes de indexar",
                Location = new Point(15, 25),
                Size = new Size(250, 20),
                Font = new Font("Segoe UI", 9),
                Checked = true
            };

            chkMostrarProgreso = new CheckBox
            {
                Text = "📊 Mostrar progreso detallado",
                Location = new Point(15, 50),
                Size = new Size(250, 20),
                Font = new Font("Segoe UI", 9),
                Checked = true
            };

            var lblOrdenamiento = new Label
            {
                Text = "Algoritmo de ordenamiento:",
                Location = new Point(280, 25),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 9)
            };

            cmbTipoOrdenamiento = new ComboBox
            {
                Location = new Point(280, 50),
                Size = new Size(120, 25),
                Font = new Font("Segoe UI", 9),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbTipoOrdenamiento.Items.AddRange(new[] { "RadixSort", "QuickSort", "MergeSort" });
            cmbTipoOrdenamiento.SelectedIndex = 0;

            grpOpciones.Controls.AddRange(new Control[] { 
                chkValidarArchivos, chkMostrarProgreso, lblOrdenamiento, cmbTipoOrdenamiento 
            });

            // Panel de botones
            var panelBotones = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(10)
            };

            btnRestaurar = new Button
            {
                Text = "🔄 Restaurar",
                Font = new Font("Segoe UI", 9),
                Size = new Size(100, 35),
                Location = new Point(20, 12),
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };

            btnCancelar = new Button
            {
                Text = "❌ Cancelar",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(100, 35),
                Location = new Point(350, 12),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };

            btnAceptar = new Button
            {
                Text = "✅ Aceptar",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(100, 35),
                Location = new Point(460, 12),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };

            panelBotones.Controls.AddRange(new Control[] { btnRestaurar, btnCancelar, btnAceptar });

            // Agregar controles al panel principal
            panelPrincipal.Controls.AddRange(new Control[] { 
                lblTitulo, grpDirectorio, grpZipf, grpOpciones 
            });

            // Agregar al formulario
            this.Controls.Add(panelPrincipal);
            this.Controls.Add(panelBotones);

            // Eventos
            btnExaminar.Click += BtnExaminar_Click;
            btnRestaurar.Click += BtnRestaurar_Click;
            btnAceptar.Click += BtnAceptar_Click;
            numZipf.ValueChanged += (s, e) => ConfiguracionModificada = true;
            chkValidarArchivos.CheckedChanged += (s, e) => ConfiguracionModificada = true;
            chkMostrarProgreso.CheckedChanged += (s, e) => ConfiguracionModificada = true;
            cmbTipoOrdenamiento.SelectedIndexChanged += (s, e) => ConfiguracionModificada = true;

            this.ResumeLayout(false);
        }

        private void CargarConfiguracionActual()
        {
            txtDirectorio.Text = DirectorioDocumentos;
            numZipf.Value = PercentilZipf;
            ActualizarInfoDirectorio();
        }

        private void BtnExaminar_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Seleccione el directorio que contiene los documentos a indexar";
                dialog.SelectedPath = DirectorioDocumentos;
                dialog.ShowNewFolderButton = false;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtDirectorio.Text = dialog.SelectedPath;
                    DirectorioDocumentos = dialog.SelectedPath;
                    ConfiguracionModificada = true;
                    ActualizarInfoDirectorio();
                }
            }
        }

        private void ActualizarInfoDirectorio()
        {
            if (string.IsNullOrEmpty(txtDirectorio.Text) || !Directory.Exists(txtDirectorio.Text))
            {
                lblInfoDirectorio.Text = "❌ Directorio no válido o no existe";
                lblInfoDirectorio.ForeColor = Color.Red;
                btnAceptar.Enabled = false;
                return;
            }

            try
            {
                var archivos = Directory.GetFiles(txtDirectorio.Text, "*.txt");
                var tamaño = archivos.Sum(archivo => new FileInfo(archivo).Length);
                var tamañoMB = tamaño / (1024.0 * 1024.0);

                lblInfoDirectorio.Text = $"✅ {archivos.Length} archivo(s) .txt encontrados (~{tamañoMB:F1} MB)";
                lblInfoDirectorio.ForeColor = Color.Green;
                btnAceptar.Enabled = archivos.Length > 0;

                if (archivos.Length == 0)
                {
                    lblInfoDirectorio.Text = "⚠️ No se encontraron archivos .txt en este directorio";
                    lblInfoDirectorio.ForeColor = Color.Orange;
                }
            }
            catch (Exception ex)
            {
                lblInfoDirectorio.Text = $"❌ Error accediendo al directorio: {ex.Message}";
                lblInfoDirectorio.ForeColor = Color.Red;
                btnAceptar.Enabled = false;
            }
        }

        private void BtnRestaurar_Click(object sender, EventArgs e)
        {
            var resultado = MessageBox.Show(
                "¿Desea restaurar la configuración a los valores predeterminados?\n\n" +
                "• Directorio: (sin cambios)\n" +
                "• Percentil Zipf: 15%\n" +
                "• Validar archivos: Activado\n" +
                "• Mostrar progreso: Activado\n" +
                "• Ordenamiento: RadixSort",
                "Restaurar Configuración",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (resultado == DialogResult.Yes)
            {
                numZipf.Value = 15;
                chkValidarArchivos.Checked = true;
                chkMostrarProgreso.Checked = true;
                cmbTipoOrdenamiento.SelectedIndex = 0;
                ConfiguracionModificada = true;
            }
        }

        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            // Validaciones finales
            if (string.IsNullOrEmpty(txtDirectorio.Text) || !Directory.Exists(txtDirectorio.Text))
            {
                MessageBox.Show("❌ Debe seleccionar un directorio válido", "Configuración Incompleta",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var archivos = Directory.GetFiles(txtDirectorio.Text, "*.txt");
            if (archivos.Length == 0)
            {
                MessageBox.Show("❌ El directorio seleccionado no contiene archivos .txt", "Sin Archivos",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Guardar configuración
            DirectorioDocumentos = txtDirectorio.Text;
            PercentilZipf = (int)numZipf.Value;

            // Mostrar resumen de configuración
            string resumen = $"Configuración aplicada:\n\n" +
                           $"📁 Directorio: {Path.GetFileName(DirectorioDocumentos)}\n" +
                           $"📄 Archivos: {archivos.Length} archivo(s) .txt\n" +
                           $"🔥 Percentil Zipf: {PercentilZipf}%\n" +
                           $"✅ Validar archivos: {(chkValidarArchivos.Checked ? "Sí" : "No")}\n" +
                           $"📊 Mostrar progreso: {(chkMostrarProgreso.Checked ? "Sí" : "No")}\n" +
                           $"⚡ Ordenamiento: {cmbTipoOrdenamiento.SelectedItem}";

            MessageBox.Show(resumen, "Configuración Guardada", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Propiedades adicionales para opciones avanzadas
        /// </summary>
        public bool ValidarArchivos => chkValidarArchivos?.Checked ?? true;
        public bool MostrarProgreso => chkMostrarProgreso?.Checked ?? true;
        public string TipoOrdenamiento => cmbTipoOrdenamiento?.SelectedItem?.ToString() ?? "RadixSort";
    }
}