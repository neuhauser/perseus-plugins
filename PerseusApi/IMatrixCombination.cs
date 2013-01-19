using BasicLib.Param;
using BasicLib.Util;

namespace PerseusApi{
	public interface IMatrixCombination : IMatrixActivity{
		string HelpOutput { get; }
		HelpType HelpOutputType { get; }
		string DataType1 { get; }
		string DataType2 { get; }

		IMatrixData CombineData(IMatrixData matrixData1, IMatrixData matrixData2, Parameters parameters,
			ProcessInfo processInfo);

		/// <summary>
		/// Define here the parameters that determine the specifics of the combination.
		/// </summary>
		/// <param name="mdata1">The parameters might depend on the first data matrix.</param>
		/// <param name="mdata2">The parameters might depend on the second data matrix.</param>
		/// <param name="errString">Set this to a value != null if an error occured. The error string will be displayed to the user.</param>
		/// <returns>The set of parameters.</returns>
		Parameters GetParameters(IMatrixData mdata1, IMatrixData mdata2, ref string errString);
	}
}