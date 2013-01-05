using BasicLib.Param;
using BasicLib.Util;

namespace PerseusApi{
	public interface IMatrixCombination : IMatrixActivity{
		IMatrixData CombineData(IMatrixData matrixData1, IMatrixData matrixData2, Parameters parameters,
			ProcessInfo processInfo);

		Parameters GetParameters(IMatrixData matrixData1, IMatrixData matrixData2, ref string errString);
		string HelpOutput { get; }
		HelpType HelpOutputType { get; }
		string DataType1 { get; }
		string DataType2 { get; }
	}
}