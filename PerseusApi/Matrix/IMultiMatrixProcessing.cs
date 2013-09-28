using BasicLib.Param;
using BasicLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;

namespace PerseusApi.Matrix{
	public interface IMultiMatrixProcessing : IMatrixActivity{
		string Heading { get; }
		string HelpOutput { get; }
		DocumentType HelpOutputType { get; }
		string[] HelpSupplTables { get; }
		DocumentType[] HelpSupplTablesType { get; }
		int NumSupplTables { get; }
		string[] HelpDocuments { get; }
		DocumentType[] HelpDocumentTypes { get; }
		int NumDocuments { get; }

		void ProcessData(IMatrixData[] inputData, Parameters param, ref IMatrixData[] outputTables,
			ref IDocumentData[] documents, ProcessInfo processInfo);

		/// <summary>
		/// Define here the parameters that determine the specifics of the processing.
		/// </summary>
		/// <param name="inputData">The parameters might depend on the data matrices.</param>
		/// <param name="errString">Set this to a value != null if an error occured. The error string will be displayed to the user.</param>
		/// <returns>The set of parameters.</returns>
		Parameters GetParameters(IMatrixData[] inputData, ref string errString);
	}
}