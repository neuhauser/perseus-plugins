using System;
using System.Collections.Generic;
using System.Drawing;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;
using PerseusApi.Matrix;
using PerseusPluginLib.Properties;
using PerseusPluginLib.Utils;

namespace PerseusPluginLib.Filter{
	public class FilterValidValues : IMatrixProcessing{
		public bool HasButton { get { return true; } }
		public Image ButtonImage { get { return Resources.missingsButton_Image; } }
		public string Name { get { return "Filter valid values"; } }
		public string Heading { get { return "Filter"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return -4; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public HelpType HelpDescriptionType { get { return HelpType.PlainText; } }
		public HelpType HelpOutputType { get { return HelpType.PlainText; } }
		public HelpType[] HelpSupplTablesType { get { return new HelpType[0]; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public HelpType[] HelpDocumentTypes { get { return new HelpType[0]; } }
		public int NumDocuments { get { return 0; } }
		public string HelpDescription {
			get{
				return
					"Rows/columns of the expression matrix are filtered to contain at least the specified numbers of valid (non-NaN) values.";
			}
		}
		public string HelpOutput { get{
			return
				"The matrix of expression values is constrained to contain only these rows/columns that fulfil the requirement.";
		} }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents, ProcessInfo processInfo) {
			bool rows = param.GetSingleChoiceParam("Matrix access").Value == 0;
			bool atLeast = param.GetSingleChoiceParam("Side").Value == 0;
			int numValids = param.GetIntParam("Number of valid values").Value;
			SingleChoiceWithSubParams modeParam = param.GetSingleChoiceWithSubParams("Mode");
			int modeInd = modeParam.Value;
			if (modeInd != 0 && mdata.CategoryRowNames.Count == 0){
				processInfo.ErrString = "No grouping is defined.";
				return;
			}
			if (modeInd != 0 && !rows){
				processInfo.ErrString = "Group-wise filtering can only be appled to rows.";
				return;
			}
			if (modeInd != 0){
				int gind = modeParam.GetSubParameters().GetSingleChoiceParam("Grouping").Value;
				string[][] groupCol = mdata.CategoryRows[gind];
				ValidValueFilterGroup(numValids, mdata, param, modeInd == 2, groupCol, atLeast);
			} else{
				ValidValueFilter1(rows, numValids, mdata, param, atLeast);
			}
		}

		private static void ValidValueFilterGroup(int minValids, IMatrixData mdata, Parameters param, bool oneGroup,
			IList<string[]> groupCol, bool atLeast){
			string[] groupVals = ArrayUtils.UniqueValuesPreserveOrder(groupCol);
			Array.Sort(groupVals);
			int[][] groupInds = CalcGroupInds(groupVals, groupCol);
			List<int> valids = new List<int>();
			for (int i = 0; i < mdata.RowCount; i++){
				int[] counts = new int[groupVals.Length];
				for (int j = 0; j < mdata.ExpressionColumnCount; j++){
					double q = mdata[i, j];
					if (!double.IsNaN(q)){
						for (int k = 0; k < groupInds[j].Length; k++){
							if (groupInds[j][k] >= 0){
								counts[groupInds[j][k]]++;
							}
						}
					}
				}
				int val = oneGroup ? ArrayUtils.Max(counts) : ArrayUtils.Min(counts);
				if (atLeast){
					if (val >= minValids){
						valids.Add(i);
					}
				} else{
					if (val <= minValids){
						valids.Add(i);
					}
				}
			}
			PerseusPluginUtils.FilterRows(mdata, param, valids.ToArray());
		}

		internal static int[][] CalcGroupInds(string[] groupVals, IList<string[]> groupCol){
			int[][] result = new int[groupCol.Count][];
			for (int i = 0; i < result.Length; i++){
				result[i] = new int[groupCol[i].Length];
				for (int j = 0; j < result[i].Length; j++){
					result[i][j] = Array.BinarySearch(groupVals, groupCol[i][j]);
				}
			}
			return result;
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			Parameters[] subParams = new Parameters[3];
			subParams[0] = new Parameters(new Parameter[0]);
			subParams[1] = new Parameters(new Parameter[]{new SingleChoiceParam("Grouping"){Values = mdata.CategoryRowNames}});
			subParams[2] = new Parameters(new Parameter[]{new SingleChoiceParam("Grouping"){Values = mdata.CategoryRowNames}});
			return
				new Parameters(new Parameter[]{
					new SingleChoiceParam("Matrix access"){
						Values = new[]{"Rows", "Columns"},
						Help = "Specifies if the analysis is performed on the rows or the columns of the matrix."
					},
					new SingleChoiceParam("Side"){
						Values = new[]{"At least", "At most"},
						Help = "Decide here if the specified threshold is a lower or an upper limit on the valid values."
					},
					new IntParam("Number of valid values", 3){
						Help =
							"If a row/column has less or more (depending on the parameter 'Side') than the specified number of valid values it" +
								" will be discarded in the output."
					},
					new SingleChoiceWithSubParams("Mode")
					{Values = new[]{"In total", "In each group", "In at least one group"}, SubParams = subParams},
					PerseusPluginUtils.GetFilterModeParam()
				});
		}

		public static void ValidValueFilter1(bool rows, int minValids, IMatrixData mdata, Parameters param, bool atLeast){
			if (rows){
				List<int> valids = new List<int>();
				for (int i = 0; i < mdata.RowCount; i++){
					int count = 0;
					for (int j = 0; j < mdata.ExpressionColumnCount; j++){
						double q = mdata[i, j];
						if (!double.IsNaN(q)){
							count++;
						}
					}
					if (atLeast){
						if (count >= minValids){
							valids.Add(i);
						}
					} else{
						if (count <= minValids){
							valids.Add(i);
						}
					}
				}
				PerseusPluginUtils.FilterRows(mdata, param, valids.ToArray());
			} else{
				List<int> valids = new List<int>();
				for (int j = 0; j < mdata.ExpressionColumnCount; j++){
					int count = 0;
					for (int i = 0; i < mdata.RowCount; i++){
						double q = mdata[i, j];
						if (!double.IsNaN(q)){
							count++;
						}
					}
					if (atLeast){
						if (count >= minValids){
							valids.Add(j);
						}
					} else{
						if (count <= minValids){
							valids.Add(j);
						}
					}
				}
				PerseusPluginUtils.FilterColumns(mdata, param, valids.ToArray());
			}
		}
	}
}