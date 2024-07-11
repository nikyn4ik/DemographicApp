using Database;
using DemographicApp.Pages;
using System.Diagnostics;
using Database.Models;

namespace DemographicApp
{
    public partial class App : Application
    {
        private readonly ApplicationContext _context;

        public App()
        {
            InitializeComponent();
            _context = new ApplicationContext();

            AppDomain.CurrentDomain.ProcessExit += OnAppExit;

            MainPage = new NavigationPage(new Login());
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

        protected override async void OnStart()
        {
            base.OnStart();
            await CheckAdminAndNavigate();
        }

        private async Task CheckAdminAndNavigate()
        {
            using (var db = new ApplicationContext())
            {
                await db.InitializeRolesAsync();

                bool isAdminExists = await db.IsAdminUserExists();

                if (isAdminExists)
                {
                    MainPage = new NavigationPage(new MainPage());
                }
                else
                {
                    var createAdminPage = new CreateAdmin();

                    createAdminPage.AdminCreated += async (sender, args) =>
                    {
                        await CheckAdminAndNavigate(); 
                    };

                    MainPage = new NavigationPage(createAdminPage);
                }
            }
        }
    }
}