using System;

namespace PerseusApi{
	public interface IData{
		string Name { get; set; }
		string Description { get; set; }
		string Origin { get; set; }
		DateTime CreationDate { get; set; }
		string User { get; set; }
	}
}