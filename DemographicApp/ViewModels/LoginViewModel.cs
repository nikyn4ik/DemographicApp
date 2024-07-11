using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Database;
using Database.Models;
using DemographicApp.Pages;
using Microsoft.EntityFrameworkCore;

namespace DemographicApp.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _username;
        private string _password;
        private string _errorMessage;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
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

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Command(async () => await LoginAsync());
            RegisterCommand = new Command(NavigateToRegister);
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Пожалуйста, введите имя пользователя и пароль.";
                return;
            }

            bool isAuthenticated = await AuthenticateUserAsync();

            if (isAuthenticated)
            {
                // Navigate to MainPage
                await Application.Current.MainPage.Navigation.PushAsync(new MainPage());
            }
            else
            {
                ErrorMessage = "Неправильное имя пользователя или пароль.";
            }
        }

        private async Task<bool> AuthenticateUserAsync()
        {
            return Username == "admin" && Password == "admin";
        }

        private async void NavigateToRegister()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new Registration());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}