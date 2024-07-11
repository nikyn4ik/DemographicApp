using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace DemographicApp.ViewModels
{
    public class RegistrationViewModel : INotifyPropertyChanged
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

        public RegistrationViewModel()
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
                var userRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "User");
                if (userRole == null)
                {
                    ErrorMessage = "Роль 'User' не найдена в системе.";
                    return;
                }

                var newUser = new User
                {
                    UserName = UserName,
                    PasswordHash = passwordHash,
                    Role = userRole
                };

                db.Users.Add(newUser);
                await db.SaveChangesAsync();
            }

            UserName = string.Empty;
            Password = string.Empty;
            ErrorMessage = "Пользователь успешно зарегистрирован.";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
