using System;
using System.Windows.Forms;
using BasicLib.Util;
using PerseusApi;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Manual{
	public partial class SelectRowsManuallyControl : UserControl{
		private readonly IMatrixData mdata;
		private readonly Action<IMatrixData> createNewMatrix;

		public SelectRowsManuallyControl(IMatrixData mdata, Action<IMatrixData> createNewMatrix){
			InitializeComponent();
			this.mdata = mdata;
			this.createNewMatrix = createNewMatrix;
			matrixDataGridView.HasStatisticsMenuItem = false;
			matrixDataGridView.HasRemoveRowsMenuItems = false;
			matrixDataGridView.TableModel = new MatrixDataTable(mdata);
		}

		private void RemoveSelectedRowsButtonClick(object sender, EventArgs e){
			int[] sel = matrixDataGridView.GetSelectedRows();
			if (sel.Length == 0){
				MessageBox.Show("Please select some rows.");
			}
			IMatrixData mx = mdata.Copy();
			mx.ExtractExpressionRows(ArrayUtils.Complement(sel, matrixDataGridView.RowCount));
			createNewMatrix(mx);
		}

		private void KeepSelectedRowsButtonClick(object sender, EventArgs e){
			int[] sel = matrixDataGridView.GetSelectedRows();
			if (sel.Length == 0){
				MessageBox.Show("Please select some rows.");
			}
			IMatrixData mx = mdata.Copy();
			mx.ExtractExpressionRows(sel);
			createNewMatrix(mx);
		}
	}
}