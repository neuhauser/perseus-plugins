using System.Collections.Generic;

namespace PerseusApi.Matrix{
	public interface IMatrixData : IData{
		int RowCount { get; }
		float[,] ExpressionValues { get; set; }
		float[,] QualityValues { get; set; }
		string QualityName { get; set; }
		bool QualityBiggerIsBetter { get; set; }
		bool HasQuality { get; set; }
		bool[,] IsImputed { get; set; }
		int ExpressionColumnCount { get; }
		List<string> ExpressionColumnNames { get; set; }
		List<string> ExpressionColumnDescriptions { get; set; }
		float[] GetExpressionRow(int row);
		float[] GetExpressionColumn(int col);
		float this[int i, int j] { get; set; }
		int CategoryColumnCount { get; }
		List<string> CategoryColumnNames { get; set; }
		List<string> CategoryColumnDescriptions { get; set; }
		List<string[][]> CategoryColumns { get; set; }
		void AddCategoryColumn(string name, string description, string[][] vals);
		int NumericColumnCount { get; }
		List<string> NumericColumnNames { get; set; }
		List<string> NumericColumnDescriptions { get; set; }
		List<double[]> NumericColumns { get; set; }
		void AddNumericColumn(string name, string description, double[] vals);
		int StringColumnCount { get; }
		List<string> StringColumnNames { get; set; }
		List<string> StringColumnDescriptions { get; set; }
		List<string[]> StringColumns { get; set; }
		void AddStringColumn(string name, string description, string[] vals);
		int MultiNumericColumnCount { get; }
		List<string> MultiNumericColumnNames { get; set; }
		List<string> MultiNumericColumnDescriptions { get; set; }
		List<double[][]> MultiNumericColumns { get; set; }
		void AddMultiNumericColumn(string name, string description, double[][] vals);
		int CategoryRowCount { get; }
		List<string> CategoryRowNames { get; set; }
		List<string> CategoryRowDescriptions { get; set; }
		List<string[][]> CategoryRows { get; set; }
		void AddCategoryRow(string name, string description, string[][] vals);
		void ClearCategoryRows();
		int NumericRowCount { get; }
		List<string> NumericRowNames { get; set; }
		List<string> NumericRowDescriptions { get; set; }
		List<double[]> NumericRows { get; set; }
		void AddNumericRow(string name, string description, double[] vals);
		void ClearNumericRows();
		void ExtractExpressionRows(int[] order);
		void ExtractExpressionColumns(int[] order);
		IMatrixData Copy();
		IMatrixData CreateNewInstance();
		void Clear();

		void SetData(string name, List<string> expressionColumnNames, float[,] expressionValues,
			List<string> stringColumnNames, List<string[]> stringColumns, List<string> categoryColumnNames,
			List<string[][]> categoryColumns, List<string> numericColumnNames, List<double[]> numericColumns,
			List<string> multiNumericColumnNames, List<double[][]> multiNumericColumns);

		void SetData(string name, List<string> expressionColumnNames, float[,] expressionValues, bool[,] isImputed,
			List<string> stringColumnNames, List<string[]> stringColumns, List<string> categoryColumnNames,
			List<string[][]> categoryColumns, List<string> numericColumnNames, List<double[]> numericColumns,
			List<string> multiNumericColumnNames, List<double[][]> multiNumericColumns);

		void SetData(string name, List<string> expressionColumnNames, float[,] expressionValues,
			List<string> stringColumnNames, List<string[]> stringColumns, List<string> categoryColumnNames,
			List<string[][]> categoryColumns, List<string> numericColumnNames, List<double[]> numericColumns,
			List<string> multiNumericColumnNames, List<double[][]> multiNumericColumns, List<string> categoryRowNames,
			List<string[][]> categoryRows, List<string> numericRowNames, List<double[]> numericRows);

		void SetData(string name, List<string> expressionColumnNames, float[,] expressionValues, bool[,] isImputed,
			List<string> stringColumnNames, List<string[]> stringColumns, List<string> categoryColumnNames,
			List<string[][]> categoryColumns, List<string> numericColumnNames, List<double[]> numericColumns,
			List<string> multiNumericColumnNames, List<double[][]> multiNumericColumns, List<string> categoryRowNames,
			List<string[][]> categoryRows, List<string> numericRowNames, List<double[]> numericRows);

		void SetData(string name, string description, List<string> expressionColumnNames,
			List<string> expressionColumnDescriptions, float[,] expressionValues, bool[,] isImputed, float[,] qualityValues,
			string qualityName, bool qualityBiggerIsBetter, List<string> stringColumnNames, List<string> stringColumnDescriptions,
			List<string[]> stringColumns, List<string> categoryColumnNames, List<string> categoryColumnDescriptions,
			List<string[][]> categoryColumns, List<string> numericColumnNames, List<string> numericColumnDescriptions,
			List<double[]> numericColumns, List<string> multiNumericColumnNames, List<string> multiNumericColumnDescriptions,
			List<double[][]> multiNumericColumns, List<string> categoryRowNames, List<string> categoryRowDescriptions,
			List<string[][]> categoryRows, List<string> numericRowNames, List<string> numericRowDescriptions,
			List<double[]> numericRows);
	}
}