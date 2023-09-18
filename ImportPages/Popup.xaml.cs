using Corel.Interop.VGCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

namespace ImportPages
{
    /// <summary>
    /// Interaction logic for Popup.xaml
    /// </summary>
    public partial class Popup : UserControl
    {

        public event Action<bool> PopupClickEvent;

        internal DataFiles dataFiles = new DataFiles();

        FileInfo[] fileInfos;

        public Popup(FileInfo[] fileInfos)
        {
            InitializeComponent();
            this.Loaded += Popup_Loaded;
            dataFiles.Files = new ObservableCollection<DataFile>();
            this.DataContext = dataFiles;
            this.fileInfos = fileInfos;
        }

        private void Popup_Loaded(object sender, RoutedEventArgs e)
        {
            load();
        }

        private void load()
        {

            ProcessFiles pf = new ProcessFiles();
            pf.FileReady += Pf_FileReady;
          
             pf.ProcessStart(fileInfos);
            
        }

        private void Pf_FileReady(DataFile obj)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                dataFiles.Files.Add(obj);
            }));
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            if (PopupClickEvent != null)
                PopupClickEvent(true);
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            if (PopupClickEvent != null)
                PopupClickEvent(false);
        }
    }
}
