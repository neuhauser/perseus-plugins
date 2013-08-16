using System;
using System.Collections.Generic;
using System.Drawing;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;
using PerseusApi.Document;
using PerseusApi.Matrix;
using PerseusPluginLib.Properties;
using PerseusPluginLib.Utils;

namespace PerseusPluginLib.Group{
	public class AverageGroups : IMatrixProcessing{
		public bool HasButton { get { return true; } }
		public Image ButtonImage { get { return Resources.average; } }
		public string HelpDescription{
			get{
				return
					"Expression columns are averaged over groups. This requires that at least one categorical annotation row is defined.";
			}
		}
		public string HelpOutput { get { return "Averaged expression matrix containing as many columns as there were groups defined."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Average groups"; } }
		public string Heading { get { return "Annotation rows"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 3; } }
		public HelpType HelpDescriptionType { get { return HelpType.PlainText; } }
		public HelpType HelpOutputType { get { return HelpType.PlainText; } }
		public HelpType[] HelpSupplTablesType { get { return new HelpType[0]; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public HelpType[] HelpDocumentTypes { get { return new HelpType[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			if (mdata.CategoryRowCount == 0){
				errorString = "No grouping is loaded.";
				return null;
			}
			return
				new Parameters(new Parameter[]{
					new SingleChoiceParam("Grouping"){Values = mdata.CategoryRowNames},
					new SingleChoiceParam("Average type")
					{Values = new[]{"median", "mean", "sum"}, Help = "Select wether median or mean should be used for the averaging."},
					new IntParam("Min. valid values per group", 1), new BoolParam("Keep original data", false)
				});
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			int avType = param.GetSingleChoiceParam("Average type").Value;
			if (mdata.CategoryRowCount == 0){
				processInfo.ErrString = "No category rows were loaded.";
				return;
			}
			int groupColInd = param.GetSingleChoiceParam("Grouping").Value;
			int validVals = param.GetIntParam("Min. valid values per group").Value;
			bool keep = param.GetBoolParam("Keep original data").Value;
			Func<IList<double>, double> func;
			switch (avType){
				case 0:
					func = ArrayUtils.Median;
					break;
				case 1:
					func = ArrayUtils.Mean;
					break;
				case 2:
					func = ArrayUtils.Sum;
					break;
				default:
					throw new Exception("Never get here.");
			}
			if (keep){
				FillMatrixKeep(groupColInd, validVals, mdata, func);
			} else{
				FillMatrixDontKeep(groupColInd, validVals, mdata, func);
			}
		}

		private static void FillMatrixDontKeep(int groupColInd, int validVals, IMatrixData mdata,
			Func<IList<double>, double> func){
			string[][] groupCol = mdata.GetCategoryRowAt(groupColInd);
			string[] groupNames = ArrayUtils.UniqueValuesPreserveOrder(groupCol);
			int[][] colInds = PerseusPluginUtils.GetExpressionColIndices(groupCol, groupNames);
			float[,] newExCols = new float[mdata.RowCount,groupNames.Length];
			for (int i = 0; i < newExCols.GetLength(0); i++){
				for (int j = 0; j < newExCols.GetLength(1); j++){
					List<double> vals = new List<double>();
					foreach (int ind in colInds[j]){
						double val = mdata[i, ind];
						if (!double.IsNaN(val) && !double.IsInfinity(val)){
							vals.Add(val);
						}
					}
					float xy = float.NaN;
					if (vals.Count >= validVals){
						xy = (float) func(vals);
					}
					newExCols[i, j] = xy;
				}
			}
			mdata.ExpressionColumnNames = new List<string>(groupNames);
			mdata.ExpressionColumnDescriptions = GetEmpty(groupNames);
			mdata.ExpressionValues = newExCols;
			mdata.RemoveCategoryRowAt(groupColInd);
			for (int i = 0; i < mdata.CategoryRowCount; i++){
				mdata.SetCategoryRowAt(AverageCategoryRow(mdata.GetCategoryRowAt(i), colInds), i);
			}
			for (int i = 0; i < mdata.NumericRows.Count; i++){
				mdata.NumericRows[i] = AverageNumericRow(mdata.NumericRows[i], colInds);
			}
		}

		public static List<string> GetEmpty(IEnumerable<string> x){
			List<string> result = new List<string>();
			foreach (string s in x){
				result.Add("");
			}
			return result;
		}

		private static void FillMatrixKeep(int groupColInd, int validVals, IMatrixData mdata, Func<IList<double>, double> func){
			string[][] groupCol = mdata.GetCategoryRowAt(groupColInd);
			string[] groupNames = ArrayUtils.UniqueValuesPreserveOrder(groupCol);
			int[][] colInds = PerseusPluginUtils.GetExpressionColIndices(groupCol, groupNames);
			double[][] newNumCols = new double[groupNames.Length][];
			for (int i = 0; i < newNumCols.Length; i++){
				newNumCols[i] = new double[mdata.RowCount];
			}
			for (int i = 0; i < mdata.RowCount; i++){
				for (int j = 0; j < groupNames.Length; j++){
					List<double> vals = new List<double>();
					foreach (int ind in colInds[j]){
						double val = mdata[i, ind];
						if (!double.IsNaN(val) && !double.IsInfinity(val)){
							vals.Add(val);
						}
					}
					float xy = float.NaN;
					if (vals.Count >= validVals){
						xy = (float) func(vals);
					}
					newNumCols[j][i] = xy;
				}
			}
			for (int i = 0; i < groupNames.Length; i++){
				mdata.AddNumericColumn(groupNames[i], groupNames[i], newNumCols[i]);
			}
		}

		private static double[] AverageNumericRow(IList<double> numericRow, IList<int[]> colInds){
			double[] result = new double[colInds.Count];
			for (int i = 0; i < result.Length; i++){
				result[i] = ArrayUtils.Mean(ArrayUtils.SubArray(numericRow, colInds[i]));
			}
			return result;
		}

		private static string[][] AverageCategoryRow(IList<string[]> categoryRow, IList<int[]> colInds){
			string[][] result = new string[colInds.Count][];
			for (int i = 0; i < result.Length; i++){
				result[i] = ArrayUtils.UniqueValues(ArrayUtils.Concat(ArrayUtils.SubArray(categoryRow, colInds[i])));
			}
			return result;
		}
	}
}