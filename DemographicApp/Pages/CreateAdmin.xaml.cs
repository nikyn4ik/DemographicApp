using Microsoft.EntityFrameworkCore;
using Database.Models;
using Database;

namespace DemographicApp.Pages
{
    public partial class CreateAdmin : ContentPage
    {

        private readonly ApplicationContext _context;
        public CreateAdmin(ApplicationContext context)
        {
            InitializeComponent();
            _context = context;
        }

        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            var userName = UserNameEntry.Text;
            var password = PasswordEntry.Text;

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                ErrorMessageLabel.Text = "Имя пользователя и пароль обязательны.";
                return;
            }

            using (var context = new ApplicationContext())
            {
                var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                if (adminRole != null)
                {
                    var user = new User
                    {
                        UserName = userName,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                        RoleId = adminRole.Id
                    };

                    context.Users.Add(user);
                    await context.SaveChangesAsync();

                    Application.Current.MainPage = new MainPage();
                }
            }
        }
    }
}
