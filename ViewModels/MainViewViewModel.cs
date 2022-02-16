using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RevitSchedule
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;
        public UIApplication uiapp { get; set; }
        public UIDocument uidoc { get; set; }
        public Document doc { get; set; }



        public List<FamilyInstanceWrapper> ElectrEquipAll { get; } = new List<FamilyInstanceWrapper>();
        public List<ViewSchedule> viewSchedules { get; } = new List<ViewSchedule>();
        public DelegateCommand SaveCommand { get; }

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            SaveCommand = new DelegateCommand(OnSaveCommand);
            uiapp = _commandData.Application;
            uidoc = uiapp.ActiveUIDocument;
            doc = uidoc.Document;

            ElectrEquipAll = new FilteredElementCollector(doc)
                                           .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                                           .WhereElementIsNotElementType()
                                           .Cast<Element>()
                                           .Cast<FamilyInstance>()
#if R2019
                                            .Where(x => x.MEPModel.AssignedElectricalSystems != null)
#elif R2022
                                            .Where(x => x.MEPModel.GetAssignedElectricalSystems().Count > 0)
#endif

                                           .Select(x => new FamilyInstanceWrapper(x))
                                           .ToList();

            viewSchedules = new FilteredElementCollector(doc)
                               .OfClass(typeof(ViewSchedule))
                               .Cast<ViewSchedule>()
                               .Where(x => x.Name.Contains("Таблица расчета нагрузок"))
                               .ToList();

            // Отметить щиты с уже существующими спецификациями
            foreach (var ElectrEquip in ElectrEquipAll)
            {
                if (viewSchedules.Any(x => x.Name.Contains(ElectrEquip.Name)))
                    ElectrEquip.IsSelected = true;
            }
        }

        private void OnSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            foreach (var ElectrEquip in ElectrEquipAll.Where(x => x.IsSelected))
            {
                ElectrEquip.GetAllConsumers(ElectrEquip.FamilyInstance);
                ElectrEquip.GetAllElectrClass(doc);
                ElectrEquip.GetAllSumm();
                using (Transaction ts = new Transaction(doc, "Работа с " + ElectrEquip.Name))
                {
                    ts.Start();
                    ElectrSchedule electrSchedule = new ElectrSchedule(doc, ElectrEquip.Name);
                    electrSchedule.AddRowsWithElectrClass(ElectrEquip.ElectrClassAll);
                    ts.Commit();
                }
            }
            MessageBox.Show("Спецификации обновлены \n Находятся в разделе: Расчетные данные(автоматически)");
            RaiseCloseRequest();
        }

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
