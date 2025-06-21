using Diplom.Helpers;
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
    /// Логика взаимодействия для EquipmentWindow.xaml
    /// </summary>
    public partial class EquipmentWindow : Window
    {

        private int _userId;
        
        public EquipmentWindow(int userId)
        {
            _userId = userId;
            InitializeComponent();
            DataContext = new EquipmentViewModel();
            Title = $"Панель оборудования (ID: {userId})";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EmployeeWindow employeeWindow = new EmployeeWindow(_userId);
            employeeWindow.Show();
            Hide();
        }
    }
}
