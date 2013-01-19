using System;

namespace PerseusApi{
	public interface IData{
		string Name { get; set; }
		string Description { get; set; }
		string Origin { get; set; }
		DateTime CreationDate { get; set; }
		/// <summary>
		/// Name of the user who created this data item.
		/// </summary>
		string User { get; set; }
	}
}