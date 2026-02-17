using Diplom.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Diplom
{
    /// <summary>
    /// Логика взаимодействия для ManagerWindow.xaml
    /// </summary>
    public partial class ManagerWindow : Window
    {
        private readonly int userId;
        private readonly string username;
        private List<SupplyModel> _supplies = new List<SupplyModel>();
        private List<MaterialModel> _materials = new List<MaterialModel>();

        public ManagerWindow(int userId, string username)
        {
            InitializeComponent();
            this.userId = userId;
            this.username = username;
            Title += $" — {username}";

            LoadMaterials();
            LoadSupplies();
        }

        private void LoadSupplies()
        {
            try
            {
                _supplies = DatabaseHelper.GetSupplies();
                ApplySuppliesFilter();
                ManagerStatusText.Text = $"Загружено поставок: {_supplies.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке поставок:\n" + ex.Message);
            }
        }

        private void LoadMaterials()
        {
            _materials = DatabaseHelper.GetMaterials();
            ApplyMaterialsFilter();
        }

        private void ApplySuppliesFilter()
        {
            string search = SupplySearchBox?.Text?.Trim() ?? string.Empty;
            var filtered = string.IsNullOrWhiteSpace(search)
                ? _supplies
                : _supplies.Where(s => s.SupplierName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            SuppliesDataGrid.ItemsSource = filtered;
        }

        private void ApplyMaterialsFilter()
        {
            string search = MaterialSearchBox?.Text?.Trim() ?? string.Empty;
            var filtered = string.IsNullOrWhiteSpace(search)
                ? _materials
                : _materials.Where(m => m.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            MaterialsDataGrid.ItemsSource = filtered;
        }

        private void SupplySearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySuppliesFilter();
        }

        private void MaterialSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyMaterialsFilter();
        }

        private void ClearSupplySearch_Click(object sender, RoutedEventArgs e)
        {
            SupplySearchBox.Text = string.Empty;
            ApplySuppliesFilter();
        }

        private void ClearMaterialSearch_Click(object sender, RoutedEventArgs e)
        {
            MaterialSearchBox.Text = string.Empty;
            ApplyMaterialsFilter();
        }

        private void AddSupply_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddSupplyWindow();
            addWindow.ShowDialog();

            if (addWindow.SupplyAdded)
            {
                LoadSupplies();
                ManagerStatusText.Text = "Поставка добавлена";
            }
        }

        private void EditSupply_Click(object sender, RoutedEventArgs e)
        {
            if (SuppliesDataGrid.SelectedItem is SupplyModel selected)
            {
                var editWindow = new EditSupplyWindow(selected);
                editWindow.ShowDialog();

                if (editWindow.SupplyUpdated)
                {
                    LoadSupplies();
                    ManagerStatusText.Text = "Поставка обновлена";
                }
            }
            else
            {
                MessageBox.Show("Выберите поставку для редактирования.");
            }
        }

        private void DeleteSupply_Click(object sender, RoutedEventArgs e)
        {
            if (SuppliesDataGrid.SelectedItem is SupplyModel selected)
            {
                var result = MessageBox.Show($"Удалить поставку #{selected.SupplyID}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    if (DatabaseHelper.DeleteSupply(selected.SupplyID))
                    {
                        LoadSupplies();
                        ManagerStatusText.Text = "Поставка удалена";
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить поставку.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите поставку для удаления.");
            }
        }

        private void AddMaterial_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddMaterialWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadMaterials();
                ManagerStatusText.Text = "Материал добавлен";
            }
        }

        private void EditMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialsDataGrid.SelectedItem is MaterialModel selected)
            {
                var editWindow = new EditMaterialWindow(selected);
                if (editWindow.ShowDialog() == true)
                {
                    LoadMaterials();
                    ManagerStatusText.Text = "Материал обновлен";
                }
            }
            else
            {
                MessageBox.Show("Выберите материал для редактирования.");
            }
        }

        private void DeleteMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialsDataGrid.SelectedItem is MaterialModel selected)
            {
                var result = MessageBox.Show($"Удалить материал «{selected.Name}»?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    if (DatabaseHelper.DeleteMaterial(selected.MaterialID))
                    {
                        LoadMaterials();
                        ManagerStatusText.Text = "Материал удален";
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить материал.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите материал для удаления.");
            }
        }
    }
}
