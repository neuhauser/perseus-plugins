using System;
using System.Windows.Forms;
using PerseusApi;

namespace PerseusPluginLib.Manual{
	public class SelectRowsManuallyResult : IMatrixAnalysisResult{
		private readonly IMatrixData mdata;

		public SelectRowsManuallyResult(IMatrixData mdata){
			this.mdata = mdata;
		}

		public Control GetControl(Action<string> updateStatusLabel, Action<IMatrixData> newMatrix){
			return new SelectRowsManuallyControl(mdata, newMatrix);
		}

		public string GetHeading(){
			return "Select rows manually";
		}
	}
}