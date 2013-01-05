using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;
using PerseusPluginLib.Properties;

namespace PerseusPluginLib.Basic{
	public class TransformationProc : IMatrixProcessing{
		public bool HasButton { get { return true; } }
		public Image ButtonImage { get { return Resources.logButton_Image; } }
		public string HelpDescription { get { return "All values in the specified columns are transformed."; } }
		public string Name { get { return "Transform"; } }
		public string Heading { get { return "Basic"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return -10; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public HelpType HelpDescriptionType { get { return HelpType.PlainText; } }
		public HelpType HelpOutputType { get { return HelpType.PlainText; } }
		public HelpType[] HelpSupplTablesType { get { return new HelpType[0]; } }
		public string HelpOutput {
			get{
				return
					"The output matrix has the same structure as the input matrix. Values in the selected columns will be transformed accordingly.";
			}
		}

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param1, ref IMatrixData[] supplTables, ProcessInfo processInfo){
			int[] cols = param1.GetMultiChoiceParam("Columns").Value;
			SingleChoiceWithSubParams sx = param1.GetSingleChoiceWithSubParams("Transformation");
			Parameters param = sx.GetSubParameters();
			string transf = sx.SelectedValue;
			switch (transf){
				case "Log":{
					int baseInd = param.GetSingleChoiceParam("Base").Value;
					double baseLog;
					switch (baseInd){
						case 0:
							baseLog = 2;
							break;
						case 1:
							baseLog = Math.E;
							break;
						case 2:
							baseLog = 10;
							break;
						default:
							throw new Exception("Never get here. Value = " + baseInd);
					}
					Transform(x => (x > 0 && !double.IsInfinity(x)) ? Math.Log(x, baseLog) : double.NaN, mdata, cols);
					break;
				}
				case "Exp":{
					int baseInd = param.GetSingleChoiceParam("Base").Value;
					double baseLog;
					switch (baseInd){
						case 0:
							baseLog = 2;
							break;
						case 1:
							baseLog = Math.E;
							break;
						case 2:
							baseLog = 10;
							break;
						default:
							throw new Exception("Never get here.");
					}
					Transform(x => Math.Pow(baseLog, x), mdata, cols);
					break;
				}
				case "Add":
					double shift = param.GetDoubleParam("Shift").Value;
					Transform(x => x + shift, mdata, cols);
					break;
				case "Multiply":
					double factor = param.GetDoubleParam("Factor").Value;
					Transform(x => x*factor, mdata, cols);
					break;
				case "Invert":
					Transform(x => 1.0/x, mdata, cols);
					break;
				case "Round":
					Transform(Math.Round, mdata, cols);
					break;
				case "Abs":
					Transform(Math.Abs, mdata, cols);
					break;
				default:
					throw new Exception("Never get here.");
			}
		}

		private static void Transform(Func<double, double> t, IMatrixData data, IEnumerable<int> exColInds,
			IEnumerable<int> numColInds){
			foreach (int c in exColInds){
				for (int i = 0; i < data.RowCount; i++){
					data[i, c] = (float) t(data[i, c]);
					if (float.IsInfinity(data[i, c])){
						data[i, c] = float.NaN;
					}
				}
			}
			foreach (double[] v in numColInds.Select(c => data.NumericColumns[c])){
				for (int i = 0; i < data.RowCount; i++){
					v[i] = t(v[i]);
					if (double.IsInfinity(v[i])){
						v[i] = double.NaN;
					}
				}
			}
		}

		public static void Transform(Func<double, double> t, IMatrixData data, int[] colInds){
			List<int> exColInds = new List<int>();
			List<int> numColInds = new List<int>();
			int n = data.ExpressionColumnCount;
			foreach (int i in colInds){
				if (i < n){
					exColInds.Add(i);
				} else{
					numColInds.Add(i - n);
				}
			}
			Transform(t, data, exColInds.ToArray(), numColInds.ToArray());
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			string[] values = ArrayUtils.Concat(mdata.ExpressionColumnNames, mdata.NumericColumnNames);
			int[] sel = ArrayUtils.ConsecutiveInts(mdata.ExpressionColumnCount);
			string[] transformations = new[]{"Log", "Exp", "Add", "Multiply", "Invert", "Round", "Abs"};
			Parameters[] param = new[]{
				new Parameters(new Parameter[]
				{new SingleChoiceParam("Base"){Values = new[]{"2", "Natural", "10"}, Help = "The base of the logarithm."}}),
				new Parameters(new Parameter[]
				{new SingleChoiceParam("Base"){Values = new[]{"2", "Natural", "10"}, Help = "The base for the exponentiation."}}),
				new Parameters(new Parameter[]{new DoubleParam("Shift", 2){Help = "Constant to be added."}}),
				new Parameters(new Parameter[]
				{new DoubleParam("Factor", 2){Help = "All cells of selected rows will be multiplied with this factor."}}),
				new Parameters(new Parameter[]{}), new Parameters(new Parameter[]{}), new Parameters(new Parameter[]{})
			};
			return
				new Parameters(new Parameter[]{
					new SingleChoiceWithSubParams("Transformation"){
						Values = transformations, SubParams = param,
						Help = "Type of transformation that is performed on the selected columns."
					},
					new MultiChoiceParam("Columns")
					{Values = values, Value = sel, Help = "Select here the expression and numeric colums that should be transformed."}
				});
		}
	}
}