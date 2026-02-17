using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using Microsoft.Office.Interop.Excel;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using Window = System.Windows.Window;
using DataTable = System.Data.DataTable;

namespace Diplom
{
    public partial class EmployeeWindow : Window
    {
        private int _userId;
        
        

        public EmployeeWindow(int userId)
        {
            _userId = userId;
            
            InitializeComponent();
            Title = $"Панель сотрудника (ID: {userId})";
            LoadOrders();
        }
        
        private void LoadOrders()
        {
            try
            {
                using (var connection = new SqlConnection(DatabaseHelper.ConnectionString))
                {
                    var adapter = new SqlDataAdapter(
                        "SELECT OrderID, CustomerName, ServiceDescription, OrderDate, TotalAmount, EquipmentID FROM Orders",
                        connection);
                    var table = new DataTable();
                    adapter.Fill(table);
                    OrdersGrid.ItemsSource = table.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}");
            }
        }

        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OrderEditWindow(_userId);
            if (dialog.ShowDialog() == true)
            {
                LoadOrders();
                StatusText.Text = "Заказ успешно добавлен";
            }
        }

        private void EditOrder_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersGrid.SelectedItem is DataRowView row)
            {
                DateTime orderDate = row["OrderDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["OrderDate"])
                    : DateTime.Now;

                var dialog = new OrderEditWindow(
                    (int)row["OrderID"],
                    row["CustomerName"].ToString(),
                    row["ServiceDescription"].ToString(),
                    Convert.ToDecimal(row["TotalAmount"]),
                    orderDate,
                    _userId);

                if (dialog.ShowDialog() == true)
                {
                    LoadOrders();
                    StatusText.Text = "Заказ успешно обновлен";
                }
            }
        }


        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersGrid.SelectedItem is DataRowView row &&
                MessageBox.Show("Удалить выбранный заказ?", "Подтверждение",
                              MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var connection = new SqlConnection(DatabaseHelper.ConnectionString))
                    {
                        connection.Open();
                        new SqlCommand(
                            "DELETE FROM Orders WHERE OrderID = @OrderID",
                            connection)
                        {
                            Parameters = { new SqlParameter("@OrderID", row["OrderID"]) }
                        }.ExecuteNonQuery();
                    }
                    LoadOrders();
                    StatusText.Text = "Заказ успешно удален";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}");
                }
            }
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var excel = new Microsoft.Office.Interop.Excel.Application();
                excel.Workbooks.Add();
                var sheet = (Worksheet)excel.ActiveSheet;

                // Заголовки
                for (int i = 0; i < OrdersGrid.Columns.Count; i++)
                    sheet.Cells[1, i + 1] = OrdersGrid.Columns[i].Header;

                // Данные
                for (int i = 0; i < ((DataView)OrdersGrid.ItemsSource).Count; i++)
                    for (int j = 0; j < OrdersGrid.Columns.Count; j++)
                        sheet.Cells[i + 2, j + 1] = ((DataRowView)((DataView)OrdersGrid.ItemsSource)[i])[j].ToString();

                excel.Visible = true;
                StatusText.Text = "Экспорт в Excel завершен";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}");
            }
        }

        private void ExportToPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var doc = new PdfDocument();
                var page = doc.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var font = new XFont("Arial", 12);

                // Заголовок
                gfx.DrawString("Список заказов", font, XBrushes.Black,
                              new XRect(0, 20, page.Width, page.Height),
                              XStringFormats.TopCenter);

                // Данные
                int y = 50;
                foreach (DataRowView row in (DataView)OrdersGrid.ItemsSource)
                {
                    gfx.DrawString($"{row["OrderID"]} {row["CustomerName"]} {row["TotalAmount"]:C}",
                                  font, XBrushes.Black, new XPoint(20, y));
                    y += 20;
                }

                var filename = $"Orders_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                doc.Save(filename);
                System.Diagnostics.Process.Start(filename);
                StatusText.Text = "Экспорт в PDF завершен";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}");
            }
        }
        private void Equipment_Click(object sender, RoutedEventArgs e)
        {
            EquipmentWindow equipmentWindow = new EquipmentWindow(_userId);
            equipmentWindow.Show();
            Hide();
        }
    }
}
