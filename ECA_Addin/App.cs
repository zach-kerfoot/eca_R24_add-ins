#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Resources;
using System.IO;

#endregion

namespace ECA_Addin
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

            RibbonPanel spoolingPanel = a.CreateRibbonPanel("ECA Toolset", "Spooling"); //Creates Spooling Ribbon

            RibbonPanel panel = a.CreateRibbonPanel("ECA Toolset", "Utilities"); // Creates Utilities Ribbon

            string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //Create Button Images
            BitmapImage pfpSchedulerImage = new BitmapImage(new Uri("pack://application:,,,/ECA_Addin;component/UI/Button Icons/PFP_Scheduler.png"));
            BitmapImage spoolExchangeImage = new BitmapImage(new Uri("pack://application:,,,/ECA_Addin;component/UI/Button Icons/Spool_Exchange.png"));
            BitmapImage modelCloneImage = new BitmapImage(new Uri("pack://application:,,,/ECA_Addin;component/UI/Button Icons/Model_Clone.png"));


            // Create PushButtonData for PFP Scheduler
            PushButtonData pfpSchedulerButtonData = new PushButtonData(
                "PFP_Scheduler",
                "PFP\nScheduler",
                assemblyPath,
                "ECA_Addin.PFP_Scheduler");

            // Add PFP Scheduler button to the panel
            PushButton pfpSchedulerButton = (PushButton)spoolingPanel.AddItem(pfpSchedulerButtonData);
            pfpSchedulerButton.ToolTip = "Creates schedules for the selected PFPs";
            pfpSchedulerButton.LargeImage = pfpSchedulerImage;
            

            // Create PushButtonData for Spool Exchange
            PushButtonData spoolExchangeButtonData = new PushButtonData(
                "Spool_Exchange",
                "Spool\nExchange",
                assemblyPath,
                "ECA_Addin.Spool_Exchange");

            // Add Spool Exchange button to the panel
            PushButton spoolExchangeButton = (PushButton)spoolingPanel.AddItem(spoolExchangeButtonData);
            spoolExchangeButton.ToolTip = "Facilitates exchanging spools within the model";
            spoolExchangeButton.LargeImage = spoolExchangeImage;

            // Create PushButtonData for Model_Clone
            PushButtonData modelCloneData = new PushButtonData(
                "Model Clone",
                "Model\nClone",
                assemblyPath,
                "ECA_Addin.Model_Clone");
            PushButton modelCloneButton = (PushButton)panel.AddItem(modelCloneData);
            modelCloneButton.ToolTip = "Copy elements from linked model to current project";
            modelCloneButton.LargeImage = modelCloneImage;



            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }

    }
}