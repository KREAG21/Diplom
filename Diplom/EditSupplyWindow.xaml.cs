using Diplom.Helpers;
using Diplom.Model;
using System;
using System.Windows;

namespace Diplom
{
    public partial class EditSupplyWindow : Window
    {
        private SupplyModel _supply;

        public bool SupplyUpdated { get; private set; }

        public EditSupplyWindow(SupplyModel supply)
        {
            InitializeComponent();
            _supply = supply;
            FillFields();
        }

        private void FillFields()
        {
            SupplierBox.Text = _supply.SupplierName;
            DatePicker.SelectedDate = _supply.DeliveryDate;
            CostBox.Text = _supply.TotalCost.ToString("F2");
            MaterialIDBox.Text = _supply.MaterialID.ToString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SupplierBox.Text) ||
                !DatePicker.SelectedDate.HasValue ||
                !decimal.TryParse(CostBox.Text, out decimal cost) ||
                !int.TryParse(MaterialIDBox.Text, out int materialId))
            {
                MessageBox.Show("Проверьте корректность всех полей.");
                return;
            }

            _supply.SupplierName = SupplierBox.Text.Trim();
            _supply.DeliveryDate = DatePicker.SelectedDate.Value;
            _supply.TotalCost = cost;
            _supply.MaterialID = materialId;

            bool success = DatabaseHelper.UpdateSupply(_supply);
            if (success)
            {
                SupplyUpdated = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Ошибка при обновлении поставки.");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
