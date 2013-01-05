using BasicLib.Param;
using BasicLib.Util;

namespace PerseusApi{
	public interface IMatrixProcessing : IMatrixActivity{
		void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ProcessInfo processInfo);
		Parameters GetParameters(IMatrixData mdata, ref string errString);
		string Heading { get; }
		string HelpOutput { get; }
		HelpType HelpOutputType { get; }
		string[] HelpSupplTables { get; }
		HelpType[] HelpSupplTablesType { get; }
		int NumSupplTables { get; }
	}
}