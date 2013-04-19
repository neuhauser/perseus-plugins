using System.Drawing;
using BasicLib.Num;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Basic{
	public class AddNoise : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Image ButtonImage { get { return null; } }
		public string HelpDescription { get { return "Modulate the data with gaussian noise."; } }
		public string HelpOutput { get { return "Same as input matrix with random noise added to the expression columns."; } }
		public HelpType HelpDescriptionType { get { return HelpType.PlainText; } }
		public HelpType HelpOutputType { get { return HelpType.PlainText; } }
		public HelpType[] HelpSupplTablesType { get { return new HelpType[0]; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Add noise"; } }
		public string Heading { get { return "Basic"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 101; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public HelpType[] HelpDocumentTypes { get { return new HelpType[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents, ProcessInfo processInfo) {
			Random2 rand = new Random2();
			double std = param.GetDoubleParam("Standard deviation").Value;
			for (int i = 0; i < mdata.RowCount; i++){
				for (int j = 0; j < mdata.ExpressionColumnCount; j++){
					mdata[i, j] += (float)rand.NextGaussian(0, std);
				}
			}
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			return new Parameters(new Parameter[]{
				new DoubleParam("Standard deviation", 0.1){Help = "Standard deviation of the noise distribution."} 
			});
		}
	}
}