using Database;
using DemographicApp.Pages;
using System.Diagnostics;

namespace DemographicApp
{
    public partial class App : Application
    {
        private readonly ApplicationContext _context;

        public App()
        {
            InitializeComponent();
            _context = new ApplicationContext();

            MainPage = new NavigationPage(new AppShell());
            AppDomain.CurrentDomain.ProcessExit += OnAppExit;
        }

        protected override async void OnStart()
        {
            base.OnStart();

            if (_context != null)
            {
                await _context.InitializeRolesAsync();

                if (!await _context.IsAdminUserExists())
                {
                    MainPage = new CreateAdmin(_context);
                }
                else
                {
                    MainPage = new NavigationPage(new MainPage());
                }
            }
            else
            {
                Debug.WriteLine("Error: _context is null");
            }
        }

        private void OnAppExit(object sender, EventArgs e)
        {
            KillProcess("sqlceip");
            KillProcess("sqlservr");
        }

        private void KillProcess(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            foreach (var process in processes)
            {
                try
                {
                    if (process.MainModule.FileName.Contains("MSSQLLocalDB"))
                    {
                        process.Kill();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error killing process {processName}: {ex.Message}");
                }
            }
        }
    }
}
