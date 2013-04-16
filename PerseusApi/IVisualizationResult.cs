using System;
using System.Windows.Forms;

namespace PerseusApi {
	public interface IVisualizationResult {
		string Heading { get; }
		Control GetControl(Action<string> updateStatus, Action<IMatrixData> newMatrix);
	}
}
