using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace EasySave.Core.Models
{
    public class ModelJob : INotifyPropertyChanged
    {
        public string Name { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public bool IsFull { get; set; }

        private string _state = "STOPPED";
        [JsonIgnore]
        public string State 
        { 
            get => _state; 
            set { _state = value; OnPropertyChanged(); } 
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
