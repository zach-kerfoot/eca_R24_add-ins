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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.UI;

namespace ECA_Addin.UI.Popup_Windows
{
    /// <summary>
    /// Interaction logic for PFP_Scheduler.xaml
    /// </summary>
    public partial class PFP_Scheduler_Window : Window
    {
        public class PrefabPackage
        {
            public string PackageId { get; set; }
        }

        public class Template
        {
            public string TemplateId { get; set; }
        }

        private List<PrefabPackage> _pfpData;
        private List<Template> _templateData;
        public PFP_Scheduler_Window(HashSet<string> pfpList, HashSet<string> templateList)
        {
            InitializeComponent();

            TaskDialog.Show("Debug", $"Templates Count: {templateList.Count}");


            _pfpData = pfpList.Select(name => new PrefabPackage { PackageId = name }).ToList() ?? new List<PrefabPackage>();
            _templateData = templateList.Select(name => new Template { TemplateId = name }).ToList() ?? new List<Template>();

            PFPGrid.ItemsSource = _pfpData;
            TemplateGrid.ItemsSource = _templateData;
        }

        private void PFPSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_pfpData == null) return; // Ensure the data list is initialized

            string searchText = PFPSearchBar.Text.ToLower();
            PFPGrid.ItemsSource = _pfpData
                .Where(item => item.PackageId != null && item.PackageId.ToLower().Contains(searchText))
                .ToList();
        }

         //Filter TempalteGrid based on search query
        private void TemplateSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = TemplateSearchBar.Text.ToLower();
            TemplateGrid.ItemsSource = _templateData
                .Where(item => item.TemplateId.ToLower().Contains(searchText))
                .ToList();
        }

    }
}
