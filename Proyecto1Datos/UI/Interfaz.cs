using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using PruebaRider.Servicios;
using PruebaRider.Estructura.Nodo;
using System.Diagnostics;

namespace PruebaRider.UI
{
    /// <summary>
    /// Interfaz gráfica principal del Motor de Búsqueda
    /// </summary>
    public partial class FormPrincipal : Form
    {
        private readonly GestorIndice gestor;
        private readonly string directorioDocumentos;
        private readonly string archivoIndice;

        // Controles UI
        private TextBox txtBusqueda;
        private Button btnBuscar;
        private Button btnCrearIndice;
        private Button btnGuardarIndice;
        private Button btnCargarIndice;
        private Button btnEstadisticas;
        private ListBox lstResultados;
        private RichTextBox txtVistaPrevia;
        private Label lblEstado;
        private Label lblTiempo;
        private ProgressBar progressBar;
        private NumericUpDown numZipf;

        public FormPrincipal()
        {
            gestor = GestorIndice.ObtenerInstancia();
            directorioDocumentos = @"C:\Users\ignab\OneDrive\Documents\Estructuras de datos\Documentos";
            archivoIndice = "indice_zipf.bin";
            
            InitializeComponent();
            ConfigurarInterfaz();
            CargarIndiceExistente();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Configuración del formulario
            this.Text = "Motor de Búsqueda - Índice Invertido + RadixSort + Zipf";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1000, 600);
            this.Icon = SystemIcons.Application;

            // Panel superior - Búsqueda
            var panelBusqueda = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(10)
            };

            var lblBusqueda = new Label
            {
                Text = "🔍 Buscar documentos:",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };

            txtBusqueda = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Location = new Point(10, 35),
                Size = new Size(600, 25),
                PlaceholderText = "Ingrese términos de búsqueda..."
            };

            btnBuscar = new Button
            {
                Text = "🔍 Buscar",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(620, 35),
                Size = new Size(100, 25),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            lblTiempo = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                AutoSize = true,
                Location = new Point(10, 70),
                ForeColor = Color.FromArgb(100, 100, 100)
            };

            panelBusqueda.Controls.AddRange(new Control[] { lblBusqueda, txtBusqueda, btnBuscar, lblTiempo });

            // Panel izquierdo - Controles
            var panelControles = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = Color.FromArgb(250, 250, 250),
                Padding = new Padding(10)
            };

            var lblControles = new Label
            {
                Text = "🎛️ Controles del Sistema",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };

            btnCrearIndice = CrearBoton("🔨 Crear Índice", new Point(10, 40), Color.FromArgb(16, 124, 16));
            btnGuardarIndice = CrearBoton("💾 Guardar", new Point(10, 80), Color.FromArgb(138, 43, 226));
            btnCargarIndice = CrearBoton("📂 Cargar", new Point(10, 120), Color.FromArgb(255, 140, 0));
            btnEstadisticas = CrearBoton("📊 Estadísticas", new Point(10, 160), Color.FromArgb(70, 130, 180));

            var lblZipf = new Label
            {
                Text = "Percentil Zipf:",
                Font = new Font("Segoe UI", 9),
                AutoSize = true,
                Location = new Point(10, 210)
            };

            numZipf = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 30,
                Value = 15,
                Location = new Point(10, 230),
                Size = new Size(60, 25)
            };

            lblEstado = new Label
            {
                Text = "❌ Sin índice",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 270),
                ForeColor = Color.Red
            };

            progressBar = new ProgressBar
            {
                Location = new Point(10, 300),
                Size = new Size(220, 10),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };

            panelControles.Controls.AddRange(new Control[] { 
                lblControles, btnCrearIndice, btnGuardarIndice, btnCargarIndice, 
                btnEstadisticas, lblZipf, numZipf, lblEstado, progressBar 
            });

            // Panel central - Resultados
            var panelResultados = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var lblResultados = new Label
            {
                Text = "📋 Resultados de Búsqueda",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };

            lstResultados = new ListBox
            {
                Font = new Font("Consolas", 9),
                Location = new Point(10, 35),
                Size = new Size(500, 300),
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 60
            };

            var lblVistaPrevia = new Label
            {
                Text = "👁️ Vista Previa del Documento",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 350)
            };

            txtVistaPrevia = new RichTextBox
            {
                Location = new Point(10, 375),
                Size = new Size(500, 200),
                ReadOnly = true,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(248, 248, 248)
            };

            panelResultados.Controls.AddRange(new Control[] { lblResultados, lstResultados, lblVistaPrevia, txtVistaPrevia });

            // Agregar paneles al formulario
            this.Controls.Add(panelResultados);
            this.Controls.Add(panelControles);
            this.Controls.Add(panelBusqueda);

            // Eventos
            btnBuscar.Click += BtnBuscar_Click;
            btnCrearIndice.Click += BtnCrearIndice_Click;
            btnGuardarIndice.Click += BtnGuardarIndice_Click;
            btnCargarIndice.Click += BtnCargarIndice_Click;
            btnEstadisticas.Click += BtnEstadisticas_Click;
            txtBusqueda.KeyDown += TxtBusqueda_KeyDown;
            lstResultados.SelectedIndexChanged += LstResultados_SelectedIndexChanged;
            lstResultados.DoubleClick += LstResultados_DoubleClick;
            lstResultados.DrawItem += LstResultados_DrawItem;

            this.ResumeLayout(false);
        }

        private Button CrearBoton(string texto, Point location, Color color)
        {
            return new Button
            {
                Text = texto,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = location,
                Size = new Size(220, 30),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
        }

        private void ConfigurarInterfaz()
        {
            lstResultados.DrawMode = DrawMode.OwnerDrawFixed;
            lstResultados.ItemHeight = 60;
            
            // Configurar placeholder para el TextBox
            if (txtBusqueda != null)
            {
                txtBusqueda.GotFocus += (s, e) => {
                    if (txtBusqueda.ForeColor == Color.Gray)
                    {
                        txtBusqueda.Text = "";
                        txtBusqueda.ForeColor = Color.Black;
                    }
                };
            }
        }

        private async void CargarIndiceExistente()
        {
            if (File.Exists(archivoIndice))
            {
                MostrarProgreso(true, "Cargando índice existente...");
                
                await Task.Run(() => {
                    gestor.CargarIndice(archivoIndice);
                });
                
                ActualizarEstado();
                MostrarProgreso(false);
                
                var stats = gestor.ObtenerEstadisticas();
                lblTiempo.Text = $"✅ Índice cargado: {stats.CantidadDocumentos} docs, {stats.CantidadTerminos} términos";
            }
        }

        private async void BtnBuscar_Click(object sender, EventArgs e)
        {
            await RealizarBusqueda();
        }

        private void TxtBusqueda_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                _ = RealizarBusqueda();
            }
        }

        private async Task RealizarBusqueda()
        {
            if (gestor.IndiceEstaVacio())
            {
                MessageBox.Show("❌ No hay índice. Cree uno primero.", "Sin Índice", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string consulta = txtBusqueda.Text?.Trim();
            if (string.IsNullOrWhiteSpace(consulta))
            {
                MessageBox.Show("❌ Ingrese términos de búsqueda", "Consulta Vacía", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MostrarProgreso(true, "Buscando...");
            lstResultados.Items.Clear();
            txtVistaPrevia.Clear();

            try
            {
                var inicio = DateTime.Now;
                var resultados = await Task.Run(() => gestor.BuscarConSimilitudCoseno(consulta));
                var duracion = DateTime.Now - inicio;

                if (resultados.Count == 0)
                {
                    lblTiempo.Text = "❌ No se encontraron documentos relevantes";
                    return;
                }

                // Cargar resultados en la lista
                var iterador = new Iterador<ResultadoBusquedaVectorial>(resultados);
                int posicion = 1;
                
                while (iterador.Siguiente() && posicion <= 20)
                {
                    var resultado = iterador.Current;
                    lstResultados.Items.Add(resultado);
                    posicion++;
                }

                lblTiempo.Text = $"✅ {resultados.Count} resultado(s) en {duracion.TotalMilliseconds:F1} ms";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error en búsqueda: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                MostrarProgreso(false);
            }
        }

        private void LstResultados_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstResultados.SelectedItem is ResultadoBusquedaVectorial resultado)
            {
                MostrarVistaPrevia(resultado);
            }
        }

        private void LstResultados_DoubleClick(object sender, EventArgs e)
        {
            if (lstResultados.SelectedItem is ResultadoBusquedaVectorial resultado)
            {
                AbrirDocumentoEnNavegador(resultado);
            }
        }

        private void LstResultados_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= lstResultados.Items.Count) return;

            var resultado = lstResultados.Items[e.Index] as ResultadoBusquedaVectorial;
            if (resultado == null) return;

            e.DrawBackground();

            var rect = e.Bounds;
            var archivo = Path.GetFileName(resultado.Documento.Ruta);
            var similitud = resultado.SimilitudCoseno * 100;

            // Colores
            var colorTitulo = e.State.HasFlag(DrawItemState.Selected) ? Color.White : Color.FromArgb(0, 120, 215);
            var colorSimilitud = e.State.HasFlag(DrawItemState.Selected) ? Color.LightGray : Color.FromArgb(0, 150, 0);
            var colorPreview = e.State.HasFlag(DrawItemState.Selected) ? Color.LightGray : Color.Gray;

            using (var g = e.Graphics)
            {
                // Título del archivo
                using (var brushTitulo = new SolidBrush(colorTitulo))
                using (var fontTitulo = new Font("Segoe UI", 10, FontStyle.Bold))
                {
                    g.DrawString($"📄 {archivo}", fontTitulo, brushTitulo, 
                        new RectangleF(rect.X + 5, rect.Y + 5, rect.Width - 10, 20));
                }

                // Similitud
                using (var brushSimilitud = new SolidBrush(colorSimilitud))
                using (var fontSimilitud = new Font("Segoe UI", 9, FontStyle.Bold))
                {
                    g.DrawString($"🎯 Similitud: {similitud:F1}%", fontSimilitud, brushSimilitud, 
                        new RectangleF(rect.X + 5, rect.Y + 25, rect.Width - 10, 15));
                }

                // Vista previa
                var preview = resultado.ObtenerVistaPrevia();
                if (preview.Length > 80) preview = preview.Substring(0, 80) + "...";
                
                using (var brushPreview = new SolidBrush(colorPreview))
                using (var fontPreview = new Font("Segoe UI", 8))
                {
                    g.DrawString($"📝 {preview}", fontPreview, brushPreview, 
                        new RectangleF(rect.X + 5, rect.Y + 40, rect.Width - 10, 15));
                }
            }

            e.DrawFocusRectangle();
        }

        private void MostrarVistaPrevia(ResultadoBusquedaVectorial resultado)
        {
            try
            {
                string contenido = File.Exists(resultado.Documento.Ruta) 
                    ? File.ReadAllText(resultado.Documento.Ruta)
                    : resultado.Documento.TextoOriginal ?? "Contenido no disponible";

                // Limitar contenido para vista previa
                if (contenido.Length > 2000)
                    contenido = contenido.Substring(0, 2000) + "\n\n[... contenido truncado ...]";

                txtVistaPrevia.Text = contenido;
            }
            catch (Exception ex)
            {
                txtVistaPrevia.Text = $"Error cargando vista previa: {ex.Message}";
            }
        }

        private void AbrirDocumentoEnNavegador(ResultadoBusquedaVectorial resultado)
        {
            try
            {
                // Generar URL HTML y abrirla automáticamente
                string urlHtml = resultado.GenerarUrlHtml();
                
                // Decodificar la URL base64 y crear archivo temporal
                if (urlHtml.StartsWith("data:text/html;charset=utf-8;base64,"))
                {
                    string base64 = urlHtml.Substring("data:text/html;charset=utf-8;base64,".Length);
                    byte[] htmlBytes = Convert.FromBase64String(base64);
                    string htmlContent = System.Text.Encoding.UTF8.GetString(htmlBytes);
                    
                    // Crear archivo temporal
                    string tempFile = Path.Combine(Path.GetTempPath(), $"documento_{resultado.Documento.Id}.html");
                    File.WriteAllText(tempFile, htmlContent, System.Text.Encoding.UTF8);
                    
                    // Abrir en navegador predeterminado
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = tempFile,
                        UseShellExecute = true
                    });
                    
                    lblTiempo.Text = $"🌐 Abriendo documento en navegador: {Path.GetFileName(resultado.Documento.Ruta)}";
                }
                else
                {
                    MessageBox.Show("Error generando URL del documento", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error abriendo documento: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnCrearIndice_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(directorioDocumentos))
            {
                MessageBox.Show($"❌ Directorio no encontrado:\n{directorioDocumentos}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var archivos = Directory.GetFiles(directorioDocumentos, "*.txt");
            if (archivos.Length == 0)
            {
                MessageBox.Show("❌ No se encontraron archivos .txt en el directorio", 
                    "Sin Archivos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"Se encontraron {archivos.Length} archivos .txt.\n\n" +
                $"Percentil Zipf: {numZipf.Value}%\n\n" +
                "¿Desea crear el índice?",
                "Confirmar Creación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirmacion != DialogResult.Yes) return;

            MostrarProgreso(true, "Creando índice...");

            try
            {
                var inicio = DateTime.Now;
                bool exito = await gestor.CrearIndiceDesdeDirectorio(directorioDocumentos, (int)numZipf.Value);

                if (exito)
                {
                    var duracion = DateTime.Now - inicio;
                    var stats = gestor.ObtenerEstadisticas();

                    // Guardar automáticamente
                    gestor.GuardarIndice(archivoIndice);

                    lblTiempo.Text = $"✅ Índice creado en {duracion.TotalSeconds:F1}s - {stats.CantidadDocumentos} docs, {stats.CantidadTerminos} términos";
                    ActualizarEstado();

                    MessageBox.Show(
                        $"✅ Índice creado exitosamente\n\n" +
                        $"⏱️ Tiempo: {duracion.TotalSeconds:F1} segundos\n" +
                        $"📄 Documentos: {stats.CantidadDocumentos}\n" +
                        $"🔤 Términos: {stats.CantidadTerminos}\n" +
                        $"🔥 Zipf aplicado: {numZipf.Value}%",
                        "Índice Creado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else
                {
                    MessageBox.Show("❌ Error creando el índice", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                MostrarProgreso(false);
            }
        }

        private void BtnGuardarIndice_Click(object sender, EventArgs e)
        {
            if (gestor.IndiceEstaVacio())
            {
                MessageBox.Show("❌ No hay índice para guardar", "Sin Índice", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (gestor.GuardarIndice(archivoIndice))
                {
                    var info = new FileInfo(archivoIndice);
                    lblTiempo.Text = $"✅ Guardado: {archivoIndice} ({info.Length / 1024:F1} KB)";
                    
                    MessageBox.Show($"✅ Índice guardado exitosamente\n\nArchivo: {archivoIndice}\nTamaño: {info.Length / 1024:F1} KB", 
                        "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("❌ Error al guardar el índice", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCargarIndice_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Archivos de Índice (*.bin)|*.bin|Todos los archivos (*.*)|*.*";
                dialog.DefaultExt = "bin";
                dialog.Title = "Cargar Índice";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (gestor.CargarIndice(dialog.FileName))
                        {
                            var stats = gestor.ObtenerEstadisticas();
                            ActualizarEstado();
                            lblTiempo.Text = $"✅ Cargado: {stats.CantidadDocumentos} docs, {stats.CantidadTerminos} términos";
                            
                            MessageBox.Show($"✅ Índice cargado exitosamente\n\n" +
                                $"📄 Documentos: {stats.CantidadDocumentos}\n" +
                                $"🔤 Términos: {stats.CantidadTerminos}\n" +
                                $"🔥 Zipf aplicado: {(stats.ZipfAplicado ? "Sí" : "No")}", 
                                "Cargado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("❌ Error al cargar el índice", "Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"❌ Error: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnConfiguracion_Click(object sender, EventArgs e)
        {
            using (var formConfig = new FormConfiguracion(directorioDocumentos, (int)numZipf.Value))
            {
                if (formConfig.ShowDialog(this) == DialogResult.OK)
                {
                    // Actualizar configuración si fue modificada
                    if (formConfig.ConfiguracionModificada)
                    {
                        // Actualizar directorio de documentos
                        if (formConfig.DirectorioDocumentos != directorioDocumentos)
                        {
                            var resultado = MessageBox.Show(
                                "Se ha cambiado el directorio de documentos.\n\n" +
                                "¿Desea crear un nuevo índice con la nueva configuración?",
                                "Configuración Modificada",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question
                            );

                            if (resultado == DialogResult.Yes)
                            {
                                // Actualizar configuración y crear índice
                                numZipf.Value = formConfig.PercentilZipf;
                                _ = CrearIndiceConNuevaConfiguracion(formConfig.DirectorioDocumentos, formConfig.PercentilZipf);
                            }
                        }
                        else
                        {
                            // Solo actualizar percentil Zipf
                            numZipf.Value = formConfig.PercentilZipf;
                            lblTiempo.Text = $"⚙️ Configuración actualizada - Zipf: {formConfig.PercentilZipf}%";
                        }
                    }
                }
            }
        }

        private async Task CrearIndiceConNuevaConfiguracion(string nuevoDirectorio, int nuevoZipf)
        {
            if (!Directory.Exists(nuevoDirectorio))
            {
                MessageBox.Show($"❌ Directorio no encontrado: {nuevoDirectorio}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var archivos = Directory.GetFiles(nuevoDirectorio, "*.txt");
            if (archivos.Length == 0)
            {
                MessageBox.Show("❌ No se encontraron archivos .txt en el nuevo directorio", 
                    "Sin Archivos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MostrarProgreso(true, "Creando índice con nueva configuración...");

            try
            {
                var inicio = DateTime.Now;
                
                // Actualizar la variable de instancia
                var directorioAnterior = directorioDocumentos;
                typeof(FormPrincipal).GetField("directorioDocumentos", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(this, nuevoDirectorio);

                bool exito = await gestor.CrearIndiceDesdeDirectorio(nuevoDirectorio, nuevoZipf);

                if (exito)
                {
                    var duracion = DateTime.Now - inicio;
                    var stats = gestor.ObtenerEstadisticas();

                    // Guardar automáticamente
                    gestor.GuardarIndice(archivoIndice);

                    lblTiempo.Text = $"✅ Índice recreado en {duracion.TotalSeconds:F1}s - {stats.CantidadDocumentos} docs, {stats.CantidadTerminos} términos";
                    ActualizarEstado();

                    MessageBox.Show(
                        $"✅ Índice recreado exitosamente con nueva configuración\n\n" +
                        $"📁 Nuevo directorio: {Path.GetFileName(nuevoDirectorio)}\n" +
                        $"⏱️ Tiempo: {duracion.TotalSeconds:F1} segundos\n" +
                        $"📄 Documentos: {stats.CantidadDocumentos}\n" +
                        $"🔤 Términos: {stats.CantidadTerminos}\n" +
                        $"🔥 Zipf aplicado: {nuevoZipf}%",
                        "Índice Recreado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else
                {
                    // Restaurar directorio anterior en caso de error
                    typeof(FormPrincipal).GetField("directorioDocumentos", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.SetValue(this, directorioAnterior);

                    MessageBox.Show("❌ Error creando el índice con la nueva configuración", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                MostrarProgreso(false);
            }
        }

        private void BtnEstadisticas_Click(object sender, EventArgs e)
        {
            if (gestor.IndiceEstaVacio())
            {
                MessageBox.Show("❌ No hay índice cargado", "Sin Índice", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Abrir formulario de estadísticas detalladas
            using (var formEstadisticas = new FormEstadisticas())
            {
                formEstadisticas.ShowDialog(this);
            }
        }

        private void MostrarProgreso(bool mostrar, string mensaje = "")
        {
            progressBar.Visible = mostrar;
            if (mostrar)
            {
                lblTiempo.Text = mensaje;
                this.Cursor = Cursors.WaitCursor;
            }
            else
            {
                this.Cursor = Cursors.Default;
            }
            Application.DoEvents();
        }

        private void ActualizarEstado()
        {
            var stats = gestor.ObtenerEstadisticas();
            if (gestor.IndiceEstaVacio())
            {
                lblEstado.Text = "❌ Sin índice";
                lblEstado.ForeColor = Color.Red;
            }
            else
            {
                lblEstado.Text = $"✅ {stats.CantidadTerminos} términos{(stats.ZipfAplicado ? " (Zipf✓)" : "")}";
                lblEstado.ForeColor = Color.Green;
            }
        }
    }
}