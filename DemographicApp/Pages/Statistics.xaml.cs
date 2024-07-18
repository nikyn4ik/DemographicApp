using Database;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace DemographicApp.Pages
{
    public partial class Statistics : ContentPage
    {
        private readonly ApplicationContext _context;
        private Dictionary<string, Dictionary<string, int>> _regionStats;
        private string _selectedFirstRegion;
        private string _selectedSecondRegion;

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
            _selectedFirstRegion = FirstRegionPicker.SelectedItem?.ToString();
            _selectedSecondRegion = SecondRegionPicker.SelectedItem?.ToString();

            if (!string.IsNullOrWhiteSpace(_selectedFirstRegion) && !string.IsNullOrWhiteSpace(_selectedSecondRegion))
            {
                LoadChartData();
            }
        }

        private async void LoadChartData()
        {
            _regionStats = new Dictionary<string, Dictionary<string, int>>();

            try
            {
                await LoadRegionData(_selectedFirstRegion);
                await LoadRegionData(_selectedSecondRegion);

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
            { "Население", demographicData.Sum(d => d.Population) },
            { "Рождаемость", demographicData.Sum(d => d.BirthRate) },
            { "Смертность", demographicData.Sum(d => d.DeathRate) }
        };

                _regionStats[regionName] = stats;
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

            if (_regionStats == null || _regionStats.Count < 2)
                return;

            var width = e.Info.Width;
            var height = e.Info.Height;

            var maxPopulation = _regionStats.Values.Select(s => s["Population"]).Max();
            var scale = height / (float)maxPopulation;

            var colors = new Dictionary<string, SKColor>
            {
                { _selectedFirstRegion, SKColors.Blue },
                { _selectedSecondRegion, SKColors.Green }
            };

            DrawLineChart(canvas, _selectedFirstRegion, width, height, scale, colors[_selectedFirstRegion]);
            DrawLineChart(canvas, _selectedSecondRegion, width, height, scale, colors[_selectedSecondRegion]);

            DrawLegend(canvas, width, height, colors);
        }

        private void DrawLineChart(SKCanvas canvas, string regionName, int width, int height, float scale, SKColor color)
        {
            if (!_regionStats.ContainsKey(regionName))
            {
                return;
            }

            var regionData = _regionStats[regionName];
            var categories = new[] { "Население", "Рождаемость", "Смертность" };
            var xInterval = width / (categories.Length + 1);

            var points = new Dictionary<string, SKPoint>();

            for (int i = 0; i < categories.Length; i++)
            {
                var category = categories[i];
                if (!regionData.ContainsKey(category))
                {
                    continue;
                }

                var x = (i + 1) * xInterval;
                var y = height - (regionData[category] * scale);
                points[category] = new SKPoint(x, y);
            }

            var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = color,
                StrokeWidth = 3,
                IsAntialias = true
            };

            var pointsArray = points.Values.ToArray();
            canvas.DrawPoints(SKPointMode.Lines, pointsArray, paint);

            paint.Style = SKPaintStyle.Fill;
            paint.TextSize = 30;
            foreach (var point in points)
            {
                canvas.DrawText($"{point.Key}: {regionData[point.Key]}", point.Value.X - 30, point.Value.Y - 10, paint);
            }
        }
        private void DrawLegend(SKCanvas canvas, int width, int height, Dictionary<string, SKColor> colors)
        {
            var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Black,
                TextSize = 30,
                IsAntialias = true
            };

            int yOffset = 30;
            foreach (var color in colors)
            {
                paint.Color = color.Value;
                canvas.DrawRect(new SKRect(10, yOffset - 20, 50, yOffset), paint);
                paint.Color = SKColors.Black;
                canvas.DrawText(color.Key, 60, yOffset, paint);
                yOffset += 40;
            }
        }
    }
}
