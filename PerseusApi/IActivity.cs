using System.Drawing;
using BasicLib.Param;
using BasicLib.Util;

namespace PerseusApi{
	/// <summary>
	/// This interface is the base from which all other activities are derived. 
	/// It provides properties that are common to all activities. 
	/// </summary>
	public interface IActivity{
		/// <summary>
		/// This is the name that appears in the drop-down menu of Perseus to start this activity.
		/// </summary>
		string Name { get; }
		float DisplayOrder { get; }
		string HelpDescription { get; }
		HelpType HelpDescriptionType { get; }
		/// <summary>
		/// If false is returned, the activity will not be available.
		/// </summary>
		bool IsActive { get; }
		bool HasButton { get; }
		Image ButtonImage { get; }
		int GetMaxThreads(Parameters parameters);
	}
}