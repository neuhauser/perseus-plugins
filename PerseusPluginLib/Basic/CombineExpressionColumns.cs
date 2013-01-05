using System;
using System.Collections.Generic;
using System.Drawing;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;

namespace PerseusPluginLib.Basic{
	public class CombineExpressionColumns : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Image ButtonImage { get { return null; } }
		public string Name { get { return "Combine expression columns"; } }
		public string Heading { get { return "Basic"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return -5; } }
		public HelpType HelpDescriptionType { get { return HelpType.PlainText; } }
		public HelpType HelpOutputType { get { return HelpType.PlainText; } }
		public HelpType[] HelpSupplTablesType { get { return new HelpType[0]; } }
		public string HelpDescription {
			get{
				return "Pairs of colums are combined into single columns. For this the four basic arithmetic operations are " +
					"available. Please make sure that the numbers of columns that are selected in the 'First' and 'Second' " +
					"box are equal.";
			}
		}
		public string HelpOutput{
			get{
				return "Expression columns are exchanged by the new combinations. The original columns are also kept in the " +
					"output if 'Keep original columns' is checked.";
			}
		}
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ProcessInfo processInfo){
			int[] colInds1 = param.GetMultiChoiceParam("First").Value;
			int[] colInds2 = param.GetMultiChoiceParam("Second").Value;
			bool keepOrig = param.GetBoolParam("Keep original columns").Value;
			if (colInds1.Length != colInds2.Length){
				processInfo.ErrString = "The number of columns has to be equal.";
				return;
			}
			if (colInds1.Length == 0){
				processInfo.ErrString = "Please select some columns.";
				return;
			}
			int operationInd = param.GetSingleChoiceParam("Operation").Value;
			bool allNumerical = AllNumerical(ArrayUtils.Concat(colInds1, colInds2), mdata);
			if (allNumerical){
				ProcessNumerical(colInds1, colInds2, mdata, operationInd);
			} else {
				ProcessExpression(colInds1, colInds2, keepOrig, mdata, operationInd);
			}
		}

		private static void ProcessNumerical(IList<int> colInds1, IList<int> colInds2, IMatrixData mdata, int operationInd) {
			int nnew = colInds1.Count;
			string[] newNames = new string[nnew];
			string[] newDescriptions = new string[nnew];
			double[][] newNum = new double[nnew][];
			for (int i = 0; i < nnew; i++){
				newNum[i] = new double[mdata.RowCount];
			}
			string symbol = GetSymbol(operationInd);
			for (int i = 0; i < nnew; i++) {
				float[] expCol1 = GetColumn(mdata, colInds1[i]);
				float[] expCol2 = GetColumn(mdata, colInds2[i]);
				for (int j = 0; j < mdata.RowCount; j++) {
					newNum[i][j] = (float)Combine(expCol1[j], expCol2[j], operationInd);
					if (double.IsInfinity(newNum[i][j])) {
						newNum[i][j] = double.NaN;
					}
				}
				newNames[i] = GetName(mdata, colInds1[i]) + "_" + symbol + "_" + GetName(mdata, colInds2[i]);
				newDescriptions[i] = "";
			}
			for (int i = 0; i < nnew; i++){
				mdata.AddNumericColumn(newNames[i], newDescriptions[i], newNum[i]);
			}
		}

		private static void ProcessExpression(IList<int> colInds1, IList<int> colInds2, bool keepOrig, IMatrixData mdata, int operationInd) {
			int nnew = colInds1.Count;
			int ntot = keepOrig ? nnew + mdata.ExpressionColumnCount : nnew;
			string[] newNames = new string[ntot];
			string[] newDescriptions = new string[ntot];
			float[,] newEx = new float[mdata.RowCount, ntot];
			string symbol = GetSymbol(operationInd);
			for (int i = 0; i < nnew; i++) {
				float[] expCol1 = GetColumn(mdata, colInds1[i]);
				float[] expCol2 = GetColumn(mdata, colInds2[i]);
				for (int j = 0; j < mdata.RowCount; j++) {
					newEx[j, i] = (float)Combine(expCol1[j], expCol2[j], operationInd);
					if (float.IsInfinity(newEx[j, i])) {
						newEx[j, i] = float.NaN;
					}
				}
				newNames[i] = GetName(mdata, colInds1[i]) + "_" + symbol + "_" + GetName(mdata, colInds2[i]);
				newDescriptions[i] = "";
			}
			if (keepOrig) {
				for (int i = 0; i < mdata.ExpressionColumnCount; i++) {
					for (int j = 0; j < mdata.RowCount; j++) {
						newEx[j, i + nnew] = mdata[j, i];
					}
					newNames[i + nnew] = mdata.ExpressionColumnNames[i];
					newDescriptions[i + nnew] = "";
				}
			}
			mdata.ExpressionValues = newEx;
			mdata.ExpressionColumnNames = new List<string>(newNames);
			mdata.ExpressionColumnDescriptions = new List<string>(newDescriptions);
			mdata.ClearCategoryRows();
			mdata.ClearNumericRows();
		}

		private static bool AllNumerical(IEnumerable<int> colInds, IMatrixData mdata) {
			foreach (int colInd in colInds){
				if (colInd < mdata.ExpressionColumnCount){
					return false;
				}
			}
			return true;
		}

		private static string GetName(IMatrixData mdata, int i){
			if (i < mdata.ExpressionColumnCount){
				return mdata.ExpressionColumnNames[i];
			}
			i -= mdata.ExpressionColumnCount;
			return mdata.NumericColumnNames[i];
		}

		private static float[] GetColumn(IMatrixData mdata, int i){
			if (i < mdata.ExpressionColumnCount){
				return mdata.GetExpressionColumn(i);
			}
			i -= mdata.ExpressionColumnCount;
			return ArrayUtils.ToFloats(mdata.NumericColumns[i]);
		}

		private static string GetSymbol(int ind){
			switch (ind){
				case 0:
					return "+";
				case 1:
					return "-";
				case 2:
					return "*";
				case 3:
					return "/";
			}
			throw new Exception("Never get here.");
		}

		private static double Combine(double x, double y, int ind){
			switch (ind){
				case 0:
					return x + y;
				case 1:
					return x - y;
				case 2:
					return x*y;
				case 3:
					return x/y;
			}
			throw new Exception("Never get here.");
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			string[] choice = ArrayUtils.Concat(mdata.ExpressionColumnNames, mdata.NumericColumnNames);
			int[] selection = new int[0];
			return
				new Parameters(new Parameter[]{
					new SingleChoiceParam("Operation"){
						Values = new[]{"add", "subtract", "multiply", "divide"},
						Help = "Here you specify how a pair of columns is being combined into a single column."
					},
					new MultiChoiceParam("First"){
						Repeats = true, Value = selection, Values = choice,
						Help =
							"These are the first partners in the respective pairs of columns. Please make sure that the same number of " +
								"columns is selected in the 'Second' field."
					},
					new MultiChoiceParam("Second"){
						Repeats = true, Values = choice, Value = selection,
						Help =
							"These are the second partners in the respective pairs of columns. Please make sure that the same number of " +
								"columns is selected in the 'First' field."
					},
					new BoolParam("Keep original columns")
					{Value = true, Help = "If checked, the original expression columns will appear in the output matrix as well."}
				});
		}
	}
}