using System;
using System.Collections.Generic;
using System.Drawing;
using BasicLib.Param;
using BasicLib.Util;
using PerseusApi;

namespace PerseusPluginLib.AbsQuant{
	public class CopyNumbers : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Image ButtonImage { get { return null; } }
		public string HelpDescription { get { return "Estimate cellular copy numbers from protein intensities."; } }
		public HelpType HelpDescriptionType { get { return HelpType.PlainText; } }
		public string HelpOutput { get { return "A column is added which contains the copy number estimation."; } }
		public HelpType HelpOutputType { get { return HelpType.PlainText; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public HelpType[] HelpSupplTablesType { get { return new HelpType[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string HelpSupplTable { get { return ""; } }
		public string Name { get { return "Estimate copy numbers"; } }
		public string Heading { get { return "Interaction"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 1; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ProcessInfo processInfo) {
			int[] intensityCols = param.GetMultiChoiceParam("Intensities").Value;
			if (intensityCols.Length == 0) {
				processInfo.ErrString = "Please select at least one column containing protein intensities.";
				return;
			}
			double[][] rawIntensitiesReplicates = new double[mdata.RowCount][];
			double[] rawIntensities = new double[mdata.RowCount];
			for (int row = 0; row < mdata.RowCount; row++) {
				double[] values = new double[intensityCols.Length];
				for (int rep = 0; rep < intensityCols.Length; rep++) {
					values[rep] = mdata.NumericColumns[intensityCols[rep]][row];
				}
				rawIntensities[row] = ArrayUtils.Median(values);
			}
			double[] molarityNormFactor = mdata.NumericColumns[param.GetSingleChoiceParam("Molarity normalization factor").Value];
			double[] intensities = new double[mdata.RowCount];
			for (int row = 0; row < mdata.RowCount; row++) {
				intensities[row] = rawIntensities[row];
			}
			if (param.GetBoolWithSubParams("is logarithmized").Value) {
				double[] logBases = new[] { 2, Math.E, 10 };
				double logBase =
					logBases[param.GetBoolWithSubParams("is logarithmized").GetSubParameters().GetSingleChoiceParam("log base").Value];
				for (int row = 0; row < mdata.RowCount; row++) {
					intensities[row] = Math.Pow(logBase, intensities[row]) / molarityNormFactor[row];
				}
			} else {
				for (int row = 0; row < mdata.RowCount; row++) {
					intensities[row] /= molarityNormFactor[row];
				}
			}
			double[] mw = mdata.NumericColumns[param.GetSingleChoiceParam("Molecular weights").Value];
			double[] copyNumbers = new double[mdata.RowCount];
			const double avogadro = 6.02214129e23;
			double mwWeightedIntensities = 0;
			for (int row = 0; row < mdata.RowCount; row++) {
				if (!double.IsNaN(intensities[row]) && !double.IsNaN(mw[row])) {
					mwWeightedIntensities += intensities[row] * mw[row];
				}
			}
			double factor = (param.GetDoubleParam("Protein amount per cell").Value * 1e-12 * avogadro) / mwWeightedIntensities;
			for (int row = 0; row < mdata.RowCount; row++) {
				copyNumbers[row] = intensities[row] * factor;
			}
			mdata.AddNumericColumn("Copy number", "", copyNumbers);
			double[] rank = ArrayUtils.Rank(copyNumbers);
			double[] relativeRank = new double[mdata.RowCount];
			double validRanks = mdata.RowCount;
			for (int row = 0; row < mdata.RowCount; row++) {
				// remove rank for protein with no copy number information
				if (double.IsNaN((copyNumbers[row])) || copyNumbers[row] == 0) {
					rank[row] = double.NaN;
					validRanks--; // do not consider as valid
				}
				// invert ranking, so that rank 0 is the most abundant protein
				rank[row] = mdata.RowCount - rank[row];
			}
			for (int row = 0; row < mdata.RowCount; row++) {
				relativeRank[row] = rank[row] / validRanks;
			}
			mdata.AddNumericColumn("Copy number rank", "", rank);
			mdata.AddNumericColumn("Relative copy number rank", "", relativeRank);
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			Parameters[] subParams = new Parameters[2];
			subParams[0] = new Parameters(new Parameter[0]);
			subParams[1] =
				new Parameters(new Parameter[]{new SingleChoiceParam("log base", 0){Values = new[]{"2", "natural", "10"}}});
			return
				new Parameters(new Parameter[]{
					new MultiChoiceParam("Intensities", IndicesOfElementsStartingWith(mdata.NumericColumnNames, "Intensity")){
						Values = mdata.NumericColumnNames,
						Help =
							"Specify the columns that contain the intensities to be used for copy number estimation. " +
								"If several columns are selected, the method will calculate the median."
					},
					new BoolWithSubParams("is logarithmized", true){
						SubParamsFalse = subParams[0], SubParamsTrue = subParams[1],
						Help = "Specify whether the intensities are logarithmized in the selected column."
					},
					new SingleChoiceParam("Molecular weights", ArrayUtils.IndexOf(mdata.NumericColumnNames, "Average molecular weight"))
					{Values = mdata.NumericColumnNames},
					new SingleChoiceParam("Molarity normalization factor",
						ArrayUtils.IndexOf(mdata.NumericColumnNames, "Number of theoretical peptides")
						){Values=mdata.NumericColumnNames,
					Help = "The values to be used for normalizing the intensities in order to get molar quantities " +
							"(typically the theoretical number of peptides or the sequence length)."},
					new DoubleParam("Protein amount per cell", 200){Help = "Specify the amount of protein per cell in picograms."}
				});
		}

		/// <summary>
		/// Returns the index of the first element in a (string) array that starts with a given string. 
		/// </summary>
		/// <param name="p">Array to be searched.</param>
		/// <param name="q">Start string to be found.</param>
		/// <returns>Index of first occurence. -1 otherwise.</returns>
		public static int IndexOfElementStartingWith(List<string> p, string q){
			for (int i = 0; i < p.Count; i++){
				if (p[i].StartsWith(q)){
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Returns the indeces of all elements in a (string) array that starts with a given string. 
		/// </summary>
		/// <param name="p">Array to be searched.</param>
		/// <param name="q">Start string to be found.</param>
		/// <returns>Indeces of all occurences. -1 otherwise.</returns>
		public static int[] IndicesOfElementsStartingWith(List<string> p, string q){
			List<int> inds = new List<int>();
			for (int i = 0; i < p.Count; i++){
				if (p[i].StartsWith(q)){
					inds.Add(i);
				}
			}
			return inds.ToArray();
		}
	}
}