using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Tooling;

namespace NukePlusPlus {
	public class OurNukeBuild : NukeBuild {
		static OurNukeBuild() {
			OurEnumConverter.EnsureRegistered();
		}

		protected void AddNewWritingItem<TYPE>(Action<TYPE> OnCreated) where TYPE : BaseOurConfigurationEntity {
			ToAddWritingItems ??= new();
			ToAddWritingItems.Add(new ToAdd<TYPE>(OnCreated));
		}
		protected List<ToAdd> ToAddWritingItems;

		protected class ToAdd<T> : ToAdd where T : BaseOurConfigurationEntity {
			Action<T> action;
			public ToAdd(Action<T> action) {
				this.action = action;
			}
			public void OnJobReady(OurGithubActionsJobWithMatrix job) {
				job.AddNewItem<T>(action);
			}

		}
		internal void DoWritingAdds(OurGithubActionsJobWithMatrix job) {
			if (ToAddWritingItems == null)
				return;
			foreach (var itm in ToAddWritingItems)
				itm.OnJobReady(job);
		}

		protected void OnConfigurationEntityInitalized<T>(Action<T> onConfig) where T : ConfigurationEntity {
			ToConfigure ??= new();
			ToConfigure.Add(new ToConfig<T>(onConfig));
		}
		protected List<ToConfig> ToConfigure;
		internal void DoConfigurationOf(ConfigurationEntity entry) {
			if (ToConfigure == null)
				return;
			foreach (var itm in ToConfigure)
				itm.DoIfMatch(entry);
		}
		protected interface ToAdd {
			void OnJobReady(OurGithubActionsJobWithMatrix job);
		}
		protected interface ToConfig {
			void DoIfMatch(ConfigurationEntity entry);
		}
		protected class ToConfig<T> : ToConfig where T : ConfigurationEntity {
			Action<T> action;
			public ToConfig(Action<T> action) {
				this.action = action;
			}
			public void DoIfMatch(ConfigurationEntity entry) {
				if (entry is T tentry)
					action(tentry);
			}

		}
		protected void TryWriteLogFileTo(String logPath, Action<string> logLine, Action OnNotFound) {

			if (File.Exists(logPath)) {
				var config = File.ReadAllLines(logPath);
				logLine($"#####Dumping file: {logPath}");
				foreach (var line in config)
					logLine(line);
				logLine($"#####Done Dumping file: {logPath}");
			} else
				OnNotFound();

		}
		protected void GCCLogHandler(OutputType logType, string msg) {//warnings get thrown to stderr but are not errors so lets only do things containing error, prolly shoudl be better mathcing
			switch (logType) {
				case OutputType.Std:
					Serilog.Log.Information(msg);
					break;
				case OutputType.Err:
					if (msg.Contains("error", StringComparison.CurrentCultureIgnoreCase))
						Serilog.Log.Error(msg);
					else
						Serilog.Log.Warning(msg);
					break;
			}
		}
		protected string Mtx(String name) => GAUtil.MatrixSub(name);

	}

}
