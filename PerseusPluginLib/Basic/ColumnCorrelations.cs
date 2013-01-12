using System;
using System.Collections.Generic;
using System.Drawing;
using BasicLib.Num;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;
using PerseusPluginLib.Properties;

namespace PerseusPluginLib.Basic{
	public class ColumnCorrelations : IMatrixProcessing{
		public bool HasButton { get { return true; } }
		public Image ButtonImage { get { return Resources.trendline_Image; } }
		public string HelpDescription { get { return "Correlation coefficients are calculated between the specified columns of the matrix."; } }
		public string HelpOutput { get { return "A matrix containing the correlation coefficients."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Column correlation"; } }
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
			int[] rows = param.GetMultiChoiceParam("Rows").Value;
			int[] cols = param.GetMultiChoiceParam("Columns").Value;
			float[,] x = GetMatrix(mdata, type, rows, cols);
			string[] q = ArrayUtils.Concat(mdata.ExpressionColumnNames, mdata.NumericColumnNames);
			string[] names2 = ArrayUtils.SubArray(q, cols);
			List<string[]> names = new List<string[]>(new[]{ArrayUtils.SubArray(q, rows)});
			mdata.SetData("Correlation matrix", new List<string>(names2), x, new List<string>(new[]{"Name"}), names,
				new List<string>(), new List<string[][]>(), new List<string>(), new List<double[]>(), new List<string>(),
				new List<double[][]>());
		}

		private static CorrelationType GetCorrelationType(int value){
			switch (value){
				case 0:
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

		public float[,] GetMatrix(IMatrixData matrixData, CorrelationType type, int[] rows, int[] cols){
			float[,] x = new float[rows.Length,cols.Length];
			for (int i = 0; i < rows.Length; i++){
				float[] coli = GetColumn(rows[i], matrixData);
				for (int j = 0; j < cols.Length; j++){
					if (rows[i] == cols[j]){
						x[i, j] = float.NaN;
						continue;
					}
					float[] colj = GetColumn(cols[j], matrixData);
					float[] i1;
					float[] j1;
					CalcIntersect(coli, colj, out i1, out j1);
					if (type == CorrelationType.Spearman){
						i1 = ArrayUtils.RankF(i1);
						j1 = ArrayUtils.RankF(j1);
					}
					x[i, j] = i1.Length < 3 ? float.NaN : (float) ArrayUtils.Correlation(i1, j1);
					if (type == CorrelationType.Rsquared){
						x[i, j] = x[i, j]*x[i, j];
					}
				}
			}
			return x;
		}

		private static float[] GetColumn(int ind, IMatrixData data){
			if (ind < data.ExpressionColumnCount){
				return data.GetExpressionColumn(ind);
			}
			ind -= data.ExpressionColumnCount;
			return ArrayUtils.ToFloats(data.NumericColumns[ind]);
		}

		internal static void CalcIntersect(float[] x, float[] y, out float[] x1, out float[] y1){
			List<float> x2 = new List<float>();
			List<float> y2 = new List<float>();
			for (int i = 0; i < x.Length; i++){
				float x3 = x[i];
				float y3 = y[i];
				if (!float.IsNaN(x3) && !float.IsNaN(y3) && !float.IsInfinity(x3) && !float.IsInfinity(y3)){
					x2.Add(x3);
					y2.Add(y3);
				}
			}
			x1 = x2.ToArray();
			y1 = y2.ToArray();
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			string[] vals = ArrayUtils.Concat(mdata.ExpressionColumnNames, mdata.NumericColumnNames);
			int[] sel = ArrayUtils.ConsecutiveInts(mdata.ExpressionColumnCount);
			return
				new Parameters(new Parameter[]{
					new SingleChoiceParam("Type"){
						Values = new[]{"Pearson correlation", "R-squared", "Spearman correlation"},
						Help = "Which type of correlation should be calculated? Spearman correlation is based on ranks."
					},
					new MultiChoiceParam("Rows"){
						Value = sel, Values = vals, Repeats = true,
						Help = "Expresion or numerical columns that will be the rows of the correlation matrix."
					},
					new MultiChoiceParam("Columns"){
						Value = sel, Values = vals, Repeats = true,
						Help = "Expresion or numerical columns that will be the columns of the correlation matrix."
					}
				});
		}
	}
}