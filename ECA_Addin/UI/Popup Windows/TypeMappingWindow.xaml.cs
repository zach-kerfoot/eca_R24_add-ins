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


namespace ECA_Addin.UI.Popup_Windows
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class TypeMappingWindow : Window
    {
        public List<TypeMappingViewModel> Mapping { get; private set; }
        public bool Confirmed { get; private set; } = false;

        public TypeMappingWindow(List<TypeMappingViewModel> mapping)
        {
            InitializeComponent();
            this.Mapping = mapping;

            this.MappingList.ItemsSource = Mapping;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.Confirmed = true;
            this.Close();
        }
    }
}
