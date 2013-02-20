using System;
using System.Collections.Generic;
using System.Drawing;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;

namespace PerseusPluginLib.Rearrange{
	public class ChangeColumnType : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Image ButtonImage { get { return null; } }
		public string HelpDescription { get { return ""; } }
		public string HelpOutput { get { return ""; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Change column type"; } }
		public string Heading { get { return "Matrix rearrangements"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 0; } }
		public HelpType HelpDescriptionType { get { return HelpType.PlainText; } }
		public HelpType HelpOutputType { get { return HelpType.PlainText; } }
		public HelpType[] HelpSupplTablesType { get { return new HelpType[0]; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ProcessInfo processInfo){
			SingleChoiceWithSubParams sp = param.GetSingleChoiceWithSubParams("Source type");
			Parameters subParams = sp.GetSubParameters();
			int[] colInds = subParams.GetMultiChoiceParam("Columns").Value;
			int which = subParams.GetSingleChoiceParam("Target type").Value;
			switch (sp.Value){
				case 0:
					ExpressionToNumeric(colInds, mdata);
					break;
				case 1:
					if (which == 0){
						NumericToCategorical(colInds, mdata);
					} else{
						NumericToExpression(colInds, mdata);
					}
					break;
				case 2:
					if (which == 0){
						CategoricalToNumeric(colInds, mdata);
					} else{
						CategoricalToString(colInds, mdata);
					}
					break;
				case 3:
					StringToCategorical(colInds, mdata);
					break;
				default:
					throw new Exception("Never get here");
			}
		}

		private static void StringToCategorical(IList<int> colInds, IMatrixData mdata){
			int[] inds = ArrayUtils.Complement(colInds, mdata.StringColumnCount);
			string[] names = ArrayUtils.SubArray(mdata.StringColumnNames, colInds);
			string[] descriptions = ArrayUtils.SubArray(mdata.StringColumnDescriptions, colInds);
			string[][] str = ArrayUtils.SubArray(mdata.StringColumns, colInds);
			string[][][] newCat = new string[str.Length][][];
			for (int j = 0; j < str.Length; j++){
				newCat[j] = new string[str[j].Length][];
				for (int i = 0; i < newCat[j].Length; i++){
					if (str[j][i] == null || str[j][i].Length == 0){
						newCat[j][i] = new string[0];
					} else{
						string[] x = str[j][i].Split(';');
						Array.Sort(x);
						newCat[j][i] = x;
					}
				}
			}
			mdata.CategoryColumnNames.AddRange(names);
			mdata.CategoryColumnDescriptions.AddRange(descriptions);
			mdata.CategoryColumns.AddRange(newCat);
			mdata.StringColumns = ArrayUtils.SubList(mdata.StringColumns, inds);
			mdata.StringColumnNames = ArrayUtils.SubList(mdata.StringColumnNames, inds);
			mdata.StringColumnDescriptions = ArrayUtils.SubList(mdata.StringColumnDescriptions, inds);
		}

		private static void CategoricalToNumeric(IList<int> colInds, IMatrixData mdata){
			int[] inds = ArrayUtils.Complement(colInds, mdata.CategoryColumnCount);
			string[] name = ArrayUtils.SubArray(mdata.CategoryColumnNames, colInds);
			string[] description = ArrayUtils.SubArray(mdata.CategoryColumnDescriptions, colInds);
			string[][][] cat = ArrayUtils.SubArray(mdata.CategoryColumns, colInds);
			double[][] newNum = new double[cat.Length][];
			for (int j = 0; j < cat.Length; j++){
				newNum[j] = new double[cat[j].Length];
				for (int i = 0; i < newNum[j].Length; i++){
					if (cat[j][i] == null || cat[j][i].Length == 0){
						newNum[j][i] = double.NaN;
					} else{
						double x;
						bool s = double.TryParse(cat[j][i][0], out x);
						if (s){
							newNum[j][i] = x;
						} else{
							newNum[j][i] = double.NaN;
						}
					}
				}
			}
			mdata.NumericColumnNames.AddRange(name);
			mdata.NumericColumnDescriptions.AddRange(description);
			mdata.NumericColumns.AddRange(newNum);
			mdata.CategoryColumns = ArrayUtils.SubList(mdata.CategoryColumns, inds);
			mdata.CategoryColumnNames = ArrayUtils.SubList(mdata.CategoryColumnNames, inds);
			mdata.CategoryColumnDescriptions = ArrayUtils.SubList(mdata.CategoryColumnDescriptions, inds);
		}

		private static void CategoricalToString(IList<int> colInds, IMatrixData mdata){
			int[] inds = ArrayUtils.Complement(colInds, mdata.CategoryColumnCount);
			string[] names = ArrayUtils.SubArray(mdata.CategoryColumnNames, colInds);
			string[] descriptions = ArrayUtils.SubArray(mdata.CategoryColumnDescriptions, colInds);
			string[][][] cat = ArrayUtils.SubArray(mdata.CategoryColumns, colInds);
			string[][] newString = new string[cat.Length][];
			for (int j = 0; j < cat.Length; j++){
				newString[j] = new string[cat[j].Length];
				for (int i = 0; i < newString[j].Length; i++){
					if (cat[j][i] == null || cat[j][i].Length == 0){
						newString[j][i] = "";
					} else{
						newString[j][i] = StringUtils.Concat(";", cat[j][i]);
					}
				}
			}
			mdata.StringColumnNames.AddRange(names);
			mdata.StringColumnDescriptions.AddRange(descriptions);
			mdata.StringColumns.AddRange(newString);
			mdata.CategoryColumns = ArrayUtils.SubList(mdata.CategoryColumns, inds);
			mdata.CategoryColumnNames = ArrayUtils.SubList(mdata.CategoryColumnNames, inds);
			mdata.CategoryColumnDescriptions = ArrayUtils.SubList(mdata.CategoryColumnDescriptions, inds);
		}

		private static void NumericToCategorical(IList<int> colInds, IMatrixData mdata){
			int[] inds = ArrayUtils.Complement(colInds, mdata.NumericColumnCount);
			string[] names = ArrayUtils.SubArray(mdata.NumericColumnNames, colInds);
			string[] descriptions = ArrayUtils.SubArray(mdata.NumericColumnDescriptions, colInds);
			double[][] num = ArrayUtils.SubArray(mdata.NumericColumns, colInds);
			string[][][] newCat = new string[num.Length][][];
			for (int j = 0; j < num.Length; j++){
				newCat[j] = new string[num[j].Length][];
				for (int i = 0; i < newCat[j].Length; i++){
					if (double.IsNaN(num[j][i]) || double.IsInfinity(num[j][i])){
						newCat[j][i] = new string[0];
					} else{
						newCat[j][i] = new[]{"" + num[j][i]};
					}
				}
			}
			mdata.CategoryColumnNames.AddRange(names);
			mdata.CategoryColumnDescriptions.AddRange(descriptions);
			mdata.CategoryColumns.AddRange(newCat);
			mdata.NumericColumns = ArrayUtils.SubList(mdata.NumericColumns, inds);
			mdata.NumericColumnNames = ArrayUtils.SubList(mdata.NumericColumnNames, inds);
			mdata.NumericColumnDescriptions = ArrayUtils.SubList(mdata.NumericColumnDescriptions, inds);
		}

		private static void NumericToExpression(IList<int> colInds, IMatrixData mdata){
			int[] inds = ArrayUtils.Complement(colInds, mdata.NumericColumnCount);
			string[] names = ArrayUtils.SubArray(mdata.NumericColumnNames, colInds);
			string[] descriptions = ArrayUtils.SubArray(mdata.NumericColumnDescriptions, colInds);
			double[][] num = ArrayUtils.SubArray(mdata.NumericColumns, colInds);
			float[][] newEx = new float[num.Length][];
			for (int j = 0; j < num.Length; j++){
				newEx[j] = new float[num[j].Length];
				for (int i = 0; i < newEx[j].Length; i++){
					newEx[j][i] = (float) num[j][i];
				}
			}
			float[,] newExp = new float[mdata.RowCount,mdata.ExpressionColumnCount + num.Length];
			for (int i = 0; i < mdata.RowCount; i++){
				for (int j = 0; j < mdata.ExpressionColumnCount; j++){
					newExp[i, j] = mdata[i, j];
				}
				for (int j = 0; j < newEx.Length; j++){
					newExp[i, j + mdata.ExpressionColumnCount] = newEx[j][i];
				}
			}
			mdata.ExpressionValues = newExp;
			mdata.ExpressionColumnNames.AddRange(names);
			mdata.ExpressionColumnDescriptions.AddRange(descriptions);
			mdata.NumericColumns = ArrayUtils.SubList(mdata.NumericColumns, inds);
			mdata.NumericColumnNames = ArrayUtils.SubList(mdata.NumericColumnNames, inds);
			mdata.NumericColumnDescriptions = ArrayUtils.SubList(mdata.NumericColumnDescriptions, inds);
			for (int i = 0; i < mdata.CategoryRows.Count; i++){
				mdata.CategoryRows[i] = ExtendCategoryRow(mdata.CategoryRows[i], num.Length);
			}
			for (int i = 0; i < mdata.NumericRows.Count; i++){
				mdata.NumericRows[i] = ExtendNumericRow(mdata.NumericRows[i], num.Length);
			}
		}

		private static void ExpressionToNumeric(IList<int> colInds, IMatrixData mdata){
			int[] remainingInds = ArrayUtils.Complement(colInds, mdata.NumericColumnCount);
			foreach (int colInd in colInds){
				double[] d = ArrayUtils.ToDoubles(mdata.GetExpressionColumn(colInd));
				mdata.AddNumericColumn(mdata.ExpressionColumnNames[colInd], mdata.ExpressionColumnDescriptions[colInd], d);
			}
			mdata.ExtractExpressionColumns(remainingInds);
		}

		private static double[] ExtendNumericRow(IList<double> numericRow, int add){
			double[] result = new double[numericRow.Count + add];
			for (int i = 0; i < numericRow.Count; i++){
				result[i] = numericRow[i];
			}
			for (int i = numericRow.Count; i < numericRow.Count + add; i++){
				result[i] = double.NaN;
			}
			return result;
		}

		private static string[][] ExtendCategoryRow(IList<string[]> categoryRow, int add){
			string[][] result = new string[categoryRow.Count + add][];
			for (int i = 0; i < categoryRow.Count; i++){
				result[i] = categoryRow[i];
			}
			for (int i = categoryRow.Count; i < categoryRow.Count + add; i++){
				result[i] = new string[0];
			}
			return result;
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			string[] choice = new[]{"Expression", "Numerical", "Categorical", "String"};
			List<Parameters> subParams = new List<Parameters>{
				GetExpressionSubParams(mdata), GetNumericSubParams(mdata), GetCategoricalSubParams(mdata), GetStringSubParams(mdata)
			};
			return
				new Parameters(new Parameter[]{
					new SingleChoiceWithSubParams("Source type")
					{Values = choice, Help = "Select here the column whose type should be changed.", SubParams = subParams}
				});
		}

		private static Parameters GetStringSubParams(IMatrixData mdata){
			return
				new Parameters(new Parameter[]{
					new MultiChoiceParam("Columns"){Values = mdata.StringColumnNames},
					new SingleChoiceParam("Target type", 0){Values = new[]{"Categorical"}}
				});
		}

		private static Parameters GetCategoricalSubParams(IMatrixData mdata){
			return
				new Parameters(new Parameter[]{
					new MultiChoiceParam("Columns"){Values = mdata.CategoryColumnNames},
					new SingleChoiceParam("Target type", 0){Values = new[]{"Numerical", "String"}}
				});
		}

		private static Parameters GetExpressionSubParams(IMatrixData mdata){
			return
				new Parameters(new Parameter[]{
					new MultiChoiceParam("Columns"){Values = mdata.ExpressionColumnNames},
					new SingleChoiceParam("Target type", 0){Values = new[]{"Numerical"}}
				});
		}

		private static Parameters GetNumericSubParams(IMatrixData mdata){
			return
				new Parameters(new Parameter[]{
					new MultiChoiceParam("Columns"){Values = mdata.NumericColumnNames},
					new SingleChoiceParam("Target type", 0){Values = new[]{"Categorical", "Expression"}}
				});
		}
	}
}