namespace PerseusApi {
	/// <summary>
	/// Grandmother of all data analysis activities. They operate on one IData and do not produce any new ones 
	/// automatically. They may do so interactively. 
	/// </summary>
	public interface IAnalysis {
		/// <summary>
		/// Does it make sense for this analysis to start it without any data. The scatter plot is one example where it does.
		/// </summary>
		bool CanStartWithEmptyData { get; }
	}
}
