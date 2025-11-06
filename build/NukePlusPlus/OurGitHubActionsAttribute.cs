using _build;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.Tooling;
using System;
using System.Collections.Generic;
using System.Linq;


namespace NukePlusPlus {
	[PublicAPI]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class OurGitHubActionsAttribute : GitHubActionsAttribute {
		public override ConfigurationEntity GetConfiguration(IReadOnlyCollection<ExecutableTarget> relevantTargets) {
			//this.build = build;
			var orig = base.GetConfiguration(relevantTargets) as GitHubActionsConfiguration;
			configuration.DetailedTriggers = orig.DetailedTriggers;
			configuration.Jobs = orig.Jobs;
			configuration.Name = orig.Name;
			configuration.ShortTriggers = orig.ShortTriggers;
			return configuration;
		}
		internal Type build;
		protected OurGitHubActionsConfiguration configuration=new();
		public OurGitHubActionsAttribute(string name, params object[] vals) : base(name, GitHubActionsImage.MacOs1015) {
			job = new(this, configuration,  vals.Cast<Enum>().ToArray());
			ActionName = name;
		}
		public string ActionName;
		

		

		public OurGithubActionsJobWithMatrix job;
		protected override GitHubActionsJob GetJobs(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets) {
			var baseRes = base.GetJobs(image, relevantTargets);//just need to get its steps
			var ourRet = job;
			ourRet.Name = image.GetValue().Replace(".", "_");

			ourRet.Steps = baseRes.Steps;
			ourRet.Name = ActionName;
			ourRet.Image = image;
			return ourRet;

		}
		public new GitHubActionsSubmodules Submodules {
			set => base.Submodules = _submodules = value;
			get => _submodules;
		}
		private GitHubActionsSubmodules _submodules;

		public new uint FetchDepth {
			set => base.FetchDepth = _fetchDepth = value;
			get => _fetchDepth;
		}
		private uint _fetchDepth;
	}
}
