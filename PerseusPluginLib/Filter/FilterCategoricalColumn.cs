using System.Collections.Generic;
using System.Drawing;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;
using PerseusApi.Document;
using PerseusApi.Matrix;
using PerseusPluginLib.Properties;

namespace PerseusPluginLib.Filter{
	public class FilterCategoricalColumn : IMatrixProcessing{
		public bool HasButton { get { return true; } }
		public Image ButtonImage { get { return Resources.filter2; } }
		public string HelpDescription { get { return "Those rows are kept or removed that have the specified value in the selected categorical column."; } }
		public string HelpOutput { get { return "The filtered matrix."; } }
		public HelpType HelpDescriptionType { get { return HelpType.PlainText; } }
		public HelpType HelpOutputType { get { return HelpType.PlainText; } }
		public HelpType[] HelpSupplTablesType { get { return new HelpType[0]; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Filter category"; } }
		public string Heading { get { return "Filter"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return -5; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public HelpType[] HelpDocumentTypes { get { return new HelpType[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			int colInd = param.GetSingleChoiceParam("Column").Value;
			if (colInd < 0){
				processInfo.ErrString = "No categorical columns available.";
				return;
			}
			string value = param.GetStringParam("Find").Value;
			bool remove = param.GetSingleChoiceParam("Mode").Value == 0;
			string[][] cats = mdata.GetCategoryColumnAt(colInd);
			List<int> valids = new List<int>();
			for (int i = 0; i < cats.Length; i++){
				bool valid = true;
				foreach (string w in cats[i]){
					if (w.Equals(value)){
						valid = false;
						break;
					}
				}
				if ((valid && remove) || (!valid && !remove)){
					valids.Add(i);
				}
			}
			mdata.ExtractExpressionRows(valids.ToArray());
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			return
				new Parameters(new Parameter[]{
					new SingleChoiceParam("Column")
					{Values = mdata.CategoryColumnNames, Help = "The categorical column that the filtering should be based on."},
					new StringParam("Find")
					{Value = "+", Help = "The value that should be present to discard/keep the corresponding row."},
					new SingleChoiceParam("Mode"){
						Values = new[]{"Remove matching rows", "Keep matching rows"},
						Help =
							"If 'Remove matching rows' is selected, rows having the value specified above will be removed while " +
								"all other rows will be kept. If 'Keep matching rows' is selected, the opposite will happen."
					}
				});
		}
	}
}