#region Namespaces
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Resources;
using System.IO;

#endregion

namespace PFP_Scheduler
{
    internal class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            try
            {
                a.CreateRibbonTab("ECA Toolset"); //Create Tab for tools
            }
            catch (Exception)
            {
                //Tab already exists
            }

            RibbonPanel panel = a.CreateRibbonPanel("ECA Toolset", "Spooling"); //Create Ribbon in toolset

            string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            PushButtonData buttonData = new PushButtonData(
                "PFP_Scheduler",
                "PFP Scheduler",
                assemblyPath,
                "PFP_Scheduler.Command");

            PushButton pushButton = (PushButton)panel.AddItem(buttonData);
            pushButton.ToolTip = "Creates scheuldes for the selected PFPs";
            pushButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/PFP_Scheduler;component/Resources/PFP_Scheduler.png"));


            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
        
    }
}
