using System;
using Microsoft.Maui.Controls;
using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace DemographicApp.Pages
{
    public partial class Registration : ContentPage
    {
        private readonly ApplicationContext _context;

        public Registration()
        {
            InitializeComponent();
            _context = new ApplicationContext();
        }

        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            var userName = UserNameEntry.Text;
            var password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessageLabel.Text = "»м€ пользовател€ и пароль не должны быть пустыми.";
                return;
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (existingUser != null)
            {
                ErrorMessageLabel.Text = "ѕользователь с таким именем уже существует.";
                return;
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (role == null)
            {
                ErrorMessageLabel.Text = "–оль пользовател€ не найдена.";
                return;
            }

            var hashedPassword = User.HashPassword(password);

            var user = new User
            {
                UserName = userName,
                PasswordHash = hashedPassword,
                RoleId = role.Id
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            await Navigation.PopAsync();
            await Navigation.PushAsync(new Login());
        }
    }
}
