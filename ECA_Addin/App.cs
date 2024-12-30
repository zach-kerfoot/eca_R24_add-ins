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

            RibbonPanel panel = a.CreateRibbonPanel("ECA Toolset", "Spooling"); //Create Ribbon in toolset

            string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            // Create PushButtonData for PFP Scheduler
            PushButtonData pfpSchedulerButtonData = new PushButtonData(
                "PFP_Scheduler",
                "PFP\nScheduler",
                assemblyPath,
                "PFP_Scheduler.Command");

            // Add PFP Scheduler button to the panel
            PushButton pfpSchedulerButton = (PushButton)panel.AddItem(pfpSchedulerButtonData);
            pfpSchedulerButton.ToolTip = "Creates schedules for the selected PFPs";

            // Create PushButtonData for Spool Exchange
            PushButtonData spoolExchangeButtonData = new PushButtonData(
                "Spool_Exchange",
                "Spool\nExchange",
                assemblyPath,
                "Spool_Exchange.Command");

            // Add Spool Exchange button to the panel
            PushButton spoolExchangeButton = (PushButton)panel.AddItem(spoolExchangeButtonData);
            spoolExchangeButton.ToolTip = "Facilitates exchanging spools within the model";



            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }

    }
}