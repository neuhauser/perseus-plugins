using System.Drawing;
using System.Linq;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;

namespace PerseusPluginLib.Rearrange{
	public class UniqueValues : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Image ButtonImage { get { return null; } }
		public string HelpDescription{
			get{
				return "Values in the selected string columns are made unique. The strings are " +
					"interpreted as separated by semicolons. These semicolon-separated values are made unique.";
			}
		}
		public string Name { get { return "Unique values"; } }
		public string Heading { get { return "Matrix rearrangements"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 16; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string HelpOutput { get { return ""; } }
		public HelpType HelpDescriptionType { get { return HelpType.PlainText; } }
		public HelpType HelpOutputType { get { return HelpType.PlainText; } }
		public HelpType[] HelpSupplTablesType { get { return new HelpType[0]; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param1, ref IMatrixData[] supplTables, ProcessInfo processInfo){
			int[] stringCols = param1.GetMultiChoiceParam("String columns").Value;
			if (stringCols.Length == 0){
				processInfo.ErrString = "Please select some columns.";
				return;
			}
			foreach (string[] col in stringCols.Select(stringCol => mdata.StringColumns[stringCol])){
				for (int i = 0; i < col.Length; i++){
					string q = col[i];
					if (q.Length == 0){
						continue;
					}
					string[] w = q.Split(';');
					w = ArrayUtils.UniqueValues(w);
					col[i] = StringUtils.Concat(";", w);
				}
			}
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			return
				new Parameters(new Parameter[]{
					new MultiChoiceParam("String columns"){
						Values = mdata.StringColumnNames, Value = new int[0],
						Help = "Select here the string colums that should be expanded."
					}
				});
		}
	}
}