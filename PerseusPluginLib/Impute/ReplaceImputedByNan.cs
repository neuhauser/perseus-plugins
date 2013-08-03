using System.Drawing;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;
using PerseusApi.Document;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Impute{
	public class ReplaceImputedByNan : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Image ButtonImage { get { return null; } }
		public string HelpDescription { get { return "Replaces all values that have been imputed with NaN."; } }
		public string HelpOutput { get { return "Same matrix but with imputed values deleted."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Replace imputed values by NaN"; } }
		public string Heading { get { return "Imputation"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 1; } }
		public HelpType HelpDescriptionType { get { return HelpType.PlainText; } }
		public HelpType HelpOutputType { get { return HelpType.PlainText; } }
		public HelpType[] HelpSupplTablesType { get { return new HelpType[0]; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public HelpType[] HelpDocumentTypes { get { return new HelpType[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString) {
			return new Parameters(new Parameter[] { });
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			Replace(mdata);
		}

		public static void Replace(IMatrixData data){
			for (int i = 0; i < data.RowCount; i++){
				for (int j = 0; j < data.ExpressionColumnCount; j++){
					if (data.IsImputed[i, j]){
						data[i, j] = float.NaN;
					}
				}
			}
		}
	}
}