using Diplom.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Diplom.Helpers;

namespace Diplom.Helpers
{
    public class EquipmentViewModel : ViewModelBase
    {
        private ObservableCollection<EquipmentModel> _equipmentList;
        public ObservableCollection<EquipmentModel> EquipmentList
        {
            get => _equipmentList;
            set => SetProperty(ref _equipmentList, value);
        }

        private EquipmentModel _selectedEquipment;
        public EquipmentModel SelectedEquipment
        {
            get => _selectedEquipment;
            set => SetProperty(ref _selectedEquipment, value);
        }

        public ICommand AddEquipmentCommand { get; }
        public ICommand EditEquipmentCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ExportToExcelCommand { get; }
        public ICommand ExportToPdfCommand { get; }

        public EquipmentViewModel()
        {
            EquipmentList = new ObservableCollection<EquipmentModel>();
            AddEquipmentCommand = new RelayCommand(AddEquipment);
            EditEquipmentCommand = new RelayCommand(EditEquipment, () => SelectedEquipment != null);
            DeleteCommand = new RelayCommand(OnDelete, () => SelectedEquipment != null);
            RefreshCommand = new RelayCommand(LoadEquipment);
            ExportToExcelCommand = new RelayCommand(ExportToExcel);
            ExportToPdfCommand = new RelayCommand(ExportToPdf);

            LoadEquipment();
        }

        private void LoadEquipment()
        {
            EquipmentList.Clear();
            var data = DatabaseHelper.GetEquipmentList(); // метод должен быть у тебя
            foreach (var item in data)
                EquipmentList.Add(item);
        }

        private void AddEquipment()
        {
            var newEquipment = new EquipmentModel
            {
                PurchaseDate = DateTime.Now
            };

            var window = new EquipmentEdit(newEquipment);
            if (window.ShowDialog() == true)
            {
                DatabaseHelper.InsertEquipment(newEquipment); // Не забудь реализовать этот метод
                LoadEquipment();
            }
        }

        private void EditEquipment()
        {
            if (SelectedEquipment == null) return;

            // Копия, чтобы не редактировать сразу
            var editCopy = new EquipmentModel
            {
                EquipmentID = SelectedEquipment.EquipmentID,
                Name = SelectedEquipment.Name,
                PurchaseDate = SelectedEquipment.PurchaseDate,
                Cost = SelectedEquipment.Cost,
                Condition = SelectedEquipment.Condition
            };

            var window = new EquipmentEdit(editCopy);
            if (window.ShowDialog() == true)
            {
                DatabaseHelper.UpdateEquipment(editCopy); // Не забудь реализовать
                LoadEquipment();
            }
        }


        private void OnDelete()
        {
            if (SelectedEquipment == null) return;
            if (MessageBox.Show("Удалить оборудование?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                DatabaseHelper.DeleteEquipment(SelectedEquipment.EquipmentID);
                LoadEquipment();
            }
        }

        private void ExportToExcel()
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel файлы (*.xlsx)|*.xlsx",
                    FileName = "Оборудование.xlsx"
                };

                if (saveDialog.ShowDialog() != true) return;

                var excelApp = new Microsoft.Office.Interop.Excel.Application();
                excelApp.Visible = false;
                var workBook = excelApp.Workbooks.Add(Type.Missing);
                var workSheet = (Microsoft.Office.Interop.Excel.Worksheet)workBook.ActiveSheet;
                workSheet.Name = "Оборудование";

                // Заголовки
                workSheet.Cells[1, 1] = "ID";
                workSheet.Cells[1, 2] = "Название";
                workSheet.Cells[1, 3] = "Дата покупки";
                workSheet.Cells[1, 4] = "Стоимость";
                workSheet.Cells[1, 5] = "Состояние";

                // Данные
                for (int i = 0; i < EquipmentList.Count; i++)
                {
                    var eq = EquipmentList[i];
                    workSheet.Cells[i + 2, 1] = eq.EquipmentID;
                    workSheet.Cells[i + 2, 2] = eq.Name;
                    workSheet.Cells[i + 2, 3] = eq.PurchaseDate.ToShortDateString();
                    workSheet.Cells[i + 2, 4] = eq.Cost.ToString();
                    workSheet.Cells[i + 2, 5] = eq.Condition;
                }

                workBook.SaveAs(saveDialog.FileName);
                workBook.Close();
                excelApp.Quit();

                MessageBox.Show("Экспорт в Excel завершён успешно!", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при экспорте в Excel: " + ex.Message);
            }
        }

        private void ExportToPdf()
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF файлы (*.pdf)|*.pdf",
                    FileName = "Оборудование.pdf"
                };
                if (saveDialog.ShowDialog() != true) return;

                var document = new PdfSharpCore.Pdf.PdfDocument();
                var page = document.AddPage();
                var gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(page);
                var font = new PdfSharpCore.Drawing.XFont("Verdana", 12);

                double y = 40;
                gfx.DrawString("Список оборудования", font, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(40, y));
                y += 30;

                foreach (var eq in EquipmentList)
                {
                    string line = $"{eq.EquipmentID} | {eq.Name} | {eq.PurchaseDate:d} | {eq.Cost:C} | {eq.Condition}";
                    gfx.DrawString(line, font, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(40, y));
                    y += 20;
                }

                using (var stream = new FileStream(saveDialog.FileName, FileMode.Create))
                {
                    document.Save(stream);
                }

                MessageBox.Show("Экспорт в PDF выполнен", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка экспорта: " + ex.Message);
            }
        }

    }

}
