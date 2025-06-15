using Diplom.Helpers;
using Diplom.Model;
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
    /// Логика взаимодействия для EquipmentEdit.xaml
    /// </summary>
    public partial class EquipmentEdit : Window
    {
        public EquipmentEdit(EquipmentModel model)
        {
            InitializeComponent();
            var vm = new EquipmentEditViewModel(model);
            vm.CloseAction = result =>
            {
                this.DialogResult = result;
                this.Close();
            };
            this.DataContext = vm;
        }
    }

}
