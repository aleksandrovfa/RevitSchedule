using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitSchedule
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var mainView = new MainView(commandData);
#if R2019
            TaskDialog.Show("RevitSchedule", "Hello Revit 2019");
#elif R2022
            TaskDialog.Show("RevitSchedule", "Hello Revit 2022");
#endif
            mainView.ShowDialog();
            return Result.Succeeded;
        }
    }
}
