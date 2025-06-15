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
    /// Логика взаимодействия для OrderEditWindow.xaml
    /// </summary>
    public partial class OrderEditWindow : Window
    {
        public OrderEditWindow(int userId)
        {
            InitializeComponent();
            DataContext = new OrderEditViewModel(userId);
        }

        public OrderEditWindow(int orderId, string customerName, string serviceDescription, decimal totalAmount, DateTime OrderDate, int userId)
        {
            InitializeComponent();
            DataContext = new OrderEditViewModel(orderId, customerName, serviceDescription, totalAmount, OrderDate, userId);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
