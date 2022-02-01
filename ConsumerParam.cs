using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitSchedule
{
    class ConsumerParam
    {
        public double S_notConvert { get; private set; }
        public double P_notConvert { get; private set; }
        public double S { get; private set; } // Полная установленная мощность
        public double P { get; private set; } // Активная установленная мощность
        public double Q { get; private set; } // Реактивная установленная мощность
        public double cosFi { get; private set; }   // Коэффициент мощности
        public ElementId ClassId { get; private set; } // id Классификации нагрузки
        public string ClassName { get; private set; } // name Классификации нагрузки
        public int Poles { get; private set;} // Количество полюсов
        public double Voltage { get; private set; } // Напряжение



        public ConsumerParam(FamilyInstance ElEq, Document doc)
        {
            System.Collections.IEnumerator it = ElEq.MEPModel.ConnectorManager.Connectors.GetEnumerator();
            Connector conn = null;
            Domain domain = Domain.DomainUndefined;
            while(domain != Domain.DomainElectrical)
            {
                it.MoveNext();
                if (it.Current is Connector)
                    conn = it.Current as Connector;
                domain = conn.Domain;
            }


            MEPFamilyConnectorInfo famConnInfo = conn.GetMEPConnectorInfo() as MEPFamilyConnectorInfo;

            ParameterValue param = famConnInfo
                                      .GetConnectorParameterValue(new ElementId(
                                      BuiltInParameter.RBS_ELEC_APPARENT_LOAD));
            var u = (param as DoubleParameterValue).Value;
            S_notConvert = u;
            S = UnitUtils.ConvertFromInternalUnits(u, UnitTypeId.VoltAmperes);


            ParameterValue param2 = famConnInfo
                                          .GetConnectorParameterValue(new ElementId(
                                          BuiltInParameter.RBS_ELEC_POWER_FACTOR));
            var u2 = (param2 as DoubleParameterValue).Value;
            cosFi = u2;

            ParameterValue param3 = famConnInfo
                                      .GetConnectorParameterValue(new ElementId(
                                      BuiltInParameter.RBS_ELEC_LOAD_CLASSIFICATION));
            var u3 = (param3 as ElementIdParameterValue).Value;
            ClassId = u3;


            ParameterValue param4 = famConnInfo
                                      .GetConnectorParameterValue(new ElementId(
                                      BuiltInParameter.RBS_ELEC_NUMBER_OF_POLES));
            var u4 = (param4 as IntegerParameterValue).Value;
            Poles = u4;

            ParameterValue param5 = famConnInfo
                          .GetConnectorParameterValue(new ElementId(
                          BuiltInParameter.RBS_ELEC_VOLTAGE));
            var u5 = (param5 as DoubleParameterValue).Value;
            Voltage = UnitUtils.ConvertFromInternalUnits(u5, UnitTypeId.Volts);

            P = S * cosFi;
            P_notConvert = S_notConvert * cosFi;
            Q = Math.Sqrt(Math.Pow(S, 2) - Math.Pow(P, 2));
            ClassName = doc.GetElement(ClassId).Name;
        }
    }
}
