﻿using System;
using System.Data.SqlClient;
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
                using (var connection = new SqlConnection("Server=(local);Database=BeautySalon;Integrated Security=True;"))
                {
                    connection.Open();
                    var cmd = _orderId == 0
                        ? new SqlCommand(
                            "INSERT INTO Orders (CustomerName, ServiceDescription, TotalAmount, OrderDate, UserID) " +
                            "VALUES (@Name, @Desc, @Amount, @Date, @UserID)", connection)
                        : new SqlCommand(
                            "UPDATE Orders SET CustomerName = @Name, ServiceDescription = @Desc, " +
                            "TotalAmount = @Amount, OrderDate = @Date WHERE OrderID = @OrderID", connection);

                    cmd.Parameters.AddWithValue("@Name", CustomerName);
                    cmd.Parameters.AddWithValue("@Desc", ServiceDescription);
                    cmd.Parameters.AddWithValue("@Amount", TotalAmount);
                    cmd.Parameters.AddWithValue("@Date", OrderDate.Value.Date);

                    if (_orderId == 0)
                        cmd.Parameters.AddWithValue("@UserID", _userId);
                    else
                        cmd.Parameters.AddWithValue("@OrderID", _orderId);

                    cmd.ExecuteNonQuery();
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