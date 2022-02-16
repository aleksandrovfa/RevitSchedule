using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitSchedule
{
    public class ElectrSchedule
    {

        public ViewSchedule ElectrViewSchedule { get; set; } //  ТРН щита

        public ViewSchedule Host { get; set; } //ШАБЛОН

        private Document Doc { get; set; }

        public string Name { get; set; } 

        public ElectrSchedule(Document doc, string electrEquipName)
        {
            Doc = doc;
            Name = electrEquipName;
            List<ViewSchedule> schedules = new FilteredElementCollector(Doc)
                               .OfClass(typeof(ViewSchedule))
                               .Cast<ViewSchedule>()
                               .ToList();

            if (schedules.Any(x => x.Name.Contains("Таблица расчета нагрузок ШАБЛОН")))
            {
                Host = schedules.Single(x => x.Name.Contains("Таблица расчета нагрузок ШАБЛОН"));
            }
            else
            {
                Host = CreateSampleViewSchedule();
            }



            if (schedules.Any(x => x.Name.Contains("Таблица расчета нагрузок " + Name)))
            {
                ElectrViewSchedule = schedules.Single(x => x.Name.Contains("Таблица расчета нагрузок " + Name));
                ElectrViewSchedule = RemoveExtraRows(ElectrViewSchedule);
            }
            else
            {
                ElectrViewSchedule = CreateElectrViewSchedule(Host);
            }
        }

        private ViewSchedule RemoveExtraRows(ViewSchedule electrViewSchedule)
        {
            int numberOfRows = Host.GetTableData().GetSectionData(SectionType.Header).NumberOfRows;
            TableSectionData header = electrViewSchedule.GetTableData().GetSectionData(SectionType.Header);
            while (header.CanRemoveRow(numberOfRows))
            {
                header.RemoveRow(numberOfRows);
            }
            return electrViewSchedule;
        }

        private ViewSchedule CreateElectrViewSchedule(ViewSchedule host)
        {

            ElementId electrViewScheduleId = host.Duplicate(ViewDuplicateOption.Duplicate);
            ViewSchedule electrViewSchedule = Doc.GetElement(electrViewScheduleId) as ViewSchedule;
            electrViewSchedule.Name = host.Name.Replace("ШАБЛОН", Name);
            try
            {
                electrViewSchedule.LookupParameter("ADSK_Назначение вида").Set("Расчетные данные (автоматически)");
            }
            catch (Exception)
            {
                TaskDialog.Show("Ошибка!", "Не найден параметр (ADSK_Назначение вида), спецификация будет создана без назначения");
            }
            TableSectionData table = electrViewSchedule.GetTableData().GetSectionData(SectionType.Header);
            string nameTable = table.GetCellText(0, 0);
            table.SetCellText(0, 0, nameTable.Replace("ШАБЛОН", Name));

            Doc.Regenerate();

            return electrViewSchedule;
        }

        private ViewSchedule CreateSampleViewSchedule()
        {
            List<ViewSchedule> schedules = new FilteredElementCollector(Doc)
                               .OfClass(typeof(ViewSchedule))
                               .Cast<ViewSchedule>()
                               .ToList();


            if (schedules.Any(x => x.Name.Contains("Таблица расчета нагрузок ЩО-2")))
            {
                ViewSchedule host = schedules.Single(x => x.Name.Contains("Таблица расчета нагрузок ЩО-2"));

                ElementId sampleId = host.Duplicate(ViewDuplicateOption.Duplicate);
                ViewSchedule sample = Doc.GetElement(sampleId) as ViewSchedule;
                sample.Name = host.Name.Replace("ЩО-2", "ШАБЛОН");
                sample.LookupParameter("ADSK_Назначение вида").Set("Расчетные данные (автоматически)");
                TableSectionData table = sample.GetTableData().GetSectionData(SectionType.Header);
                table.SetCellText(0, 0, "Таблица расчета нагрузок ШАБЛОН");
                while (table.CanRemoveRow(4))
                {
                    table.RemoveRow(4);
                }

                Doc.Regenerate();

                return sample;
            }
            else
            {
                string nameSample = "Таблица расчета нагрузок ШАБЛОН";
                ViewSchedule sample = ViewSchedule.CreateSchedule(Doc, Category.GetCategory(Doc, BuiltInCategory.OST_Entourage).Id);
                sample.Name = nameSample;
                try
                {
                    sample.LookupParameter("ADSK_Назначение вида").Set("Расчетные данные (автоматически)");
                }
                catch (Exception)
                {
                    TaskDialog.Show("Ошибка!", "Не найден параметр (ADSK_Назначение вида), спецификация будет создана без назначения");
                }
                SchedulableField schedulableField = sample.Definition.GetSchedulableFields().Single(x => x.GetName(Doc) == "Группа модели");
                sample.Definition.AddField(schedulableField);
                TableSectionData header = sample.GetTableData().GetSectionData(SectionType.Header);
                TableSectionData body = sample.GetTableData().GetSectionData(SectionType.Body);

                #region Create table and set SizeTable
                double columnWidth = 0.6;
                header.ClearCell(0, 0);
                body.SetColumnWidth(0, columnWidth);

                for (int i = 1; i < 10; i++)
                    header.InsertColumn(i);

                header.SetColumnWidth(0, 0.025);
                columnWidth = columnWidth - 0.025;
                header.SetColumnWidth(1, 0.15);
                columnWidth = columnWidth - 0.15;

                for (int i = 2; i < 10; i++)
                    header.SetColumnWidth(i, columnWidth / 8);

                header.InsertRow(1);
                header.InsertRow(2);
                header.SetRowHeight(2, header.GetRowHeight(2) / 2);
                header.InsertRow(3);
                header.SetRowHeight(3, header.GetRowHeight(3) / 2);
                #endregion

                #region Add text in cells and merge cells
                header.SetCellText(0, 0, nameSample);
                TableMergedCell mergedCell = header.GetMergedCell(0, 0);
                mergedCell.Right = 9;
                header.MergeCells(mergedCell);

                header.SetCellText(1, 0, "№");
                TableMergedCell mergedCell1 = header.GetMergedCell(1, 0);
                mergedCell1.Bottom = 3;
                header.MergeCells(mergedCell1);


                header.SetCellText(1, 1, "Наименование потребителя");
                TableMergedCell mergedCell2 = header.GetMergedCell(1, 1);
                mergedCell2.Bottom = 3;
                header.MergeCells(mergedCell2);


                header.SetCellText(1, 2, "Установл. мощность");
                header.SetCellText(2, 2, "Ру");
                header.SetCellText(3, 2, "кВт");

                header.SetCellText(1, 3, "Коэфф. спроса");
                header.SetCellText(2, 3, "Кс");
                TableMergedCell mergedCell3 = header.GetMergedCell(2, 3);
                mergedCell3.Bottom = 3;
                header.MergeCells(mergedCell3);

                header.SetCellText(1, 4, "Коэффициент мощности");
                TableMergedCell mergedCell5 = header.GetMergedCell(1, 4);
                mergedCell5.Right = 5;
                header.MergeCells(mergedCell5);
                header.SetCellText(2, 4, "cosφ");
                TableMergedCell mergedCell4 = header.GetMergedCell(2, 4);
                mergedCell4.Bottom = 3;
                header.MergeCells(mergedCell4);

                header.SetCellText(2, 5, "tgφ");
                TableMergedCell mergedCell6 = header.GetMergedCell(2, 5);
                mergedCell6.Bottom = 3;
                header.MergeCells(mergedCell6);


                header.SetCellText(1, 6, "Расчетные нагрузки");
                TableMergedCell mergedCell7 = header.GetMergedCell(1, 6);
                mergedCell7.Right = 8;
                header.MergeCells(mergedCell7);
                header.SetCellText(2, 6, "Рр");
                header.SetCellText(3, 6, "кВт");

                header.SetCellText(2, 7, "Qр");
                header.SetCellText(3, 7, "кВтАр");


                header.SetCellText(2, 8, "Sр");
                header.SetCellText(3, 8, "кВА");

                header.SetCellText(1, 9, "I");
                TableMergedCell mergedCell8 = header.GetMergedCell(1, 9);
                mergedCell8.Bottom = 2;
                header.MergeCells(mergedCell8);
                header.SetCellText(3, 9, "А");
                #endregion
                Doc.Regenerate();

                return sample;
            }
        }

        public void AddRowsWithElectrClass(List<ElectrClass> electrs)
        {

            TableSectionData table = ElectrViewSchedule.GetTableData().GetSectionData(SectionType.Header);
            int rowStart = Host.GetTableData().GetSectionData(SectionType.Header).NumberOfRows;
            for (int i = 0; i < electrs.Count; i++)
            {
                ElectrClass el = electrs[i];
                table.InsertRow(rowStart + i);
                if (el.Name != "Итого:")
                    table.SetCellText(rowStart + i, 0, (i + 1).ToString());
                table.SetCellText(rowStart + i, 1, el.Name.Replace("ETL_", ""));
                table.SetCellText(rowStart + i, 2, (el.P_all / 1000).ToString("F3"));
                table.SetCellText(rowStart + i, 3, el.Kc.ToString("F3"));
                table.SetCellText(rowStart + i, 4, el.cos_fi.ToString("F2"));
                table.SetCellText(rowStart + i, 5, el.tg_fi.ToString("F2"));
                table.SetCellText(rowStart + i, 6, (el.P_calc / 1000).ToString("F3"));
                table.SetCellText(rowStart + i, 7, (el.Q_calc / 1000).ToString("F3"));
                table.SetCellText(rowStart + i, 8, (el.S_calc / 1000).ToString("F3"));
            }

        }
    }
}
