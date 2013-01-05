using BasicLib.Param;

namespace PerseusApi{
	public interface IMatrixExport : IMatrixActivity{
		void Export(Parameters parameters, IMatrixData data, ProcessInfo processInfo);
		Parameters GetParameters(IMatrixData matrixData, ref string errString);
	}
}