using System;

namespace PerseusApi{
	public class ProcessInfo{
		public Settings Settings { get; private set; }
		public Action<string> Status { get; private set; }
		public Action<int> Progress { get; private set; }
		public int NumThreads { get; private set; }
		public Action<int> ReduceThreads { get; private set; }
		public string ErrString { get; set; }

		public ProcessInfo(Settings settings, Action<string> status, Action<int> progress, int numThreads,
			Action<int> reduceThreads){
			Settings = settings;
			Status = status;
			Progress = progress;
			NumThreads = numThreads;
			ReduceThreads = reduceThreads;
		}
	}
}