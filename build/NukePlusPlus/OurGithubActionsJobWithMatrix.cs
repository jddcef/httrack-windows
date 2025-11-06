using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NukePlusPlus {
	public class OurGitHubActionsConfiguration : GitHubActionsConfiguration {
		public override void Write(CustomFileWriter writer) {
			OnGlobalWriteBegin?.Invoke(this, writer);
			base.Write(writer);
		}
		public event EventHandler<CustomFileWriter> OnGlobalWriteBegin;
	}
	public class OurGithubActionsJobWithMatrix : GitHubActionsJob {
		public Enum[] MatrixOptions;

		protected OurGitHubActionsConfiguration configuration;
		public OurGithubActionsJobWithMatrix(OurGitHubActionsAttribute attribute, OurGitHubActionsConfiguration configuration, params Enum[] matrixOptions) {
			this.attribute = attribute;
			this.MatrixOptions = matrixOptions;
			this.configuration = configuration;
			configuration.OnGlobalWriteBegin += Configuration_OnGlobalWriteBegin;
		}

		

		private OurGitHubActionsAttribute attribute;
		public void AddNewItem<TYPE>(Action<TYPE> OnCreated) where TYPE : BaseOurConfigurationEntity {
			var it = (TYPE)Activator.CreateInstance<TYPE>();
			it.SetJob(this, attribute);
			OnCreated(it);
			addedItems.Add(it);
		}
		private List<BaseOurConfigurationEntity> addedItems = new();
		public List<ConfigurationEntity> AllWriteItems = new();

		public void PrepareForWrite() {
			AllWriteItems.AddRange(Steps);

			var it = AllWriteItems.FirstOrDefault(a => a is GitHubActionsCheckoutStep);
			if (it != null) {

				var pos = AllWriteItems.IndexOf(it);
				AllWriteItems.Remove(it);
				var newStep = new OurGitHubActionsCheckoutStep {//we are ok newing this here as it is not an OurConfigrationEntry
					Submodules = attribute.Submodules,
					FetchDepth = attribute.FetchDepth
				};
				AllWriteItems.Insert(pos, newStep);
			}
			var oBuild = Activator.CreateInstance(attribute.build) as OurNukeBuild;
			if (oBuild != null)
				oBuild.DoWritingAdds(this);

			AllWriteItems.AddRange(addedItems);
			addedItems.Clear();
			Steps = null;

			var toInitItems = AllWriteItems.ToList();
			while (toInitItems.Count > 0) {//we do this for items that may add new items during the configs

				foreach (var step in toInitItems)
					if (step is BaseOurConfigurationEntity oStep)
						oStep.BeforeInitialization();
				//oStep.SetJob(this, attribute);//all should probably be coming through AddNewItem so not need this


				if (oBuild != null)
					foreach (var step in toInitItems)
						oBuild.DoConfigurationOf(step);
				foreach (var step in toInitItems)
					if (step is BaseOurConfigurationEntity oStep)
						oStep.AfterInitialization();
				toInitItems.Clear();
				toInitItems.AddRange(addedItems);
				AllWriteItems.AddRange(addedItems);
				addedItems.Clear();
			}

			var orderedSteps = new List<ConfigurationEntity>();
			var firstItems = AllWriteItems.Where(a => (a as BaseOurConfigurationEntity)?.priority == BaseOurConfigurationEntity.PRIORITY.First).ToList();

			void AddItemSorted(ConfigurationEntity entity) {
				if (orderedSteps.Contains(entity))
					throw new Exception(message: "Already added this item, circular dependency?");
				var oEntity = entity as BaseOurConfigurationEntity;
				AllWriteItems.Remove(entity);
				firstItems.Remove(entity);//if it isn't in there no harm:)

				if (oEntity?.after != null) {//if it still has an after then the parent must not yet be added, back of the line
					AllWriteItems.Add(oEntity);
					return;
				}
				orderedSteps.Add(entity);
				var areAfter = AllWriteItems.Where(a => (a as BaseOurConfigurationEntity)?.after == entity).ToArray();
				foreach (var step in areAfter) {
					if (step is BaseOurConfigurationEntity ours)
						ours.after = null;
					AddItemSorted(step);
				}

			}

			
			while (firstItems.Count > 0) {
				AddItemSorted(firstItems.First());
			}
			while (AllWriteItems.Count > 0) {
				AddItemSorted(AllWriteItems.First());
			}
			AllWriteItems = orderedSteps;

			FilterSteps?.Invoke(this, this);
			
		}
		private void Configuration_OnGlobalWriteBegin(object sender, CustomFileWriter writer) {
			Console.WriteLine("OurGithubActionsJobWithMatrix.Configuration_OnGlobalWriteBegin: entering");
			AddNewItem<GithubMatrixOptionWriter>(matrixWriter => {
				matrixWriter.InitialRange = MatrixOptions;
				this.matrixWriter = matrixWriter;
			}
				);
			Console.WriteLine("OurGithubActionsJobWithMatrix.Configuration_OnGlobalWriteBegin: about to PrepareForWrite");
			PrepareForWrite();
			Console.WriteLine("OurGithubActionsJobWithMatrix.Configuration_OnGlobalWriteBegin: returned from PrepareForWrite");
			var global = AllWriteItems.Where(a => (a as IConfigurationEntityHasLevel)?.level == BaseOurConfigurationEntity.WRITE_LEVEL.Global).ToArray();
			Console.WriteLine($"OurGithubActionsJobWithMatrix.Configuration_OnGlobalWriteBegin: global items count = {global.Length}");
			foreach(var itm in global) {
				AllWriteItems.Remove(itm);
				Console.WriteLine($"OurGithubActionsJobWithMatrix: writing global item of type {itm.GetType().FullName}");
				itm.Write(writer);
			}
		}
		protected GithubMatrixOptionWriter matrixWriter;
		public static event EventHandler<OurGithubActionsJobWithMatrix> FilterSteps;
		public override void Write(CustomFileWriter writer) {
			//base.Write(writer);
			
			writer.WriteLine($"{Name}:");

			using (writer.Indent()) {
				writer.WriteLine($"name: Run");

				var jobLevel = AllWriteItems.Where(a => (a as IConfigurationEntityHasLevel)?.level == GithubActionsEnvWriter.WRITE_LEVEL.Job).ToArray();
				foreach (var itm in jobLevel) {
					AllWriteItems.Remove(itm);
					itm.Write(writer);
				}
				
				writer.WriteLine("steps:");
				using (writer.Indent()) {
					foreach (var itm in AllWriteItems)
						itm.Write(writer);
				}
			}

		}
	}
}
