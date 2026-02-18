using System;
using System.Windows;
using System.Windows.Input;

namespace Diplom
{
    public class OrderEditViewModel : ViewModelBase
    {
        private readonly int _userId;
        private readonly int _orderId;

        private string _customerName;
        private string _serviceDescription;
        private decimal _totalAmount;


        public string WindowTitle => _orderId == 0 ? "Добавление заказа" : "Редактирование заказа";

        public string CustomerName
        {
            get => _customerName;
            set
            {
                if (SetField(ref _customerName, value))
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        public string ServiceDescription
        {
            get => _serviceDescription;
            set
            {
                if (SetField(ref _serviceDescription, value))
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                if (SetField(ref _totalAmount, value))
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand SaveCommand { get; }

        public OrderEditViewModel(int userId)
        {
            _userId = userId;
            SaveCommand = new RelayCommand(Save, CanSave);
        }

        public OrderEditViewModel(int orderId, string customerName, string serviceDescription, decimal totalAmount, DateTime orderDate, int userId)
            : this(userId)
        {
            _orderId = orderId;
            CustomerName = customerName;
            ServiceDescription = serviceDescription;
            TotalAmount = totalAmount;
            OrderDate = orderDate;
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(CustomerName) &&
                   !string.IsNullOrWhiteSpace(ServiceDescription) &&
                   TotalAmount > 0 && OrderDate.HasValue;
        }

        private void Save()
        {
            try
            {
                bool success = DatabaseHelper.SaveOrder(
                    _orderId,
                    CustomerName,
                    ServiceDescription,
                    TotalAmount,
                    OrderDate.Value.Date,
                    _userId);

                if (!success)
                {
                    MessageBox.Show("Не удалось сохранить заказ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBox.Show("Заказ сохранен");

                // Закрытие окна
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is OrderEditWindow editWindow && editWindow.DataContext == this)
                    {
                        editWindow.DialogResult = true;
                        editWindow.Close();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DateTime? _orderDate = DateTime.Now;

        public DateTime? OrderDate
        {
            get => _orderDate;
            set
            {
                if (SetField(ref _orderDate, value))
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
    }
}