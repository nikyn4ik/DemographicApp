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
            LoadRegions();
            InitializeReportsFolder();
            SetNextReportNumberAsync();
        }

        private async void LoadRegions()
        {
            try
            {
                var regions = await _context.Regions.ToListAsync();
                ParentRegionPicker.ItemsSource = regions;
                ChildRegionPicker.ItemsSource = regions;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка загрузки регионов: {ex.Message}", "OK");
            }
        }

        private void InitializeReportsFolder()
        {
            try
            {
                string projectRoot = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string reportsFolder = Path.Combine(projectRoot, "Documentation", "Reports");
                if (!Directory.Exists(reportsFolder))
                {
                    Directory.CreateDirectory(reportsFolder);
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Ошибка", $"Ошибка инициализации папки отчетов: {ex.Message}", "OK");
            }
        }

        private async Task SetNextReportNumberAsync()
        {
            try
            {
                var reports = await _context.Reports.ToListAsync();
                var latestReport = reports.OrderByDescending(r => r.ReportId).FirstOrDefault();
                _nextReportNumber = latestReport != null ? latestReport.ReportId + 1 : 1;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка установки номера следующего отчета: {ex.Message}", "OK");
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
                var parentDemographicData = await _context.DemographicData
                    .Where(d => d.RegionId == parentRegion.Id)
                    .OrderByDescending(d => d.Date)
                    .FirstOrDefaultAsync();

                var childDemographicData = await _context.DemographicData
                    .Where(d => d.RegionId == childRegion.Id)
                    .OrderByDescending(d => d.Date)
                    .FirstOrDefaultAsync();

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

        private string[] CompareAndFormatDemographicData(string[] parentDataLines, string[] childDataLines)
        {
            if (parentDataLines.Length != childDataLines.Length)
            {
                throw new ArgumentException("Невозможно сравнить данные: количество строк различно.");
            }

            var comparisonResult = new string[parentDataLines.Length];

            for (int i = 0; i < parentDataLines.Length; i++)
            {
                var parentLine = parentDataLines[i].Split(':');
                var childLine = childDataLines[i].Split(':');

                var parentValue = double.Parse(parentLine[1].Trim());
                var childValue = double.Parse(childLine[1].Trim());

                var difference = parentValue - childValue;
                comparisonResult[i] = $"{parentLine[0]}: {difference}";
            }

            return comparisonResult;
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

        private async Task GeneratePdfReportAsync(Database.Models.Region parentRegion, Database.Models.Region childRegion, string[] parentDataLines, string[] childDataLines, string[] comparisonResult, string fileName)
        {
            try
            {
                string projectRoot = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string pdfPath = Path.Combine(projectRoot, "Documentation", "Reports", fileName);

                await Task.Run(() =>
                {
                    using (var stream = new FileStream(pdfPath, FileMode.Create))
                    {
                        var document = new Document(PageSize.A4);
                        PdfWriter.GetInstance(document, stream);
                        document.Open();

                        // Загрузка шрифтов
                        string fontPath = "C:\\Windows\\Fonts\\arial.ttf";
                        var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                        var titleFont = new iTextSharp.text.Font(baseFont, 16, iTextSharp.text.Font.BOLD);
                        var regularFont = new iTextSharp.text.Font(baseFont, 12, iTextSharp.text.Font.NORMAL);

                        // Добавление данных каждого региона с заголовком
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

                        Paragraph comparisonHeader = new Paragraph($"Разница между {parentRegion.Name} и {childRegion.Name}:\n\n", titleFont)
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

                await DisplayAlert("Отчет создан", $"Отчет сохранен в папке Documentation/Reports под именем {fileName}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка при генерации PDF отчета: {ex.Message}", "OK");
            }
        }
    }
}
