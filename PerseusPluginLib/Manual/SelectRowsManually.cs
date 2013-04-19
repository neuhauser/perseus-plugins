using System;
using System.Drawing;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;
using PerseusApi.Matrix;
using PerseusPluginLib.Properties;

namespace PerseusPluginLib.Manual{
	public class SelectRowsManually : IMatrixAnalysis{
		public string HelpDescription { get { return ""; } }
		public bool HasButton { get { return true; } }
		public Image ButtonImage { get { return Resources.hand; } }
		public string Heading { get { return "Manual editing"; } }
		public string Name { get { return "Select rows manually"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 0; } }
		public HelpType HelpDescriptionType { get { return HelpType.PlainText; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public IMatrixAnalysisResult AnalyzeData(IMatrixData mdata, Parameters param, ProcessInfo processInfo){
			return new SelectRowsManuallyResult(mdata);
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			return new Parameters(new Parameter[]{});
		}

		public Tuple<IMatrixProcessing, Func<Parameters, IMatrixData, Parameters, string>>[] Replacements { get { return new Tuple<IMatrixProcessing, Func<Parameters, IMatrixData, Parameters, string>>[0]; } }
		public bool CanStartWithEmptyData { get { return false; } }
	}
}