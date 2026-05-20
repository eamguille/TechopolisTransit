using TechopolisTransit.Algorithms;
using TechopolisTransit.Data;
using TechopolisTransit.Models;
using TechopolisTransit.UI;

namespace TechopolisTransit
{
    public partial class MainForm : Form
    {
        // ── Grafo y algoritmos ──────────────────────────────────────────
        private TransporteGrafo _graph = null!;
        private BFS _bfs = null!;
        private DFS _dfs = null!;
        private Dijkstra _dijkstra = null!;

        // ── Controles – Tab Exploración ─────────────────────────────────
        private ComboBox cbOriginExplore = null!;
        private Button btnBFS = null!, btnDFS = null!;
        private ListBox lstBFSResult = null!, lstDFSResult = null!;
        private Label lblBFSCount = null!, lblDFSCount = null!;

        // ── Controles – Tab Ruta más Corta ──────────────────────────────
        private ComboBox cbShortOrigin = null!, cbShortDest = null!;
        private Button btnFindPath = null!;
        private ListBox lstPath = null!;
        private Label lblTotalTime = null!;

        // ── Controles – Tab Gestión de Rutas ────────────────────────────
        private DataGridView dgvRoutes = null!;
        private Button btnSaveWeights = null!;

        private GraphMapPanel _mapPanel = null!;
        private TabControl _tabControl = null!;

        // ════════════════════════════════════════════════════════════════
        public MainForm()
        {
            InitializeComponent();
            CargarGrafo();
            ConstruirUI();
            PoblarComboBoxes();
            CargarRutasEnGrid();
        }

        // ── Carga datos ─────────────────────────────────────────────────
        private void CargarGrafo()
        {
            _graph = CiudadMapaLoader.LoadTechopolisMap();
            _bfs = new BFS(_graph);
            _dfs = new DFS(_graph);
            _dijkstra = new Dijkstra(_graph);
        }

        // ════════════════════════════════════════════════════════════════
        //  CONSTRUCCIÓN DE LA INTERFAZ
        // ════════════════════════════════════════════════════════════════
        private void ConstruirUI()
        {
            // Encabezado
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.FromArgb(30, 90, 160)
            };
            var lblTitle = new Label
            {
                Text = "TECHÓPOLIS — Sistema de Planificación de Transporte",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlHeader.Controls.Add(lblTitle);

            // TabControl
            _tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
            var tabExp = new TabPage("Exploración del Grafo");
            var tabShort = new TabPage("Ruta más Corta");
            var tabRoutes = new TabPage("Gestión de Rutas");

            var tabMapa = new TabPage("Mapa Visual");
            _tabControl.TabPages.Insert(0, tabMapa);   // se inserta como primera tab
            ConstruirTabMapa(tabMapa);

            _tabControl.TabPages.AddRange(new[] { tabExp, tabShort, tabRoutes });

            ConstruirTabExploracion(tabExp);
            ConstruirTabRutaCorta(tabShort);
            ConstruirTabGestionRutas(tabRoutes);

            this.Controls.Add(_tabControl);
            this.Controls.Add(pnlHeader);
        }

        // ── Tab 1: Exploración ──────────────────────────────────────────
        private void ConstruirTabExploracion(TabPage tab)
        {
            // Panel superior
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 65, Padding = new Padding(12, 12, 12, 0) };

            var lblOrigen = new Label { Text = "Estación de origen:", Location = new Point(12, 18), AutoSize = true };
            cbOriginExplore = new ComboBox
            {
                Location = new Point(170, 14),
                Width = 240,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            btnBFS = new Button { Text = "Explorar con BFS", Location = new Point(430, 12), Width = 155, Height = 32 };
            btnDFS = new Button { Text = "Explorar con DFS", Location = new Point(595, 12), Width = 155, Height = 32 };

            EstilarBoton(btnBFS, Color.FromArgb(0, 120, 215));
            EstilarBoton(btnDFS, Color.FromArgb(16, 137, 62));

            btnBFS.Click += BtnBFS_Click;
            btnDFS.Click += BtnDFS_Click;

            pnlTop.Controls.AddRange(new Control[] { lblOrigen, cbOriginExplore, btnBFS, btnDFS });

            // Paneles de resultado
            var pnlResults = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };

            // BFS
            var grpBFS = new GroupBox
            {
                Text = "Recorrido BFS  (Búsqueda en Anchura)",
                Dock = DockStyle.Left,
                Width = 420,
                Padding = new Padding(6)
            };
            lblBFSCount = new Label { Text = "", Dock = DockStyle.Top, Height = 22, ForeColor = Color.Gray };
            lstBFSResult = new ListBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 10) };
            grpBFS.Controls.AddRange(new Control[] { lstBFSResult, lblBFSCount });

            // DFS
            var grpDFS = new GroupBox
            {
                Text = "Recorrido DFS  (Búsqueda en Profundidad)",
                Dock = DockStyle.Fill,
                Padding = new Padding(6)
            };
            lblDFSCount = new Label { Text = "", Dock = DockStyle.Top, Height = 22, ForeColor = Color.Gray };
            lstDFSResult = new ListBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 10) };
            grpDFS.Controls.AddRange(new Control[] { lstDFSResult, lblDFSCount });

            pnlResults.Controls.AddRange(new Control[] { grpDFS, grpBFS });

            tab.Controls.AddRange(new Control[] { pnlResults, pnlTop });
        }

        // ── Tab 2: Ruta más corta ───────────────────────────────────────
        private void ConstruirTabRutaCorta(TabPage tab)
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 75, Padding = new Padding(12) };

            var lblOrig = new Label { Text = "Origen:", Location = new Point(12, 14), AutoSize = true };
            cbShortOrigin = new ComboBox { Location = new Point(75, 10), Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblDest = new Label { Text = "Destino:", Location = new Point(12, 46), AutoSize = true };
            cbShortDest = new ComboBox { Location = new Point(75, 42), Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };

            btnFindPath = new Button { Text = "Encontrar Ruta Óptima", Location = new Point(325, 22), Width = 200, Height = 35 };
            EstilarBoton(btnFindPath, Color.FromArgb(136, 0, 21));
            btnFindPath.Click += BtnFindPath_Click;

            pnlTop.Controls.AddRange(new Control[] { lblOrig, cbShortOrigin, lblDest, cbShortDest, btnFindPath });

            var grpResult = new GroupBox
            {
                Text = "Ruta encontrada",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            lblTotalTime = new Label
            {
                Text = "Tiempo total: —",
                Dock = DockStyle.Bottom,
                Height = 35,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 90, 160),
                TextAlign = ContentAlignment.MiddleLeft
            };
            lstPath = new ListBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 10) };

            grpResult.Controls.AddRange(new Control[] { lblTotalTime, lstPath });
            tab.Controls.AddRange(new Control[] { grpResult, pnlTop });
        }

        // ── Tab 3: Gestión de rutas ─────────────────────────────────────
        private void ConstruirTabGestionRutas(TabPage tab)
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 55, Padding = new Padding(12) };

            btnSaveWeights = new Button
            {
                Text = "Guardar cambios de pesos",
                Location = new Point(12, 12),
                Width = 230,
                Height = 32
            };
            EstilarBoton(btnSaveWeights, Color.FromArgb(16, 137, 62));
            btnSaveWeights.Click += BtnSaveWeights_Click;

            var lblInfo = new Label
            {
                Text = "Edita el tiempo (minutos) directamente en la columna \"Tiempo\". Los cambios afectan a BFS, DFS y Dijkstra.",
                Location = new Point(258, 17),
                AutoSize = true,
                ForeColor = Color.DimGray
            };

            pnlTop.Controls.AddRange(new Control[] { btnSaveWeights, lblInfo });

            dgvRoutes = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                Font = new Font("Segoe UI", 10)
            };

            dgvRoutes.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "ColOrigen", HeaderText = "Origen", ReadOnly = true });
            dgvRoutes.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "ColDestino", HeaderText = "Destino", ReadOnly = true });
            dgvRoutes.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "ColTiempo", HeaderText = "Tiempo (min)", ReadOnly = false });

            tab.Controls.AddRange(new Control[] { dgvRoutes, pnlTop });
        }

        private void ConstruirTabMapa(TabPage tab)
        {
            _mapPanel = new GraphMapPanel(_graph)
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(4)
            };

            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 45, Padding = new Padding(8, 6, 8, 6) };

            var btnClear = new Button
            {
                Text = "✖  Limpiar ruta resaltada",
                Dock = DockStyle.Left,
                Width = 210,
                Height = 32
            };
            EstilarBoton(btnClear, Color.FromArgb(100, 100, 110));
            btnClear.Click += (_, _) => _mapPanel.ClearHighlight();

            var lblHint = new Label
            {
                Text = "Calcula una ruta en la pestaña \"Ruta más Corta\" para verla resaltada aquí.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 9)
            };

            pnlBottom.Controls.AddRange(new Control[] { lblHint, btnClear });
            tab.Controls.Add(_mapPanel);
            tab.Controls.Add(pnlBottom);
        }

        // ════════════════════════════════════════════════════════════════
        //  HELPERS DE UI
        // ════════════════════════════════════════════════════════════════
        private static void EstilarBoton(Button btn, Color color)
        {
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
        }

        private void PoblarComboBoxes()
        {
            foreach (var station in _graph.Stations)
            {
                cbOriginExplore.Items.Add(station);
                cbShortOrigin.Items.Add(station);
                cbShortDest.Items.Add(station);
            }

            if (cbOriginExplore.Items.Count > 0) cbOriginExplore.SelectedIndex = 0;
            if (cbShortOrigin.Items.Count > 0) cbShortOrigin.SelectedIndex = 0;
            if (cbShortDest.Items.Count > 1) cbShortDest.SelectedIndex = 1;
        }

        private void CargarRutasEnGrid()
        {
            dgvRoutes.Rows.Clear();
            foreach (var route in _graph.GetAllRoutes())
                dgvRoutes.Rows.Add(route.Origin.Name, route.Destination.Name, route.TravelTime);
        }

        // ════════════════════════════════════════════════════════════════
        //  EVENTOS
        // ════════════════════════════════════════════════════════════════
        private void BtnBFS_Click(object? sender, EventArgs e)
        {
            if (cbOriginExplore.SelectedItem is not Estacion origin) return;

            lstBFSResult.Items.Clear();
            var visitadas = _bfs.Traverse(origin);

            for (int i = 0; i < visitadas.Count; i++)
                lstBFSResult.Items.Add($"  {i + 1,2}.  {visitadas[i].Name}");

            lblBFSCount.Text = $"  Total de estaciones accesibles: {visitadas.Count}";
        }

        private void BtnDFS_Click(object? sender, EventArgs e)
        {
            if (cbOriginExplore.SelectedItem is not Estacion origin) return;

            lstDFSResult.Items.Clear();
            var visitadas = _dfs.Traverse(origin);

            for (int i = 0; i < visitadas.Count; i++)
                lstDFSResult.Items.Add($"  {i + 1,2}.  {visitadas[i].Name}");

            lblDFSCount.Text = $"  Total de estaciones accesibles: {visitadas.Count}";
        }

        private void BtnFindPath_Click(object? sender, EventArgs e)
        {
            if (cbShortOrigin.SelectedItem is not Estacion origin ||
                cbShortDest.SelectedItem is not Estacion dest) return;

            if (origin.Equals(dest))
            {
                MessageBox.Show("El origen y el destino deben ser estaciones diferentes.",
                    "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lstPath.Items.Clear();
            var result = _dijkstra.FindShortestPath(origin, dest);

            if (!result.PathFound)
            {
                lblTotalTime.Text = "No existe ruta entre las estaciones seleccionadas.";
                lblTotalTime.ForeColor = Color.Red;
                lstPath.Items.Add("No se encontró ninguna ruta disponible.");
                return;
            }

            lblTotalTime.ForeColor = Color.FromArgb(30, 90, 160);
            var path = result.Path;

            // Resaltar el camino en el mapa visual
            _mapPanel.HighlightPath(path);
            _tabControl.SelectedIndex = 0;  // cambia a la tab "Mapa Visual"

            for (int i = 0; i < path.Count; i++)
            {
                if (i == 0)
                {
                    lstPath.Items.Add($" {path[i].Name}  (Origen)");
                }
                else
                {
                    var routeSegment = _graph.GetRoutes(path[i - 1])
                        .FirstOrDefault(r => r.Destination.Equals(path[i]));
                    int segTime = routeSegment?.TravelTime ?? 0;
                    string esDestino = (i == path.Count - 1) ? "  ← Destino" : "";
                    lstPath.Items.Add($"  ➡ {path[i].Name}  (+{segTime} min){esDestino}");
                }
            }

            lblTotalTime.Text = $"Tiempo total de viaje: {result.TotalTime} minutos  |  Paradas: {path.Count - 1}";
        }

        private void BtnSaveWeights_Click(object? sender, EventArgs e)
        {
            var errores = new List<string>();

            foreach (DataGridViewRow row in dgvRoutes.Rows)
            {
                if (row.IsNewRow) continue;

                string? origenNombre = row.Cells["ColOrigen"].Value?.ToString();
                string? destinoNombre = row.Cells["ColDestino"].Value?.ToString();
                string? tiempoStr = row.Cells["ColTiempo"].Value?.ToString();

                if (string.IsNullOrWhiteSpace(tiempoStr) || !int.TryParse(tiempoStr, out int nuevoTiempo) || nuevoTiempo <= 0)
                {
                    errores.Add($"• Ruta {origenNombre} → {destinoNombre}: el tiempo debe ser un número entero positivo.");
                    continue;
                }

                var origen = _graph.GetStationByName(origenNombre ?? "");
                var destino = _graph.GetStationByName(destinoNombre ?? "");

                if (origen != null && destino != null)
                    _graph.AddRoute(origen, destino, nuevoTiempo, true);
            }

            if (errores.Count > 0)
            {
                MessageBox.Show("Se encontraron los siguientes errores:\n\n" + string.Join("\n", errores),
                    "Errores de validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show("Los pesos de las rutas han sido actualizados correctamente.\n" +
                                "Los cálculos de BFS, DFS y ruta más corta reflejarán los nuevos tiempos.",
                    "Guardado exitoso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
