using System;
using System.Collections.Generic;
using System.Drawing;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;

namespace PerseusPluginLib.Norm{
	public class Divide : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Image ButtonImage { get { return null; } }
		public string HelpDescription { get { return "Divide all values by the specified quantity calculated on each row/column."; } }
		public string HelpOutput { get { return "Normalized expression matrix."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Divide"; } }
		public string Heading { get { return "Normalization"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return -5; } }
		public HelpType HelpDescriptionType { get { return HelpType.PlainText; } }
		public HelpType HelpOutputType { get { return HelpType.PlainText; } }
		public HelpType[] HelpSupplTablesType { get { return new HelpType[0]; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ProcessInfo processInfo){
			SingleChoiceParam access = param.GetSingleChoiceParam("Matrix access");
			bool rows = access.Value == 0;
			int what = param.GetSingleChoiceParam("Divide by what").Value;
			DivideImpl(rows, ArrayUtils.Mean, mdata);
			switch (what){
				case 0:
					DivideImpl(rows, ArrayUtils.Mean, mdata);
					break;
				case 1:
					DivideImpl(rows, ArrayUtils.Median, mdata);
					break;
				case 2:
					DivideImpl(rows, ArrayUtils.MostFrequentValue, mdata);
					break;
				case 3:
					DivideImpl(rows, ArrayUtils.TukeyBiweight, mdata);
					break;
				//case 4:
				//	DivideImpl(rows, ArrayUtils.TukeyBiweightSe, mdata);
				//	break;
				default:
					throw new Exception("Never get here.");
			}
		}

		public static void DivideImpl(bool rows, Func<double[], double> summarize, IMatrixData data){
			if (rows){
				for (int i = 0; i < data.RowCount; i++){
					List<double> vals = new List<double>();
					for (int j = 0; j < data.ExpressionColumnCount; j++){
						double q = data[i, j];
						if (!double.IsNaN(q) && !double.IsInfinity(q)){
							vals.Add(q);
						}
					}
					double med = summarize(vals.ToArray());
					for (int j = 0; j < data.ExpressionColumnCount; j++){
						data[i, j] /= (float) med;
					}
				}
			} else{
				for (int j = 0; j < data.ExpressionColumnCount; j++){
					List<double> vals = new List<double>();
					for (int i = 0; i < data.RowCount; i++){
						double q = data[i, j];
						if (!double.IsNaN(q) && !double.IsInfinity(q)){
							vals.Add(q);
						}
					}
					double med = summarize(vals.ToArray());
					for (int i = 0; i < data.RowCount; i++){
						data[i, j] /= (float) med;
					}
				}
			}
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			return
				new Parameters(new Parameter[]{
					new SingleChoiceParam("Matrix access"){
						Values = new[]{"Rows", "Columns"},
						Help = "Specifies if the analysis is performed on the rows or the columns of the matrix."
					},
					new SingleChoiceParam("Divide by what")
					{Values = new[]{"Mean", "Median", "Most frequent value", "Tukey's biweight"//, "Tukey's biweight se"
					}, Value = 1}
				});
		}
	}
}