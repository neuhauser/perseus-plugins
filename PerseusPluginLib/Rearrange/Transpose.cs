using System.Collections.Generic;
using System.Drawing;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Rearrange{
	public class Transpose : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Image ButtonImage { get { return null; } }
		public string HelpDescription{
			get{
				return
					"The matrix of expression values is being transposed, i.e. rows become columns and columns become rows. Annotation colums will be lost.";
			}
		}
		public string HelpOutput{
			get{
				return
					"The transpose of the matrix of expression values. One string column can be selected to become the new column names.";
			}
		}
		public HelpType HelpDescriptionType { get { return HelpType.PlainText; } }
		public HelpType HelpOutputType { get { return HelpType.PlainText; } }
		public HelpType[] HelpSupplTablesType { get { return new HelpType[0]; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Transpose"; } }
		public string Heading { get { return "Matrix rearrangements"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 5; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public HelpType[] HelpDocumentTypes { get { return new HelpType[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents, ProcessInfo processInfo) {
			int nameCol = param.GetSingleChoiceParam("New column names").Value;
			float[,] x = ArrayUtils.Transpose(mdata.ExpressionValues);
			List<string> colNames;
			if (nameCol >= 0){
				colNames = new List<string>(mdata.StringColumns[nameCol]);
			} else{
				string[] s = new string[mdata.RowCount];
				for (int i = 0; i < s.Length; i++){
					s[i] = "Column" + (i + 1);
				}
				colNames = new List<string>(s);
			}
			List<string> rowNames = mdata.ExpressionColumnNames;
			mdata.SetData(mdata.Name, colNames, x, new List<string>(new[]{"Name"}), new List<string[]>(new[]{rowNames.ToArray()}),
				new List<string>(), new List<string[][]>(), new List<string>(), new List<double[]>(), new List<string>(),
				new List<double[][]>());
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			return
				new Parameters(new Parameter[]{
					new SingleChoiceParam("New column names"){
						Values = mdata.StringColumnNames,
						Help = "Select the column that should become the column names of the transposed matrix."
					}
				});
		}
	}
}