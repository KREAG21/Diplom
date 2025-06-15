using Diplom.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Diplom.Helpers;

namespace Diplom.Helpers
{
    public class EquipmentEditViewModel : ViewModelBase
    {
        public EquipmentModel Equipment { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public Action<bool> CloseAction { get; set; }

        public EquipmentEditViewModel(EquipmentModel equipment)
        {
            Equipment = equipment;

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Save()
        {
            // можно добавить валидацию здесь
            CloseAction?.Invoke(true);
        }

        private void Cancel()
        {
            CloseAction?.Invoke(false);
        }
    }
}
