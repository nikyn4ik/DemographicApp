using SkiaSharp;
using SkiaSharp.Views.Maui;
using Database;
using Microsoft.EntityFrameworkCore;

namespace DemographicApp.Pages
{
    public partial class Statistics : ContentPage
    {
        private readonly ApplicationContext _context;
        private Dictionary<string, Dictionary<string, int>> regionStats;

        private string selectedFirstRegion;
        private string selectedSecondRegion;

        public Statistics()
        {
            InitializeComponent();
            _context = new ApplicationContext();
            LoadRegions();
        }

        private async void LoadRegions()
        {
            var regions = await _context.Regions.ToListAsync();
            foreach (var region in regions)
            {
                FirstRegionPicker.Items.Add(region.Name);
                SecondRegionPicker.Items.Add(region.Name);
            }
        }

        private void OnRegionSelected(object sender, EventArgs e)
        {
            selectedFirstRegion = FirstRegionPicker.SelectedItem?.ToString();
            selectedSecondRegion = SecondRegionPicker.SelectedItem?.ToString();

            if (!string.IsNullOrWhiteSpace(selectedFirstRegion) && !string.IsNullOrWhiteSpace(selectedSecondRegion))
            {
                LoadChartData();
            }
        }

        private async void LoadChartData()
        {
            regionStats = new Dictionary<string, Dictionary<string, int>>();

            await LoadRegionData(selectedFirstRegion);
            await LoadRegionData(selectedSecondRegion);

            canvasView.InvalidateSurface();
        }

        private async Task LoadRegionData(string regionName)
        {
            var demographicData = await _context.DemographicData
                .Include(d => d.Region)
                .Where(d => d.Region.Name == regionName)
                .ToListAsync();

            var stats = new Dictionary<string, int>();
            stats["Population"] = demographicData.Sum(d => d.Population);
            stats["BirthRate"] = demographicData.Sum(d => d.BirthRate);
            stats["DeathRate"] = demographicData.Sum(d => d.DeathRate);

            regionStats[regionName] = stats;
        }

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.White);

            if (regionStats == null || regionStats.Count < 2)
                return;

            var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            var width = e.Info.Width;
            var height = e.Info.Height;
            var barWidth = width / (regionStats.Count * 4);

            var maxPopulation = regionStats.Values.Select(s => s["Population"]).Max();
            var scale = height / (float)maxPopulation;

            var index = 0;
            foreach (var stat in regionStats)
            {
                var x = index * 4 * barWidth;
                DrawBar(canvas, stat.Key, x, height, barWidth, scale, SKColors.Blue, "Population", stat.Value["Population"]);

                x += barWidth;
                DrawBar(canvas, stat.Key, x, height, barWidth, scale, SKColors.Green, "Birth Rate", stat.Value["BirthRate"]);

                x += barWidth;
                DrawBar(canvas, stat.Key, x, height, barWidth, scale, SKColors.Red, "Death Rate", stat.Value["DeathRate"]);

                index++;
            }
        }

        private void DrawBar(SKCanvas canvas, string regionName, float x, float height, float barWidth, float scale, SKColor color, string label, int value)
        {
            var barHeight = value * scale;
            var y = height - barHeight;

            var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = color,
                IsAntialias = true
            };

            canvas.DrawRect(x, y, barWidth, barHeight, paint);

            paint.TextSize = 20;
            paint.Color = SKColors.Black;
            canvas.DrawText($"{label}: {value}", x + barWidth / 4, height - 10, paint);
            canvas.DrawText(regionName, x + barWidth / 4, height - 40, paint);
        }
    }
}
