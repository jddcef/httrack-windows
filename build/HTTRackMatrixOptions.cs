using Nuke.Common.CI.GitHubActions;
using NukePlusPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _build {
	public enum ArchOptions { x86, x64 }
	public enum ConfigOptions { Debug, Release }
	public class MatrixParameterConfigOptions : MatrixParameter<ConfigOptions> {
		public override string Name => nameof(Build.Configuration);
	}
	public class MatrixParameterArchOption : MatrixParameter<ArchOptions> {
		public override string Name => nameof(Build.Arch);
		public override bool IsCompatConfig(BuildConfiguration config) { //https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.enumconverter
			if (Value == ArchOptions.x86 && config.parameterValues.Any(a => a.ValueEquals(GitHubActionsImage.WindowsLatest)) == false)
				return false;
			return true;
			
		}
	}
	public class MatrixParameterBaseImage : MatrixParameter<GitHubActionsImage> {
		public override string Name => nameof(Build.BaseImage);
	}

}
