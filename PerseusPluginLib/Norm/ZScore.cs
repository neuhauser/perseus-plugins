using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;
using PerseusPluginLib.Properties;

namespace PerseusPluginLib.Norm{
	public class ZScore : IMatrixProcessing{
		public bool HasButton { get { return true; } }
		public Image ButtonImage { get { return Resources.zscoreButton_Image; } }
		public string Name { get { return "Z-score"; } }
		public string Heading { get { return "Normalization"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return -10; } }
		public string HelpOutput { get { return "Normalized expression matrix."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string HelpDescription{
			get{
				return
					"The mean of each row/column is subtracted from each value. The result is then divided by the standard deviation of the row/column.";
			}
		}
		public HelpType HelpDescriptionType { get { return HelpType.PlainText; } }
		public HelpType HelpOutputType { get { return HelpType.PlainText; } }
		public HelpType[] HelpSupplTablesType { get { return new HelpType[0]; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ProcessInfo processInfo){
			SingleChoiceWithSubParams access = param.GetSingleChoiceWithSubParams("Matrix access");
			bool rows = access.Value == 0;
			int groupInd;
			if (rows){
				groupInd = access.GetSubParameters().GetSingleChoiceParam("Grouping").Value - 1;
			} else{
				groupInd = -1;
			}
			if (groupInd < 0){
				Zscore(rows, mdata);
			} else{
				string[][] catRow = mdata.CategoryRows[groupInd];
				foreach (string[] t in catRow){
					if (t.Length > 1){
						processInfo.ErrString = "The groups are overlapping.";
						return;
					}
				}
				ZscoreGroups(mdata, catRow);
			}
		}

		private static void ZscoreGroups(IMatrixData data, IList<string[]> catRow){
			string[] groupVals = ArrayUtils.UniqueValuesPreserveOrder(catRow);
			foreach (int[] inds in groupVals.Select(groupVal => GetIndices(catRow, groupVal))){
				ZscoreGroup(data, inds);
			}
		}

		private static void ZscoreGroup(IMatrixData data, IList<int> inds){
			for (int i = 0; i < data.RowCount; i++){
				double[] vals = new double[inds.Count];
				for (int j = 0; j < inds.Count; j++){
					double q = data[i, inds[j]];
					vals[j] = q;
				}
				double stddev;
				double mean = ArrayUtils.MeanAndStddev(vals, out stddev);
				foreach (int t in inds){
					data[i, t] = (float) ((data[i, t] - mean)/stddev);
				}
			}
		}

		internal static int[] GetIndices(IList<string[]> catRow, string groupVal){
			List<int> result = new List<int>();
			for (int i = 0; i < catRow.Count; i++){
				Array.Sort(catRow[i]);
				if (Array.BinarySearch(catRow[i], groupVal) >= 0){
					result.Add(i);
				}
			}
			return result.ToArray();
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			return
				new Parameters(new Parameter[]{
					new SingleChoiceWithSubParams("Matrix access"){
						Values = new[]{"Rows", "Columns"}, ParamNameWidth = 136, TotalWidth = 731,
						SubParams =
							new[]{
								new Parameters(new SingleChoiceParam("Grouping"){
									Values = ArrayUtils.Concat(new[]{"<No grouping>"}, mdata.CategoryRowNames),
									Help = "The z-scoring will be done separately in groups if a grouping is specified here."
								}),
								new Parameters()
							},
						Help = "Specifies if the z-scoring is performed on the rows or the columns of the matrix."
					}
				});
		}

		public static void Zscore(bool rows, IMatrixData data){
			if (rows){
				for (int i = 0; i < data.RowCount; i++){
					double[] vals = new double[data.ExpressionColumnCount];
					for (int j = 0; j < data.ExpressionColumnCount; j++){
						double q = data[i, j];
						vals[j] = q;
					}
					double stddev;
					double mean = ArrayUtils.MeanAndStddev(vals, out stddev);
					for (int j = 0; j < data.ExpressionColumnCount; j++){
						data[i, j] = (float) ((data[i, j] - mean)/stddev);
					}
				}
			} else{
				for (int j = 0; j < data.ExpressionColumnCount; j++){
					double[] vals = new double[data.RowCount];
					for (int i = 0; i < data.RowCount; i++){
						double q = data[i, j];
						vals[i] = q;
					}
					double stddev;
					double mean = ArrayUtils.MeanAndStddev(vals, out stddev);
					for (int i = 0; i < data.RowCount; i++){
						data[i, j] = (float) ((data[i, j] - mean)/stddev);
					}
				}
			}
		}
	}
}