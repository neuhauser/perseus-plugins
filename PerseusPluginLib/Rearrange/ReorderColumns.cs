using System.Collections.Generic;
using System.Drawing;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusPluginLib.Utils;

namespace PerseusPluginLib.Rearrange{
	public class ReorderColumns : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Image ButtonImage { get { return null; } }
		public string HelpOutput { get { return "Same matrix but with columns in the new order."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Heading { get { return "Matrix rearrangements"; } }
		public string Name { get { return "Reorder/remove columns"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 3; } }
		public DocumentType HelpDescriptionType { get { return DocumentType.PlainText; } }
		public DocumentType HelpOutputType { get { return DocumentType.PlainText; } }
		public DocumentType[] HelpSupplTablesType { get { return new DocumentType[0]; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public DocumentType[] HelpDocumentTypes { get { return new DocumentType[0]; } }
		public int NumDocuments { get { return 0; } }
		public string HelpDescription{
			get{
				return
					"The order of the columns as they appear in the matrix can be changed. Columns can also be omitted. For example, " +
						"this can be useful for displaying columns in a specific order in a heat map.";
			}
		}

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData data, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			int[] exColInds = param.GetMultiChoiceParam("Expression columns").Value;
			int[] numColInds = param.GetMultiChoiceParam("Numerical columns").Value;
			int[] multiNumColInds = param.GetMultiChoiceParam("Multi-numerical columns").Value;
			int[] catColInds = param.GetMultiChoiceParam("Categorical columns").Value;
			int[] textColInds = param.GetMultiChoiceParam("Text columns").Value;
			data.ExtractExpressionColumns(exColInds);
			data.NumericColumns = ArrayUtils.SubList(data.NumericColumns, numColInds);
			data.NumericColumnNames = ArrayUtils.SubList(data.NumericColumnNames, numColInds);
			data.NumericColumnDescriptions = ArrayUtils.SubList(data.NumericColumnDescriptions, numColInds);
			data.MultiNumericColumns = ArrayUtils.SubList(data.MultiNumericColumns, multiNumColInds);
			data.MultiNumericColumnNames = ArrayUtils.SubList(data.MultiNumericColumnNames, multiNumColInds);
			data.MultiNumericColumnDescriptions = ArrayUtils.SubList(data.MultiNumericColumnDescriptions, multiNumColInds);
			data.CategoryColumns = PerseusPluginUtils.GetCategoryColumns(data, catColInds);
			data.CategoryColumnNames = ArrayUtils.SubList(data.CategoryColumnNames, catColInds);
			data.CategoryColumnDescriptions = ArrayUtils.SubList(data.CategoryColumnDescriptions, catColInds);
			data.StringColumns = ArrayUtils.SubList(data.StringColumns, textColInds);
			data.StringColumnNames = ArrayUtils.SubList(data.StringColumnNames, textColInds);
			data.StringColumnDescriptions = ArrayUtils.SubList(data.StringColumnDescriptions, textColInds);
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			List<string> exCols = mdata.ExpressionColumnNames;
			List<string> numCols = mdata.NumericColumnNames;
			List<string> multiNumCols = mdata.MultiNumericColumnNames;
			List<string> catCols = mdata.CategoryColumnNames;
			List<string> textCols = mdata.StringColumnNames;
			return
				new Parameters(new Parameter[]{
					new MultiChoiceParam("Expression columns"){
						Value = ArrayUtils.ConsecutiveInts(exCols.Count), Values = exCols,
						Help = "Specify here the new order in which the expression columns should appear."
					},
					new MultiChoiceParam("Numerical columns"){
						Value = ArrayUtils.ConsecutiveInts(numCols.Count), Values = numCols,
						Help = "Specify here the new order in which the numerical columns should appear."
					},
					new MultiChoiceParam("Multi-numerical columns"){
						Value = ArrayUtils.ConsecutiveInts(multiNumCols.Count), Values = multiNumCols,
						Help = "Specify here the new order in which the numerical columns should appear."
					},
					new MultiChoiceParam("Categorical columns"){
						Value = ArrayUtils.ConsecutiveInts(catCols.Count), Values = catCols,
						Help = "Specify here the new order in which the categorical columns should appear."
					},
					new MultiChoiceParam("Text columns"){
						Value = ArrayUtils.ConsecutiveInts(textCols.Count), Values = textCols,
						Help = "Specify here the new order in which the text columns should appear."
					}
				});
		}
	}
}