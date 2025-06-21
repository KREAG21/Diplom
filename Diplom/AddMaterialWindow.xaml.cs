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
    /// Логика взаимодействия для AddMaterialWindow.xaml
    /// </summary>
    public partial class AddMaterialWindow : Window
    {
        public AddMaterialWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text) ||
                !int.TryParse(QuantityBox.Text, out int quantity) ||
                !decimal.TryParse(UnitPriceBox.Text, out decimal unitPrice))
            {
                MessageBox.Show("Проверьте правильность введённых данных.");
                return;
            }

            var material = new MaterialModel
            {
                Name = NameBox.Text,
                Quantity = quantity,
                UnitPrice = unitPrice
            };

            if (DatabaseHelper.AddMaterial(material))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Ошибка при добавлении.");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

}
