using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using A2LEngine.Models;
using static A2LEngine.Helpers;

namespace A2LEngine
{
    public class A2LEngine
    {
        public A2LCollection A2LCollection = new A2LCollection();
        public List<string> CompuMethodRef = new List<string>();
        public List<string> MeasurementRef = new List<string>();
        public List<string> CalibrationRef = new List<string>();
        public List<string> CompuMethodVTabRef = new List<string>();
        public List<string> RecordLayoutRef = new List<string>();

        private const string M_ST_START = @"/begin(\s*)MEASUREMENT";
        private const string M_ST_END = @"/end(\s*)MEASUREMENT";
        private const string C_ST_START = @"/begin(\s*)CHARACTERISTIC";
        private const string C_ST_END = @"/end(\s*)CHARACTERISTIC";
        private const string CM_ST_START = @"/begin(\s*)COMPU_METHOD";
        private const string CM_ST_END = @"/end(\s*)COMPU_METHOD";
        private const string CMV_ST_START = @"/begin(\s*)COMPU_VTAB";
        private const string CMV_ST_END = @"/end(\s*)COMPU_VTAB";
        private const string AXD_ST_START = @"/begin(\s*)AXIS_DESCR";
        private const string AXD_ST_END = @"/end(\s*)AXIS DESCR";
        public async Task<A2LCollection> LoadA2l(string filePath, ComponentType type = ComponentType.All)
        {
            var data = System.IO.File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            switch (type)
            {
                case ComponentType.All:
                case ComponentType.CompuMethods:
                    A2LCollection.CompuMethods = await GetCompuMethods(data);
                    if (type != ComponentType.All) break;
                    goto case ComponentType.Calibrations;
                case ComponentType.Calibrations:
                case ComponentType.Map:
                case ComponentType.Curve:
                    A2LCollection.Calibrations = await GetCalibrations(data);
                    if (type != ComponentType.All) break;
                    goto case ComponentType.Measurement;
                case ComponentType.Measurement:
                    A2LCollection.Measurements = await GetMeasurements(data);
                    break;
                default:
                    break;
            }
            return A2LCollection;
        }

        public async Task<List<Measurement>> GetMeasurements(string data)
        {
            if (!A2LCollection.IsCompuMethodsLoaded)
                A2LCollection.CompuMethods = await GetCompuMethods(data);
            List<Measurement> Measurements = new List<Measurement>();
            foreach (string measurementStr in Helpers.GetFirstInstanceTextBetween(data, M_ST_START, M_ST_END))
            {
                Measurement measVar = new Measurement();
                string[] strSplt = measurementStr.Cleanup().Split('\n');
                measVar.Name = strSplt[0].Trim();
                measVar.Description = strSplt[1].Cleanup();
                measVar.DataType = strSplt[2].Cleanup().ToEnum<DataType>();
                var compuMethodRefIndex = CompuMethodRef.IndexOf(strSplt[3].Cleanup());
                if(compuMethodRefIndex!=-1) measVar.CompuMethod = A2LCollection.CompuMethods[compuMethodRefIndex];
                measVar.DefinedResolution = double.Parse(strSplt[4].Cleanup());
                measVar.DefinedAccuracy_Prc = double.Parse(strSplt[5].Cleanup());
                measVar.Minimum = double.Parse(strSplt[6].Cleanup());
                measVar.Maximum = double.Parse(strSplt[7].Cleanup());
                measVar.ECU_Address = GetStringAfter(measurementStr, "ECU_ADDRESS ").Cleanup();
                measVar.CustomDisplayIdentifier = GetStringAfter(measurementStr, "DISPLAY_IDENTIFIER ").Cleanup();
                //TODO: Functions References are not fetched - get reference
                Measurements.Add(measVar);
            }
            A2LCollection.IsMeasurmentsLoaded = true;
            return Measurements;
        }

        public async Task<List<Calibration>> GetCalibrations(string data)
        {
            if (!A2LCollection.IsRecordLayoutsLoaded)
                await GetRecordLayouts(data);
            if (!A2LCollection.IsCompuMethodsLoaded)
                A2LCollection.CompuMethods = await GetCompuMethods(data);
            List<Calibration> Calibrations = new List<Calibration>();
            foreach (string calibrationStr in GetFirstInstanceTextBetween(data, C_ST_START, C_ST_END))
            {
                Calibration calib = new Calibration();
                string[] strSplt = calibrationStr.Cleanup().Split('\n');
                calib.Name = strSplt[0].Cleanup();
                calib.Description = strSplt[1].Cleanup();
                calib.Type = strSplt[2].Cleanup().ToEnum<CalibrationType>();
                calib.ECU_Address = strSplt[3].Cleanup();
                var recordLayoutRefIndex = RecordLayoutRef.IndexOf(strSplt[4].Cleanup());
                if (recordLayoutRefIndex != -1) calib.RecordLayoutReference = A2LCollection.RecordLayouts[recordLayoutRefIndex];
                calib.MaxDifference = double.Parse(strSplt[5].Cleanup());
                var compuMethodRefIndex = CompuMethodRef.IndexOf(strSplt[6].Cleanup());
                if (compuMethodRefIndex != -1) calib.CompuMethod = A2LCollection.CompuMethods[compuMethodRefIndex];
                calib.Minimum = double.Parse(strSplt[7].Cleanup());
                calib.Maximum = double.Parse(strSplt[8].Cleanup());
                if(calib.Type == CalibrationType.CUBE_4 || calib.Type == CalibrationType.CUBE_5 || calib.Type == CalibrationType.CUBOID || calib.Type == CalibrationType.CURVE || calib.Type == CalibrationType.MAP)
                {
                    calib.AxisDescriptors = await GetAxisDescriptors(calibrationStr);
                }
                CalibrationRef.Add(calib.Name);
                Calibrations.Add(calib);
            }
            A2LCollection.IsCalibrationsLoaded = true;
            return Calibrations;
        }

        public async Task<List<AxisDescriptor>> GetAxisDescriptors(string data)
        {
            List<AxisDescriptor> AxisDescriptors = new List<AxisDescriptor>();
            foreach (string axisDesStr in GetFirstInstanceTextBetween(data, AXD_ST_START, AXD_ST_END))
            {
                AxisDescriptor axisDes = new AxisDescriptor();
                string[] strSplt = axisDesStr.Cleanup().Split('\n');
                axisDes.Type = strSplt[0].Cleanup().ToEnum<AxisDescriptionType>();
                var measurementRefIndex = MeasurementRef.IndexOf(strSplt[1].Cleanup());
                if (measurementRefIndex != -1) axisDes.ReferenceMeasurement = A2LCollection.Measurements[measurementRefIndex];
                var compuMethodRefIndex = CompuMethodRef.IndexOf(strSplt[2].Cleanup());
                if (compuMethodRefIndex != -1) axisDes.CompuMethod = A2LCollection.CompuMethods[compuMethodRefIndex];
                axisDes.NumAxis = int.Parse(strSplt[3].Cleanup());
                axisDes.Minimum = double.Parse(strSplt[4].Cleanup());
                axisDes.Maximum = double.Parse(strSplt[5].Cleanup());
                var max_grad = GetStringAfter(axisDesStr, "MAX_GRAD").Cleanup();
                if(max_grad!="")
                    axisDes.Maximum_Gradient = double.Parse(max_grad.Cleanup());
                AxisDescriptors.Add(axisDes);

            }
            return AxisDescriptors;
        }

        public async Task GetRecordLayouts(string data)
        {
            A2LCollection.IsRecordLayoutsLoaded = true;
        }

        public async Task<List<CompuMethod>> GetCompuMethods(string data)
        {
            if (!A2LCollection.IsCompuMethodVTabsLoaded)
                A2LCollection.CompuMethodVTab = await GetCompuMethodVTabs(data);
            List<CompuMethod> CompuMethods = new List<CompuMethod>();
            foreach(string compuMethodStr in GetFirstInstanceTextBetween(data, CM_ST_START, CM_ST_END))
            {
                CompuMethod compMet = new CompuMethod();
                string[] strSplt = compuMethodStr.Cleanup().Split('\n');
                compMet.Name = strSplt[0].Cleanup();
                compMet.Description = strSplt[1].Cleanup();
                compMet.Type = strSplt[2].Cleanup().ToEnum<CompuMethodType>();
                compMet.DisplayFormat = strSplt[3].Cleanup();
                compMet.Unit = strSplt[4].Cleanup();
                var coeffStr = GetStringAfter(compuMethodStr, "COEFFS").Cleanup().Split(' ');
                if (coeffStr.Length > 1)
                    compMet.Coefficients = Array.ConvertAll<string, double>(coeffStr, item => double.Parse(item));
                var compuTabRefStrIndex = CompuMethodVTabRef.IndexOf(GetStringAfter(compuMethodStr, "COMPU_TAB_REF").Cleanup());
                if (compuTabRefStrIndex != -1) 
                    compMet.CompuTableReference = A2LCollection.CompuMethodVTab[compuTabRefStrIndex];
                CompuMethods.Add(compMet);
                CompuMethodRef.Add(compMet.Name);
            }
            A2LCollection.IsCompuMethodsLoaded = true;
            return CompuMethods;
        }

        public async Task<List<CompuTable>> GetCompuMethodVTabs(string data)
        {
            List<CompuTable> CompuMethodVTabs = new List<CompuTable>();
            foreach (string compuMethodVTabStr in GetFirstInstanceTextBetween(data, CMV_ST_START, CMV_ST_END))
            {
                CompuTable table = new CompuTable();
                string[] strSplt = compuMethodVTabStr.Cleanup().Split('\n');
                table.Name = strSplt[0].Cleanup();
                table.Description = strSplt[1].Cleanup();
                table.Type = strSplt[2].Cleanup().ToEnum<CompuMethodType>();
                table.NumberOfPairs = int.Parse(strSplt[3].Cleanup());
                table.Values = new Dictionary<int, object>();
                for (int i = 4; i < table.NumberOfPairs + 4; i++)
                {
                    var cmptSplt = strSplt[i].Cleanup().Split(' ');
                    table.Values.Add(int.Parse(cmptSplt[0].Cleanup()), cmptSplt.Length == 1 ? "" : cmptSplt[1].Cleanup());
                }
                CompuMethodVTabs.Add(table);
                CompuMethodVTabRef.Add(table.Name);
            }
            A2LCollection.IsCompuMethodVTabsLoaded = true;
            return CompuMethodVTabs;
        }


        public enum ComponentType
        {
            Measurement,
            Map,
            Curve,
            Calibrations,
            CompuMethods,
            All
        }
    }
}
