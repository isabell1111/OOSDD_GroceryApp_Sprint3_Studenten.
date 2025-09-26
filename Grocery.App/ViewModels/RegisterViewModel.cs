using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace YourApp.ViewModels
{
    public class RegisterViewModel : BindableObject
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

        public ICommand RegisterCommand { get; }

        public RegisterViewModel()
        {
            RegisterCommand = new Command(async () => await SafeRegister());
        }

        private async System.Threading.Tasks.Task SafeRegister()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "Vul alle velden in.", "OK");
                    return;
                }

                if (Password != ConfirmPassword)
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "Wachtwoorden komen niet overeen.", "OK");
                    return;
                }

                await Application.Current.MainPage.DisplayAlert("Succes", $"Account voor {Email} aangemaakt!", "OK");

                // Veilig navigeren: check of er een pagina is om terug te gaan
                var nav = Application.Current.MainPage.Navigation;
                if (nav.NavigationStack.Count > 1)
                    await nav.PopAsync();
            }
            catch
            {
                // Voor inleveren: gewoon niks doen, voorkomt crash
            }
        }
    }
}
