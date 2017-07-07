using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Wing.Physician.HEDIS.Model;
using Monahrq.Wing.Physician.HEDIS.Views;

namespace Monahrq.Wing.Physician.HEDIS.ViewModel
{
    public class ImportProcessor : ProcessFileViewModel, IDataImporter
    {
        //public string HeaderLine = "PHYNPI,DIABHBA1CTEST,DIABHBA1CCONTROL,DIABBPCONTROL,ASTHMEDICATIONRATIO,CARDCONDLDLCSCREENING,CARDCONDITIONSLDLCCONTROL,HYPERBPCONTROL,COPD,DIABHBA1CTESTSTAVG,DIABHBA1CCONTROLSTAVG,DIABBPCONTROLSTAVG,ASTHMEDICATIONRATIOSTAVG,CARDCONDLDLCSCREENINGSTAVG,CARDCONDITIONSLDLCCONTROLSTAVG,HYPERBPCONTROLSTAVG,COPDSTAVG";
        public string HeaderLine = "MEDICALPRACTICEID,PHYNPI,DIABHBA1CTEST,DIABHBA1CCONTROL,DIABBPCONTROL,ASTHMEDICATIONRATIO,HYPERBPCONTROL,COPD,DIABHBA1CTESTSTAVG,DIABHBA1CCONTROLSTAVG,DIABBPCONTROLSTAVG,ASTHMEDICATIONRATIOSTAVG,HYPERBPCONTROLSTAVG,COPDSTAVG";
        readonly IList<string> _processedIdNumbers = new List<string>();  

        public ImportProcessor(WizardContext context)
            : base(context)
        {}

        // This processes a single line for AREA files
        // TODO: since this saves 1 line at a time to SQL, we should set up NH to use batches
        public bool LineFunction(string inputLine)
        {          
            if (string.IsNullOrEmpty(inputLine)) throw new Exception("Empty line !");

            // In the main import of the rows.
            var cols = inputLine.Split(new[] {","}, StringSplitOptions.None);
            var practiceId = cols[0];

            if(string.IsNullOrEmpty(practiceId))
                throw new ArgumentNullException(@"MedicalPracticeId", @"The MedicalPracticeId is a required field and can't be null.");

            long phyTemp;
            var phyNpi = long.TryParse(cols[1], out phyTemp) ? (long?)phyTemp : null;

            // Process if it's not a blank line.
            var hedisSurveyTarget = new HEDISTarget
            {
                //Dataset = DataContextObject.DatasetItem,
                MedicalPracticeId = practiceId,
                PhyNpi = phyNpi,
                DiabHbA1CTest = GetDoubleFromString(cols[2]),
                DiabHbA1CControl = GetDoubleFromString(cols[3]),
                DiabBPControl = GetDoubleFromString(cols[4]),
                AsthMedicationRatio = GetDoubleFromString(cols[5]),
                //CardCondLdlCScreening = GetDoubleFromString(cols[5]),
                //CardConditionsLdlCControl = GetDoubleFromString(cols[6]),
                HyperBPControl = GetDoubleFromString(cols[6]),
                COPD = GetDoubleFromString(cols[7]),
                DiabHbA1CTestSTAVG = GetDoubleFromString(cols[8]),
                DiabHbA1CControlSTAVG = GetDoubleFromString(cols[9]),
                DiabBPControlSTAVG = GetDoubleFromString(cols[10]),
                AsthMedicationRatioSTAVG = GetDoubleFromString(cols[11]),
                //CardCondLdlCScreeningSTAVG = GetDoubleFromString(cols[13]),
                //CardConditionsLdlCControlSTAVG = GetDoubleFromString(cols[14]),
                HyperBPControlSTAVG = GetDoubleFromString(cols[12]),
                COPDSTAVG = GetDoubleFromString(cols[13])
            };

            //if (Any<PhysicianHEDISTarget>(nhCAHPSSurveyTarget.PhyNpi, DataContextObject.DatasetItem.Id)) return false;
            if(_processedIdNumbers.Any(medicalPracticeId => medicalPracticeId.EqualsIgnoreCase(hedisSurveyTarget.MedicalPracticeId)))
                return false;

            //try
            //{
            var dataRow = MedicalPracticeHedisDataTable.NewRow();

            dataRow["MedicalPracticeId"] = hedisSurveyTarget.MedicalPracticeId;
                
            if(hedisSurveyTarget.PhyNpi != null)
                dataRow["PhyNpi"] = hedisSurveyTarget.PhyNpi.Value;

            dataRow["DiabHbA1CTest"] = hedisSurveyTarget.DiabHbA1CTest;
            dataRow["DiabHbA1CControl"] = hedisSurveyTarget.DiabHbA1CControl;
            dataRow["DiabBPControl"] = hedisSurveyTarget.DiabBPControl;
            dataRow["AsthMedicationRatio"] = hedisSurveyTarget.AsthMedicationRatio;
            //dataRow["CardCondLdlCScreening"] = nhCAHPSSurveyTarget.CardCondLdlCScreening;
            //dataRow["CardConditionsLdlCControl"] = nhCAHPSSurveyTarget.CardConditionsLdlCControl;
            dataRow["HyperBPControl"] = hedisSurveyTarget.HyperBPControl;
            dataRow["COPD"] = hedisSurveyTarget.COPD;
            dataRow["DiabHbA1CTestSTAVG"] = hedisSurveyTarget.DiabHbA1CTestSTAVG;
            dataRow["DiabHbA1CControlSTAVG"] = hedisSurveyTarget.DiabHbA1CControlSTAVG;
            dataRow["DiabBPControlSTAVG"] = hedisSurveyTarget.DiabBPControlSTAVG;
            dataRow["AsthMedicationRatioSTAVG"] = hedisSurveyTarget.AsthMedicationRatioSTAVG;
            //dataRow["CardCondLdlCScreeningSTAVG"] = nhCAHPSSurveyTarget.CardCondLdlCScreeningSTAVG;
            //dataRow["CardConditionsLdlCControlSTAVG"] = nhCAHPSSurveyTarget.CardConditionsLdlCControlSTAVG;
            dataRow["HyperBPControlSTAVG"] = hedisSurveyTarget.HyperBPControlSTAVG;
            dataRow["COPDSTAVG"] = hedisSurveyTarget.COPDSTAVG;

            if (DataContextObject.DatasetItem != null)
                dataRow["Dataset_Id"] = DataContextObject.DatasetItem.Id;

            if (_processedIdNumbers.Any(medicalPracticeId => medicalPracticeId.EqualsIgnoreCase(hedisSurveyTarget.MedicalPracticeId)))
                return false;

            _processedIdNumbers.Add(Convert.ToString(dataRow["MedicalPracticeId"]));
            MedicalPracticeHedisDataTable.Rows.Add(dataRow);

            return true;
            //}
            //catch (Exception exc)
            //{
                
            //    return false;
            //}
        }
    }
}