using System;
using System.Windows.Forms;

namespace PerseusApi.Matrix{
	/// <summary>
	/// This interface wraps the control that is produced by IMatrixAnalysis.
	/// </summary>
	public interface IMatrixAnalysisResult : IAnalysisResult{
		Control GetControl(Action<string> updateStatus, Action<IMatrixData> newMatrix, Action<IDocumentData> newDocument);
	}
}