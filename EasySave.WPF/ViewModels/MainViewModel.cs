using EasySave.Core.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace EasySave.WPF.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<ModelJob> JobsList { get; set; }

        private SauvegardeModel _model;

        public MainViewModel()
        {
            _model = new SauvegardeModel();
            _model.LoadData();
            JobsList = new ObservableCollection<ModelJob>(_model.myJobs);
        }
    }
}