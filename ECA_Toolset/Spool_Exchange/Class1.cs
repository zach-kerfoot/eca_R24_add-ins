using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;


namespace Spool_Exchange.UI
{
    public class BrowserWindow : Form
    {
        public BrowserWindow()
        {
            this.Text = "Browser";
            this.Width = 800;
            this.Height = 600;

            var webView = new WebView2
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(webView);

            string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string htmlFilePath = Path.Combine(assemblyDirectory, "UI", "index.html");

            if (!File.Exists(htmlFilePath))
            {
                throw new FileNotFoundException("HTML file not found.", htmlFilePath);
            }

            // Load the HTML file into the WebView2 control
            webView.Source = new Uri($"file:///{htmlFilePath.Replace("\\", "/")}");
        }
    }
}