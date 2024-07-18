using Database;
using Database.Models;
using iText.Kernel.Pdf;
using iText.Kernel.Font;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using Style = iText.Layout.Style;
using System.Diagnostics;
using Cell = iText.Layout.Element.Cell;
using TextAlignment = iText.Layout.Properties.TextAlignment;

namespace DemographicApp.Pages
{
    public partial class Compare : ContentPage
    {
        private readonly ApplicationContext _context;
        private int _nextReportNumber = 1;
        private readonly object _lock = new object();
        private readonly string _reportsFolder;

        public Compare()
        {
            InitializeComponent();
            _context = new ApplicationContext();
            _reportsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Documentation", "Reports");
            EnsureReportsFolderExists();
            SetNextReportNumber();
            LoadRegions();
        }

        private void EnsureReportsFolderExists()
        {
            try
            {
                if (!Directory.Exists(_reportsFolder))
                {
                    Directory.CreateDirectory(_reportsFolder);
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Ошибка", $"Ошибка при создании папки для отчетов: {ex.Message}", "OK");
            }
        }

        private void LoadRegions()
        {
            try
            {
                var regions = _context.Regions.ToList();
                ParentRegionPicker.ItemsSource = regions;
                ChildRegionPicker.ItemsSource = regions;
            }
            catch (Exception ex)
            {
                DisplayAlert("Ошибка", $"Ошибка загрузки регионов: {ex.Message}", "OK");
            }
        }

        private void SetNextReportNumber()
        {
            try
            {
                var reports = _context.Reports.ToList();
                var latestReport = reports.OrderByDescending(r => r.ReportId).FirstOrDefault();
                _nextReportNumber = latestReport != null ? latestReport.ReportId + 1 : 1;
            }
            catch (Exception ex)
            {
                DisplayAlert("Ошибка", $"Ошибка установки номера следующего отчета: {ex.Message}", "OK");
            }
        }

        private async void OnCompareButtonClicked(object sender, EventArgs e)
        {
            var parentRegion = (Database.Models.Region)ParentRegionPicker.SelectedItem;
            var childRegion = (Database.Models.Region)ChildRegionPicker.SelectedItem;

            if (parentRegion == null || childRegion == null)
            {
                await DisplayAlert("Ошибка", "Пожалуйста, выберите оба региона (родительский и дочерний).", "OK");
                return;
            }

            try
            {
                var parentDemographicData = _context.DemographicData
                    .Where(d => d.RegionId == parentRegion.Id)
                    .OrderByDescending(d => d.Date)
                    .FirstOrDefault();

                var childDemographicData = _context.DemographicData
                    .Where(d => d.RegionId == childRegion.Id)
                    .OrderByDescending(d => d.Date)
                    .FirstOrDefault();

                if (parentDemographicData == null || childDemographicData == null)
                {
                    await DisplayAlert("Ошибка", "Демографические данные не найдены для выбранных регионов.", "OK");
                    return;
                }

                var parentDataLines = FormatDemographicData(parentRegion.Name, parentDemographicData);
                var childDataLines = FormatDemographicData(childRegion.Name, childDemographicData);
                var comparisonResult = CompareAndFormatDemographicData(parentDataLines, childDataLines);
                SaveComparisonResult(parentRegion.Id, childRegion.Id, comparisonResult);

                var reportFileName = $"Report_{_nextReportNumber}.pdf";
                await GeneratePdfReportAsync(parentRegion, childRegion, parentDataLines, childDataLines, comparisonResult, reportFileName);

                lock (_lock)
                {
                    _nextReportNumber++;
                }

                OpenPdf(reportFileName);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка при выполнении операции сравнения: {ex.Message}", "OK");
            }
        }

        private string[] FormatDemographicData(string regionName, DemographicData data)
        {
            return new string[]
            {
                $"Регион: {regionName}",
                $"Население: {data.Population}",
                $"Рождаемость: {data.BirthRate}",
                $"Смертность: {data.DeathRate}",
                $"Мужское население: {data.MalePopulation}",
                $"Женское население: {data.FemalePopulation}"
            };
        }

        private void SaveComparisonResult(int parentRegionId, int childRegionId, string[] comparisonResult)
        {
            try
            {
                var result = new Report
                {
                    ParentRegionId = parentRegionId,
                    ChildRegionId = childRegionId,
                    ReportDate = DateTime.Now,
                    ReportData = string.Join("\n", comparisonResult),
                    Title = $"Report_{_nextReportNumber}"
                };

                _context.Reports.Add(result);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                DisplayAlert("Ошибка", $"Ошибка сохранения сравнения: {ex.Message}", "OK");
            }
        }

        private string[] CompareAndFormatDemographicData(string[] parentDataLines, string[] childDataLines)
        {
            if (parentDataLines.Length != childDataLines.Length)
            {
                throw new ArgumentException("Невозможно сравнить данные: количество строк различно.");
            }

            var comparisonResult = new string[parentDataLines.Length];
            for (int i = 0; i < parentDataLines.Length; i++)
            {
                var parentLineParts = parentDataLines[i].Split(':');
                var childLineParts = childDataLines[i].Split(':');

                if (parentLineParts.Length < 2 || childLineParts.Length < 2)
                {
                    throw new FormatException($"Неверный формат данных в строке {i}: {parentDataLines[i]} или {childDataLines[i]}");
                }

                var parentValueStr = parentLineParts[1].Trim();
                var childValueStr = childLineParts[1].Trim();

                if (i == 0)
                {
                    comparisonResult[0] = $"Регионы: {parentValueStr} - {childValueStr}";
                    continue;
                }

                if (!double.TryParse(parentValueStr, out var parentValue) || !double.TryParse(childValueStr, out var childValue))
                {
                    throw new FormatException($"Неверный формат числового значения в строке {i}: {parentValueStr} или {childValueStr}");
                }

                var difference = parentValue - childValue;
                comparisonResult[i] = $"{parentLineParts[0]}: {difference}";
            }

            return comparisonResult;
        }
        private async Task GeneratePdfReportAsync(Database.Models.Region parentRegion, Database.Models.Region childRegion, string[] parentDataLines, string[] childDataLines, string[] comparisonResult, string fileName)
        {
            try
            {
                string pdfPath = Path.Combine(_reportsFolder, fileName);

                await Task.Run(() =>
                {
                    using (var writer = new PdfWriter(pdfPath))
                    {
                        using (var pdf = new PdfDocument(writer))
                        {
                            var document = new Document(pdf, iText.Kernel.Geom.PageSize.A4);
                            document.SetMargins(20, 20, 20, 20);

                            var font = PdfFontFactory.CreateFont("C:\\Windows\\Fonts\\arial.ttf");

                            var titleFont = new Style()
                                .SetFont(font)
                                .SetFontSize(16)
                                .SetBold();

                            var regularFont = new Style()
                                .SetFont(font)
                                .SetFontSize(12);

                            var reportTitle = new Paragraph("Сравнительный отчет по регионам")
                                .AddStyle(titleFont)
                                .SetTextAlignment(TextAlignment.CENTER);
                            document.Add(reportTitle);

                            var table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1 }))
                                .UseAllAvailableWidth()
                                .SetMarginTop(20)
                                .SetMarginBottom(20);

                            for (int i = 0; i < parentDataLines.Length; i++)
                            {
                                var parentLine = parentDataLines[i];
                                var childLine = childDataLines[i];

                                var parentLineParts = parentLine.Split(':');
                                var childLineParts = childLine.Split(':');

                                if (parentLineParts.Length < 2 || childLineParts.Length < 2)
                                {
                                    throw new FormatException($"Неверный формат данных в строке {i}: {parentLine} или {childLine}");
                                }

                                var parentLabel = parentLineParts[0].Trim();
                                var childLabel = childLineParts[0].Trim();

                                var parentValueStr = parentLineParts[1].Trim();
                                var childValueStr = childLineParts[1].Trim();

                                var parentParagraph = new Paragraph(parentLine)
                                    .AddStyle(regularFont);
                                var childParagraph = new Paragraph(childLine)
                                    .AddStyle(regularFont);

                                if (i == 0)
                                {
                                    parentParagraph = new Paragraph(parentLine)
                                        .AddStyle(titleFont);
                                    childParagraph = new Paragraph(childLine)
                                        .AddStyle(titleFont);
                                }

                                table.AddCell(new Cell().Add(parentParagraph));
                                table.AddCell(new Cell().Add(childParagraph));
                            }

                            document.Add(table);

                            var comparisonHeader = new Paragraph("Результаты сравнения:")
                                .AddStyle(titleFont)
                                .SetMarginTop(20)
                                .SetMarginBottom(10);
                            document.Add(comparisonHeader);

                            foreach (var line in comparisonResult)
                            {
                                var paragraph = new Paragraph(line).AddStyle(regularFont);
                                paragraph.SetMarginBottom(10);
                                document.Add(paragraph);
                            }

                            document.Close();
                        }
                    }
                });

                await DisplayAlert("Успех", $"PDF-отчет успешно сгенерирован и сохранен: {pdfPath}", "OK");

                OpenPdf(pdfPath);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка генерации PDF-отчета: {ex.Message}", "OK");
            }
        }

        private void OpenPdf(string pdfPath)
        {
            try
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Launcher.OpenAsync(new OpenFileRequest
                    {
                        File = new ReadOnlyFile(pdfPath)
                    });
                });
            }
            catch (Exception ex)
            {
                DisplayAlert("Ошибка", $"Ошибка при открытии PDF-отчета: {ex.Message}", "OK");
            }
            }
    }
}
