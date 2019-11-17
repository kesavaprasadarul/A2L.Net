using System;
using System.Collections.Generic;
using System.Text;

namespace A2LEngine.Models
{
    public class A2LCollection
    {
        public string Descriptor { get; set; }
        public List<Measurement> Measurements { get; set; }
        public List<Calibration> Calibrations { get; set; }
        public List<CompuMethod> CompuMethods { get; set; }
        public List<Function> Functions { get; set; }
        public List<RecordLayout> RecordLayouts { get; set; }
        public List<CompuTable> CompuMethodVTab { get; set; }
        public bool IsMeasurmentsLoaded { get; set; }
        public bool IsCalibrationsLoaded { get; set; }
        public bool IsCompuMethodsLoaded { get; set; }
        public bool IsFunctionsLoaded { get; set; }
        public bool IsRecordLayoutsLoaded { get; set; }
        public bool IsCompuMethodVTabsLoaded { get; set; }

    }
    public class Measurement
    {
        public string Name { get; set; }
        public string CustomDisplayIdentifier { get; set; }         //DISPLAY IDENTIFIER
        public string Description { get; set; }
        public DataType DataType { get; set; }
        public CompuMethod CompuMethod { get; set; }
        public double DefinedResolution { get; set; }
        public double DefinedAccuracy_Prc { get; set; }
        public double Maximum { get; set; }
        public double Minimum { get; set; }
        public string ECU_Address { get; set; }
        public List<Function> FunctionReference { get; set; }
    }

    public class CompuMethod
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public CompuMethodType Type { get; set; }
        public string DisplayFormat { get; set; }
        public string Unit { get; set; }
        public double[] Coefficients { get; set; }                  //COEFFS
        public CompuTable CompuTableReference { get; set; }

    }

    public class CompuTable
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public CompuMethodType Type { get; set; }
        public int NumberOfPairs { get; set; }
        public Dictionary<int, object> Values { get; set; }
    }

    public class Calibration
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public CalibrationType Type { get; set; }
        public string ECU_Address { get; set; }
        public string CustomDisplayIdentifier { get; set; }         //DISPLAY IDENTIFIER
        public RecordLayout RecordLayoutReference { get; set; }
        public double MaxDifference { get; set; }
        public CompuMethod CompuMethod { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public string BitMask { get; set; }                         //BIT_MASK
        public List<Function> FunctionReference { get; set; }
        public List<AxisDescriptor> AxisDescriptors { get; set; }
    }

    public class Function
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Calibration> DefinedCalibrations { get; set; }  //DEF_CHARACTERISTIC
        public List<Measurement> ImportedMeasurements { get; set; } //IN_MEASUREMENT
        public List<Measurement> ExportedMeasurements { get; set; } //OUT_MEASUREMENT
        public List<Measurement> LocalMeasurements { get; set; }    //LOC_MEASUREMENT
    }

    public class AxisDescriptor
    {
        public AxisDescriptionType Type { get; set; }
        public Measurement ReferenceMeasurement { get; set; }
        public CompuMethod CompuMethod { get; set; }
        public int NumAxis { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double Maximum_Gradient { get; set; }                //MAX_GRAD
        public string DisplayFormat { get; set; }                   //FORMAT
    }

    public class RecordLayout
    {
        public string Name { get; set; }
        public List<RecordLayoutAttrBase> LayoutAttributes { get; set; }
    }

    public class RecordLayoutAttrBase
    {
        public int MemPos { get; set; }                             //POSITION IN MEMORY
    }

    public class RLA_Num_AxisPoints : RecordLayoutAttrBase          //NO_AXIS_PTS_(X/Y)
    {
        public DataType DataType { get; set; }
        public string DistinctIdentifier { get; set; }              //DEFINE X OR Y AXIS HERE
    }

    public class RLA_Axispoints : RecordLayoutAttrBase              //AXIS_PTS_(X/Y)
    {
        public DataType DataType { get; set; }
        public string AddressingType { get; set; }
    }

    public class RLA_FunctionValues : RecordLayoutAttrBase          //FNC_VALUES
    {
        public DataType DataType { get; set; }
        public string AddressingType { get; set; }
    }

    public class RLA_SourceAddress : RecordLayoutAttrBase           //SRC_ADDR_(X/Y)
    {
        public DataType DataType { get; set; }
        public string DistinctIdentifier { get; set; }              //DEFINE X OR Y AXIS HERE
    }

    public enum CalibrationType
    {
        VALUE,              //SCALAR
        ASCII,              //STRING
        VAL_BLK,            //ARRAY
        CURVE,              //CURVE - 1D TABLE
        MAP,                //MAP - 2D TABLE
        CUBOID,             //3D TABLE
        CUBE_4,             //4D TABLE
        CUBE_5              //5D TABLE
    }

    public enum AxisDescriptionType
    {
        STD_AXIS,           //AXIS SPECIFIC TO ONE TABLE
        FIX_AXIS,           //AXIS SPECIFIC TO ONE TABLE WITH CALCULATED AXIS POINTS - AXIS POINTS ARE NOT STORED IN ECU MEMORY
        COM_AXIS,           //AXIS SHARED BY VARIOUS TABLES
        CURVE_AXIS,         //AXIS SHARED BY VARIOUS TABLES AND RESCALED - NORMALIZED BY A CURVE_AXIS_REF
        RES_AXIS            //AXIS SHARED BY VARIOUS TABLES AND RESCALED - NORMALIZED BY ANOTHER AXIS AXIS_PTS_REF
    }

    public enum DataType
    {
        UBYTE,              //UNSIGNED INTEGER - 8 BIT
        SBYTE,              //SIGNED INTEGER - 8 BIT
        UWORD,              //UNSIGNED INTEGER - 16 BIT
        SWORD,              //SIGNED INTEGER - 16 BIT
        ULONG,              //UNSIGNED INTEGER - 32 BIT
        SLONG,              //SIGNED INTEGER - 32 BIT
        A_UINT64,           //UNSIGNED INTEGER - 64 BIT
        A_INT64,            //SIGNED INTEGER - 64 BIT
        FLOAT32_IEEE,       //SINGLE PRECISION FLOAT - 32 BIT
        FLOAT64_IEEE        //DOUBLE PRECISION FLOAT - 64 BIT
    }

    public enum CompuMethodType
    {
        IDENTICAL,          //NO CONVERSION
        LINEAR,             //LINEAR, 2 COEFF WITH SLOPE AND OFFSET
        RAT_FUNC,           //6-COEFF WITH 2ND DEGREE NUMERATOR/DENOMINATOR
        TAB_INTP,           //TABLE WITH INTERPOLATION
        TAB_NOINTP,         //TABLE WITHOUT INTERPOLATION
        TAB_VERB,           //VERBAL TABLE/ENUMERATION
        FORM                //FORMULA WITH OPERATORS AND FUNCTIONS
    }
  
}
