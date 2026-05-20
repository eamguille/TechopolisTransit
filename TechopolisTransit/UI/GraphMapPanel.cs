using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using TechopolisTransit.Models;
using static System.Collections.Specialized.BitVector32;

namespace TechopolisTransit.UI
{
    public class GraphMapPanel : Panel
    {
        private readonly TransporteGrafo _graph;

        // Posiciones relativas (0.0 a 1.0) para cada estación por ID
        private readonly Dictionary<int, PointF> _relPos = new()
        {
            { 1,  new PointF(0.50f, 0.46f) },  // Estación Central
            { 2,  new PointF(0.50f, 0.16f) },  // Terminal Norte
            { 3,  new PointF(0.50f, 0.77f) },  // Terminal Sur
            { 4,  new PointF(0.72f, 0.46f) },  // Mercado del Este
            { 5,  new PointF(0.27f, 0.46f) },  // Parque Oeste
            { 6,  new PointF(0.35f, 0.20f) },  // Universidad
            { 7,  new PointF(0.35f, 0.73f) },  // Hospital General
            { 8,  new PointF(0.88f, 0.26f) },  // Aeropuerto
            { 9,  new PointF(0.76f, 0.20f) },  // Centro Comercial
            { 10, new PointF(0.76f, 0.70f) },  // Complejo Deportivo
            { 11, new PointF(0.12f, 0.32f) },  // Museo Nacional
            { 12, new PointF(0.88f, 0.72f) },  // Zona Industrial
            { 13, new PointF(0.62f, 0.07f) },  // Residencial Norte
            { 14, new PointF(0.63f, 0.88f) },  // Residencial Sur
        };

        private List<Estacion> _highlightedPath = new();
        private Estacion? _hoveredStation = null;
        private const int R = 18; // radio del nodo en píxeles

        public GraphMapPanel(TransporteGrafo graph)
        {
            _graph = graph;
            DoubleBuffered = true;
            BackColor = Color.FromArgb(235, 242, 255);
            MouseMove += OnMouseMove;
            Resize += (_, _) => Invalidate();
        }

        // ── API pública ──────────────────────────────────────────────────
        public void HighlightPath(List<Estacion> path)
        {
            _highlightedPath = path ?? new();
            Invalidate();
        }

        public void ClearHighlight()
        {
            _highlightedPath.Clear();
            Invalidate();
        }

        // ── Conversión posición relativa → píxeles ───────────────────────
        private PointF GetPx(int id)
        {
            var rel = _relPos.TryGetValue(id, out var p) ? p : new PointF(0.5f, 0.5f);
            float mx = 75f, my = 55f;
            return new PointF(
                mx + rel.X * (Width - 2 * mx),
                my + rel.Y * (Height - 2 * my)
            );
        }

        // ── Mouse hover ──────────────────────────────────────────────────
        private void OnMouseMove(object? sender, MouseEventArgs e)
        {
            Estacion? found = null;
            foreach (var s in _graph.Stations)
            {
                var pos = GetPx(s.Id);
                double d = Math.Sqrt(Math.Pow(e.X - pos.X, 2) + Math.Pow(e.Y - pos.Y, 2));
                if (d <= R + 5) { found = s; break; }
            }
            if (found != _hoveredStation)
            {
                _hoveredStation = found;
                Cursor = found != null ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  PAINT
        // ════════════════════════════════════════════════════════════════
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            PaintBackground(g);
            PaintEdges(g);
            PaintNodes(g);
            PaintLegend(g);
        }

        // ── Fondo con patrón de puntos ───────────────────────────────────
        private void PaintBackground(Graphics g)
        {
            using var dot = new SolidBrush(Color.FromArgb(35, 140, 155, 210));
            for (int x = 25; x < Width; x += 35)
                for (int y = 25; y < Height; y += 35)
                    g.FillEllipse(dot, x - 1.5f, y - 1.5f, 3f, 3f);

            using var font = new Font("Segoe UI", 11, FontStyle.Bold);
            using var brush = new SolidBrush(Color.FromArgb(25, 75, 140));
            g.DrawString("  Mapa del Sistema de Transporte — Techópolis", font, brush, 8f, 8f);
        }

        // ── Aristas ──────────────────────────────────────────────────────
        private void PaintEdges(Graphics g)
        {
            foreach (var route in _graph.GetAllRoutes())
            {
                var pA = GetPx(route.Origin.Id);
                var pB = GetPx(route.Destination.Id);
                bool hl = IsEdgeInPath(route.Origin, route.Destination);

                // Línea de la ruta
                using var pen = new Pen(
                    hl ? Color.FromArgb(220, 200, 30, 30)
                       : Color.FromArgb(160, 90, 130, 215),
                    hl ? 4f : 2.2f
                );
                g.DrawLine(pen, pA, pB);

                // Flecha en el centro de la línea (dirección visual)
                float midX = (pA.X + pB.X) / 2;
                float midY = (pA.Y + pB.Y) / 2;

                // Etiqueta de tiempo
                string lbl = $"{route.TravelTime} m";
                using var wf = new Font("Segoe UI", 7.5f, FontStyle.Bold);
                var sz = g.MeasureString(lbl, wf);
                var bg = new RectangleF(midX - sz.Width / 2 - 3, midY - sz.Height / 2 - 1,
                                        sz.Width + 6, sz.Height + 2);

                using var bgBr = new SolidBrush(hl
                    ? Color.FromArgb(225, 255, 228, 228)
                    : Color.FromArgb(215, 238, 244, 255));
                using var bgPen = new Pen(hl
                    ? Color.FromArgb(180, 200, 0, 0)
                    : Color.FromArgb(100, 90, 130, 210), 1f);

                g.FillRectangle(bgBr, bg);
                g.DrawRectangle(bgPen, bg.X, bg.Y, bg.Width, bg.Height);

                using var wBr = new SolidBrush(hl ? Color.DarkRed : Color.FromArgb(55, 55, 80));
                g.DrawString(lbl, wf, wBr, midX - sz.Width / 2, midY - sz.Height / 2);
            }
        }

        // ── Nodos ────────────────────────────────────────────────────────
        private void PaintNodes(Graphics g)
        {
            foreach (var s in _graph.Stations)
            {
                var pos = GetPx(s.Id);
                bool hl = _highlightedPath.Contains(s);
                bool hov = s == _hoveredStation;
                bool isOri = _highlightedPath.Count > 0 && _highlightedPath[0] == s;
                bool isDst = _highlightedPath.Count > 0 && _highlightedPath[^1] == s;

                // Sombra
                using var shadow = new SolidBrush(Color.FromArgb(50, 0, 0, 0));
                g.FillEllipse(shadow, pos.X - R + 2, pos.Y - R + 3, R * 2, R * 2);

                // Relleno del nodo según estado
                Color fill = isOri ? Color.FromArgb(0, 155, 72)
                           : isDst ? Color.FromArgb(195, 40, 40)
                           : hl ? Color.FromArgb(220, 120, 20)
                           : hov ? Color.FromArgb(55, 135, 215)
                           : Color.FromArgb(30, 90, 160);

                using var fb = new SolidBrush(fill);
                g.FillEllipse(fb, pos.X - R, pos.Y - R, R * 2, R * 2);

                // Borde blanco
                using var bp = new Pen(Color.White, hl || hov ? 3f : 2f);
                g.DrawEllipse(bp, pos.X - R, pos.Y - R, R * 2, R * 2);

                // Número dentro del círculo
                using var nf = new Font("Segoe UI", 8f, FontStyle.Bold);
                var nSz = g.MeasureString(s.Id.ToString(), nf);
                using var nb = new SolidBrush(Color.White);
                g.DrawString(s.Id.ToString(), nf, nb,
                    pos.X - nSz.Width / 2, pos.Y - nSz.Height / 2);

                // Etiqueta con nombre
                PaintLabel(g, s, pos, hl, hov);
            }
        }

        private void PaintLabel(Graphics g, Estacion s, PointF pos, bool hl, bool hov)
        {
            var rel = _relPos[s.Id];

            // Desplazamiento de etiqueta según zona del mapa
            float ox = 0, oy = R + 5;
            if (rel.Y < 0.14f) oy = -(R + 18);
            else if (rel.X < 0.17f) { ox = R + 5; oy = -6; }
            else if (rel.X > 0.83f) { ox = -(R + 5); oy = -6; }

            using var font = new Font("Segoe UI", 7.5f, hl || hov ? FontStyle.Bold : FontStyle.Regular);
            var sz = g.MeasureString(s.Name, font);

            float lx = pos.X + ox - sz.Width / 2;
            float ly = pos.Y + oy;
            if (rel.X > 0.83f) lx = pos.X + ox - sz.Width;
            if (rel.X < 0.17f) lx = pos.X + ox;

            // Fondo semitransparente para legibilidad
            using var bgBr = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
            g.FillRectangle(bgBr, lx - 2, ly - 1, sz.Width + 4, sz.Height + 2);

            using var lb = new SolidBrush(
                hl ? Color.DarkRed
              : hov ? Color.FromArgb(20, 80, 160)
              : Color.FromArgb(25, 25, 50));
            g.DrawString(s.Name, font, lb, lx, ly);
        }

        // ── Leyenda (sólo cuando hay ruta resaltada) ─────────────────────
        private void PaintLegend(Graphics g)
        {
            if (_highlightedPath.Count == 0) return;

            var items = new[]
            {
                (Color.FromArgb(0,   155, 72),  "Estación de origen"),
                (Color.FromArgb(220, 120, 20),  "Parada intermedia"),
                (Color.FromArgb(195, 40,  40),  "Estación de destino"),
            };

            int x = 14, y = Height - 82;
            using var bgBr = new SolidBrush(Color.FromArgb(205, 255, 255, 255));
            g.FillRectangle(bgBr, x - 5, y - 5, 210, items.Length * 23 + 10);

            using var font = new Font("Segoe UI", 8.5f);
            foreach (var (color, label) in items)
            {
                using var br = new SolidBrush(color);
                g.FillEllipse(br, x, y + 4, 13, 13);
                using var lb = new SolidBrush(Color.FromArgb(35, 35, 35));
                g.DrawString(label, font, lb, x + 20, y + 1);
                y += 23;
            }
        }

        // ── Utilidad: ¿esta arista está en el camino resaltado? ──────────
        private bool IsEdgeInPath(Estacion a, Estacion b)
        {
            for (int i = 0; i < _highlightedPath.Count - 1; i++)
                if ((_highlightedPath[i].Equals(a) && _highlightedPath[i + 1].Equals(b)) ||
                    (_highlightedPath[i].Equals(b) && _highlightedPath[i + 1].Equals(a)))
                    return true;
            return false;
        }
    }
}
