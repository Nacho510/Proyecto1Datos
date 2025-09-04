using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using PruebaRider.Servicios;
using PruebaRider.Estructura.Nodo;

namespace PruebaRider.UI
{
    /// <summary>
    /// Modelo de datos para los resultados de búsqueda
    /// </summary>
    public class ResultadoViewModel : INotifyPropertyChanged
    {
        public int Posicion { get; set; }
        public string NombreArchivo { get; set; }
        public string SimilitudTexto { get; set; }
        public double SimilitudValor { get; set; }
        public string Score { get; set; }
        public int DocumentoId { get; set; }
        public string UrlCodificada { get; set; }
        public Brush ColorFondo { get; set; }
        public ResultadoBusquedaVectorial ResultadoOriginal { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// Ventana principal con diseño moderno
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GestorIndice gestor;

        private readonly string DIRECTORIO_DOCUMENTOS =
            @"C:\Users\ignab\OneDrive\Documents\Estructuras de datos\Documentos";

        private readonly string ARCHIVO_INDICE = @"indice_radix.bin";

        public ObservableCollection<ResultadoViewModel> Resultados { get; set; }
        public ObservableCollection<string> LogMensajes { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            gestor = GestorIndice.ObtenerInstancia();
            Resultados = new ObservableCollection<ResultadoViewModel>();
            LogMensajes = new ObservableCollection<string>();

            DataContext = this;
            _ = InicializarSistemaAsync();
        }

        private void InitializeComponent()
        {
            // Configuración de la ventana
            Title = "🚀 Motor de Búsqueda Vectorial - RadixSort + Similitud Coseno";
            Width = 1400;
            Height = 900;
            MinWidth = 1200;
            MinHeight = 700;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Crear el diseño principal
            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Search
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Buttons
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) }); // Results
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Log
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Status

            Content = mainGrid;

            // HEADER
            CreateHeader(mainGrid);

            // BÚSQUEDA
            CreateSearchSection(mainGrid);

            // BOTONES
            CreateButtonSection(mainGrid);

            // RESULTADOS
            CreateResultsSection(mainGrid);

            // LOG
            CreateLogSection(mainGrid);

            // STATUS BAR
            CreateStatusBar(mainGrid);

            // Aplicar tema oscuro/moderno
            ApplyModernTheme();
        }

        private void CreateHeader(Grid mainGrid)
        {
            var headerPanel = new StackPanel
            {
                Background = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(41, 128, 185), 0),
                        new GradientStop(Color.FromRgb(52, 152, 219), 1)
                    }
                },
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var titulo = new TextBlock
            {
                Text = "🔍 Motor de Búsqueda Vectorial Avanzado",
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 5)
            };

            var subtitulo = new TextBlock
            {
                Text = "Vector Ordenado • RadixSort • Similitud Coseno • URLs Codificadas",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(236, 240, 241)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            headerPanel.Children.Add(titulo);
            headerPanel.Children.Add(subtitulo);

            Grid.SetRow(headerPanel, 0);
            mainGrid.Children.Add(headerPanel);
        }

        private void CreateSearchSection(Grid mainGrid)
        {
            var searchPanel = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(248, 249, 250)),
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(20, 0, 20, 20),
                Padding = new Thickness(20)
            };

            var searchGrid = new Grid();
            searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // TextBox de búsqueda
            var txtBusqueda = new TextBox
            {
                Name = "txtBusqueda",
                FontSize = 16,
                Padding = new Thickness(15, 12, 15, 12),
                BorderBrush = new SolidColorBrush(Color.FromRgb(189, 195, 199)),
                BorderThickness = new Thickness(2),
                Background = Brushes.White,
                Text = "Ingrese términos de búsqueda..."
            };

            txtBusqueda.GotFocus += (s, e) =>
            {
                if (txtBusqueda.Text == "Ingrese términos de búsqueda...")
                {
                    txtBusqueda.Text = "";
                    txtBusqueda.Foreground = Brushes.Black;
                }
            };

            txtBusqueda.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtBusqueda.Text))
                {
                    txtBusqueda.Text = "Ingrese términos de búsqueda...";
                    txtBusqueda.Foreground = Brushes.Gray;
                }
            };

            txtBusqueda.KeyDown += async (s, e) =>
            {
                if (e.Key == Key.Enter)
                    await EjecutarBusquedaAsync();
            };

            // Botón de búsqueda
            var btnBuscar = new Button
            {
                Content = "🔍 Buscar",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(20, 12, 20, 12),
                Margin = new Thickness(15, 0, 0, 0),
                Background = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(46, 204, 113), 0),
                        new GradientStop(Color.FromRgb(39, 174, 96), 1)
                    }
                },
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };

            btnBuscar.Click += async (s, e) => await EjecutarBusquedaAsync();

            Grid.SetColumn(txtBusqueda, 0);
            Grid.SetColumn(btnBuscar, 1);

            searchGrid.Children.Add(txtBusqueda);
            searchGrid.Children.Add(btnBuscar);
            searchPanel.Child = searchGrid;

            Grid.SetRow(searchPanel, 1);
            mainGrid.Children.Add(searchPanel);

            // Registrar el TextBox para acceso posterior
            RegisterName("txtBusqueda", txtBusqueda);
        }

        private void CreateButtonSection(Grid mainGrid)
        {
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var botones = new[]
            {
                new
                {
                    Texto = "🔨 Crear Índice", Color = Color.FromRgb(230, 126, 34),
                    Handler = new Func<object, RoutedEventArgs, Task>(async (s, e) => await CrearIndiceAsync())
                },
                new
                {
                    Texto = "📊 Estadísticas", Color = Color.FromRgb(241, 196, 15), Handler =
                        new Func<object, RoutedEventArgs, Task>((s, e) =>
                        {
                            MostrarEstadisticas();
                            return Task.CompletedTask;
                        })
                },
                new
                {
                    Texto = "💾 Guardar", Color = Color.FromRgb(155, 89, 182), Handler =
                        new Func<object, RoutedEventArgs, Task>((s, e) =>
                        {
                            GuardarIndice();
                            return Task.CompletedTask;
                        })
                },
                new
                {
                    Texto = "✅ Validar", Color = Color.FromRgb(26, 188, 156), Handler =
                        new Func<object, RoutedEventArgs, Task>((s, e) =>
                        {
                            ValidarSistema();
                            return Task.CompletedTask;
                        })
                }
            };

            foreach (var boton in botones)
            {
                var btn = new Button
                {
                    Content = boton.Texto,
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Padding = new Thickness(20, 10, 20, 10),
                    Margin = new Thickness(10, 0, 10, 0),
                    Background = new SolidColorBrush(boton.Color),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand
                };

                btn.Click += async (s, e) => await boton.Handler(s, e);
                buttonPanel.Children.Add(btn);
            }

            Grid.SetRow(buttonPanel, 2);
            mainGrid.Children.Add(buttonPanel);
        }

        private void CreateResultsSection(Grid mainGrid)
        {
            var resultsPanel = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(189, 195, 199)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(20, 0, 20, 20)
            };

            var resultsGrid = new Grid();
            resultsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            resultsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Header de resultados
            var headerPanel = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(52, 73, 94)),
                Padding = new Thickness(20, 15, 20, 15)
            };

            var headerText = new TextBlock
            {
                Text = "📄 Resultados de Búsqueda (Clic en URL para copiar)",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };

            headerPanel.Child = headerText;
            Grid.SetRow(headerPanel, 0);
            resultsGrid.Children.Add(headerPanel);

            // DataGrid para resultados
            var dataGrid = new DataGrid
            {
                Name = "dataGridResultados",
                AutoGenerateColumns = false,
                CanUserAddRows = false,
                CanUserDeleteRows = false,
                CanUserResizeRows = false,
                SelectionMode = DataGridSelectionMode.Single,
                GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                Background = Brushes.White,
                AlternatingRowBackground = new SolidColorBrush(Color.FromRgb(248, 249, 250)),
                FontSize = 12
            };

            // Configurar columnas
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Pos",
                Binding = new Binding("Posicion"),
                Width = 50,
                IsReadOnly = true
            });

            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Documento",
                Binding = new Binding("NombreArchivo"),
                Width = 200,
                IsReadOnly = true
            });

            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Similitud",
                Binding = new Binding("SimilitudTexto"),
                Width = 100,
                IsReadOnly = true
            });

            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Score",
                Binding = new Binding("Score"),
                Width = 100,
                IsReadOnly = true
            });

            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "ID",
                Binding = new Binding("DocumentoId"),
                Width = 60,
                IsReadOnly = true
            });

            var urlColumn = new DataGridTemplateColumn
            {
                Header = "URL Codificada",
                Width = 300
            };

            var urlTemplate = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(Button));
            factory.SetValue(Button.ContentProperty, "📋 Copiar URL");
            factory.SetValue(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(52, 152, 219)));
            factory.SetValue(Button.ForegroundProperty, Brushes.White);
            factory.SetValue(Button.BorderThicknessProperty, new Thickness(0));
            factory.SetValue(Button.PaddingProperty, new Thickness(10, 5, 10, 5));
            factory.SetValue(Button.CursorProperty, Cursors.Hand);
            factory.AddHandler(Button.ClickEvent, new RoutedEventHandler(CopiarUrl));

            urlTemplate.VisualTree = factory;
            urlColumn.CellTemplate = urlTemplate;
            dataGrid.Columns.Add(urlColumn);

            dataGrid.ItemsSource = Resultados;

            Grid.SetRow(dataGrid, 1);
            resultsGrid.Children.Add(dataGrid);

            resultsPanel.Child = resultsGrid;
            Grid.SetRow(resultsPanel, 3);
            mainGrid.Children.Add(resultsPanel);

            RegisterName("dataGridResultados", dataGrid);
        }

        private void CreateLogSection(Grid mainGrid)
        {
            var logPanel = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(52, 73, 94)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(20, 0, 20, 20)
            };

            var logGrid = new Grid();
            logGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            logGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Header del log
            var logHeaderPanel = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(52, 73, 94)),
                Padding = new Thickness(20, 10, 20, 10)
            };

            var logHeaderText = new TextBlock
            {
                Text = "📊 Log del Sistema",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };

            logHeaderPanel.Child = logHeaderText;
            Grid.SetRow(logHeaderPanel, 0);
            logGrid.Children.Add(logHeaderPanel);

            // ListBox para el log
            var logListBox = new ListBox
            {
                Name = "logListBox",
                Background = new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                Foreground = new SolidColorBrush(Color.FromRgb(236, 240, 241)),
                BorderThickness = new Thickness(0),
                FontFamily = new FontFamily("Consolas"),
                FontSize = 11,
                Padding = new Thickness(10),
                ItemsSource = LogMensajes
            };

            // Scroll automático
            logListBox.Items.CurrentChanged += (s, e) =>
            {
                if (logListBox.Items.Count > 0)
                    logListBox.ScrollIntoView(logListBox.Items[logListBox.Items.Count - 1]);
            };

            Grid.SetRow(logListBox, 1);
            logGrid.Children.Add(logListBox);

            logPanel.Child = logGrid;
            Grid.SetRow(logPanel, 4);
            mainGrid.Children.Add(logPanel);

            RegisterName("logListBox", logListBox);
        }

        private void CreateStatusBar(Grid mainGrid)
        {
            var statusPanel = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(52, 73, 94)),
                Padding = new Thickness(20, 10, 20, 10)
            };

            var statusText = new TextBlock
            {
                Name = "statusText",
                Text = "🟡 Inicializando sistema...",
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold
            };

            statusPanel.Child = statusText;
            Grid.SetRow(statusPanel, 5);
            mainGrid.Children.Add(statusPanel);

            RegisterName("statusText", statusText);
        }

        private void ApplyModernTheme()
        {
            Background = new SolidColorBrush(Color.FromRgb(250, 250, 250));

            var style = new Style(typeof(Button));
            style.Setters.Add(new Setter(Button.TemplateProperty, CreateModernButtonTemplate()));
            Resources.Add(typeof(Button), style);
        }

        private ControlTemplate CreateModernButtonTemplate()
        {
            var template = new ControlTemplate(typeof(Button));

            var border = new FrameworkElementFactory(typeof(Border));
            border.Name = "border";
            border.SetBinding(Border.BackgroundProperty, new Binding("Background")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent)
            });
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(5));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(0));

            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            border.AppendChild(contentPresenter);
            template.VisualTree = border;

            return template;
        }

        private async Task InicializarSistemaAsync()
        {
            LogInfo("🔄 Inicializando sistema...");
            ActualizarEstado("Inicializando...");

            try
            {
                if (!Directory.Exists(DIRECTORIO_DOCUMENTOS))
                {
                    LogError($"❌ Directorio no encontrado: {DIRECTORIO_DOCUMENTOS}");
                    LogInfo("💡 Cree el directorio y agregue archivos .txt");
                    ActualizarEstado("Error: Directorio no encontrado");
                    return;
                }

                if (File.Exists(ARCHIVO_INDICE))
                {
                    LogInfo("📂 Cargando índice existente...");
                    if (gestor.CargarIndice(ARCHIVO_INDICE))
                    {
                        LogSuccess("✅ Índice cargado con RadixSort");
                        MostrarEstadisticasResumen();
                        ActualizarEstado("Índice cargado - Listo para búsquedas");
                        return;
                    }
                }

                LogInfo("🔨 Creando índice nuevo...");
                if (await gestor.CrearIndiceDesdeDirectorio(DIRECTORIO_DOCUMENTOS))
                {
                    LogSuccess("✅ Índice creado con RadixSort");
                    gestor.GuardarIndice(ARCHIVO_INDICE);
                    LogSuccess($"💾 Guardado como {ARCHIVO_INDICE}");
                    MostrarEstadisticasResumen();
                    ActualizarEstado("Sistema listo - RadixSort activo");
                }
                else
                {
                    LogError("❌ Error al crear índice inicial");
                    ActualizarEstado("Error en inicialización");
                }
            }
            catch (Exception ex)
            {
                LogError($"❌ Error: {ex.Message}");
                ActualizarEstado("Error crítico");
            }
        }

        private async Task EjecutarBusquedaAsync()
        {
            var txtBusqueda = FindName("txtBusqueda") as TextBox;
            string consulta = txtBusqueda?.Text.Trim();

            if (string.IsNullOrEmpty(consulta) || consulta == "Ingrese términos de búsqueda...")
            {
                MessageBox.Show("Por favor ingrese términos de búsqueda", "Búsqueda",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (gestor.IndiceEstaVacio())
            {
                LogError("❌ No hay índice. Creando automáticamente...");
                await CrearIndiceAsync();
                return;
            }

            LogInfo($"🔍 Búsqueda vectorial: '{consulta}'");
            ActualizarEstado("Ejecutando búsqueda vectorial...");

            Resultados.Clear();

            try
            {
                var inicio = DateTime.Now;
                var resultados = gestor.BuscarConSimilitudCoseno(consulta);
                var duracion = DateTime.Now - inicio;

                MostrarResultados(resultados, consulta, duracion);
                ActualizarEstado(
                    $"Búsqueda completada: {resultados.Count} resultados en {duracion.TotalMilliseconds:F0}ms");
            }
            catch (Exception ex)
            {
                LogError($"❌ Error en búsqueda: {ex.Message}");
                ActualizarEstado("Error en búsqueda");
            }
        }

        private void MostrarResultados(ListaDobleEnlazada<ResultadoBusquedaVectorial> resultados, string consulta,
            TimeSpan duracion)
        {
            LogInfo($"📊 RESULTADOS - {resultados.Count} encontrados");
            LogInfo($"⏱️ Tiempo: {duracion.TotalMilliseconds:F2} ms");
            LogInfo($"🎯 Algoritmo: Similitud Coseno con Vector Ordenado");

            if (resultados.Count == 0)
            {
                LogInfo($"🔍 No se encontraron resultados para '{consulta}'");
                LogInfo("💡 Sugerencias: Use términos más generales, verifique ortografía");
                return;
            }

            var iterador = new Iterador<ResultadoBusquedaVectorial>(resultados);
            int posicion = 1;

            while (iterador.Siguiente() && posicion <= 20)
            {
                var resultado = iterador.Current;

                // Generar URL codificada
                string urlCodificada = GenerarUrlCodificada(resultado);

                // Determinar color según similitud
                Brush colorFondo;
                if (resultado.SimilitudCoseno >= 0.5)
                    colorFondo = new SolidColorBrush(Color.FromRgb(212, 237, 218));
                else if (resultado.SimilitudCoseno >= 0.2)
                    colorFondo = new SolidColorBrush(Color.FromRgb(255, 243, 205));
                else
                    colorFondo = new SolidColorBrush(Color.FromRgb(248, 215, 218));

                var viewModel = new ResultadoViewModel
                {
                    Posicion = posicion,
                    NombreArchivo = Path.GetFileName(resultado.Documento.Ruta),
                    SimilitudTexto = $"{resultado.SimilitudCoseno * 100:F1}%",
                    SimilitudValor = resultado.SimilitudCoseno,
                    Score = resultado.SimilitudCoseno.ToString("F4"),
                    DocumentoId = resultado.Documento.Id,
                    UrlCodificada = urlCodificada,
                    ColorFondo = colorFondo,
                    ResultadoOriginal = resultado
                };

                Resultados.Add(viewModel);
                posicion++;
            }

            if (resultados.Count > 20)
            {
                LogInfo($"... y {resultados.Count - 20} resultados más");
            }

            MostrarEstadisticasSimilitud(resultados);
        }

        private string GenerarUrlCodificada(ResultadoBusquedaVectorial resultado)
        {
            try
            {
                string contenido = File.Exists(resultado.Documento.Ruta)
                    ? File.ReadAllText(resultado.Documento.Ruta)
                    : resultado.Documento.TextoOriginal ?? "Contenido no disponible";

                byte[] bytes = Encoding.UTF8.GetBytes(contenido);
                string base64 = Convert.ToBase64String(bytes);

                // Crear URL que puede ser usada en navegadores o aplicaciones
                return $"data:text/plain;charset=utf-8;base64,{base64}";
            }
            catch (Exception)
            {
                byte[] errorBytes = Encoding.UTF8.GetBytes(
                    $"Error al cargar: {Path.GetFileName(resultado.Documento.Ruta)}");
                string errorBase64 = Convert.ToBase64String(errorBytes);
                return $"data:text/plain;charset=utf-8;base64,{errorBase64}";
            }
        }

        private void CopiarUrl(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ResultadoViewModel resultado)
            {
                try
                {
                    Clipboard.SetText(resultado.UrlCodificada);
                    LogInfo($"📋 URL copiada al portapapeles: {resultado.NombreArchivo}");

                    // Feedback visual
                    var originalContent = button.Content;
                    button.Content = "✅ Copiado!";
                    button.Background = new SolidColorBrush(Color.FromRgb(39, 174, 96));

                    var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                    timer.Tick += (s, args) =>
                    {
                        button.Content = originalContent;
                        button.Background = new SolidColorBrush(Color.FromRgb(52, 152, 219));
                        timer.Stop();
                    };
                    timer.Start();
                }
                catch (Exception ex)
                {
                    LogError($"❌ Error copiando URL: {ex.Message}");
                    MessageBox.Show($"No se pudo copiar la URL: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task CrearIndiceAsync()
        {
            LogInfo("🔨 CREAR ÍNDICE CON RADIX SORT");

            if (!Directory.Exists(DIRECTORIO_DOCUMENTOS))
            {
                MessageBox.Show($"Directorio no encontrado:\n{DIRECTORIO_DOCUMENTOS}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var archivos = Directory.GetFiles(DIRECTORIO_DOCUMENTOS, "*.txt");
            if (archivos.Length == 0)
            {
                MessageBox.Show("No se encontraron archivos .txt en el directorio", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            LogInfo($"📂 {archivos.Length} archivo(s) .txt encontrados");
            ActualizarEstado("Creando índice con RadixSort...");

            try
            {
                if (await gestor.CrearIndiceDesdeDirectorio(DIRECTORIO_DOCUMENTOS))
                {
                    LogSuccess("✅ Índice creado exitosamente");

                    if (gestor.GuardarIndice(ARCHIVO_INDICE))
                    {
                        LogSuccess($"💾 Guardado como {ARCHIVO_INDICE}");
                    }

                    MostrarEstadisticasResumen();
                    ActualizarEstado("Índice creado - RadixSort aplicado");

                    MessageBox.Show("Índice creado exitosamente con RadixSort", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    LogError("❌ Error al crear índice");
                    ActualizarEstado("Error al crear índice");
                }
            }
            catch (Exception ex)
            {
                LogError($"❌ Error: {ex.Message}");
                ActualizarEstado("Error crítico");
            }
        }

        private void MostrarEstadisticas()
        {
            if (gestor.IndiceEstaVacio())
            {
                MessageBox.Show("No hay índice cargado", "Estadísticas",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            LogInfo("📊 ESTADÍSTICAS DEL SISTEMA");

            var stats = gestor.ObtenerEstadisticas();

            LogInfo("📈 DATOS PRINCIPALES:");
            LogInfo($"   📄 Documentos: {stats.CantidadDocumentos}");
            LogInfo($"   🔤 Términos: {stats.CantidadTerminos}");
            LogInfo($"   📊 Promedio términos/doc: {stats.PromedioTerminosPorDocumento:F1}");

            LogInfo("⚡ RENDIMIENTO:");
            LogInfo($"   💾 Memoria: {stats.MemoriaEstimadaKB} KB");
            LogInfo($"   🔤 Vector ordenado: {(stats.IndiceOrdenado ? "✅ Sí (RadixSort)" : "❌ No")}");
            LogInfo($"   ⚡ Complejidad búsqueda: {(stats.IndiceOrdenado ? "O(log n)" : "O(n)")}");

            if (File.Exists(ARCHIVO_INDICE))
            {
                var fileInfo = new FileInfo(ARCHIVO_INDICE);
                LogInfo("💾 ARCHIVO:");
                LogInfo($"   📁 {ARCHIVO_INDICE}");
                LogInfo($"   📊 Tamaño: {fileInfo.Length / 1024.0:F1} KB");
                LogInfo($"   🗓️ Modificado: {fileInfo.LastWriteTime:dd/MM/yyyy HH:mm}");
            }

            var mensaje = $"📊 ESTADÍSTICAS DEL SISTEMA\n\n" +
                          $"📈 DATOS:\n" +
                          $"  📄 Documentos: {stats.CantidadDocumentos}\n" +
                          $"  🔤 Términos: {stats.CantidadTerminos}\n" +
                          $"  📊 Promedio términos/doc: {stats.PromedioTerminosPorDocumento:F1}\n\n" +
                          $"⚡ RENDIMIENTO:\n" +
                          $"  💾 Memoria: {stats.MemoriaEstimadaKB} KB\n" +
                          $"  🔤 RadixSort: {(stats.IndiceOrdenado ? "✅ Activo" : "❌ Inactivo")}\n" +
                          $"  ⚡ Complejidad: {(stats.IndiceOrdenado ? "O(log n)" : "O(n)")}";

            MessageBox.Show(mensaje, "Estadísticas del Sistema", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MostrarEstadisticasResumen()
        {
            if (!gestor.IndiceEstaVacio())
            {
                var stats = gestor.ObtenerEstadisticas();
                LogInfo(
                    $"📊 {stats.CantidadDocumentos} docs | {stats.CantidadTerminos} términos | {stats.MemoriaEstimadaKB} KB");
            }
        }

        private void GuardarIndice()
        {
            if (gestor.IndiceEstaVacio())
            {
                MessageBox.Show("No hay índice para guardar", "Guardar",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Archivos de índice (*.bin)|*.bin|Todos los archivos (*.*)|*.*",
                FileName = ARCHIVO_INDICE,
                Title = "Guardar Índice"
            };

            if (dialog.ShowDialog() == true)
            {
                LogInfo($"💾 Guardando {dialog.FileName}...");

                if (gestor.GuardarIndice(dialog.FileName))
                {
                    var fileInfo = new FileInfo(dialog.FileName);
                    LogSuccess($"✅ Guardado exitosamente ({fileInfo.Length / 1024.0:F1} KB)");
                    MessageBox.Show($"Índice guardado exitosamente\nTamaño: {fileInfo.Length / 1024.0:F1} KB",
                        "Guardado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    LogError("❌ Error al guardar");
                    MessageBox.Show("Error al guardar el índice", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ValidarSistema()
        {
            LogInfo("🔍 VALIDANDO INTEGRIDAD DEL SISTEMA");

            var validacion = gestor.ValidarIntegridad();

            if (validacion.EsValido)
            {
                LogSuccess("✅ SISTEMA VÁLIDO");
                LogSuccess($"📊 {validacion.Mensaje}");
            }
            else
            {
                LogError("⚠️ SISTEMA CON PROBLEMAS");
                LogError($"📊 {validacion.Mensaje}");
            }

            LogInfo($"📄 Índice poblado: {(validacion.IndiceNoVacio ? "✅" : "❌")}");
            LogInfo($"🔤 Vector ordenado: {(validacion.Vector ? "✅" : "❌")}");
            LogInfo($"🎯 Buscador activo: {(validacion.BuscadorFuncional ? "✅" : "❌")}");
            LogInfo($"🏗️ Estructuras OK: {(validacion.EstructurasConsistentes ? "✅" : "❌")}");

            MessageBox.Show(validacion.ToString(), "Validación del Sistema",
                MessageBoxButton.OK,
                validacion.EsValido ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }

        private void MostrarEstadisticasSimilitud(ListaDobleEnlazada<ResultadoBusquedaVectorial> resultados)
        {
            if (resultados.Count == 0) return;

            double suma = 0, max = 0, min = 1.0;
            var iterador = new Iterador<ResultadoBusquedaVectorial>(resultados);

            while (iterador.Siguiente())
            {
                double sim = iterador.Current.SimilitudCoseno;
                suma += sim;
                max = Math.Max(max, sim);
                min = Math.Min(min, sim);
            }

            double promedio = suma / resultados.Count;

            LogInfo("📈 ESTADÍSTICAS DE SIMILITUD:");
            LogInfo($"   📊 Promedio: {promedio * 100:F1}%");
            LogInfo($"   📊 Máxima: {max * 100:F1}%");
            LogInfo($"   📊 Mínima: {min * 100:F1}%");
        }

        private void LogInfo(string mensaje)
        {
            Dispatcher.Invoke(() =>
            {
                LogMensajes.Add($"{DateTime.Now:HH:mm:ss} {mensaje}");

                // Mantener solo los últimos 100 mensajes para rendimiento
                if (LogMensajes.Count > 100)
                {
                    LogMensajes.RemoveAt(0);
                }
            });
        }

        private void LogSuccess(string mensaje) => LogInfo(mensaje);
        private void LogError(string mensaje) => LogInfo(mensaje);

        private void ActualizarEstado(string mensaje)
        {
            Dispatcher.Invoke(() =>
            {
                var statusText = FindName("statusText") as TextBlock;
                if (statusText != null)
                {
                    statusText.Text = $"✅ {mensaje}";
                }
            });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            LogInfo("👋 Cerrando aplicación...");
            gestor.LimpiarSistema();
            base.OnClosing(e);
        }
    }
    
}