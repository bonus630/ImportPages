using Corel.Interop.VGCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Shapes;

namespace ImportPages.DataSource
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class ImportPagesDataSource : BaseDataSource
    {

        private string caption = "ImportPages";
        private string icon = "guid://6946e359-c240-47c5-888a-e3fc69608d6d";

        public ImportPagesDataSource(DataSourceProxy proxy) : base(proxy)
        {

        }

        // You can change caption/icon dynamically setting a new value here 
        //or loading the value from resource specifying the id of the caption/icon 
        public string Caption
        {
            get { return caption; }
            set { caption = value; NotifyPropertyChanged(); }
        }
        public string Icon
        {
            get { return icon; }
            set { icon = value; NotifyPropertyChanged(); }
        }

        public void MenuItemCommand()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.DefaultExt = ".cdr";
            ofd.Title = "Select cdr files!";
            //ofd.Filter = "(*.cdr)|*.cdr";
            if (ofd.ShowDialog() == true)
            {

                string[] files = ofd.FileNames;
                FileInfo[] fileInfos = new FileInfo[files.Length];
                for (int i = 0; i < files.Length; i++)
                {
                    fileInfos[i] = new FileInfo(files[i]);
                }
                Process(fileInfos);
            }
        }
        public void Process(FileInfo[] fileInfos)
        {
            try
            {
                Popup popup = new Popup(fileInfos);

                System.Windows.Window w = new System.Windows.Window();

                popup.PopupClickEvent += (ok) =>
                {
                    if (ok)
                    {

                        PartialImport(popup.dataFiles);

                    }

                    w.Close();

                };
                w.Content = popup;
                w.ShowDialog();
            }
            catch { }
            finally
            {


            }

        }
        internal void PartialImport(DataFiles dataFiles)
        {

            try
            {
                //file file flyout xpatt /uiConfig/commandBars/commandBarData[@guid='2cfa7035-cbef-4276-b686-adb4f77c80d6']
                // import menuitem xpath /uiConfig/commandBars/commandBarData[@guid='2cfa7035-cbef-4276-b686-adb4f77c80d6']/menu/item[16]
                //guid guidRef='785c273f-c462-4f02-9ee4-c354018e0878'
                ControlUI.corelApp.Optimization = true;
                //pages = new int[] { 2, 3 };
                int docId = ControlUI.corelApp.ActiveDocument.Index;



                Document document = ControlUI.corelApp.Documents[docId];
                int position = document.ActivePage.Index + 1;
                document.InsertPages(dataFiles.AllPagesCount, true, position);


                for (int i = 0; i < dataFiles.Files.Count; i++)
                {
                    Document doc = ControlUI.corelApp.OpenDocument(dataFiles.Files[i].Name);
                    try
                    {
                        for (int k = 1; k <= doc.Pages.Count; k++)
                        {
                            if (dataFiles.Files[i].Pages[k - 1].Selected)
                            {
                                // ShapeRange sr = doc.Pages[pages[i]].Shapes.All();
                                Page currentPage = doc.Pages[k];
                                Page page = document.Pages[position];
                                page.Activate();
                                page.Name = currentPage.Name;
                                page.SizeWidth = currentPage.SizeWidth;
                                page.SizeHeight = currentPage.SizeHeight;
                                //int layerIndex = page.ActiveLayer.AbsoluteIndex;
                                page.ActiveLayer.Delete();
                                for (int ir = 1; ir <= currentPage.Layers.Count; ir++)
                                {
                                    if (currentPage.Layers[ir].IsGuidesLayer)
                                        continue;
                                    ShapeRange sr = currentPage.Layers[ir].Shapes.All();
                                    sr.Copy();
                                    Layer layer = page.CreateLayer(currentPage.Layers[ir].Name);
                                    layer.Activate();
                                    layer.Paste();
                                }
                                position++;
                               
                            }
                        }
                    }
                    catch { }
                    finally
                    {
                        doc.Close();
                    }

                }
                
            }
            catch (Exception e)
            {

            }
            finally
            {
                
                ControlUI.corelApp.Optimization = false;
                ControlUI.corelApp.Refresh();
            }
        }
    }

}
