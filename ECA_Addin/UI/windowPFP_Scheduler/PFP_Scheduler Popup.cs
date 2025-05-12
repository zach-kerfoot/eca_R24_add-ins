using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace ECA_Addin.UI.windowPFP_Scheduler
{
    public class PFP_Scheduler_Window : Form
    {
        private WebView2 _webView;

        public PFP_Scheduler_Window()
        {
            this.Text = "My Web UI";
            this.Width = 800;
            this.Height = 600;

            _webView = new WebView2
            {
                Dock = DockStyle.Fill
            };

            this.Controls.Add(_webView);
            this.Load += async (sender, e) =>
            {
                await _webView.EnsureCoreWebView2Async(null);
                _webView.Source = new Uri("file:///C:/PathToYourHtml/index.html");
            };
        }
    }
}
