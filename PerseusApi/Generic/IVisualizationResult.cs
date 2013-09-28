using System;
using System.Windows.Forms;
using PerseusApi.Document;
using PerseusApi.Matrix;

namespace PerseusApi.Generic {
	public interface IVisualizationResult {
		string Heading { get; }
		Control GetControl(Action<string> updateStatus, Action<IMatrixData> newMatrix, Action<IDocumentData> newDocument);
	}
}
