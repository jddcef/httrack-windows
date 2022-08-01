using Nuke.Common.CI;
using Nuke.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NukePlusPlus {
	public class GithubActionsCommentWriter : OurConfigurationEntityWPublicLevelSet {
		public List<string> comments=new();

		public override void Write(CustomFileWriter writer) {
			if (comments.Count == 0)
				return;
			if (level == WRITE_LEVEL.Invalid)
				throw new ArgumentException();

			foreach (var line in comments)

			writer.WriteLine($"# {line}");
		}
		public void AddComment(String comment) => comments.Add(comment);
		
	}
}
