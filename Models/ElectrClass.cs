using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RevitSchedule
{
    public class ElectrClass
    {
        public ElectricalLoadClassification LoadClass { get; set; }
        public string Name {get; set;}
        public ElementId Id { get; set; }
        public double S_all_notConvert { get; set; }
        public double P_all_notConvert { get; set; }

        public double S_all { get; set; }
        public double P_all { get; set; }
        public double Q_all { get; set; }
        public double I_all { get; set; }
        
        public double S_calc { get;  set; }
        public double P_calc { get;  set; }
        public double Q_calc { get;  set; }
        public double I_calc { get;  set; }

        public double cos_fi { get; private set; }
        public double tg_fi { get; private set; }

        public double Kc { get; set; }
        public int Count { get; set; }

        public ElectrClass(ElectricalLoadClassification load)
        {
            LoadClass = load;
            Name = load.Name;
            Id = load.Id;
        }

        public ElectrClass(string name)
        {
            Name = name;
        }

        //Нахождение Кс из настроек ревита с использованием суммы мощностей/количества элементов подключенных к щиту
        //И его установка с помощью Set_Kc
        public void Get_Kc(Document doc)
        {
            ElectricalDemandFactorDefinition demandFactor = doc.GetElement(LoadClass.DemandFactorId) as ElectricalDemandFactorDefinition;
            switch (demandFactor.RuleType)
            {
                case ElectricalDemandFactorRule.Constant:
                    Set_Kc(demandFactor.GetValues().Single().Factor);
                    break;
                case ElectricalDemandFactorRule.LoadTable:
                case ElectricalDemandFactorRule.LoadTablePerPortion:

                    var dfLoad = demandFactor.GetValues().Single(x => x.MinRange < P_all_notConvert && x.MaxRange >= P_all_notConvert);
                    if (dfLoad.MaxRange == 1e+30)
                    {
                        Set_Kc(dfLoad.Factor);
                        break;
                    }
                    if (dfLoad.MinRange == 0)
                    {
                        Set_Kc(dfLoad.Factor);
                        break;
                    }
                    var dfLoad1 = demandFactor.GetValues().Single(x => x.MaxRange == dfLoad.MinRange);
                    double kc = dfLoad1.Factor - (dfLoad1.Factor - dfLoad.Factor) /
                        (dfLoad.MaxRange - dfLoad.MinRange) * (P_all_notConvert - dfLoad.MinRange);
                    Set_Kc(kc);
                    break;
                case ElectricalDemandFactorRule.QuantityTable:
                case ElectricalDemandFactorRule.QuantityTablePerPortion:
                    // то же самое что и выше только по количеству элементов
                    var dfQuan = demandFactor.GetValues().Single(x => x.MinRange < Count && x.MaxRange >= Count);
                    if (dfQuan.MaxRange == 1e+30)
                    {
                        Set_Kc(dfQuan.Factor);
                        break;
                    }
                    if (dfQuan.MinRange == 0)
                    {
                        Set_Kc(dfQuan.Factor);
                        break;
                    }
                    var dfQuan1 = demandFactor.GetValues().Single(x => x.MaxRange == dfQuan.MinRange);
                    double kc1 = dfQuan1.Factor - (dfQuan1.Factor - dfQuan.Factor) /
                        (dfQuan.MaxRange - dfQuan.MinRange) * (Count - dfQuan.MinRange);
                    Set_Kc(kc1);
                    break;
            }
        }



        //Метод создан для заполнения дополнительных параметров после нахождения Кс
        public void Set_Kc(double kc)
        {
            Kc = kc;
            S_calc = S_all * Kc;
            P_calc = P_all * Kc;
            Q_calc = Q_all * Kc;
            cos_fi = P_all / S_all;
            tg_fi = Q_all / P_all;
            //MessageBox.Show(Kc.ToString() + " " + Name + "\n"+
            //    "S_all:" +S_all + " S_calc:" + S_calc +"\n" +
            //    "P_all:" + P_all + " P_calc:" + P_calc );
        }
    }
}
