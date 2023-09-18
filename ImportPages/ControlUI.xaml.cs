using Corel.Interop.VGCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using corel = Corel.Interop.VGCore;

namespace ImportPages
{
    public partial class ControlUI : UserControl
    {
        public static corel.Application corelApp;

        public ControlUI(object app)
        {

            try
            {
                corelApp = app as corel.Application;
                var dsf = new DataSource.DataSourceFactory();
                dsf.AddDataSource("ImportPagesDS", typeof(DataSource.ImportPagesDataSource));
                dsf.Register();

            }
            catch
            {
                global::System.Windows.MessageBox.Show("VGCore Erro");
            }

        }
     



    }
}
