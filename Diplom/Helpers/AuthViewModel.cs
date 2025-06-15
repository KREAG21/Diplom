using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Diplom
{
    public class AuthViewModel
    {
        public ICommand ShowRegisterCommand { get; }

        public AuthViewModel()
        {
            ShowRegisterCommand = new RelayCommand(ShowRegister);
        }

        private void ShowRegister()
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Show();

            // Закрываем текущее окно (если нужно)
            Application.Current.Windows.OfType<MainWindow>().FirstOrDefault()?.Close();
        }
    }
}