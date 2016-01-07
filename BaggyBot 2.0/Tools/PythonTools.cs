using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;


namespace BaggyBot.Tools
{
	public class PythonTools
	{
		// ReSharper disable InconsistentNaming
		[PythonFunction]
		public string plot(IEnumerable<object> data, string x = "Index", string y = "Value")

		{
			string filename;
			int num;
			using (var w = new StreamWriter(MiscTools.GetContentName(out filename, out num, "plots", ".csv", 4))) {
				w.WriteLine(x + ", " + y);
				var i = 0;
				foreach (var item in data) {
					w.WriteLine("{0}, {1}", i, item);
					i++;
				}
			}
			Process.Start("R", string.Format("-f /var/www/html/usercontent/plots/generate-plot.R --args /var/www/html/usercontent/plots/{0} /var/www/html/usercontent/plots/{1:X4}.png {2} {3}", filename, num, x, y));
			return string.Format(" http://jgeluk.net/usercontent/plots/{0:X4}.png ", num); 		
		}

		[PythonFunction]
		public string plot(params object[] data)
		{
			return plot(data, "Index", "Value");
		}

		[PythonFunction]
		public string plotfunction(Func<object, object> function, int resolution = 50, string x = "Index", string y = "Value")
		{
			var values = new List<object>();

			for (float f = 0; f < resolution; f++) {
				values.Add(function(f));
			}
			return plot(values, x, y);
		}
		// ReSharper restore InconsistentNaming
	}
}
