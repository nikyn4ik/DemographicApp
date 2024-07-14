using Database;
using Database.Models;

namespace DemographicApp.Pages
{
    public partial class Login : ContentPage
    {
        private readonly Action<User> _onLoginSuccess;

        public Login()
        {
            InitializeComponent();
        }

        public Login(Action<User> onLoginSuccess)
        {
            InitializeComponent();
            _onLoginSuccess = onLoginSuccess;
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            var username = UsernameEntry.Text;
            var password = PasswordEntry.Text;

            using (var context = new ApplicationContext())
            {
                var user = context.Users.FirstOrDefault(u => u.UserName == username);
                if (user != null && user.VerifyPassword(password))
                {
                    _onLoginSuccess?.Invoke(user);
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Ошибка", "Неверное имя пользователя или пароль", "OK");
                }
            }
        }

        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Registration());
        }
    }
}
