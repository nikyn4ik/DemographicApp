using Database;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace DemographicApp.Pages
{
    public partial class Statistics : ContentPage
    {
        private readonly ApplicationContext _context;
        private Dictionary<string, Dictionary<string, int>> regionStats;

        private string selectedFirstRegion;
        private string selectedSecondRegion;
        private string selectedStatType;

        public Statistics()
        {
            InitializeComponent();
            _context = new ApplicationContext();
            LoadRegions();
        }

        private async void LoadRegions()
        {
            try
            {
                var regions = await _context.Regions.ToListAsync();
                foreach (var region in regions)
                {
                    FirstRegionPicker.Items.Add(region.Name);
                    SecondRegionPicker.Items.Add(region.Name);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить регионы: {ex.Message}", "OK");
            }
        }

        private void OnRegionSelected(object sender, EventArgs e)
        {
            selectedFirstRegion = FirstRegionPicker.SelectedItem?.ToString();
            selectedSecondRegion = SecondRegionPicker.SelectedItem?.ToString();

            if (!string.IsNullOrWhiteSpace(selectedFirstRegion) &&
                !string.IsNullOrWhiteSpace(selectedSecondRegion) &&
                !string.IsNullOrWhiteSpace(selectedStatType))
            {
                LoadChartData();
            }
        }

        private void OnStatTypeSelected(object sender, EventArgs e)
        {
            var picker = sender as Picker;
            var selectedStatTypeLocalized = picker?.SelectedItem?.ToString();

            selectedStatType = selectedStatTypeLocalized switch
            {
                "Население" => "Population",
                "Рождаемость" => "BirthRate",
                "Смертность" => "DeathRate",
                _ => selectedStatType
            };

            if (!string.IsNullOrWhiteSpace(selectedFirstRegion) &&
                !string.IsNullOrWhiteSpace(selectedSecondRegion) &&
                !string.IsNullOrWhiteSpace(selectedStatType))
            {
                LoadChartData();
            }
        }

        private async void LoadChartData()
        {
            regionStats = new Dictionary<string, Dictionary<string, int>>();

            try
            {
                await LoadRegionData(selectedFirstRegion);
                await LoadRegionData(selectedSecondRegion);

                canvasView.InvalidateSurface();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить данные для графика: {ex.Message}", "OK");
            }
        }

        private async Task LoadRegionData(string regionName)
        {
            try
            {
                var demographicData = await _context.DemographicData
                    .Include(d => d.Region)
                    .Where(d => d.Region.Name == regionName)
                    .ToListAsync();

                var stats = new Dictionary<string, int>
                {
                    ["Population"] = demographicData.Sum(d => d.Population),
                    ["BirthRate"] = demographicData.Sum(d => d.BirthRate),
                    ["DeathRate"] = demographicData.Sum(d => d.DeathRate)
                };

                regionStats[regionName] = stats;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить данные для региона '{regionName}': {ex.Message}", "OK");
            }
        }

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.White);

            if (regionStats == null || regionStats.Count < 2 || string.IsNullOrWhiteSpace(selectedStatType))
                return;

            var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            var width = e.Info.Width;
            var height = e.Info.Height;
            var barWidth = width / (regionStats.Count * 2);

            var firstRegionStat = regionStats.ContainsKey(selectedFirstRegion) ? regionStats[selectedFirstRegion] : new Dictionary<string, int>();
            var secondRegionStat = regionStats.ContainsKey(selectedSecondRegion) ? regionStats[selectedSecondRegion] : new Dictionary<string, int>();

            if (firstRegionStat.ContainsKey(selectedStatType) && secondRegionStat.ContainsKey(selectedStatType))
            {
                var firstValue = firstRegionStat[selectedStatType];
                var secondValue = secondRegionStat[selectedStatType];

                var maxStatValue = Math.Max(firstValue, secondValue);
                var scale = (height - 100) / (float)maxStatValue; 

                var x = 0f; 
                DrawBar(canvas, selectedFirstRegion, x, height - 60, barWidth, scale, SKColors.Blue, selectedStatType, firstValue);

                x += barWidth * 1.5f; 
                DrawBar(canvas, selectedSecondRegion, x, height - 60, barWidth, scale, SKColors.Green, selectedStatType, secondValue);
            }
        }

        private void DrawBar(SKCanvas canvas, string regionName, float x, float bottomY, float barWidth, float scale, SKColor color, string label, int value)
        {
            var barHeight = (int)(value * scale); 
            var y = bottomY - barHeight;

            var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = color,
                IsAntialias = true
            };

            canvas.DrawRect(x, y, barWidth, barHeight, paint);

            paint.Color = SKColors.Black;
            paint.TextSize = 16;
            canvas.DrawText($"{label}: {value}", x + barWidth / 4, bottomY + 20, paint);
            canvas.DrawText(regionName, x + barWidth / 4, bottomY + 40, paint);
        }
    }
}
