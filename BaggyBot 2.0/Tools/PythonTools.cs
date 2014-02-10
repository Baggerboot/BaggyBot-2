using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;

namespace BaggyBot.Tools
{
	public class PythonTools
	{
		private IrcInterface ircInterface;

		public PythonTools(IrcInterface ircInterface)
		{
			this.ircInterface = ircInterface;
		}

		public string plot(IEnumerable<object> data, string x = "Index", string y = "Value")
		{
			string filename;
			int num;
			using (var w = new StreamWriter(Tools.MiscTools.GetContentName(out filename, out num, "plots", ".csv", 4))) {
				w.WriteLine(x + ", " + y);
				int i = 0;
				foreach (var item in data) {
					w.WriteLine("{0}, {1}", i, item.ToString());
					i++;
				}
			}
			Process p = Process.Start("R", string.Format("-f /var/www/html/usercontent/plots/generate-plot.R --args /var/www/html/usercontent/plots/{0} /var/www/html/usercontent/plots/{1:X4}.png {2} {3}", filename, num, x, y));
			return string.Format(" http://jgeluk.net/usercontent/plots/{0:X4}.png ", num); 		
		}

		public string plot(params object[] data)
		{
			return plot(data, "Index", "Value");
		}
		public string plotfunction(Func<object, object> function, int resolution = 50, string x = "Index", string y = "Value")
		{
			List<object> values = new List<object>();

			for (float f = 0; f < resolution; f+= 0.1f) {
				values.Add(function(f));
			}
			return plot(values, x, y);
		}
	}
}
