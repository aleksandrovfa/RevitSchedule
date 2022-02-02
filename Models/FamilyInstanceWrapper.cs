using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitSchedule
{
    public class FamilyInstanceWrapper
    {
        public FamilyInstance FamilyInstance { get; }
        public string Name { get; private set; }
        public bool IsSelected { get; set; }

        public List<FamilyInstance> Consumers { get; } = new List<FamilyInstance>();
        public List<ElectrClass> ElectrClassAll { get; } = new List<ElectrClass>();
        public FamilyInstanceWrapper(FamilyInstance familyInstance)
        {
            FamilyInstance = familyInstance;
            Name = familyInstance.Name;
        }

        //Получение всех FamilyInstance которые подключены к щиту и формирование списка потребителей Consumers
        //Метод работает через рекурсию
        public void GetAllConsumers(FamilyInstance ElEq)
        {
            //ISet<ElectricalSystem> systems = ElEq.MEPModel.GetAssignedElectricalSystems();
            ElectricalSystemSet systems = ElEq.MEPModel.AssignedElectricalSystems;
            if (systems != null)
            {
                foreach (var system in systems)
                {
                    //ElementSet systems2 = system.Elements;
                    ElementSet systems2 = (system as ElectricalSystem).Elements;
                    foreach (var system2 in systems2)
                    {
                        var i = system2 as FamilyInstance;
                        if (i.Category.Name == "Электрооборудование")
                            GetAllConsumers(i);
                        else
                            Consumers.Add(i);
                    }
                }

            }

        }

        //Преобразование электрического коннектора в класс СonsumerParam
        // Далее идет создание списка с категориями потребителей(строки в итоговой таблице) и заполнение его параметрами, 
        // Которые подключены к щиту
        public void GetAllElectrClass(Document doc)
        {
            foreach (var Consumer in Consumers)
            {
                ConsumerParam consumerParam = new ConsumerParam(Consumer, doc);
                if (!ElectrClassAll.Any(x => x.Id == consumerParam.ClassId))
                {
                    ElectricalLoadClassification load = doc.GetElement(consumerParam.ClassId) as ElectricalLoadClassification;
                    ElectrClass electrClass = new ElectrClass(load);
                    ElectrClassAll.Add(electrClass);
                }
                ElectrClass electr = ElectrClassAll.Find(x => x.Id == consumerParam.ClassId);
                electr.S_all_notConvert = electr.S_all_notConvert + consumerParam.S_notConvert;
                electr.P_all_notConvert = electr.P_all_notConvert + consumerParam.P_notConvert;
                electr.S_all = electr.S_all + consumerParam.S;
                electr.P_all = electr.P_all + consumerParam.P;
                electr.Q_all = electr.Q_all + consumerParam.Q;
                electr.Count++;
            }
            // После заполнения идет поиск Кс и заполнение недостающих параметров
            foreach (var electr in ElectrClassAll)
            {
                electr.Get_Kc(doc);
            }
        }

        internal void GetAllSumm()
        {
            ElectrClass summ = new ElectrClass("Итоги");
            summ.S_all = ElectrClassAll.Sum(x => x.S_all);
            summ.P_all = ElectrClassAll.Sum(x => x.P_all);
            summ.Q_all = ElectrClassAll.Sum(x => x.Q_all);

            summ.P_calc = ElectrClassAll.Sum(x => x.P_calc);
            summ.Set_Kc(summ.P_calc / summ.P_all);
            ElectrClassAll.Add(summ);

        }
    }
}
