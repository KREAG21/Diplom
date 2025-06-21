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
    /// Логика взаимодействия для EditMaterialWindow.xaml
    /// </summary>
    public partial class EditMaterialWindow : Window
    {
        private MaterialModel _material;

        public EditMaterialWindow(MaterialModel material)
        {
            InitializeComponent();
            _material = material;

            // Заполнение полей
            NameBox.Text = _material.Name;
            QuantityBox.Text = _material.Quantity.ToString();
            UnitPriceBox.Text = _material.UnitPrice.ToString("N2");
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

            _material.Name = NameBox.Text;
            _material.Quantity = quantity;
            _material.UnitPrice = unitPrice;

            if (DatabaseHelper.UpdateMaterial(_material))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Ошибка при обновлении.");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

}
