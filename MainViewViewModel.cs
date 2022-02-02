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

        //public List<FamilyInstance> Consumers { get; } = new List<FamilyInstance>();

        public List<FamilyInstanceWrapper> ElectrEquipAll { get; } = new List<FamilyInstanceWrapper>();
        public List<ViewSchedule> viewSchedules { get; } = new List<ViewSchedule>();
        //public Element SelectedElEq { get; set; }
        //public List<Level> Levels { get; }
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
                                           .Where(x => x.MEPModel.GetAssignedElectricalSystems().Count > 0)
                                           .Select(x => new FamilyInstanceWrapper(x))
                                           .ToList();

            viewSchedules = new FilteredElementCollector(doc)
                               .OfClass(typeof(ViewSchedule))
                               .Cast<ViewSchedule>()
                               .Where(x => x.Name.Contains("Таблица расчета нагрузок"))
                               .ToList();

            // Отметить щиты с уже существующими спецификациями
            foreach (var item in ElectrEquipAll)
            {
                if (viewSchedules.Any(x => x.Name.Contains(item.Name)))
                    item.IsSelected = true;
            }
        }

        private void OnSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            foreach (var ElectrEquip in ElectrEquipAll.Where(x => x.IsSelected))
            {
                //MessageBox.Show(ElectrEquip.Name);
                ElectrEquip.GetAllConsumers(ElectrEquip.FamilyInstance);
                ElectrEquip.GetAllElectrClass(doc);
                ElectrEquip.GetAllSumm();
                ElectrSchedule electrSchedule = new ElectrSchedule(doc, ElectrEquip.Name);
                electrSchedule.AddRowsWithElectrClass(ElectrEquip.ElectrClassAll);


            }

            #region comments
            //FamilyInstance ElEq = SelectedElEq as FamilyInstance;
            //Rec(ElEq);

            //List<ElectrClass> electrClasses = new List<ElectrClass>();

            //foreach (var Consumer in ElectrEquip.Consumers)
            //{
            //    ConsumerParam consumerParam = new ConsumerParam(Consumer, doc);
            //    if (!electrClasses.Any(x => x.Id == consumerParam.ClassId))
            //    {
            //        ElectricalLoadClassification load = doc.GetElement(consumerParam.ClassId) as ElectricalLoadClassification;
            //        ElectrClass electrClass = new ElectrClass(load);
            //        electrClasses.Add(electrClass);
            //    }
            //    ElectrClass electr = electrClasses.Find(x => x.Id == consumerParam.ClassId);
            //    electr.S_all_notConvert = electr.S_all_notConvert + consumerParam.S_notConvert;
            //    electr.P_all_notConvert = electr.P_all_notConvert + consumerParam.P_notConvert;
            //    electr.S_all = electr.S_all + consumerParam.S;
            //    electr.P_all = electr.P_all + consumerParam.P;
            //    electr.Q_all = electr.Q_all + consumerParam.Q;
            //    electr.Count++;
            //}

            ////electrClasses.Select(x => x.Get_Kc(doc));
            //foreach (var electr in electrClasses)
            //{
            //    electr.Get_Kc(doc);
            //}














            //foreach (var electr in electrClasses)
            //{
            //    ElectricalDemandFactorDefinition demandFactor = doc.GetElement(electr.LoadClass.DemandFactorId) as ElectricalDemandFactorDefinition;
            //    switch (demandFactor.RuleType)
            //    {
            //        case ElectricalDemandFactorRule.Constant:
            //            electr.Kc = demandFactor.GetValues().Single().Factor;
            //            break;
            //        case ElectricalDemandFactorRule.LoadTable:
            //        case ElectricalDemandFactorRule.LoadTablePerPortion:

            //            var dfLoad = demandFactor.GetValues().Single(x => x.MinRange < electr.P_all_notConvert && x.MaxRange >= electr.P_all_notConvert);
            //            if (dfLoad.MaxRange == 1e+30)
            //            {
            //                electr.Kc = dfLoad.Factor;
            //                break;
            //            }
            //            if (dfLoad.MinRange == 0)
            //            {
            //                electr.Kc = dfLoad.Factor;
            //                break;
            //            }
            //            var dfLoad1 = demandFactor.GetValues().Single(x => x.MaxRange == dfLoad.MinRange);
            //            electr.Kc = dfLoad1.Factor - (dfLoad1.Factor - dfLoad.Factor) / 
            //                (dfLoad.MaxRange - dfLoad.MinRange) * (electr.P_all_notConvert - dfLoad.MinRange);
            //            break;
            //        case ElectricalDemandFactorRule.QuantityTable:
            //        case ElectricalDemandFactorRule.QuantityTablePerPortion:
            //            // то же самое что и выше только по количеству элементов
            //            var dfQuan = demandFactor.GetValues().Single(x => x.MinRange < electr.Count && x.MaxRange >= electr.Count);
            //            if (dfQuan.MaxRange == 1e+30)
            //            {
            //                electr.Kc = dfQuan.Factor;
            //                break;
            //            }
            //            if (dfQuan.MinRange == 0)
            //            {
            //                electr.Kc = dfQuan.Factor;
            //                break;
            //            }
            //            var dfQuan1 = demandFactor.GetValues().Single(x => x.MaxRange == dfQuan.MinRange);
            //            electr.Kc = dfQuan1.Factor - (dfQuan1.Factor - dfQuan.Factor) / 
            //                (dfQuan.MaxRange - dfQuan.MinRange) *(electr.Count - dfQuan.MinRange);
            //            break;
            //    }
            //    MessageBox.Show(electr.Kc.ToString() + " " + electr.Name);
            //}

            //foreach (var ElEqparam in ElEqparams)
            //{
            //    string[] nameArr = ElEqparam.Definition.Name.Split('_');
            //    int index = Array.IndexOf(nameArr, "ETLe");
            //    string name = nameArr[index + 1];
            //    if (!electrClasses.Any(x => x.Name == name))
            //    {
            //        ElectrClass electrClass = new ElectrClass(name);
            //        electrClasses.Add(electrClass);
            //    }
            //    ElectrClass electr = electrClasses.Find(x => x.Name == name);


            //    if (nameArr.Contains(" подключено"))
            //        electr.S_all = ElEqparam.AsValueString();
            //    else if (nameArr.Contains(", подключенный, ток"))
            //        electr.I_all = ElEqparam.AsValueString();
            //    else if (nameArr.Contains(", расчетная нагрузка, ток"))
            //        electr.I_calc = ElEqparam.AsValueString();
            //    else if (nameArr.Contains("Коэффициент спроса нагрузки "))
            //        electr.Kc = ElEqparam.AsValueString();
            //    else if (nameArr.Contains("Расчетная нагрузка "))
            //        electr.S_calc = ElEqparam.AsValueString();
            //}
            //MessageBox.Show("dfs");

            #endregion 


            using (Transaction ts = new Transaction(doc, "sdf"))
            {
                ts.Start();

                //ViewSchedule schedules = new FilteredElementCollector(doc)
                //               .OfClass(typeof(ViewSchedule))
                //               .Cast<ViewSchedule>()
                //               .Single(x => x.Name == "ЭОМ_П_Таблица расчета нагрузок ШАБЛОН");


                List<ViewSchedule> schedules = new FilteredElementCollector(doc)
                               .OfClass(typeof(ViewSchedule))
                               .Cast<ViewSchedule>()
                               .ToList();
                //string nameSample = "Таблица расчета нагрузок ШАБЛОН";
                //if (!schedules.Any(x => x.Name.Contains(nameSample)))
                //{
                //    ViewSchedule shablon = ViewSchedule.CreateSchedule(doc, Category.GetCategory(doc, BuiltInCategory.OST_Entourage).Id);
                //    shablon.Name = nameSample;
                //    shablon.LookupParameter("ADSK_Назначение вида").Set("Расчетные данные (автоматически)");
                //    SchedulableField schedulableField = shablon.Definition.GetSchedulableFields().Single(x => x.GetName(doc) == "Группа модели");
                //    shablon.Definition.AddField(schedulableField);
                //    TableSectionData header = shablon.GetTableData().GetSectionData(SectionType.Header);
                //    TableSectionData body = shablon.GetTableData().GetSectionData(SectionType.Body);

                //    #region Create table and set SizeTable
                //    double columnWidth = 0.6;
                //    header.ClearCell(0, 0);
                //    body.SetColumnWidth(0, columnWidth);

                //    for (int i = 1; i < 10; i++)
                //        header.InsertColumn(i);

                //    header.SetColumnWidth(0, 0.025);
                //    columnWidth = columnWidth - 0.025;
                //    header.SetColumnWidth(1, 0.15);
                //    columnWidth = columnWidth - 0.15;

                //    for (int i = 2; i < 10; i++)
                //        header.SetColumnWidth(i, columnWidth / 8);

                //    header.InsertRow(1);
                //    header.InsertRow(2);
                //    header.SetRowHeight(2,header.GetRowHeight(2)/2);
                //    header.InsertRow(3);
                //    header.SetRowHeight(3, header.GetRowHeight(3)/2);
                //    #endregion


                //    #region Add text in cells and merge cells
                //    header.SetCellText(0, 0, nameSample);
                //    TableMergedCell mergedCell = header.GetMergedCell(0, 0);
                //    mergedCell.Right = 9;
                //    header.MergeCells(mergedCell);

                //    header.SetCellText(1, 0, "№");
                //    TableMergedCell mergedCell1 = header.GetMergedCell(1, 0);
                //    mergedCell1.Bottom = 3;
                //    header.MergeCells(mergedCell1);


                //    header.SetCellText(1, 1, "Наименование потребителя");
                //    TableMergedCell mergedCell2 = header.GetMergedCell(1, 1);
                //    mergedCell2.Bottom = 3;
                //    header.MergeCells(mergedCell2);


                //    header.SetCellText(1, 2, "Установл. мощность");
                //    header.SetCellText(2, 2, "Ру");
                //    header.SetCellText(3, 2, "кВт");

                //    header.SetCellText(1, 3, "Коэфф. спроса");
                //    header.SetCellText(2, 3, "Кс");
                //    TableMergedCell mergedCell3 = header.GetMergedCell(2, 3);
                //    mergedCell3.Bottom = 3;
                //    header.MergeCells(mergedCell3);

                //    header.SetCellText(1, 4, "Коэффициент мощности");
                //    TableMergedCell mergedCell5 = header.GetMergedCell(1, 4);
                //    mergedCell5.Right = 5;
                //    header.MergeCells(mergedCell5);
                //    header.SetCellText(2, 4, "cosφ");
                //    TableMergedCell mergedCell4 = header.GetMergedCell(2, 4);
                //    mergedCell4.Bottom = 3;
                //    header.MergeCells(mergedCell4);

                //    header.SetCellText(2, 5, "tgφ");
                //    TableMergedCell mergedCell6 = header.GetMergedCell(2, 5);
                //    mergedCell6.Bottom = 3;
                //    header.MergeCells(mergedCell6);


                //    header.SetCellText(1, 6, "Расчетные нагрузки");
                //    TableMergedCell mergedCell7 = header.GetMergedCell(1, 6);
                //    mergedCell7.Right = 8;
                //    header.MergeCells(mergedCell7);
                //    header.SetCellText(2, 6, "Рр");
                //    header.SetCellText(3, 6, "кВт");

                //    header.SetCellText(2, 7, "Qр");
                //    header.SetCellText(3, 7, "кВтАр");


                //    header.SetCellText(2, 8, "Sр");
                //    header.SetCellText(3, 8, "кВА");

                //    header.SetCellText(1, 9, "I");
                //    TableMergedCell mergedCell8 = header.GetMergedCell(1, 9);
                //    mergedCell8.Bottom = 2;
                //    header.MergeCells(mergedCell8);
                //    header.SetCellText(3, 9, "А");
                //    #endregion

                //}

                /////////////////////////////////////////////////////////////////////////////////////////
               

                //TableSectionData table = schedules.GetTableData().GetSectionData(SectionType.Header);
                //int rowStart = 4;
                //while (table.CanRemoveRow(rowStart))
                //    table.RemoveRow(rowStart);

                //FamilyInstanceWrapper eleq = ElectrEquipAll.First(x => x.IsSelected);
                //table.SetCellText(0, 0, "Таблица расчета нагрузок " + eleq.Name);
                //for (int i = 1; i <= eleq.ElectrClassAll.Count; i++)
                //{
                //    ElectrClass el = eleq.ElectrClassAll[i - 1];
                //    table.InsertRow(rowStart + i-1);
                //    if(el.Name != "Итоги")
                //        table.SetCellText(rowStart + i-1, 0, i.ToString());
                //    table.SetCellText(rowStart + i-1, 1, el.Name.Replace("ETL_",""));
                //    table.SetCellText(rowStart + i-1, 2, (el.P_all/1000).ToString("F2"));
                //    table.SetCellText(rowStart + i-1, 3, el.Kc.ToString("F3"));
                //    table.SetCellText(rowStart + i-1, 4, el.cos_fi.ToString("F2"));
                //    table.SetCellText(rowStart + i-1, 5, el.tg_fi.ToString("F2"));
                //    table.SetCellText(rowStart + i-1, 6, (el.P_calc/1000).ToString("F2"));
                //    table.SetCellText(rowStart + i-1, 7, (el.Q_calc / 1000).ToString("F2"));
                //    table.SetCellText(rowStart + i-1, 8, (el.S_calc / 1000).ToString("F2"));
                //}
                ts.Commit();
            }
            RaiseCloseRequest();
        }

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
