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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Diplom
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new AuthViewModel();
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var (success, userId, password) = DatabaseHelper.AuthenticateUser(
                UsernameBox.Text,
                PasswordBox.Password
            );

            if (success)
            {
                EmployeeWindow employeeWindow = new EmployeeWindow(userId);
                employeeWindow.Show();
                Hide();
            }
            else
            {
                MessageBox.Show("Неверное имя пользователя или пароль");
            }
        }
        // Остальные методы без изменений
        private void ShowLogin_Click(object sender, RoutedEventArgs e) { /* ... */ }
        private void ShowRegister_Click(object sender, RoutedEventArgs e)
        {
           /* ... */
        }
    }
}


