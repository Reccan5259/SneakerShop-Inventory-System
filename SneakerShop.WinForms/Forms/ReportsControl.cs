using System.Drawing;
using System.Text;
using SneakerShop.WinForms.Models;
using SneakerShop.WinForms.Services;
using SneakerShop.WinForms.Styles;

namespace SneakerShop.WinForms.Forms
{
    public class ReportsControl : UserControl
    {
        private readonly FlowLayoutPanel _summaryPanel;
        private readonly DataGridView _restockGrid;
        private readonly Label _statusLabel;
        private readonly Label _lastUpdatedLabel;
        private readonly Button _refreshButton;
        private readonly Button _exportButton;

        private List<RestockSuggestion> _restockSuggestions = new();

        public ReportsControl()
        {
            Dock = DockStyle.Fill;
            BackColor = AppTheme.PageBackground;
            Padding = new Padding(24);

            Panel headerPanel = new()
            {
                Dock = DockStyle.Top,
                Height = 82,
                BackColor = AppTheme.CardBackground,
                Padding = new Padding(20, 12, 20, 12)
            };

            Label titleLabel = new()
            {
                Text = "Reports & Analytics",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = AppTheme.DarkRed,
                AutoSize = true,
                Location = new Point(20, 10)
            };

            Label descriptionLabel = new()
            {
                Text = "Business performance, sales insights and restock recommendations.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = AppTheme.TextMuted,
                AutoSize = true,
                Location = new Point(22, 48)
            };

            _refreshButton = new Button
            {
                Text = "Refresh",
                Width = 110,
                Height = 38,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(760, 21)
            };
            AppTheme.StyleSecondaryButton(_refreshButton);
            _refreshButton.Click += async (_, _) => await LoadReportsAsync();

            _exportButton = new Button
            {
                Text = "Export CSV",
                Width = 120,
                Height = 38,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(880, 21)
            };
            AppTheme.StylePrimaryButton(_exportButton);
            _exportButton.Click += ExportButton_Click;

            headerPanel.Resize += (_, _) =>
            {
                _exportButton.Left =
                    headerPanel.ClientSize.Width -
                    _exportButton.Width - 20;

                _refreshButton.Left =
                    _exportButton.Left -
                    _refreshButton.Width - 10;
            };

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(descriptionLabel);
            headerPanel.Controls.Add(_refreshButton);
            headerPanel.Controls.Add(_exportButton);

            Panel statusPanel = new()
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = AppTheme.PageBackground
            };

            _statusLabel = new Label
            {
                Text = "Loading reports...",
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = AppTheme.TextMuted,
                Location = new Point(4, 15)
            };

            _lastUpdatedLabel = new Label
            {
                Text = string.Empty,
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = AppTheme.TextMuted,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            statusPanel.Resize += (_, _) =>
            {
                _lastUpdatedLabel.Left =
                    statusPanel.ClientSize.Width -
                    _lastUpdatedLabel.Width - 5;

                _lastUpdatedLabel.Top = 15;
            };

            statusPanel.Controls.Add(_statusLabel);
            statusPanel.Controls.Add(_lastUpdatedLabel);

            _summaryPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 350,
                AutoScroll = true,
                WrapContents = true,
                BackColor = AppTheme.PageBackground,
                Padding = new Padding(0, 10, 0, 10)
            };

            Panel tableHeaderPanel = new()
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = AppTheme.CardBackground,
                Padding = new Padding(16, 10, 16, 10)
            };

            Label tableTitleLabel = new()
            {
                Text = "Smart Restock Recommendations",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = AppTheme.TextDark,
                AutoSize = true,
                Location = new Point(16, 15)
            };

            tableHeaderPanel.Controls.Add(tableTitleLabel);

            _restockGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoGenerateColumns = false,
                MultiSelect = false,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                BackgroundColor = Color.White
            };

            AppTheme.StyleDataGridView(_restockGrid);
            ConfigureRestockGrid();

            Controls.Add(_restockGrid);
            Controls.Add(tableHeaderPanel);
            Controls.Add(_summaryPanel);
            Controls.Add(statusPanel);
            Controls.Add(headerPanel);

            Load += async (_, _) => await LoadReportsAsync();
        }

        private void ConfigureRestockGrid()
        {
            _restockGrid.Columns.Clear();

            AddColumn("Code", "Code", 80);
            AddColumn("Sneaker", "Name", 140);
            AddColumn("Brand", "Brand", 90);
            AddColumn("Colorway", "Colorway", 100);
            AddColumn("Size", "Size", 60);
            AddColumn("Current Stock", "CurrentQuantity", 80);
            AddColumn("Reorder Level", "ReorderLevel", 80);
            AddColumn(
                "30-Day Sales",
                "UnitsSoldLast30Days",
                80);
            AddColumn("Target Stock", "TargetStock", 80);
            AddColumn(
                "Suggested Order",
                "SuggestedOrderQuantity",
                95);
            AddColumn("Urgency", "Urgency", 75);

            _restockGrid.CellFormatting +=
                RestockGrid_CellFormatting;
        }

        private void AddColumn(
            string header,
            string property,
            float fillWeight)
        {
            _restockGrid.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    HeaderText = header,
                    DataPropertyName = property,
                    FillWeight = fillWeight,
                    AutoSizeMode =
                        DataGridViewAutoSizeColumnMode.Fill
                });
        }

        private async Task LoadReportsAsync()
        {
            SetLoading(true);

            try
            {
                Task<DashboardSummary> summaryTask =
                    ApiService.Instance.GetDashboardAsync();

                Task<List<RestockSuggestion>> restockTask =
                    ApiService.Instance
                        .GetRestockSuggestionsAsync();

                await Task.WhenAll(summaryTask, restockTask);

                DashboardSummary summary =
                    await summaryTask;

                _restockSuggestions =
                    (await restockTask)
                    .OrderBy(GetUrgencyOrder)
                    .ThenByDescending(
                        item => item.SuggestedOrderQuantity)
                    .ToList();

                DisplaySummaryCards(summary);

                _restockGrid.DataSource = null;
                _restockGrid.DataSource =
                    _restockSuggestions;

                _statusLabel.Text =
                    $"{_restockSuggestions.Count} " +
                    "restock recommendation(s) found.";

                _statusLabel.ForeColor =
                    AppTheme.Success;

                _lastUpdatedLabel.Text =
                    $"Last updated: " +
                    $"{DateTime.Now:MMM dd, yyyy hh:mm tt}";
            }
            catch (Exception ex)
            {
                _statusLabel.Text =
                    $"Unable to load reports: {ex.Message}";

                _statusLabel.ForeColor =
                    AppTheme.Danger;
            }
            finally
            {
                SetLoading(false);
            }
        }

        private void DisplaySummaryCards(
            DashboardSummary summary)
        {
            _summaryPanel.Controls.Clear();

            AddCard(
                "Gross Sales",
                summary.GrossSales.ToString("C"),
                "Total revenue from customer orders",
                AppTheme.Success);

            AddCard(
                "Inventory Value",
                summary.InventoryValue.ToString("C"),
                "Value of all available sneaker stock",
                AppTheme.PrimaryRed);

            AddCard(
                "Total Orders",
                summary.TotalOrders.ToString(),
                $"{summary.CancelledOrders} cancelled order(s)",
                AppTheme.DarkRed);

            AddCard(
                "Net Units Sold",
                summary.NetUnitsSold.ToString(),
                "Units sold after returns",
                AppTheme.Success);

            AddCard(
                "Best-Selling Brand",
                string.IsNullOrWhiteSpace(
                    summary.BestSellingBrand)
                    ? "No Data"
                    : summary.BestSellingBrand,
                $"{summary.BestSellingBrandUnits} unit(s) sold",
                AppTheme.PrimaryRed);

            AddCard(
                "Best-Selling Size",
                summary.BestSellingSize.HasValue
                    ? summary.BestSellingSize.Value.ToString(
                        "0.##")
                    : "No Data",
                $"{summary.BestSellingSizeUnits} unit(s) sold",
                AppTheme.PrimaryRed);

            AddCard(
                "Low Stock",
                summary.LowStockCount.ToString(),
                $"{summary.OutOfStockCount} out of stock",
                summary.LowStockCount > 0
                    ? AppTheme.Warning
                    : AppTheme.Success);

            AddCard(
                "Authenticity",
                summary.VerifiedCount.ToString(),
                $"{summary.PendingAuthenticity} pending check(s)",
                summary.PendingAuthenticity > 0
                    ? AppTheme.Warning
                    : AppTheme.Success);

            AddCard(
                "Product Models",
                summary.TotalModels.ToString(),
                $"{summary.TotalProducts} product variation(s)",
                AppTheme.DarkRed);

            AddCard(
                "Registered Users",
                summary.RegisteredUsers.ToString(),
                "Authorized system accounts",
                AppTheme.DarkRed);
        }

        private void AddCard(
            string title,
            string value,
            string description,
            Color accentColor)
        {
            Panel card = new()
            {
                Width = 245,
                Height = 135,
                BackColor = AppTheme.CardBackground,
                Margin = new Padding(0, 0, 14, 14)
            };

            Panel accentPanel = new()
            {
                Dock = DockStyle.Left,
                Width = 6,
                BackColor = accentColor
            };

            Label titleLabel = new()
            {
                Text = title.ToUpperInvariant(),
                Font = new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold),
                ForeColor = AppTheme.TextMuted,
                AutoSize = true,
                Location = new Point(20, 15)
            };

            Label valueLabel = new()
            {
                Text = value,
                Font = new Font(
                    "Segoe UI",
                    21,
                    FontStyle.Bold),
                ForeColor = accentColor,
                AutoSize = true,
                Location = new Point(18, 42)
            };

            Label descriptionLabel = new()
            {
                Text = description,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = AppTheme.TextMuted,
                AutoEllipsis = true,
                Width = 210,
                Height = 35,
                Location = new Point(20, 95)
            };

            card.Controls.Add(descriptionLabel);
            card.Controls.Add(valueLabel);
            card.Controls.Add(titleLabel);
            card.Controls.Add(accentPanel);

            _summaryPanel.Controls.Add(card);
        }

        private static int GetUrgencyOrder(
            RestockSuggestion item)
        {
            return item.Urgency.ToLowerInvariant() switch
            {
                "critical" => 0,
                "high" => 1,
                "medium" => 2,
                "low" => 3,
                _ => 4
            };
        }

        private void RestockGrid_CellFormatting(
            object? sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (_restockGrid.Columns[e.ColumnIndex]
                    .DataPropertyName != "Urgency" ||
                e.Value == null)
            {
                return;
            }

            string urgency =
                e.Value.ToString()?.ToLowerInvariant() ??
                string.Empty;

            e.CellStyle.Font = new Font(
                _restockGrid.Font,
                FontStyle.Bold);

            switch (urgency)
            {
                case "critical":
                    e.CellStyle.ForeColor =
                        AppTheme.Danger;
                    break;

                case "high":
                    e.CellStyle.ForeColor =
                        AppTheme.PrimaryRed;
                    break;

                case "medium":
                    e.CellStyle.ForeColor =
                        AppTheme.Warning;
                    break;

                default:
                    e.CellStyle.ForeColor =
                        AppTheme.Success;
                    break;
            }
        }

        private void ExportButton_Click(
            object? sender,
            EventArgs e)
        {
            if (_restockSuggestions.Count == 0)
            {
                MessageBox.Show(
                    "There are no restock suggestions to export.",
                    "Nothing to Export",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using SaveFileDialog dialog = new()
            {
                Filter = "CSV file (*.csv)|*.csv",
                FileName =
                    $"RestockReport_" +
                    $"{DateTime.Now:yyyyMMdd_HHmm}.csv",
                Title = "Export Restock Report"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            try
            {
                StringBuilder csv = new();

                csv.AppendLine(
                    "Code,Name,Brand,Colorway,Size," +
                    "Current Stock,Reorder Level," +
                    "30-Day Sales,Target Stock," +
                    "Suggested Order,Urgency");

                foreach (
                    RestockSuggestion item
                    in _restockSuggestions)
                {
                    csv.AppendLine(string.Join(",",
                        EscapeCsv(item.Code),
                        EscapeCsv(item.Name),
                        EscapeCsv(item.Brand),
                        EscapeCsv(item.Colorway),
                        item.Size,
                        item.CurrentQuantity,
                        item.ReorderLevel,
                        item.UnitsSoldLast30Days,
                        item.TargetStock,
                        item.SuggestedOrderQuantity,
                        EscapeCsv(item.Urgency)));
                }

                File.WriteAllText(
                    dialog.FileName,
                    csv.ToString(),
                    Encoding.UTF8);

                MessageBox.Show(
                    "The report was exported successfully.",
                    "Export Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Export Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static string EscapeCsv(string value)
        {
            string escaped =
                value.Replace("\"", "\"\"");

            return $"\"{escaped}\"";
        }

        private void SetLoading(bool loading)
        {
            _refreshButton.Enabled = !loading;
            _exportButton.Enabled = !loading;
            UseWaitCursor = loading;

            if (loading)
            {
                _statusLabel.Text =
                    "Loading reports and analytics...";

                _statusLabel.ForeColor =
                    AppTheme.TextMuted;
            }
        }
    }
}