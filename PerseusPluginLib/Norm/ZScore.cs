using System.Drawing;
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
			SingleChoiceParam access = param.GetSingleChoiceParam("Matrix access");
			bool rows = access.Value == 0;
			Zscore(rows, mdata);
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			return
				new Parameters(new Parameter[]{
					new SingleChoiceParam("Matrix access"){
						Values = new[]{"Rows", "Columns"},
						Help = "Specifies if the analysis is performed on the rows or the columns of the matrix."
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