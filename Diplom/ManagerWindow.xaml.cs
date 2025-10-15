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
    /// Логика взаимодействия для ManagerWindow.xaml
    /// </summary>
    public partial class ManagerWindow : Window
    {
        private int userId;
        private string username;

        public ManagerWindow(int userId, string username)
        {
            InitializeComponent();
            this.userId = userId;
            this.username = username;
            LoadMaterials();
            this.Title += $" — {username}";
            LoadSupplies();
        }

        //Подгрузка даннх об оборудовании из БД
        private void LoadSupplies()
        {
            try
            {
                var supplies = DatabaseHelper.GetSupplies();
                SuppliesDataGrid.ItemsSource = supplies;
                MessageBox.Show($"Загружено записей: {supplies.Count}");

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке поставок:\n" + ex.Message);
            }
        }
        
        //Подгрузка данных о материалах из БД
        private void LoadMaterials()
        {
            MaterialsDataGrid.ItemsSource = DatabaseHelper.GetMaterials();
        }


        private void AddSupply_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddSupplyWindow();
            addWindow.ShowDialog();

            if (addWindow.SupplyAdded)
            {
                LoadSupplies(); // Перезагружаем данные
            }
        }

        private void EditSupply_Click(object sender, RoutedEventArgs e)
        {
            if (SuppliesDataGrid.SelectedItem is SupplyModel selected)
            {
                var editWindow = new EditSupplyWindow(selected);
                editWindow.ShowDialog();

                if (editWindow.SupplyUpdated)
                    LoadSupplies();
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
                        LoadSupplies();
                    else
                        MessageBox.Show("Не удалось удалить поставку.");
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
                        LoadMaterials();
                    else
                        MessageBox.Show("Не удалось удалить материал.");
                }
            }
            else
            {
                MessageBox.Show("Выберите материал для удаления.");
            }
        }
    }
}
