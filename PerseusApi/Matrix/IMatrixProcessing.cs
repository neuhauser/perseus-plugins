using BasicLib.Param;
using BasicLib.Util;

namespace PerseusApi.Matrix{
	public interface IMatrixProcessing : IMatrixActivity{
		string Heading { get; }
		string HelpOutput { get; }
		HelpType HelpOutputType { get; }
		string[] HelpSupplTables { get; }
		HelpType[] HelpSupplTablesType { get; }
		int NumSupplTables { get; }
		string[] HelpDocuments { get; }
		HelpType[] HelpDocumentTypes { get; }
		int NumDocuments { get; }

		void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents,
			ProcessInfo processInfo);

		/// <summary>
		/// Define here the parameters that determine the specifics of the processing.
		/// </summary>
		/// <param name="mdata">The parameters might depend on the data matrix.</param>
		/// <param name="errString">Set this to a value != null if an error occured. The error string will be displayed to the user.</param>
		/// <returns>The set of parameters.</returns>
		Parameters GetParameters(IMatrixData mdata, ref string errString);
	}
}