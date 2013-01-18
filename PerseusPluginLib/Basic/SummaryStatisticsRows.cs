using System;
using System.Collections.Generic;
using System.Drawing;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;
using PerseusPluginLib.Utils;

namespace PerseusPluginLib.Basic{
	public delegate double DoubleArrayToArray(IList<double> x);

	public class SummaryStatisticsRows : IMatrixProcessing{
		//TODO
		//Groeneveld & Meeden’s coefficient
		//Cyhelský's skewness coefficient
		//Distance skewness
		//L-moments
		//Tukey's biweight
		//Trimmed mean
		//Pearson's skewness coefficients
		//Quantile based skewness measures
		internal static Tuple<string, DoubleArrayToArray, string>[] procs = new[]{
			new Tuple<string, DoubleArrayToArray, string>("Mean", ArrayUtils.Mean,
				"Sum of all values divided by the number of values."),
			new Tuple<string, DoubleArrayToArray, string>("Median", ArrayUtils.Median,
				"For an odd number of values the middle value is taken. For an even number of values the average of the two values in " +
					"the middle is calculated."),
			new Tuple<string, DoubleArrayToArray, string>("Standard deviation", ArrayUtils.StandardDeviation, ""),
			new Tuple<string, DoubleArrayToArray, string>("Coefficient of variation", ArrayUtils.CoefficientOfVariation,
				"The coefficient of variation is defined as the standard deviation divided by the mean."),
			new Tuple<string, DoubleArrayToArray, string>("Median absolute deviation", ArrayUtils.MeanAbsoluteDeviation,
				"This is the median of the absolute values of the difference of each value to the median."),
			new Tuple<string, DoubleArrayToArray, string>("Minimum", ArrayUtils.Min, ""),
			new Tuple<string, DoubleArrayToArray, string>("Maximum", ArrayUtils.Max, ""),
			new Tuple<string, DoubleArrayToArray, string>("Range", ArrayUtils.Range, "Maximum minus minimum."),
			new Tuple<string, DoubleArrayToArray, string>("Valid values", x => x.Count,
				"The number of values are counted in each row that are neither infinite nor 'NaN'."),
			new Tuple<string, DoubleArrayToArray, string>("Inter-quartile range", ArrayUtils.InterQuartileRange, ""),
			new Tuple<string, DoubleArrayToArray, string>("1st quartile", ArrayUtils.FirstQuartile, ""),
			new Tuple<string, DoubleArrayToArray, string>("3rd quartile", ArrayUtils.ThirdQuartile, ""),
			new Tuple<string, DoubleArrayToArray, string>("Skewness", ArrayUtils.Skewness, ""),
			new Tuple<string, DoubleArrayToArray, string>("Kurtosis", ArrayUtils.Kurtosis, "")
		};
		public bool HasButton { get { return false; } }
		public Image ButtonImage { get { return null; } }
		public string Name { get { return "Summary statistics (rows)"; } }
		public string Heading { get { return "Basic"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return -5; } }
		public HelpType HelpDescriptionType { get { return HelpType.PlainText; } }
		public HelpType HelpOutputType { get { return HelpType.PlainText; } }
		public HelpType[] HelpSupplTablesType { get { return new HelpType[0]; } }
		public string HelpDescription { get{
			return
				"A set of simple descriptive quantities are calculated that help summarizing the expression data in each row.";
		} }
		public string HelpOutput{
			get{
				return
					"For each selected summary statistic, a numerical column is added containing the specific quantitiy for each row of " +
						"expression values. 'NaN' and 'Infinity' are ignored for all calculations.";
			}
		}
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ProcessInfo processInfo){
			BoolWithSubParams bp = param.GetBoolWithSubParams("Within groups");
			bool groups = bp.Value;
			string[] groupNames = null;
			int[][] colInds = null;
			if (groups){
				int groupRowInd = bp.GetSubParameters().GetSingleChoiceParam("Group").Value;
				string[][] groupCol = mdata.CategoryRows[groupRowInd];
				groupNames = ArrayUtils.UniqueValuesPreserveOrder(groupCol);
				colInds = PerseusPluginUtils.GetExpressionColIndices(groupCol, groupNames);
			}
			bool[] include = new bool[procs.Length];
			double[][] columns = new double[procs.Length][];
			double[][][] columnsG = null;
			if (groups){
				columnsG = new double[procs.Length][][];
				for (int i = 0; i < columnsG.Length; i++){
					columnsG[i] = new double[groupNames.Length][];
				}
			}
			for (int i = 0; i < include.Length; i++){
				include[i] = param.GetBoolParam(procs[i].Item1).Value;
				if (include[i]){
					columns[i] = new double[mdata.RowCount];
					if (groups){
						for (int j = 0; j < groupNames.Length; j++){
							columnsG[i][j] = new double[mdata.RowCount];
						}
					}
				}
			}
			for (int i = 0; i < mdata.RowCount; i++){
				List<double> v = new List<double>();
				for (int j = 0; j < mdata.ExpressionColumnCount; j++){
					double x = mdata[i, j];
					if (!double.IsNaN(x) && !double.IsInfinity(x)){
						v.Add(x);
					}
				}
				for (int j = 0; j < include.Length; j++){
					if (include[j]){
						columns[j][i] = procs[j].Item2(v);
					}
				}
				if (groups){
					List<double>[] vg = new List<double>[groupNames.Length];
					for (int j = 0; j < colInds.Length; j++){
						for (int k = 0; k < colInds[j].Length; k++){
							double x = mdata[i, colInds[j][k]];
							if (!double.IsNaN(x) && !double.IsInfinity(x)){
								vg[j].Add(x);
							}
						}
					}
					for (int j = 0; j < include.Length; j++){
						if (include[j]){
							for (int k = 0; k < groupNames.Length; k++){
								columnsG[j][k][i] = procs[j].Item2(vg[k]);
							}
						}
					}
				}
			}
			for (int i = 0; i < include.Length; i++){
				if (include[i]){
					mdata.AddNumericColumn(procs[i].Item1, procs[i].Item3, columns[i]);
					if (groups){
						for (int k = 0; k < groupNames.Length; k++){
							mdata.AddNumericColumn(procs[i].Item1 + " " + groupNames[k], procs[i].Item3, columnsG[i][k]);
						}
					}
				}
			}
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			List<Parameter> p = new List<Parameter>{
				new BoolWithSubParams("Within groups")
				{SubParamsTrue = new Parameters(new SingleChoiceParam("Group"){Values = mdata.CategoryRowNames})}
			};
			foreach (Tuple<string, DoubleArrayToArray, string> proc in procs){
				p.Add(new BoolParam(proc.Item1){Value = true, Help = proc.Item3});
			}
			return new Parameters(p);
		}
	}
}