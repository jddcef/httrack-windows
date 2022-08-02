using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace NukePlusPlus {
	public class OurGitHubActionsCheckoutStep : GitHubActionsCheckoutStep {
		public string Repository { get; set; }
		public override void Write(CustomFileWriter writer) {
			base.Write(writer);
			if (string.IsNullOrWhiteSpace(Repository))
				return;
			using (writer.Indent()) {
				using (writer.Indent()) {
					writer.WriteLine($"repository: {Repository}");
				}
			}
		}
	}
}
