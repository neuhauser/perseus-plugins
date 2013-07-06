using System;
using System.Collections.Generic;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Utils{
	public static class PerseusPluginUtils{
		public static SingleChoiceParam GetFilterModeParam(){
			return new SingleChoiceParam("Filter mode"){Values = new[]{"Reduce matrix", "Add categorical column"}};
		}

		private static SingleChoiceParam GetModeParam1(){
			return new SingleChoiceParam("Mode"){
				Values = new[]{"Remove matching rows", "Keep matching rows"},
				Help =
					"If 'Remove matching rows' is selected, rows having the value specified above will be removed while " +
						"all other rows will be kept. If 'Keep matching rows' is selected, the opposite will happen."
			};
		}

		private static SingleChoiceParam GetModeParam2(){
			return new SingleChoiceParam("Mode"){
				Values = new[]{"Mark matching rows", "Mark non-matching rows"},
				Help =
					"If 'Mark matching rows' is selected, rows having the value specified above will be indicated with a '+' in the output column. " +
						"If 'Keep matching rows' is selected, the opposite will happen."
			};
		}

		internal static SingleChoiceWithSubParams GetFilterModeParamNew(){
			SingleChoiceWithSubParams p = new SingleChoiceWithSubParams("Filter mode"){
				Values = new[]{"Reduce matrix", "Add categorical column", "Split matrix"},
				SubParams =
					new List<Parameters>(new[]{new Parameters(GetModeParam1()), new Parameters(GetModeParam2()), new Parameters()})
			};
			return p;
		}

		public static void FilterRows(IMatrixData mdata, Parameters parameters, int[] rows){
			bool reduceMatrix = GetReduceMatrix(parameters);
			if (reduceMatrix){
				mdata.ExtractExpressionRows(rows);
			} else{
				Array.Sort(rows);
				string[][] col = new string[mdata.RowCount][];
				for (int i = 0; i < col.Length; i++){
					bool contains = Array.BinarySearch(rows, i) >= 0;
					col[i] = contains ? new[]{"Keep"} : new[]{"Discard"};
				}
				mdata.AddCategoryColumn("Filter", "", col);
			}
		}

		private static bool GetReduceMatrix(Parameters parameters){
			return parameters.GetSingleChoiceParam("Filter mode").Value == 0;
		}

		public static void FilterColumns(IMatrixData mdata, Parameters parameters, int[] cols){
			bool reduceMatrix = GetReduceMatrix(parameters);
			if (reduceMatrix){
				mdata.ExtractExpressionColumns(cols);
			} else{
				Array.Sort(cols);
				string[][] row = new string[mdata.ExpressionColumnCount][];
				for (int i = 0; i < row.Length; i++){
					bool contains = Array.BinarySearch(cols, i) >= 0;
					row[i] = contains ? new[]{"Keep"} : new[]{"Discard"};
				}
				mdata.AddCategoryRow("Filter", "", row);
			}
		}

		public static string[][] CollapseCatCol(string[][] catCol, int[][] collapse) {
			string[][] result = new string[collapse.Length][];
			for (int i = 0; i < collapse.Length; i++) {
				result[i] = CollapseCatCol(catCol, collapse[i]);
			}
			return result;
		}

		private static string[] CollapseCatCol(IList<string[]> catCol, IEnumerable<int> collapse) {
			HashSet<string> all = new HashSet<string>();
			foreach (int x in collapse) {
				all.UnionWith(catCol[x]);
			}
			string[] y = ArrayUtils.ToArray(all);
			Array.Sort(y);
			return y;
		}

		public static float[] CollapseNumCol(float[] numCol, int[][] collapse) {
			float[] result = new float[collapse.Length];
			for (int i = 0; i < collapse.Length; i++) {
				result[i] = CollapseNumCol(numCol, collapse[i]);
			}
			return result;
		}

		private static float CollapseNumCol(IList<float> numCol, IEnumerable<int> collapse) {
			List<float> all = new List<float>();
			foreach (int x in collapse) {
				if (!float.IsNaN(numCol[x]) && !float.IsInfinity(numCol[x])) {
					all.Add(numCol[x]);
				}
			}
			float y = ArrayUtils.Median(all.ToArray());
			return y;
		}

		public static double[] CollapseNumCol(double[] numCol, int[][] collapse) {
			double[] result = new double[collapse.Length];
			for (int i = 0; i < collapse.Length; i++) {
				result[i] = CollapseNumCol(numCol, collapse[i]);
			}
			return result;
		}

		private static double CollapseNumCol(IList<double> numCol, IEnumerable<int> collapse) {
			List<double> all = new List<double>();
			foreach (int x in collapse) {
				if (!double.IsNaN(numCol[x]) && !double.IsInfinity(numCol[x])) {
					all.Add(numCol[x]);
				}
			}
			double y = ArrayUtils.Median(all.ToArray());
			return y;
		}

		public static int[][] GetExpressionColIndices(IList<string[]> groupCol, string[] groupNames) {
			int[][] colInds = new int[groupNames.Length][];
			for (int i = 0; i < colInds.Length; i++) {
				colInds[i] = GetExpressionColIndices(groupCol, groupNames[i]);
			}
			return colInds;
		}

		private static int[] GetExpressionColIndices(IList<string[]> groupCol, string groupName) {
			List<int> result = new List<int>();
			for (int i = 0; i < groupCol.Count; i++) {
				string[] w = groupCol[i];
				Array.Sort(w);
				if (Array.BinarySearch(w, groupName) >= 0) {
					result.Add(i);
				}
			}
			return result.ToArray();
		}

		public static int[] GetIndicesOfCol(IMatrixData data, string categoryName, string value) {
			int index = GetIndexOfCol(data, categoryName);
			List<int> result = new List<int>();
			for (int i = 0; i < data.ExpressionColumnCount; i++) {
				string[] s = data.CategoryRows[index][i];
				foreach (string s1 in s) {
					if (s1.Equals(value)) {
						result.Add(i);
						break;
					}
				}
			}
			return result.ToArray();
		}

		public static int[] GetIndicesOf(IMatrixData data, string categoryName, string value) {
			int index = GetIndexOf(data, categoryName);
			List<int> result = new List<int>();
			for (int i = 0; i < data.RowCount; i++) {
				string[] s = data.CategoryColumns[index][i];
				foreach (string s1 in s) {
					if (s1.Equals(value)) {
						result.Add(i);
						break;
					}
				}
			}
			return result.ToArray();
		}

		private static int GetIndexOf(IMatrixData data, string categoryName) {
			for (int i = 0; i < data.CategoryColumnNames.Count; i++) {
				if (data.CategoryColumnNames[i].Equals(categoryName)) {
					return i;
				}
			}
			return -1;
		}

		private static int GetIndexOfCol(IMatrixData data, string categoryName) {
			for (int i = 0; i < data.CategoryRowNames.Count; i++) {
				if (data.CategoryRowNames[i].Equals(categoryName)) {
					return i;
				}
			}
			return -1;
		}
	}
}