using System;
using System.Windows.Forms;
using PerseusApi.Matrix;

namespace PerseusApi.Document {
	public interface IDocumentAnalysisResult : IAnalysisResult {
		Control GetControl(Action<string> updateStatus, Action<IMatrixData> newMatrix, Action<IDocumentData> newDocument);
	}
}
