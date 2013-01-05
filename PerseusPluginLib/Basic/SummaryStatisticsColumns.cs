using System;
using System.Collections.Generic;
using System.Drawing;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;

namespace PerseusPluginLib.Basic{
	public class SummaryStatisticsColumns : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Image ButtonImage { get { return null; } }
		public string Name { get { return "Summary statistics (columns)"; } }
		public string Heading { get { return "Basic"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return -8; } }
		public HelpType HelpDescriptionType { get { return HelpType.PlainText; } }
		public HelpType HelpOutputType { get { return HelpType.PlainText; } }
		public HelpType[] HelpSupplTablesType { get { return new HelpType[0]; } }
		public string HelpDescription {
			get{
				return
					"A set of simple descriptive quantities are calculated that help summarizing the data in the selected expression or numerical columns.";
			}
		}
		public string HelpOutput{
			get{
				return
					"For each selected summary statistic, a new matrix is created containing the specific quantities for each selected column.";
			}
		}
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ProcessInfo processInfo){
			int[] cols = param.GetMultiChoiceParam("Columns").Value;
			bool[] include = new bool[SummaryStatisticsRows.procs.Length];
			double[][] rowws = new double[SummaryStatisticsRows.procs.Length][];
			for (int i = 0; i < include.Length; i++){
				include[i] = param.GetBoolParam(SummaryStatisticsRows.procs[i].Item1).Value;
				if (include[i]){
					rowws[i] = new double[cols.Length];
				}
			}
			for (int i = 0; i < cols.Length; i++){
				double[] vals = GetColumn(cols[i], mdata);
				for (int j = 0; j < include.Length; j++){
					if (include[j]){
						rowws[j][i] = SummaryStatisticsRows.procs[j].Item2(vals);
					}
				}
			}
			List<double[]> ex = new List<double[]>();
			List<string> names = new List<string>();
			for (int i = 0; i < include.Length; i++){
				if (include[i]){
					ex.Add(rowws[i]);
					names.Add(SummaryStatisticsRows.procs[i].Item1);
				}
			}
			float[,] exVals = GetExVals(ex);
			IEnumerable<string> colNames = GetColNames(mdata, cols);
			mdata.SetData("Summary", new List<string>(colNames), exVals, new List<string>(new[]{"Type"}),
				new List<string[]>(new[]{names.ToArray()}), new List<string>(), new List<string[][]>(), new List<string>(),
				new List<double[]>(), new List<string>(), new List<double[][]>());
		}

		private static IEnumerable<string> GetColNames(IMatrixData mdata, IEnumerable<int> cols){
			List<string> result = new List<string>();
			foreach (int col in cols){
				result.Add(GetColName(mdata, col));
			}
			return result.ToArray();
		}

		private static string GetColName(IMatrixData mdata, int col){
			if (col < mdata.ExpressionColumnCount){
				return mdata.ExpressionColumnNames[col];
			}
			col -= mdata.ExpressionColumnCount;
			if (col < mdata.NumericColumnCount){
				return mdata.NumericColumnNames[col];
			}
			col -= mdata.NumericColumnCount;
			return mdata.MultiNumericColumnNames[col];
		}

		private static double[] GetColumn(int col, IMatrixData mdata){
			if (col < mdata.ExpressionColumnCount){
				List<double> v = new List<double>();
				for (int j = 0; j < mdata.RowCount; j++){
					double x = mdata[j, col];
					if (!double.IsNaN(x) && !double.IsInfinity(x)){
						v.Add(x);
					}
				}
				return v.ToArray();
			}
			col -= mdata.ExpressionColumnCount;
			if (col < mdata.NumericColumnCount){
				double[] w = mdata.NumericColumns[col];
				List<double> v = new List<double>();
				foreach (double x in w){
					if (!double.IsNaN(x) && !double.IsInfinity(x)){
						v.Add(x);
					}
				}
				return v.ToArray();
			}
			col -= mdata.NumericColumnCount;
			{
				double[][] w = mdata.MultiNumericColumns[col];
				List<double> v = new List<double>();
				foreach (double[] y in w){
					foreach (double x in y){
						if (!double.IsNaN(x) && !double.IsInfinity(x)){
							v.Add(x);
						}
					}
				}
				return v.ToArray();
			}
		}

		private static float[,] GetExVals(IList<double[]> rows){
			float[,] result = new float[rows.Count,rows[0].Length];
			for (int i = 0; i < result.GetLength(0); i++){
				for (int j = 0; j < result.GetLength(1); j++){
					result[i, j] = (float) rows[i][j];
				}
			}
			return result;
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			List<Parameter> p = new List<Parameter>{
				new MultiChoiceParam("Columns"){
					Value = ArrayUtils.ConsecutiveInts(mdata.ExpressionColumnCount),
					Values =
						ArrayUtils.Concat(ArrayUtils.Concat(mdata.ExpressionColumnNames, mdata.NumericColumnNames),
							mdata.MultiNumericColumnNames),
					Help = "Specify here the columns for which the summary statistics quantities should be calculated."
				}
			};
			foreach (Tuple<string, DoubleArrayToArray, string> proc in SummaryStatisticsRows.procs){
				p.Add(new BoolParam(proc.Item1){Value = true, Help = proc.Item3});
			}
			return new Parameters(p);
		}
	}
}