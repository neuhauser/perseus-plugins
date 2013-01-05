using System;
using System.Collections.Generic;
using System.Drawing;
using BasicLib.Num;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;

namespace PerseusPluginLib.Basic{
	public class RowCorrelations : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Image ButtonImage { get { return null; } }
		public string HelpDescription{
			get{
				return
					"Correlation coefficients are calculated between rows of the matrix. Which rows of the original matrix become rows or " +
						"columns of the correlation matrix can be configured in the parameter panel.";
			}
		}
		public string HelpOutput { get { return "A matrix containing the correlation coefficients."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Row correlation"; } }
		public string Heading { get { return "Basic"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return -8; } }
		public HelpType HelpDescriptionType { get { return HelpType.PlainText; } }
		public HelpType HelpOutputType { get { return HelpType.PlainText; } }
		public HelpType[] HelpSupplTablesType { get { return new HelpType[0]; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ProcessInfo processInfo){
			CorrelationType type = GetCorrelationType(param.GetSingleChoiceParam("Type").Value);
			int colColInd = param.GetSingleChoiceParam("Select columns based on").Value - 1;
			int rowColInd = param.GetSingleChoiceParam("Select rows based on").Value - 1;
			string[] colTerms = param.GetMultiStringParam("Column terms").Value;
			string[] rowTerms = param.GetMultiStringParam("Row terms").Value;
			int[] colInds = GetIndices(colColInd, colTerms, mdata);
			int[] rowInds = GetIndices(rowColInd, rowTerms, mdata);
			float[,] x = GetMatrix(mdata, type, rowInds, colInds);
			List<string[]> names = mdata.StringColumns;
			string[] names2 = (names != null && names.Count > 0) ? names[0] : null;
			if (names2 != null){
				names2 = ArrayUtils.SubArray(names2, colInds);
			}
			for (int i = 0; i < mdata.CategoryColumns.Count; i++){
				mdata.CategoryColumns[i] = ArrayUtils.SubArray(mdata.CategoryColumns[i], rowInds);
			}
			for (int i = 0; i < mdata.NumericColumns.Count; i++){
				mdata.NumericColumns[i] = ArrayUtils.SubArray(mdata.NumericColumns[i], rowInds);
			}
			for (int i = 0; i < mdata.StringColumns.Count; i++){
				mdata.StringColumns[i] = ArrayUtils.SubArray(mdata.StringColumns[i], rowInds);
			}
			mdata.ExpressionValues = x;
			mdata.ExpressionColumnNames = new List<string>(names2);
		}

		private static int[] GetIndices(int colInd, string[] terms, IMatrixData data){
			Array.Sort(terms);
			if (colInd < 0){
				return ArrayUtils.ConsecutiveInts(data.RowCount);
			}
			if (colInd < data.StringColumnCount){
				string[] x = data.StringColumns[colInd];
				List<int> result = new List<int>();
				for (int i = 0; i < x.Length; i++){
					string[] t = x[i].Length > 0 ? x[i].Split(';') : new string[0];
					foreach (string s in t){
						if (Array.BinarySearch(terms, s) >= 0){
							result.Add(i);
							break;
						}
					}
				}
				return result.ToArray();
			}
			colInd -= data.StringColumnCount;
			string[][] x1 = data.CategoryColumns[colInd];
			List<int> result1 = new List<int>();
			for (int i = 0; i < x1.Length; i++){
				string[] t = x1[i] ?? new string[0];
				foreach (string s in t){
					if (Array.BinarySearch(terms, s) >= 0){
						result1.Add(i);
						break;
					}
				}
			}
			return result1.ToArray();
		}

		private static CorrelationType GetCorrelationType(int value){
			if (value == 0){
				return CorrelationType.Pearson;
			}
			if (value == 1){
				return CorrelationType.Rsquared;
			}
			if (value == 2){
				return CorrelationType.Spearman;
			}
			throw new Exception("Never get here.");
		}

		public float[,] GetMatrix(IMatrixData matrixData, CorrelationType type, int[] rowInds, int[] colInds){
			float[,] x1 = new float[rowInds.Length,colInds.Length];
			for (int i = 0; i < rowInds.Length; i++){
				float[] coli = matrixData.GetExpressionRow(rowInds[i]);
				for (int j = 0; j < colInds.Length; j++){
					if (rowInds[i] == colInds[j]){
						x1[i, j] = float.NaN;
						continue;
					}
					float[] colj = matrixData.GetExpressionRow(colInds[j]);
					float[] i1;
					float[] j1;
					NumUtils.CalcIntersect(coli, colj, out i1, out j1);
					if (type == CorrelationType.Spearman){
						i1 = ArrayUtils.RankF(i1);
						j1 = ArrayUtils.RankF(j1);
					}
					x1[i, j] = i1.Length < 3 ? float.NaN : (float) ArrayUtils.Correlation(i1, j1);
					if (type == CorrelationType.Rsquared){
						x1[i, j] = x1[i, j]*x1[i, j];
					}
				}
			}
			return x1;
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			string[] choice =
				ArrayUtils.Concat(new[]{new[]{"<None>"}, mdata.StringColumnNames.ToArray(), mdata.CategoryColumnNames.ToArray()});
			return
				new Parameters(new Parameter[]{
					new SingleChoiceParam("Type"){
						Values = new[]{"Pearson correlation", "R-squared", "Spearman correlation"},
						Help = "Which type of correlation should be calculated? Spearman correlation is based on ranks."
					},
					new SingleChoiceParam("Select columns based on"){
						Values = choice,
						Help =
							"The columns of the resulting correlation matrix can be selected here. If <None> is chosen all rows of the original matrix will become columns of the correlation matrix. Otherwise the columns of the correlation matrix will be selected based on the specified name or categorical column."
					},
					new MultiStringParam("Column terms"){
						Help =
							"Specify here the terms that define if a certain row of the original matrix will become a column of the correlation matrix."
					},
					new SingleChoiceParam("Select rows based on"){
						Values = choice,
						Help =
							"The rows of the resulting correlation matrix can be selected here. If <None> is chosen all rows of the original matrix will become rows of the correlation matrix. Otherwise the rows of the correlation matrix will be selected based on the specified name or categorical column."
					},
					new MultiStringParam("Row terms"){
						Help =
							"Specify here the terms that define if a certain row of the original matrix will become a row of the correlation matrix."
					}
				});
		}
	}
}