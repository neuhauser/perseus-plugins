using BasicLib.Param;

namespace PerseusApi{
	public interface IMatrixUpload : IMatrixActivity{
		void LoadData(IMatrixData matrixData, Parameters parameters, ProcessInfo processInfo);
		Parameters GetParameters(ref string errString);
	}
}