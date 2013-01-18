using System.Collections.Generic;
using System.Drawing;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;
using PerseusPluginLib.Properties;

namespace PerseusPluginLib.Group{
	public class CreateCategoricalAnnotRow : IMatrixProcessing{
		public bool HasButton { get { return true; } }
		public Image ButtonImage { get { return Resources.groupButton_Image; } }
		public string HelpDescription { get { return ""; } }
		public string HelpOutput { get { return "Same matrix with groups added."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Categorical annotation rows"; } }
		public string Heading { get { return "Annotation rows"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 1; } }
		public HelpType HelpDescriptionType { get { return HelpType.PlainText; } }
		public HelpType HelpOutputType { get { return HelpType.PlainText; } }
		public HelpType[] HelpSupplTablesType { get { return new HelpType[0]; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ProcessInfo processInfo){
			SingleChoiceWithSubParams scwsp = param.GetSingleChoiceWithSubParams("Action");
			Parameters spar = scwsp.GetSubParameters();
			switch (scwsp.Value){
				case 0:
					ProcessDataCreate(mdata, spar);
					break;
				case 1:
					ProcessDataEdit(mdata, spar);
					break;
				case 2:
					ProcessDataRename(mdata, spar);
					break;
				case 3:
					ProcessDataDelete(mdata, spar);
					break;
			}
		}

		private static void ProcessDataRename(IMatrixData mdata, Parameters param){
			int groupColInd = param.GetSingleChoiceParam("Category row").Value;
			string newName = param.GetStringParam("New name").Value;
			string newDescription = param.GetStringParam("New description").Value;
			mdata.CategoryRowNames[groupColInd] = newName;
			mdata.CategoryRowDescriptions[groupColInd] = newDescription;
		}

		private static void ProcessDataDelete(IMatrixData mdata, Parameters param){
			int groupColInd = param.GetSingleChoiceParam("Category row").Value;
			mdata.CategoryRows.RemoveAt(groupColInd);
			mdata.CategoryRowNames.RemoveAt(groupColInd);
			mdata.CategoryRowDescriptions.RemoveAt(groupColInd);
		}

		private static void ProcessDataEdit(IMatrixData mdata, Parameters param){
			SingleChoiceWithSubParams s = param.GetSingleChoiceWithSubParams("Category row");
			int groupColInd = s.Value;
			Parameters sp = s.GetSubParameters();
			for (int i = 0; i < mdata.ExpressionColumnCount; i++){
				string t = mdata.ExpressionColumnNames[i];
				string x = sp.GetStringParam(t).Value;
				mdata.CategoryRows[groupColInd][i] = x.Length > 0 ? x.Split(';') : new string[0];
			}
		}

		public Parameters GetEditParameters(IMatrixData mdata){
			Parameters[] subParams = new Parameters[mdata.CategoryRowCount];
			for (int i = 0; i < subParams.Length; i++){
				subParams[i] = GetEditParameters(mdata, i);
			}
			List<Parameter> par = new List<Parameter>{
				new SingleChoiceWithSubParams("Category row")
				{Values = mdata.CategoryRowNames, SubParams = subParams, Help = "Select the category row that should be edited."}
			};
			return new Parameters(par);
		}

		public Parameters GetEditParameters(IMatrixData mdata, int ind){
			List<Parameter> par = new List<Parameter>();
			for (int i = 0; i < mdata.ExpressionColumnCount; i++){
				string t = mdata.ExpressionColumnNames[i];
				string help = "Specify a category value for the column '" + t + "'.";
				par.Add(new StringParam(t, StringUtils.Concat(";", mdata.CategoryRows[ind][i])){Help = help});
			}
			return new Parameters(par);
		}

		private static void ProcessDataCreate(IMatrixData mdata, Parameters param){
			string name = param.GetStringParam("Row name").Value;
			string[][] groupCol = new string[mdata.ExpressionColumnCount][];
			for (int i = 0; i < mdata.ExpressionColumnCount; i++){
				string ename = mdata.ExpressionColumnNames[i];
				string value = param.GetStringParam(ename).Value;
				groupCol[i] = value.Length > 0 ? value.Split(';') : new string[0];
			}
			mdata.AddCategoryRow(name, name, groupCol);
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			SingleChoiceWithSubParams scwsp = new SingleChoiceWithSubParams("Action"){
				Values = new[]{"Create", "Edit", "Rename", "Delete"},
				SubParams =
					new[]{GetCreateParameters(mdata), GetEditParameters(mdata), GetRenameParameters(mdata), GetDeleteParameters(mdata)}
			};
			return new Parameters(new Parameter[]{scwsp});
		}

		public Parameters GetDeleteParameters(IMatrixData mdata){
			List<Parameter> par = new List<Parameter>{
				new SingleChoiceParam("Category row")
				{Values = mdata.CategoryRowNames, Help = "Select the category row that should be deleted."}
			};
			return new Parameters(par);
		}

		public Parameters GetRenameParameters(IMatrixData mdata){
			List<Parameter> par = new List<Parameter>{
				new SingleChoiceParam("Category row")
				{Values = mdata.CategoryRowNames, Help = "Select the category row that should be renamed."},
				new StringParam("New name"), new StringParam("New description")
			};
			return new Parameters(par);
		}

		public Parameters GetCreateParameters(IMatrixData mdata){
			List<Parameter> par = new List<Parameter>
			{new StringParam("Row name"){Value = "Group1", Help = "Name of the new category annotation row."}};
			foreach (string t in mdata.ExpressionColumnNames){
				string help = "Specify a value for the column '" + t + "'.";
				par.Add(new StringParam(t){Value = t, Help = help});
			}
			return new Parameters(par);
		}
	}
}