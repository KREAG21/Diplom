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
            string username = UsernameBox.Text.Trim();
            string password = PasswordBox.Password;

            var (success, userId, role) = DatabaseHelper.AuthenticateUser(username, password);

            if (!success)
            {
                MessageBox.Show("Неверное имя пользователя или пароль.", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            switch (role.ToLower())
            {
                case "менеджер":
                    ManagerWindow managerWindow = new ManagerWindow(userId, username);
                    managerWindow.Show();
                    break;
                case "сотрудник":
                    EmployeeWindow employeeWindow = new EmployeeWindow(userId, username);
                    employeeWindow.Show();
                    break;
                default:
                    MessageBox.Show("Роль не распознана.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
            }

            this.Close();
        }

        // Остальные методы без изменений
        private void ShowLogin_Click(object sender, RoutedEventArgs e) { /* ... */ }
        private void ShowRegister_Click(object sender, RoutedEventArgs e)
        {
           /* ... */
        }
    }
}


