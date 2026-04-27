using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompilerScanner;

namespace LexicalAnalysis_ph1_logic
{
    public class GUI : Form
    {
        #region ── Color Palette ──────────────────────────────────────────────
        static readonly Color C_BG = Color.FromArgb(13, 13, 23);
        static readonly Color C_SURFACE = Color.FromArgb(20, 20, 35);
        static readonly Color C_CARD = Color.FromArgb(24, 24, 40);
        static readonly Color C_BORDER = Color.FromArgb(42, 42, 68);
        static readonly Color C_ACCENT = Color.FromArgb(82, 193, 255);
        static readonly Color C_TEXT = Color.FromArgb(220, 220, 240);
        static readonly Color C_MUTED = Color.FromArgb(110, 110, 145);
        static readonly Color C_VAR = Color.FromArgb(82, 193, 255);  // cyan-blue
        static readonly Color C_IDENT = Color.FromArgb(115, 215, 140);  // green
        static readonly Color C_RESERVED = Color.FromArgb(165, 130, 255);  // violet
        static readonly Color C_NUM = Color.FromArgb(245, 240, 70);   // yellow
        static readonly Color C_OP = Color.FromArgb(255, 175, 75);   // amber
        static readonly Color C_SYM = Color.FromArgb(75, 100, 245);   // blue
        static readonly Color C_UNKNOWN = Color.FromArgb(255, 95, 95);   // red
        #endregion

        RichTextBox _inputBox;
        DataGridView _grid;
        Label _statusLabel;
        Label _tokenCountLabel;

        public GUI()
        {

            BuildUI();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  UI CONSTRUCTION
        // ═══════════════════════════════════════════════════════════════════
        public void BuildUI()
        {
            Text = "Compiler Scanner  —  Phase 1: Lexical Analysis";
            Size = new Size(980, 740);
            MinimumSize = new Size(780, 580);
            BackColor = C_BG;
            Font = new Font("Consolas", 9.5f);
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;

            // ── HEADER ─────────────────────────────────────────────────────
            var header = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = C_SURFACE };
            header.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 3, BackColor = C_ACCENT });
            header.Controls.Add(new Label
            {
                Text = "◈  COMPILER SCANNER",
                Font = new Font("Consolas", 17f, FontStyle.Bold),
                ForeColor = C_ACCENT,
                AutoSize = true,
                Location = new Point(22, 16)
            });
            header.Controls.Add(new Label
            {
                Text = "Compiler Design Project  •  Spring 2026  •  Phase 1 — Lexical Analysis",
                Font = new Font("Consolas", 8f),
                ForeColor = C_MUTED,
                AutoSize = true,
                Location = new Point(26, 50)
            });

            // ── STATUS BAR ─────────────────────────────────────────────────
            var statusBar = new Panel { Dock = DockStyle.Bottom, Height = 26, BackColor = Color.FromArgb(8, 8, 18) };
            _statusLabel = new Label
            {
                Text = "●  Ready",
                ForeColor = C_IDENT,
                Font = new Font("Consolas", 8.5f),
                AutoSize = true,
                Location = new Point(14, 6)
            };
            _tokenCountLabel = new Label
            {
                Text = "Tokens: 0",
                ForeColor = C_MUTED,
                Font = new Font("Consolas", 8.5f),
                AutoSize = true
            };
            statusBar.Controls.Add(_statusLabel);
            statusBar.Controls.Add(_tokenCountLabel);
            statusBar.Resize += (_, __) =>
                _tokenCountLabel.Location = new Point(statusBar.Width - _tokenCountLabel.Width - 14, 6);

            // ── MAIN TABLE LAYOUT ───────────────────────────────────────────
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(18, 14, 18, 10),
                BackColor = C_BG
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 175)); // input card
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));  // legend strip
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // results card

            // ── INPUT CARD ─────────────────────────────────────────────────
            var inputCard = MakeCard();
            inputCard.Dock = DockStyle.Fill;
            inputCard.Controls.Add(MakeSectionLabel("▸  INPUT SENTENCE", new Point(16, 14)));

            _inputBox = new RichTextBox
            {
                Location = new Point(16, 40),
                BackColor = Color.FromArgb(10, 10, 20),
                ForeColor = C_TEXT,
                Font = new Font("Consolas", 11f),
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                WordWrap = true,
            };
            inputCard.Controls.Add(_inputBox);

            // Placeholder text behaviour
            SetPlaceholder(_inputBox, "// Enter your code or expression here...");

            var btnScan = MakeButton("⚡  SCAN", Color.FromArgb(30, 100, 60), Color.FromArgb(45, 145, 88), Color.FromArgb(55, 170, 100));
            var btnClear = MakeButton("✕  CLEAR", Color.FromArgb(65, 18, 18), Color.FromArgb(105, 38, 38), Color.FromArgb(130, 55, 55));
            btnClear.ForeColor = Color.FromArgb(220, 95, 95);
            btnScan.Click += OnScan;
            btnClear.Click += OnClear;

            var btnRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(14, 5, 0, 0),
                WrapContents = false
            };
            btnRow.Controls.Add(btnScan);
            btnRow.Controls.Add(btnClear);
            inputCard.Controls.Add(btnRow);

            inputCard.Resize += (_, __) =>
                _inputBox.Size = new Size(inputCard.Width - 32, inputCard.Height - 92);

            layout.Controls.Add(inputCard, 0, 0);

            // ── LEGEND STRIP ────────────────────────────────────────────────
            var legend = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            int lx = 4;

            // FIX: replaced tuple array (requires C# 7.3+ ValueTuple but the foreach
            // deconstruction syntax with named fields needs 8.0) — use explicit structs instead
            var legendItems = new[]
            {
                new LegendItem("VARIABLE",   C_VAR),
                new LegendItem("IDENTIFIER", C_IDENT),
                new LegendItem("RESERVED",   C_RESERVED),
                new LegendItem("NUMBER", C_NUM),
                new LegendItem("OPERATOR",   C_OP),
                new LegendItem("SYMBOL", C_SYM),
                new LegendItem("UNKNOWN",    C_UNKNOWN)
            };

            foreach (var item in legendItems)
            {
                legend.Controls.Add(new Panel { Size = new Size(9, 9), BackColor = item.Color, Location = new Point(lx, 15) });
                var lbl = new Label { Text = item.Name, ForeColor = C_MUTED, Font = new Font("Consolas", 7.8f), AutoSize = true, Location = new Point(lx + 13, 11) };
                legend.Controls.Add(lbl);
                lx += lbl.PreferredWidth + 36;
            }
            layout.Controls.Add(legend, 0, 1);

            // ── RESULTS CARD ────────────────────────────────────────────────
            var resultsCard = MakeCard();
            resultsCard.Dock = DockStyle.Fill;
            resultsCard.Controls.Add(MakeSectionLabel("▸  TOKEN STREAM", new Point(16, 14)));

            _grid = new DataGridView
            {
                Location = new Point(16, 42),
                BackgroundColor = Color.FromArgb(10, 10, 20),
                BorderStyle = BorderStyle.None,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(32, 32, 55),
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                Font = new Font("Consolas", 9.5f),
                ColumnHeadersHeight = 34,
                RowTemplate = { Height = 28 },
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                //Cursor = Cursors.Default
            };
            ApplyGridStyle();
            BuildGridColumns();

            resultsCard.Controls.Add(_grid);
            resultsCard.Resize += (_, __) =>
                _grid.Size = new Size(resultsCard.Width - 32, resultsCard.Height - 52);

            layout.Controls.Add(resultsCard, 0, 2);

            // ── ASSEMBLE ────────────────────────────────────────────────────
            Controls.Add(layout);
            Controls.Add(statusBar);
            Controls.Add(header);
        }


        // ═══════════════════════════════════════════════════════════════════
        //  HELPER BUILDERS
        // ═══════════════════════════════════════════════════════════════════
        Panel MakeCard()
        {
            var p = new Panel { BackColor = C_CARD };

            // FIX: replaced "using var" (C# 8.0) with "using ( ) { }" block (C# 7.3)
            p.Paint += (s, e) =>
            {
                using (var pen = new Pen(C_BORDER, 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, ((Panel)s).Width - 1, ((Panel)s).Height - 1);
                }
            };
            return p;
        }

        Label MakeSectionLabel(string text, Point loc) => new Label
        {
            Text = text,
            Font = new Font("Consolas", 9f, FontStyle.Bold),
            ForeColor = C_ACCENT,
            AutoSize = true,
            Location = loc
        };

        Button MakeButton(string text, Color bg, Color bgHover, Color border)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Consolas", 9.5f, FontStyle.Bold),
                BackColor = bg,
                ForeColor = C_TEXT,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(118, 30),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 10, 0),
                UseVisualStyleBackColor = false
            };
            btn.FlatAppearance.BorderColor = border;
            btn.FlatAppearance.BorderSize = 1;
            btn.MouseEnter += (_, __) => btn.BackColor = bgHover;
            btn.MouseLeave += (_, __) => btn.BackColor = bg;
            return btn;
        }

        void SetPlaceholder(RichTextBox box, string placeholder)
        {
            box.Text = placeholder;
            box.ForeColor = C_MUTED;
            box.GotFocus += (_, __) =>
            {
                if (box.Text == placeholder) { box.Text = ""; box.ForeColor = C_TEXT; }
            };
            box.LostFocus += (_, __) =>
            {
                if (string.IsNullOrWhiteSpace(box.Text)) { box.Text = placeholder; box.ForeColor = C_MUTED; }
            };
        }

        void ApplyGridStyle()
        {
            _grid.DefaultCellStyle.BackColor = Color.FromArgb(10, 10, 20);
            _grid.DefaultCellStyle.ForeColor = C_TEXT;
            _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(35, 58, 90);
            _grid.DefaultCellStyle.SelectionForeColor = Color.White;
            _grid.DefaultCellStyle.Padding = new Padding(7, 2, 7, 2);

            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(16, 16, 30);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = C_ACCENT;
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Consolas", 8.8f, FontStyle.Bold);
            _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(16, 16, 30);
            _grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(9, 0, 0, 0);

            _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(14, 14, 25);
        }

        void BuildGridColumns()
        {
            _grid.Columns.Clear();
            AddCol("#", 52, DataGridViewAutoSizeColumnMode.None);
            AddCol("LEXEME / TOKEN", 0, DataGridViewAutoSizeColumnMode.Fill);
            AddCol("TOKEN TYPE", 165, DataGridViewAutoSizeColumnMode.None);
        }

        void AddCol(string header, int width, DataGridViewAutoSizeColumnMode mode)
        {
            var col = new DataGridViewTextBoxColumn { HeaderText = header, AutoSizeMode = mode };
            if (width > 0) col.Width = width;
            _grid.Columns.Add(col);
        }

        // ═══════════════════════════════════════════════════════════════════
        //  EVENT HANDLERS
        // ═══════════════════════════════════════════════════════════════════
        void OnScan(object sender, EventArgs e)
        {
            var input = _inputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(input) || _inputBox.ForeColor == C_MUTED)
            {
                SetStatus("⚠  No input provided — type something first", C_OP);
                return;
            }

            SetStatus("⟳  Scanning...", C_VAR);
            _grid.Rows.Clear();

            Scanner s = new Scanner();
            List<Token> tokens = s.Scan(input);

            int rowNum = 1;
            foreach (var tok in tokens)
            {
                int idx = _grid.Rows.Add(rowNum++, tok.Value, tok.Type);
                var color = TypeColor(tok.Type);
                var row = _grid.Rows[idx];

                //row.Cells[0].Style.ForeColor = color;
                row.Cells[1].Style.ForeColor = color;
                row.Cells[2].Style.ForeColor = color;
            }

            int count = _grid.Rows.Count;
            _tokenCountLabel.Text = string.Format("Tokens: {0}", count);
            _tokenCountLabel.Location = new Point(
                _tokenCountLabel.Parent.Width - _tokenCountLabel.Width - 14, 6);

            SetStatus(string.Format("✔  Scan complete  —  {0} token{1} found", count, count != 1 ? "s" : ""), C_IDENT);
        }

        void OnClear(object sender, EventArgs e)
        {
            _inputBox.Text = "// Enter your code or expression here...";
            _inputBox.ForeColor = C_MUTED;
            _grid.Rows.Clear();
            _tokenCountLabel.Text = "Tokens: 0";
            SetStatus("●  Ready", C_IDENT);
        }

        void SetStatus(string msg, Color color)
        {
            _statusLabel.Text = msg;
            _statusLabel.ForeColor = color;
        }

        // FIX: replaced switch expression (C# 8.0) with traditional switch statement (C# 7.3)
        Color TypeColor(string cat)
        {
            switch (cat)
            {
                case "IDENTIFIER": return C_IDENT;
                case "OPERATOR": return C_OP;
                case "KEYWORD": return C_RESERVED;
                case "VARIABLE": return C_VAR;
                case "NUMBER": return C_NUM;
                case "SYMBOL": return C_SYM;
                default: return C_UNKNOWN;
            }       
        }
        internal struct LegendItem
        {
            public string Name { get; }
            public Color Color { get; }
            public LegendItem(string name, Color color) { Name = name; Color = color; }
        }
        List<Token> Tokenize(string input)
        {
            Scanner s = new Scanner();
            return s.Scan(input);
        }
    }
}