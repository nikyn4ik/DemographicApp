using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;

namespace DemographicApp.Pages
{
    public partial class Compare : ContentPage
    {
        private readonly ApplicationContext _context;

        public Compare()
        {
            InitializeComponent();
            _context = new ApplicationContext();
            LoadRegions();
        }

        private async void LoadRegions()
        {
            var regions = await _context.Regions.ToListAsync();
            ParentRegionPicker.ItemsSource = regions;
            ChildRegionPicker.ItemsSource = regions;
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
                await DisplayAlert("������", "��������������� ������ �� ������� ��� ��������� ��������.", "OK");
                return;
            }

            var comparisonResult = CompareDemographicData(parentRegion, childRegion, parentDemographicData, childDemographicData);

            SaveComparisonResult(parentRegion.Id, childRegion.Id, comparisonResult);

            await GeneratePdfReportAsync(parentRegion, childRegion, comparisonResult);
        }

        private string CompareDemographicData(Database.Models.Region parentRegion, Database.Models.Region childRegion, DemographicData parentData, DemographicData childData)
        {
            return $"��������� ����� {parentRegion.Name} � {childRegion.Name}:\n\n" +
                   $"������� � ���������: {Math.Abs(parentData.Population - childData.Population)}\n" +
                   $"������� � �����������: {Math.Abs(parentData.BirthRate - childData.BirthRate)}\n" +
                   $"������� � ����������: {Math.Abs(parentData.DeathRate - childData.DeathRate)}\n" +
                   $"������� � ������� ���������: {Math.Abs(parentData.MalePopulation - childData.MalePopulation)}\n" +
                   $"������� � ������� ���������: {Math.Abs(parentData.FemalePopulation - childData.FemalePopulation)}";
        }

        private void SaveComparisonResult(int parentRegionId, int childRegionId, string comparisonResult)
        {
            var result = new ComparisonResult
            {
                ParentRegionId = parentRegionId,
                ChildRegionId = childRegionId,
                ComparisonDate = DateTime.Now,
                ComparisonResultData = comparisonResult
            };

            _context.ComparisonResults.Add(result);
            _context.SaveChanges();
        }

        private async Task GeneratePdfReportAsync(Database.Models.Region parentRegion, Database.Models.Region childRegion, string comparisonResult)
        {
            string fileName = Path.Combine(FileSystem.AppDataDirectory, "ComparisonResult.pdf");

            await Task.Run(() =>
            {
                using (var stream = new FileStream(fileName, FileMode.Create))
                {
                    Document document = new Document();
                    PdfWriter.GetInstance(document, stream);
                    document.Open();
                    document.Add(new Paragraph($"��������� ����� {parentRegion.Name} � {childRegion.Name}"));
                    document.Add(new Paragraph(comparisonResult));
                    document.Close();
                }
            });

            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(fileName)
            });
        }
    }
}
