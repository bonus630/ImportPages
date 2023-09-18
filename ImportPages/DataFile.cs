using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using Microsoft.Win32;
using System.IO;
using System.Windows.Threading;

namespace ImportPages
{
    internal class DataFile : DataBase
    {
        private int version;

        public int Version
        {
            get { return version; }
            set { version = value; OnPropertyChanged(); }
        }
        public string PageDimension { get; set; }

        public int NumPages { get; set; }

        private ObservableCollection<DataPage> pages;

        public ObservableCollection<DataPage> Pages
        {
            get { return pages; }
            set { pages = value; OnPropertyChanged(); }
        }

        public int SelectedPagesCount { get { return Pages.Count(r =>r.Selected); } }

        public void SetAll()
        {
            SetDefault(true, true);
        }
        public void SetDefault(bool even,bool odd)
        {
            for (int i = 0; i < pages.Count; i++)
            {
                if(even && i%2==0)
                {
                    Pages[i].Selected = even;
                }
                if(odd && i%2!=0)
                {
                    Pages[i].Selected = odd;
                }
            }
        }


    }
    internal class DataPage : DataBase
    {

        public DataPage(string name,Bitmap preview )
        {
            this.Name = name;
            this.Preview =  Imaging.CreateBitmapSourceFromHBitmap(preview.GetHbitmap(),
                                                                                  IntPtr.Zero,
                                                                                  Int32Rect.Empty,
                                                                                  BitmapSizeOptions.FromEmptyOptions()
                  );
            preview.Dispose();
        }

        private bool selected;

        public bool Selected
        {
            get { return selected; }
            set { selected = value; OnPropertyChanged(); }
        }

        private BitmapSource preview;

        public BitmapSource Preview
        {
            get { return preview; }
            set { preview = value;  OnPropertyChanged(); }
        }



    }
    internal class DataFiles
    {
        private Dispatcher dispatcher;

        private ObservableCollection<DataFile> files;

        public ObservableCollection<DataFile> Files
        {
            get { return files; }
            set { files = value; }
        }


        public DataFiles()
        {
        }
        public int AllPagesCount { get {
                int count = 0;
                for (int i = 0; i < Files.Count; i++)
                {
                    count += Files[i].SelectedPagesCount;
                }
                return count;
            } }
      
    }
    internal class DataBase : INotifyPropertyChanged
    {

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
