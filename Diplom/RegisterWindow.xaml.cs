using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Diplom
{
    /// <summary>
    /// Логика взаимодействия для RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (RegPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Пароли не совпадают");
                return;
            }

            if (!DatabaseHelper.RegisterUser(
                RegUsernameBox.Text,
                EmailBox.Text,
                RegPasswordBox.Password,
                RoleBox.Text,
                out string error))
            {
                MessageBox.Show(error);
                return;
            }
            else
            {
                MessageBox.Show("Регистрация успешна!");
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                Hide();
            }
        }
    }
}
