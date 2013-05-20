using System;
using System.Collections.Generic;
using System.Drawing;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;
using PerseusApi.Document;
using PerseusApi.Matrix;
using PerseusPluginLib.Utils;

namespace PerseusPluginLib.Filter{
	public class FilterNumericalColumn : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Image ButtonImage { get { return null; } }
		public string HelpOutput { get { return "The filtered matrix."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Filter numerical column"; } }
		public string Heading { get { return "Filter"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 0; } }
		public HelpType HelpDescriptionType { get { return HelpType.PlainText; } }
		public HelpType HelpOutputType { get { return HelpType.PlainText; } }
		public HelpType[] HelpSupplTablesType { get { return new HelpType[0]; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public HelpType[] HelpDocumentTypes { get { return new HelpType[0]; } }
		public int NumDocuments { get { return 0; } }
		public string HelpDescription {
			get{
				return
					"Only those rows are kept that have a value in the numerical column fulfiling the equation or inequality relation.";
			}
		}

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents, ProcessInfo processInfo) {
			int colInd = param.GetSingleChoiceParam("Column").Value;
			double value = param.GetDoubleParam("Value").Value;
			int ruleInd = param.GetSingleChoiceParam("Remove if").Value;
			bool keepNan = param.GetBoolParam("Keep NaN").Value;
			double[] vals = colInd < mdata.NumericColumnCount
				? mdata.NumericColumns[colInd] : ArrayUtils.ToDoubles(mdata.GetExpressionColumn(colInd - mdata.NumericColumnCount));
			List<int> valids = new List<int>();
			for (int i = 0; i < vals.Length; i++){
				bool valid;
				double val = vals[i];
				if (double.IsNaN(val)){
					valid = keepNan;
				} else{
					switch (ruleInd){
						case 0:
							valid = val > value;
							break;
						case 1:
							valid = val >= value;
							break;
						case 2:
							valid = val != value;
							break;
						case 3:
							valid = val == value;
							break;
						case 4:
							valid = val <= value;
							break;
						case 5:
							valid = val < value;
							break;
						default:
							throw new Exception("Never get here.");
					}
				}
				if (valid){
					valids.Add(i);
				}
			}
			PerseusPluginUtils.FilterRows(mdata, param, valids.ToArray());
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			string[] choice = ArrayUtils.Concat(mdata.NumericColumnNames, mdata.ExpressionColumnNames);
			return
				new Parameters(new Parameter[]{
					new SingleChoiceParam("Column")
					{Values = choice, Help = "The numerical column that the filtering should be based on."},
					new DoubleParam("Value", 1){Help = "Threshold value used for the equation or inequality relation."},
					new SingleChoiceParam("Remove if")
					{Values = new[]{"<=", "<", "==", "!=", ">", ">="}, Help = "The type of relation that needs to be fulfiled."},
					new BoolParam("Keep NaN"){
						Value = false,
						Help =
							"Rows that have NaN as a value in the column will be kept or removed if this parameter is true or false, respectively."
					},
					PerseusPluginUtils.GetFilterModeParam()
				});
		}
	}
}