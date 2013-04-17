namespace PerseusPluginLib.Basic{
	public class PerformanceColumnTypeTpNp : PerformanceColumnType{
		public override string Name { get { return "TP/NP"; } }

		public override double Calculate(double tp, double tn, double fp, double fn, double np, double nn){
			return tp/np;
		}
	}
}