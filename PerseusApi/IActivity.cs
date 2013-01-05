using System.Drawing;
using BasicLib.Param;
using BasicLib.Util;

namespace PerseusApi{
	/// <summary>
	/// This interface is base from which all other activities are derived. 
	/// It provides properties that are common to all activities. 
	/// </summary>
	public interface IActivity{
		string Name { get; }
		float DisplayOrder { get; }
		string HelpDescription { get; }
		HelpType HelpDescriptionType { get; }
		bool IsActive { get; }
		bool HasButton { get; }
		Image ButtonImage { get; }
		int GetMaxThreads(Parameters parameters);
	}
}