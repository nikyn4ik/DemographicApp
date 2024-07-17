using Database;
using Database.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;

namespace DemographicApp.Pages
{
    public partial class Compare : ContentPage
    {
        private readonly ApplicationContext _context;
        private int _nextReportNumber = 1;
        private readonly object _lock = new object();

        public Compare()
        {
            InitializeComponent();
            _context = new ApplicationContext();
            SetNextReportNumber();
            LoadRegions();
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


        private bool IsValidNumber(string valueStr)
        {
            return double.TryParse(valueStr, out _);
        }

        private async Task GeneratePdfReportAsync(Database.Models.Region parentRegion, Database.Models.Region childRegion, string[] parentDataLines, string[] childDataLines, string[] comparisonResult, string fileName)
        {
            try
            {
                string projectRoot = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string reportsFolder = Path.Combine(projectRoot, "Documentation", "Reports");
                if (!Directory.Exists(reportsFolder))
                {
                    Directory.CreateDirectory(reportsFolder);
                }

                string pdfPath = Path.Combine(reportsFolder, fileName);

                await Task.Run(() =>
                {
                using (var stream = new FileStream(pdfPath, FileMode.Create))
                {
                    var document = new Document(PageSize.A4);
                    PdfWriter.GetInstance(document, stream);
                    document.Open();

                    string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                    var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    var titleFont = new iTextSharp.text.Font(baseFont, 16, iTextSharp.text.Font.BOLD);
                    var regularFont = new iTextSharp.text.Font(baseFont, 12, iTextSharp.text.Font.NORMAL);

                    foreach (var line in parentDataLines)
                    {
                        Paragraph paragraph = new Paragraph(line, titleFont)
                        {
                            SpacingAfter = 10f
                        };
                            document.Add(paragraph);
                        }

                        document.NewPage();

                        foreach (var line in childDataLines)
                        {
                            Paragraph paragraph = new Paragraph(line, titleFont)
                            {
                                SpacingAfter = 10f
                            };
                            document.Add(paragraph);
                        }

                        Paragraph comparisonHeader = new Paragraph("Результаты сравнения:", titleFont)
                        {
                            SpacingBefore = 20f,
                            SpacingAfter = 10f
                        };
                        document.Add(comparisonHeader);

                        foreach (var line in comparisonResult)
                        {
                            Paragraph paragraph = new Paragraph(line, regularFont)
                            {
                                SpacingAfter = 10f
                            };
                            document.Add(paragraph);
                        }

                        document.Close();
                    }
                });

                await DisplayAlert("Успех", $"PDF-отчет успешно сгенерирован и сохранен: {pdfPath}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка генерации PDF-отчета: {ex.Message}", "OK");
            }
        }
    }
}