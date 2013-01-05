using System;
using BasicLib.Param;

namespace PerseusApi{
	public interface IMatrixAnalysis : IMatrixActivity, IAnalysis{
		IMatrixAnalysisResult AnalyzeData(IMatrixData mdata, Parameters param, ProcessInfo processInfo);
		Parameters GetParameters(IMatrixData matrixData, ref string errString);
		string Heading { get; }
		Tuple<IMatrixProcessing, Func<Parameters, IMatrixData, Parameters, string>>[] Replacements { get; }
	}
}