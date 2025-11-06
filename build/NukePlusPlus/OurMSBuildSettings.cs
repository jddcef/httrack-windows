using JetBrains.Annotations;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.MSBuild;
using System;
using System.Diagnostics.CodeAnalysis;


namespace NukePlusPlus {
	[PublicAPI]
	[ExcludeFromCodeCoverage]
	public class OurMSBuildSettings : MSBuildSettings {
		protected override Arguments ConfigureProcessArguments(Arguments arguments) {
			var args = base.ConfigureProcessArguments(arguments);
			if (Restore == true)
				args.Add("-p:RestorePackagesConfig=true");
			return args;
		}

	}
}
