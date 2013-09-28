using System.Drawing;
using System.Text.RegularExpressions;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using System.Collections.Generic;
using System;

namespace PerseusPluginLib.Group{
	/// <remarks>author: Marco Hein</remarks>>
	public class NameBasedGrouping : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Image ButtonImage { get { return null; } }
		public string HelpDescription{
			get{
				return
					"Group the sample automatically from the sample names. You can select pre-defined patterns or define your own regular expression.";
			}
		}
		public string HelpOutput { get { return "An annotation row will be generated."; } }
		public DocumentType HelpDescriptionType { get { return DocumentType.PlainText; } }
		public DocumentType HelpOutputType { get { return DocumentType.PlainText; } }
		public DocumentType[] HelpSupplTablesType { get { return new DocumentType[0]; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Name-based grouping"; } }
		public string Heading { get { return "Annotation rows"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 100; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public DocumentType[] HelpDocumentTypes { get { return new DocumentType[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			string regexString = param.GetSingleChoiceWithSubParams("Pattern").Value < SelectableRegexes().Count
				? SelectableRegexes()[param.GetSingleChoiceWithSubParams("Pattern").Value][1]
				: param.GetSingleChoiceWithSubParams("Pattern").GetSubParameters().GetStringParam("Regex").Value;
			Regex regex;
			try{
				regex = new Regex(regexString);
			} catch (ArgumentException){
				processInfo.ErrString = "The regular expression you provided has invalid syntax.";
				return;
			}
			List<string> sampleNames = mdata.ExpressionColumnNames;
			List<string[]> groupNames = new List<string[]>();
			foreach (string sampleName in sampleNames){
				string groupName = regex.Match(sampleName).Groups[1].Value;
				if (string.IsNullOrEmpty(groupName)){
					groupName = sampleName;
				}
				groupNames.Add(new[]{groupName});
			}
			mdata.AddCategoryRow("Grouping", "", groupNames.ToArray());
		}

		private static List<string[]> SelectableRegexes(){
			return new List<string[]>{
				new[]{"..._01,02,03", "^(.*)_[0-9]*$"}, new[]{"(LFQ) intensity ..._01,02,03", "^(?:LFQ )?[Ii]ntensity (.*)_[0-9]*$"},
				new[]{"(Normalized) ratio H/L ..._01,02,03", "^(?:Normalized )?[Rr]atio(?: [HML]/[HML]) (.*)_[0-9]*$"}
			};
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			List<string> vals = new List<string>();
			foreach (string[] s in SelectableRegexes()){
				vals.Add(s[0]);
			}
			vals.Add("define regular expression");
			List<Parameters> subparams = new List<Parameters>();
			for (int i = 0; i < SelectableRegexes().Count; i++){
				subparams.Add(new Parameters(new Parameter[]{}));
			}
			subparams.Add(new Parameters(new Parameter[]{new StringParam("Regex", "")}));
			return
				new Parameters(new Parameter[]{
					new SingleChoiceWithSubParams("Pattern", 0)
					{Values = vals, SubParams = subparams, ParamNameWidth = 100, TotalWidth = 400}
				});
		}
	}
}