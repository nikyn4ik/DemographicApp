using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace DemographicApp.ViewModels
{
    public class AdminRegistrationViewModel : INotifyPropertyChanged
    {
        private string _userName;
        private string _password;
        private string _errorMessage;

        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand RegisterCommand { get; }

        public AdminRegistrationViewModel()
        {
            RegisterCommand = new Command(Register);
        }

        private async void Register()
        {
            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Все поля обязательны для заполнения.";
                return;
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(Password);

            using (var db = new ApplicationContext())
            {
                await db.InitializeRolesAsync();

                var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                if (adminRole == null)
                {
                    ErrorMessage = "Роль администратора не найдена.";
                    return;
                }

                var existingAdmin = await db.Users.AnyAsync(u => u.RoleId == adminRole.Id);
                if (existingAdmin)
                {
                    ErrorMessage = "Администратор уже существует.";
                    return;
                }

                var newUser = new User
                {
                    UserName = UserName,
                    PasswordHash = passwordHash,
                    RoleId = adminRole.Id
                };

                db.Users.Add(newUser);
                await db.SaveChangesAsync();
            }

            ErrorMessage = "Администратор успешно зарегистрирован.";
            await Application.Current.MainPage.Navigation.PushAsync(new MainPage());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}