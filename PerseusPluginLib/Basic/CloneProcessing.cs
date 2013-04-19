using System.Drawing;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;
using PerseusApi.Matrix;
using PerseusPluginLib.Properties;

namespace PerseusPluginLib.Basic{
	public class CloneProcessing : IMatrixProcessing{
		public bool HasButton { get { return true; } }
		public Image ButtonImage { get { return Resources.sheepButton_Image; } }
		public string HelpDescription { get { return "A copy of the input matrix is generated."; } }
		public string HelpOutput { get { return "Same as input matrix."; } }
		public HelpType HelpDescriptionType { get { return HelpType.PlainText; } }
		public HelpType HelpOutputType { get { return HelpType.PlainText; } }
		public HelpType[] HelpSupplTablesType { get { return new HelpType[0]; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Clone"; } }
		public string Heading { get { return "Basic"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 100; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public HelpType[] HelpDocumentTypes { get { return new HelpType[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents, ProcessInfo processInfo) { }

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			return new Parameters(new Parameter[]{});
		}
	}
}