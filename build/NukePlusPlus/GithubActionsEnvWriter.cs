using Nuke.Common.CI;
using Nuke.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NukePlusPlus {
	public class GithubActionsEnvWriter : OurConfigurationEntityWPublicLevelSet {

		public List<KeyValuePair<string, string>> vars = new();

		

		public override void Write(CustomFileWriter writer) {
			if (vars.Count == 0)
				return;
			if (level == WRITE_LEVEL.Invalid)
				throw new ArgumentException();
			//we will rely on parent taller for idents like everything else
			//var disposeOf = new List<IDisposable>();
			//var tabs = (int)level - 1;
			//for (var x = 0; x < tabs; x++)
			//	disposeOf.Add(writer.Indent());
			writer.WriteLine("env:");
			using (writer.Indent()) {
				foreach (var kvp in vars)
					writer.WriteLine($"{kvp.Key}: {kvp.Value}");
			}
			//disposeOf.Reverse();
			//foreach (var itm in disposeOf)
			//	itm.Dispose();

		}
		public void AddVar(string key, string value) => AddVar(new(key, value));
		public void AddVar(KeyValuePair<string, string> kvp) => vars.Add(kvp);
	}
}
