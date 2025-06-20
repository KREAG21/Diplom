using System;
using System.Windows;

namespace Diplom
{
    public partial class AddSupplyWindow : Window
    {
        public bool SupplyAdded { get; private set; }

        public AddSupplyWindow()
        {
            InitializeComponent();
            DatePicker.SelectedDate = DateTime.Now;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string supplier = SupplierBox.Text.Trim();
            DateTime? date = DatePicker.SelectedDate;
            string costText = CostBox.Text.Trim();
            string materialIdText = MaterialIDBox.Text.Trim();

            if (string.IsNullOrEmpty(supplier) || !date.HasValue ||
                !decimal.TryParse(costText, out decimal cost) ||
                !int.TryParse(materialIdText, out int materialId))
            {
                MessageBox.Show("Проверьте корректность всех полей.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = DatabaseHelper.InsertSupply(supplier, date.Value, cost, materialId);
            if (success)
            {
                SupplyAdded = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Ошибка при добавлении поставки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
