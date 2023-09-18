using System.Drawing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace ImportPages
{
    internal class ProcessFiles
    {
        public event Action<DataFile> FileReady;


        void processFile(object param)
        {
            FileInfo[] files = (FileInfo[])param;
            for (int r = 0; r < files.Length; r++)
            {
                if (files[r].Extension != ".cdr")
                    continue;
                try
                {
                    DataFile Data = SetDataNewVersions(files[r]);

                    if (FileReady != null)
                        FileReady(Data);
                }
                catch
                {
                    try
                    {
                        DataFile Data = SetDataOldVersions(files[r]);
                        if (FileReady != null)
                            FileReady(Data);
                    }
                    catch
                    { }
                }
            }

        }
        private DataFile SetDataNewVersions(FileInfo file)
        {
            DataFile Data = new DataFile();


            Data.Name = file.FullName;
            using (ZipArchive zipFile = new ZipArchive(file.Open(FileMode.Open)))
            {
                Data.NumPages = GetNumPages(zipFile);
                Data.Version = GetCorelVersion(zipFile);
                Data.Pages = GetPagesPreview(zipFile);
                string pageDimension;
                GetMetaData(zipFile, out pageDimension);
                Data.PageDimension = pageDimension;
            }
            return Data;
        }
        #region old methods
        private DataFile SetDataOldVersions(FileInfo file)
        {
            DataFile Data = new DataFile();
            Data.Name = file.FullName;
            using (ZipArchive zipFile = new ZipArchive(file.Open(FileMode.Open)))
            {

                Data.NumPages = GetNumPagesOld(zipFile);
                Data.Version = GetCorelVersionOld(zipFile);
                Data.Pages = GetPagesPreviewOld(zipFile, Data.NumPages);
                string pageDimension;
                GetMetaDataOld(zipFile, out pageDimension);
                Data.PageDimension = pageDimension;
            }
            return Data;
        }
        private void GetMetaDataOld(ZipArchive zipFile, out string pageDimension)
        {
            string xml = GetStringFromEntry("metadata/metadata.xml", zipFile);
            pageDimension = "";
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            XmlNodeList elList = xmlDoc.GetElementsByTagName("rdf:Description")[0].ChildNodes;
            for (int i = 0; i < elList.Count; i++)
            {
                XmlNode node = elList[i];
                switch (node.Name)
                {

                    case "PageDimensions":
                        pageDimension = node.InnerText;
                        break;

                }
            }
        }
        private int GetNumPagesOld(ZipArchive zipFile)
        {
            string xml = GetStringFromEntry("metadata/metadata.xml", zipFile);
            int numPages = 0;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlNodeList nodes = doc.LastChild.FirstChild.ChildNodes[0].ChildNodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Name == "NumPages")
                {
                    numPages = Int32.Parse(nodes[i].InnerText);
                    break;
                }
            }
            return numPages;

        }
        private ObservableCollection<DataPage> GetPagesPreviewOld(ZipArchive zipFile, int numPages)
        {
            ObservableCollection<DataPage> temp = new ObservableCollection<DataPage>();
            int i = 1;
            while (i <= numPages)
            {
                Bitmap preview = GetBitmapFromEntry(string.Format("metadata/thumbnails/page{0}.bmp", i), zipFile);
                temp.Add(new DataPage(string.Format("Page {0}", i), preview));
                i++;
            }

            return temp;
        }
        private int GetCorelVersionOld(ZipArchive zipFile)
        {
            string xml = GetStringFromEntry("metadata/metadata.xml", zipFile);
            int corelVersion = 0;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlNodeList elList = doc.GetElementsByTagName("rdf:Description")[0].ChildNodes;
            for (int i = 0; i < elList.Count; i++)
            {
                XmlNode node = elList[i];
                if (node.Name == "CoreVersion")
                {
                    corelVersion = Int32.Parse(node.InnerText.Substring(0, 2));
                    break;
                }
            }
            return corelVersion;
        }
        #endregion
     
        private void GetMetaData(ZipArchive zipFile, out string pageDimension)
        {
            string xml = GetStringFromEntry("META-INF/metadata.xml", zipFile);

            pageDimension = "";

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            XmlNodeList elList = xmlDoc.GetElementsByTagName("rdf:Description")[0].ChildNodes;
            for (int i = 0; i < elList.Count; i++)
            {
                XmlNode node = elList[i];
                switch (node.Name)
                {
                    case "cdrinfo:PageDimensions":
                        pageDimension = node.InnerText;
                        break;

                }
            }
        }

        private int GetNumPages(ZipArchive zipFile)
        {

            string xml = GetStringFromEntry("META-INF/container.xml", zipFile);
            int numPages = 0;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlNodeList nodes = doc.LastChild.FirstChild.ChildNodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Attributes["crl:file-kind"].InnerText == "page")
                    numPages++;
            }
            return numPages;
        }

        private int GetCorelVersion(ZipArchive zipFile)
        {
            string xml = GetStringFromEntry("META-INF/metadata.xml", zipFile);
            int corelVersion = 0;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlNodeList elList = doc.GetElementsByTagName("rdf:Description")[0].ChildNodes;
            for (int i = 0; i < elList.Count; i++)
            {
                XmlNode node = elList[i];
                if (node.Name == "cdr:CoreVersion")
                {
                    corelVersion = Int32.Parse(node.InnerText.Substring(0, 2));
                    break;
                }
            }
            return corelVersion;
        }

        private ObservableCollection<DataPage> GetPagesPreview(ZipArchive zipFile)
        {
            ObservableCollection<DataPage> temp = new ObservableCollection<DataPage>();
            string xml = GetStringFromEntry("META-INF/container.xml", zipFile);
            int numPage = 1;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlNodeList nodes = doc.LastChild.FirstChild.ChildNodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Attributes["crl:file-kind"].InnerText == "page")
                {
                    string pageName = nodes[i].Attributes["crl:caption"].InnerText;
                    pageName = pageName.Substring(pageName.IndexOf('[') + 1, pageName.IndexOf(']') - 2);
                    Bitmap preview = GetBitmapFromEntry(string.Format("previews/page{0}.png", numPage), zipFile);
                    temp.Add(new DataPage(pageName, preview));
                    numPage++;
                }
            }
            return temp;
        }
        private string GetStringFromEntry(string entry, ZipArchive zipFile)
        {
            string text = "";
            ZipArchiveEntry textEntry = zipFile.GetEntry(entry);
            using (Stream stEntry = textEntry.Open())
            {
                using (StreamReader sr = new StreamReader(stEntry))
                {
                    text = sr.ReadToEnd();
                }
            }
            return text;
        }
        private Bitmap GetBitmapFromEntry(string entry, ZipArchive zipFile)
        {
            try
            {
                ZipArchiveEntry thumbEntry = zipFile.GetEntry(entry);
                Bitmap bitmap;
                using (Stream thumbStream = thumbEntry.Open())
                {
                    bitmap = new Bitmap(thumbStream);

                }
                return bitmap;
            }
            catch { return null; }
        }


        public void ProcessStart(string[] dirs)
        {
            List<string> directories = dirs.ToList<string>();
            directories.Sort();

            for (int i = 0; i < directories.Count; i++)
            {

                DirectoryInfo dir = new DirectoryInfo(directories[i]);
                FileInfo[] files;
                if (dir.Exists)
                {
                    files = dir.GetFiles();


                }
                else
                {
                    files = new FileInfo[1] { new FileInfo(directories[i]) };

                }
                List<FileInfo> fileInfos = files.ToList<FileInfo>();
                IEnumerable<FileInfo> query = fileInfos.OrderBy(file => file.Name);
                files = query.ToArray<FileInfo>();

                ProcessStart(files);
            }
        }
        public void ProcessStart(FileInfo[] files)
        {
            processFile(files);
            //Thread th = new Thread(new ParameterizedThreadStart(processFile));
            //th.IsBackground = true;
            //th.Start(files);
        }
    }
}


