using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using BasicLib.Parse;
using BasicLib.Util;

namespace PerseusPluginLib.Load{
	public partial class PerseusLoadFileParameterPanel : UserControl{
		private static readonly HashSet<string> commentPrefix = new HashSet<string>(new[]{"#", "!"});
		private static readonly HashSet<string> commentPrefixExceptions = new HashSet<string>(new[]{"#N/A", "#n/a"});
		private static readonly HashSet<string> categoricalCols =
			new HashSet<string>(new[]{
				"pfam names", "gocc names", "gomf names", "gobp names", "kegg pathway names", "chromosome", "strand",
				"interpro name", "prints name", "prosite name", "smart name", "sequence motifs", "reactome", "transcription factors"
				, "microrna", "scop class", "scop fold", "scop superfamily", "scop family", "phospho motifs", "mim", "pdb", "intact"
				, "corum", "motifs", "best motif", "reverse", "contaminant", "only identified by site", "type", "amino acid"
			});
		private static readonly HashSet<string> textualCols =
			new HashSet<string>(new[]{
				"protein ids", "majority protein ids", "protein names", "gene names", "uniprot", "ensembl", "ensg", "ensp", "enst",
				"mgi", "kegg ortholog", "dip", "hprd interactors", "sequence window", "sequence", "orf name", "names", "proteins",
				"positions within proteins"
			});
		private static readonly HashSet<string> numericCols =
			new HashSet<string>(new[]{
				"position", "total position", "peptides (seq)", "razor peptides (seq)", "unique peptides (seq)", "localization prob"
				, "size", "p value", "benj. hoch. fdr", "score", "score for localization", "pep"
			});
		public string Filter { get; set; }
		public PerseusLoadFileParameterPanel() : this(new string[0]) {}

		public PerseusLoadFileParameterPanel(IList<string> items){
			InitializeComponent();
			multiListSelector1.Init(items, new[]{"Expression", "Numerical", "Categorical", "Text", "Multi-numerical"});
		}

		public string Filename { get { return textBox.Text; } }
		public int[] ExpressionColumnIndices { get { return multiListSelector1.GetSelectedIndices(0); } }
		public int[] NumericalColumnIndices { get { return multiListSelector1.GetSelectedIndices(1); } }
		public int[] CategoryColumnIndices { get { return multiListSelector1.GetSelectedIndices(2); } }
		public int[] TextColumnIndices { get { return multiListSelector1.GetSelectedIndices(3); } }
		public int[] MultiNumericalColumnIndices { get { return multiListSelector1.GetSelectedIndices(4); } }
		public string[] Value{
			get{
				string[] result = new string[7];
				result[0] = Filename;
				result[1] = StringUtils.Concat(";", multiListSelector1.items);
				result[2] = StringUtils.Concat(";", ExpressionColumnIndices);
				result[3] = StringUtils.Concat(";", NumericalColumnIndices);
				result[4] = StringUtils.Concat(";", CategoryColumnIndices);
				result[5] = StringUtils.Concat(";", TextColumnIndices);
				result[6] = StringUtils.Concat(";", MultiNumericalColumnIndices);
				return result;
			}
			set{
				textBox.Text = value[0];
				multiListSelector1.items = value[1].Length > 0 ? value[1].Split(';') : new string[0];
				for (int i = 0; i < 5; i++){
					foreach (int ind in GetIndices(value[i + 2])){
						multiListSelector1.SetSelected(i, ind, true);
					}
				}
			}
		}

		private static IEnumerable<int> GetIndices(string s){
			string[] q = s.Length > 0 ? s.Split(';') : new string[0];
			int[] result = new int[q.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = int.Parse(q[i]);
			}
			return result;
		}

		private void ButtonClick(object sender, EventArgs e){
			OpenFileDialog ofd = new OpenFileDialog();
			if (Filter != null && !Filter.Equals("")){
				ofd.Filter = Filter;
			}
			if (ofd.ShowDialog() != DialogResult.OK){
				return;
			}
			textBox.Text = ofd.FileName;
			string filename = textBox.Text;
			if (string.IsNullOrEmpty(filename)){
				MessageBox.Show("Please specify a filename");
				return;
			}
			if (!File.Exists(filename)){
				MessageBox.Show("File '" + filename + "' does not exist.");
				return;
			}
			bool csv = filename.ToLower().EndsWith(".csv");
			char separator = csv ? ',' : '\t';
			string[] colNames;
			Dictionary<string, string[]> annotationRows = new Dictionary<string, string[]>();
			try{
				colNames = TabSep.GetColumnNames(filename, commentPrefix, commentPrefixExceptions, annotationRows, separator);
			} catch (Exception){
				MessageBox.Show("Could not open the file '" + filename + "'. It is probably opened by another program.");
				return;
			}
			string[] colDescriptions = null;
			string[] colTypes = null;
			bool[] colVisible = null;
			if (annotationRows.ContainsKey("Description")){
				colDescriptions = annotationRows["Description"];
				annotationRows.Remove("Description");
			}
			if (annotationRows.ContainsKey("Type")){
				colTypes = annotationRows["Type"];
				annotationRows.Remove("Type");
			}
			if (annotationRows.ContainsKey("Visible")){
				string[] colVis = annotationRows["Visible"];
				colVisible = new bool[colVis.Length];
				for (int i = 0; i < colVisible.Length; i++){
					colVisible[i] = bool.Parse(colVis[i]);
				}
				annotationRows.Remove("Visible");
			}
			string msg = TabSep.CanOpen(filename);
			if (msg != null){
				MessageBox.Show(msg);
				return;
			}
			multiListSelector1.Init(colNames);
			if (colTypes != null){
				SelectExact(colNames, colTypes, colVisible);
			} else{
				SelectHeuristic(colNames);
			}
		}

		private void SelectExact(ICollection<string> colNames, IList<string> colTypes, IList<bool> colVisible){
			for (int i = 0; i < colNames.Count; i++){
				if (colVisible == null || colVisible[i]){
					switch (colTypes[i]){
						case "E":
							multiListSelector1.SetSelected(0, i, true);
							break;
						case "N":
							multiListSelector1.SetSelected(1, i, true);
							break;
						case "C":
							multiListSelector1.SetSelected(2, i, true);
							break;
						case "T":
							multiListSelector1.SetSelected(3, i, true);
							break;
						case "M":
							multiListSelector1.SetSelected(4, i, true);
							break;
						default:
							throw new Exception("Unknown type: " + colTypes[i]);
					}
				}
			}
		}

		private void SelectHeuristic(IList<string> colNames){
			char guessedType = GuessSilacType(colNames);
			for (int i = 0; i < colNames.Count; i++){
				if (categoricalCols.Contains(colNames[i].ToLower())){
					multiListSelector1.SetSelected(2, i, true);
					continue;
				}
				if (textualCols.Contains(colNames[i].ToLower())){
					multiListSelector1.SetSelected(3, i, true);
					continue;
				}
				if (numericCols.Contains(colNames[i].ToLower())){
					multiListSelector1.SetSelected(1, i, true);
					continue;
				}
				switch (guessedType){
					case 's':
						if (colNames[i].StartsWith("Norm. Intensity")){
							multiListSelector1.SetSelected(0, i, true);
						}
						break;
					case 'd':
						if (colNames[i].StartsWith("Ratio H/L Normalized ")){
							multiListSelector1.SetSelected(0, i, true);
						}
						break;
				}
			}
		}

		private static char GuessSilacType(IEnumerable<string> colnames){
			bool isSilac = false;
			foreach (string s in colnames){
				if (s.StartsWith("Ratio M/L")){
					return 't';
				}
				if (s.StartsWith("Ratio H/L")){
					isSilac = true;
				}
			}
			return isSilac ? 'd' : 's';
		}

		public override string Text { get { return textBox.Text; } set { textBox.Text = value; } }
	}
}