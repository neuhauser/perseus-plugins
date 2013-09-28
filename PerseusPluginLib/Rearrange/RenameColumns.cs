using System.Collections.Generic;
using System.Drawing;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Rearrange{
	public class RenameColumns : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Image ButtonImage { get { return null; } }
		public string HelpDescription { get { return "New names can be specified for each expression column. The new names are typed in explicitly."; } }
		public string HelpOutput { get { return "Same matrix but with the new expression column names."; } }
		public DocumentType HelpDescriptionType { get { return DocumentType.PlainText; } }
		public DocumentType HelpOutputType { get { return DocumentType.PlainText; } }
		public DocumentType[] HelpSupplTablesType { get { return new DocumentType[0]; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Rename columns"; } }
		public string Heading { get { return "Matrix rearrangements"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 0; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public DocumentType[] HelpDocumentTypes { get { return new DocumentType[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			List<string> expressionColumnNames = new List<string>();
			for (int i = 0; i < mdata.ExpressionColumnCount; i++){
				expressionColumnNames.Add(param.GetStringParam(mdata.ExpressionColumnNames[i]).Value);
			}
			mdata.ExpressionColumnNames = expressionColumnNames;
			List<string> numericColumnNames = new List<string>();
			for (int i = 0; i < mdata.NumericColumnCount; i++){
				numericColumnNames.Add(param.GetStringParam(mdata.NumericColumnNames[i]).Value);
			}
			mdata.NumericColumnNames = numericColumnNames;
			List<string> categoryColumnNames = new List<string>();
			for (int i = 0; i < mdata.CategoryColumnCount; i++){
				categoryColumnNames.Add(param.GetStringParam(mdata.CategoryColumnNames[i]).Value);
			}
			mdata.CategoryColumnNames = categoryColumnNames;
			List<string> stringColumnNames = new List<string>();
			for (int i = 0; i < mdata.StringColumnCount; i++){
				stringColumnNames.Add(param.GetStringParam(mdata.StringColumnNames[i]).Value);
			}
			mdata.StringColumnNames = stringColumnNames;
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			List<Parameter> par = new List<Parameter>();
			foreach (string t in mdata.ExpressionColumnNames){
				string help = "Specify the new name for the column '" + t + "'.";
				par.Add(new StringParam(t){Value = t, Help = help});
			}
			foreach (string t in mdata.NumericColumnNames){
				string help = "Specify the new name for the column '" + t + "'.";
				par.Add(new StringParam(t){Value = t, Help = help});
			}
			foreach (string t in mdata.CategoryColumnNames){
				string help = "Specify the new name for the column '" + t + "'.";
				par.Add(new StringParam(t){Value = t, Help = help});
			}
			foreach (string t in mdata.StringColumnNames){
				string help = "Specify the new name for the column '" + t + "'.";
				par.Add(new StringParam(t){Value = t, Help = help});
			}
			return new Parameters(par);
		}
	}
}