﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using BasicLib.Param;
using BasicLib.Parse;
using BasicLib.Util;
using PerseusApi;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusPluginLib.Properties;
using PerseusPluginLib.Utils;

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
		public DocumentType HelpDescriptionType { get { return DocumentType.PlainText; } }
		public DocumentType HelpOutputType { get { return DocumentType.PlainText; } }
		public DocumentType[] HelpSupplTablesType { get { return new DocumentType[0]; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public DocumentType[] HelpDocumentTypes { get { return new DocumentType[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			SingleChoiceWithSubParams scwsp = param.GetSingleChoiceWithSubParams("Action");
			Parameters spar = scwsp.GetSubParameters();
			switch (scwsp.Value){
				case 0:
					ProcessDataCreate(mdata, spar);
					break;
                case 1:
                    ProcessDataCreateFromGoupNames(mdata, spar, processInfo);
                    break;
				case 2:
					ProcessDataEdit(mdata, spar);
					break;
				case 3:
					ProcessDataRename(mdata, spar);
					break;
				case 4:
					ProcessDataDelete(mdata, spar);
					break;
				case 5:
					ProcessDataWriteTemplateFile(mdata, spar);
					break;
				case 6:
					string err = ProcessDataReadFromFile(mdata, spar);
					if (err != null){
						processInfo.ErrString = err;
					}
					break;
			}
		}

		private static string ProcessDataReadFromFile(IMatrixData mdata, Parameters param){
			FileParam fp = param.GetFileParam("Input file");
			string filename = fp.Value;
			string[] colNames = TabSep.GetColumnNames(filename, '\t');
			int nameIndex = GetNameIndex(colNames);
			if (nameIndex < 0){
				return "Error: the file has to contain a column called 'Name'.";
			}
			if (colNames.Length < 2){
				return "Error: the file does not contain a grouping column.";
			}
			string[] nameCol = TabSep.GetColumn(colNames[nameIndex], filename, '\t');
			Dictionary<string, int> map = ArrayUtils.InverseMap(nameCol);
			for (int i = 0; i < colNames.Length; i++){
				if (i == nameIndex){
					continue;
				}
				string groupName = colNames[i];
				string[] groupCol = TabSep.GetColumn(groupName, filename, '\t');
				string[][] newCol = new string[mdata.ExpressionColumnCount][];
				for (int j = 0; j < newCol.Length; j++){
					string colName = mdata.ExpressionColumnNames[j];
					if (!map.ContainsKey(colName)){
						newCol[j] = new string[0];
						continue;
					}
					int ind = map[colName];
					string group = groupCol[ind];
					newCol[j] = new[]{group};
				}
				mdata.AddCategoryRow(groupName, groupName, newCol);
			}
			return null;
		}

		private static int GetNameIndex(IList<string> colNames){
			for (int i = 0; i < colNames.Count; i++){
				if (colNames[i].ToLower().Equals("name")){
					return i;
				}
			}
			return -1;
		}

		private static void ProcessDataWriteTemplateFile(IMatrixData mdata, Parameters param){
			FileParam fp = param.GetFileParam("Output file");
			StreamWriter writer = new StreamWriter(fp.Value);
			writer.WriteLine("Name\tNew grouping");
			for (int i = 0; i < mdata.ExpressionColumnCount; i++){
				string colName = mdata.ExpressionColumnNames[i];
				writer.WriteLine(colName + "\t" + colName);
			}
			writer.Close();
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
			mdata.RemoveCategoryRowAt(groupColInd);
		}

		private static void ProcessDataEdit(IMatrixData mdata, Parameters param){
			SingleChoiceWithSubParams s = param.GetSingleChoiceWithSubParams("Category row");
			int groupColInd = s.Value;
			Parameters sp = s.GetSubParameters();
			string[][] newRow = new string[mdata.ExpressionColumnCount][];
			for (int i = 0; i < mdata.ExpressionColumnCount; i++){
				string t = mdata.ExpressionColumnNames[i];
				string x = sp.GetStringParam(t).Value;
				newRow[i] = x.Length > 0 ? x.Split(';') : new string[0];
			}
			mdata.SetCategoryRowAt(newRow, groupColInd);
		}

        private static int DetermineGroup(IList<int[]> colInds, IEnumerable<int> inds){
            for (int i = 0; i < colInds.Count; i++){
                if (CompletelyContained(colInds[i], inds)){
                    return i;
                }
            }
            return -1;
        }

        private static bool CompletelyContained(int[] colInds1, IEnumerable<int> inds){
            foreach (int ind in inds){
                if (Array.BinarySearch(colInds1, ind) < 0){
                    return false;
                }
            }
            return true;
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
				par.Add(new StringParam(t, StringUtils.Concat(";", mdata.GetCategoryRowAt(ind)[i])){Help = help});
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

        private static void ProcessDataCreateFromGoupNames(IMatrixData mdata, Parameters param, ProcessInfo processInfo){
            SingleChoiceWithSubParams scwsp = param.GetSingleChoiceWithSubParams("Pattern");
            Parameters spar = scwsp.GetSubParameters();
            string regexString = "";
            string replacement = "";
            switch (scwsp.Value) { 
                case 0:
                case 1:
                case 2:
                    regexString = GetSelectableRegexes()[scwsp.Value][1];
                    break;
                case 3:
                    regexString = spar.GetStringParam("Regex").Value;
                    break;
                case 4:
                    regexString = spar.GetStringParam("Regex").Value;
                    replacement = spar.GetStringParam("Replace with").Value;
                    break;
                default:
                    break;
            }               
            Regex regex;
            try{
                regex = new Regex(regexString);
            }
            catch (ArgumentException){
                processInfo.ErrString = "The regular expression you provided has invalid syntax.";
                return;
            }
            List<string[]> groupNames = new List<string[]>();
            foreach (string sampleName in mdata.ExpressionColumnNames) {
                string groupName = scwsp.Value < 4 ? regex.Match(sampleName).Groups[1].Value : regex.Replace(sampleName, replacement);
                if (string.IsNullOrEmpty(groupName))
                    groupName = sampleName;
                groupNames.Add(new[] { groupName });
            }
            mdata.AddCategoryRow("Grouping", "", groupNames.ToArray());
        }

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			SingleChoiceWithSubParams scwsp = new SingleChoiceWithSubParams("Action"){
				Values = new[]{"Create", "Create from experiment name", "Edit", "Rename", "Delete", "Write template file", "Read from file"},
				SubParams =
					new[]{
						GetCreateParameters(mdata), GetCreateFromExperimentNamesParameters(mdata, ref errorString), GetEditParameters(mdata), 
                        GetRenameParameters(mdata), GetDeleteParameters(mdata),
						GetWriteTemplateFileParameters(mdata), GetReadFromFileParameters(mdata)
					},
				ParamNameWidth = 136, TotalWidth = 731
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

        public Parameters GetReadFromFileParameters(IMatrixData mdata){
			List<Parameter> par = new List<Parameter>
			{new FileParam("Input file"){Filter = "Tab separated file (*.txt)|*.txt", Save = false}};
			return new Parameters(par);
		}

		public Parameters GetWriteTemplateFileParameters(IMatrixData mdata){
			List<Parameter> par = new List<Parameter>
			{new FileParam("Output file", "Groups.txt"){Filter = "Tab separated file (*.txt)|*.txt", Save = true}};
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

        /// <remarks>author: Marco Hein</remarks>>
        public Parameters GetCreateFromExperimentNamesParameters(IMatrixData mdata, ref string errorString){
            List<string[]> selectableRegexes = GetSelectableRegexes();            
            List<string> vals = new List<string>();
            foreach (string[] s in selectableRegexes){
                vals.Add(s[0]);
            }
            vals.Add("match regular expression");
            vals.Add("replace regular expression");
            List<Parameters> subparams = new List<Parameters>();
            for (int i = 0; i < selectableRegexes.Count; i++){
                subparams.Add(new Parameters(new Parameter[] { }));
            }
            subparams.Add(new Parameters(new Parameter[] { new StringParam("Regex", "") }));
            subparams.Add(new Parameters(new Parameter[] { new StringParam("Regex", ""), new StringParam("Replace with","")}));
            return
                new Parameters(new Parameter[]{
					new SingleChoiceWithSubParams("Pattern", 0)
					{Values = vals, SubParams = subparams, ParamNameWidth = 100, TotalWidth = 400}
				});
        }

        private static List<string[]> GetSelectableRegexes(){
			return new List<string[]>{
				new[]{"..._01,02,03", "^(.*)_[0-9]*$"}, new[]{"(LFQ) intensity ..._01,02,03", "^(?:LFQ )?[Ii]ntensity (.*)_[0-9]*$"},
				new[]{"(Normalized) ratio H/L ..._01,02,03", "^(?:Normalized )?[Rr]atio(?: [HML]/[HML]) (.*)_[0-9]*$"}
			};
		}
	}
}