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
            InitializeReportsFolder();
            SetNextReportNumberAsync();
            LoadRegionsAsync();
        }

        private async Task LoadRegionsAsync()
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                    var regions = await context.Regions.ToListAsync();
                    ParentRegionPicker.ItemsSource = regions;
                    ChildRegionPicker.ItemsSource = regions;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("������", $"������ �������� ��������: {ex.Message}", "OK");
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
                DisplayAlert("������", $"������ ������������� ����� �������: {ex.Message}", "OK");
            }
        }

        private async Task SetNextReportNumberAsync()
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                    var reports = await context.Reports.ToListAsync();
                    var latestReport = reports.OrderByDescending(r => r.ReportId).FirstOrDefault();
                    _nextReportNumber = latestReport != null ? latestReport.ReportId + 1 : 1;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("������", $"������ ��������� ������ ���������� ������: {ex.Message}", "OK");
            }
        }

        private async void OnCompareButtonClicked(object sender, EventArgs e)
        {
            var parentRegion = (Database.Models.Region)ParentRegionPicker.SelectedItem;
            var childRegion = (Database.Models.Region)ChildRegionPicker.SelectedItem;

            if (parentRegion == null || childRegion == null)
            {
                await DisplayAlert("������", "����������, �������� ��� ������� (������������ � ��������).", "OK");
                return;
            }

            try
            {
                using (var context = new ApplicationContext()) // Use a new instance for this operation
                {
                    var parentDemographicData = await context.DemographicData
                        .Where(d => d.RegionId == parentRegion.Id)
                        .OrderByDescending(d => d.Date)
                        .FirstOrDefaultAsync();

                    var childDemographicData = await context.DemographicData
                        .Where(d => d.RegionId == childRegion.Id)
                        .OrderByDescending(d => d.Date)
                        .FirstOrDefaultAsync();

                    if (parentDemographicData == null || childDemographicData == null)
                    {
                        await DisplayAlert("������", "��������������� ������ �� ������� ��� ��������� ��������.", "OK");
                        return;
                    }

                    var parentDataLines = FormatDemographicData(parentRegion.Name, parentDemographicData);
                    var childDataLines = FormatDemographicData(childRegion.Name, childDemographicData);
                    var comparisonResult = CompareAndFormatDemographicData(parentDataLines, childDataLines);

                    SaveComparisonResult(parentRegion.Id, childRegion.Id, comparisonResult);

                    var reportFileName = $"Report_{_nextReportNumber}.pdf";
                    await GeneratePdfReportAsync(context, parentRegion, childRegion, parentDataLines, childDataLines, comparisonResult, reportFileName);

                    lock (_lock)
                    {
                        _nextReportNumber++;
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("������", $"������ ��� ���������� �������� ���������: {ex.Message}", "OK");
            }
        }
        private string[] FormatDemographicData(string regionName, DemographicData data)
        {
            return new string[]
            {
                $"������: {regionName}",
                $"���������: {data.Population}",
                $"�����������: {data.BirthRate}",
                $"����������: {data.DeathRate}",
                $"������� ���������: {data.MalePopulation}",
                $"������� ���������: {data.FemalePopulation}"
            };
        }

        private string[] CompareAndFormatDemographicData(string[] parentDataLines, string[] childDataLines)
        {
            if (parentDataLines.Length != childDataLines.Length)
            {
                throw new ArgumentException("���������� �������� ������: ���������� ����� ��������.");
            }

            var comparisonResult = new string[parentDataLines.Length];

            for (int i = 0; i < parentDataLines.Length; i++)
            {
                var parentLineParts = parentDataLines[i].Split(':');
                var childLineParts = childDataLines[i].Split(':');

                if (parentLineParts.Length < 2 || childLineParts.Length < 2)
                {
                    throw new FormatException($"�������� ������ ������ � ������ {i}: {parentDataLines[i]} ��� {childDataLines[i]}");
                }

                var parentValueStr = parentLineParts[1].Trim();
                var childValueStr = childLineParts[1].Trim();

                if (!double.TryParse(parentValueStr, out var parentValue) || !double.TryParse(childValueStr, out var childValue))
                {
                    throw new FormatException($"�������� ������ ��������� �������� � ������ {i}: {parentValueStr} ��� {childValueStr}");
                }

                var difference = parentValue - childValue;
                comparisonResult[i] = $"{parentLineParts[0]}: {difference}";
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
                DisplayAlert("������", $"������ ���������� ���������: {ex.Message}", "OK");
            }
        }

        private async Task GeneratePdfReportAsync(ApplicationContext context, Database.Models.Region parentRegion, Database.Models.Region childRegion, string[] parentDataLines, string[] childDataLines, string[] comparisonResult, string fileName)
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

                        string fontPath = "C:\\Windows\\Fonts\\arial.ttf";
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

                        Paragraph comparisonHeader = new Paragraph($"������� ����� {parentRegion.Name} � {childRegion.Name}:\n\n", titleFont)
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

                await DisplayAlert("����� ������", $"����� �������� � ����� Documentation/Reports ��� ������ {fileName}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("������", $"������ ��� ��������� PDF ������: {ex.Message}", "OK");
            }
        }

    }
}
