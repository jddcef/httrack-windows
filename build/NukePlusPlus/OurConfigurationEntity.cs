using Nuke.Common;
using Nuke.Common.CI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NukePlusPlus {
	public abstract class OurConfigurationEntityWPublicLevelSet : BaseOurConfigurationEntity {
		public WRITE_LEVEL level { get; set; }
		internal override WRITE_LEVEL _level => level;
	}
	public abstract class OurConfigurationEntity : BaseOurConfigurationEntity {
		public abstract WRITE_LEVEL level { get; }
		internal override WRITE_LEVEL _level => level;
	}
	internal interface IConfigurationEntityHasLevel {
		internal BaseOurConfigurationEntity.WRITE_LEVEL level { get; }
	}
	public abstract class BaseOurConfigurationEntity : ConfigurationEntity, IConfigurationEntityHasLevel {
		public enum WRITE_LEVEL { Invalid, Global = 1, Job = 3, Step = 4 }
		public enum PRIORITY { Normal, First}
		public PRIORITY priority { get; set; }
		internal abstract WRITE_LEVEL _level { get; }

		public ConfigurationEntity after { get; set; }

		protected OurGithubActionsJobWithMatrix job { get; private set; }
		protected NukeBuild build => attribute.build;
		protected OurGitHubActionsAttribute attribute { get; private set; }
		WRITE_LEVEL IConfigurationEntityHasLevel.level => _level;

		internal void SetJob(OurGithubActionsJobWithMatrix job, OurGitHubActionsAttribute attribute) {
			this.job = job;
			this.attribute = attribute;
		}

		public virtual void BeforeInitialization() {

		}
		public virtual void AfterInitialization() {

		}
	}
}
