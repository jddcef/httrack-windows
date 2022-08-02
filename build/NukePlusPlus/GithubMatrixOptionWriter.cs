using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NukePlusPlus {
	public interface IMatrixParameter {
		string Name { get; }
		Enum Value { get; set; }
		Type ValueType { get; }
		bool ValueEquals(Enum otherValue);
		bool IsCompatConfig(BuildConfiguration config);
	}

	public abstract class MatrixParameter<T> : IMatrixParameter where T : Enum {
		public abstract string Name { get; }//ie base-image
		public T Value { get; set; }
		public Type ValueType => typeof(T);
		Enum IMatrixParameter.Value {
			get => Value;
			set => Value = (T)value;
		}

		public virtual bool IsCompatConfig(BuildConfiguration config) => true;
		public bool ValueEquals(Enum otherValue) => Equals(Value, otherValue);
	}
	public static class GAUtil {
		public const string MPrefix = "${{matrix.";
		public const string MPostfix = "}}";
		public static string MatrixSub(string name) => $"{MPrefix}{name}{MPostfix}";
	}
	public class BuildConfiguration {
		public IMatrixParameter[] parameterValues;
		public override string ToString() => string.Join(", ", parameterValues.Select(a => $"{a.Name}: {a.Value}"));
	}
	public class GithubMatrixOptionWriter : OurConfigurationEntity {
		public GithubMatrixOptionWriter() {
			this.priority = PRIORITY.First;
		}
		public override void AfterInitialization() {
			CalculateTheMatrix();
			base.AfterInitialization();
		}

		private void CalculateTheMatrix() {
			//Debugger.Launch();
			var matrixTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes().Where(a => a.IsClass && a.IsAbstract == false && a.IsAssignableTo(typeof(IMatrixParameter)))).ToList();

			var enum_vals = new Dictionary<Type, IMatrixParameter[]>();
			foreach (var type in matrixTypes.ToArray()) {
				var instance = (IMatrixParameter)Activator.CreateInstance(type);
				//IMatrixParameter GetInstanceForValue(Enum val)
				//var instance = (IMatrixParameter)_instance;
				//var vals = Enum.GetValues(instance.ValueType).Cast<Enum>();//this would just be all fro mevery
				var vals = InitialRange.Where(a => a.GetType() == instance.ValueType).ToArray();
				if (vals.Length == 0) {
					matrixTypes.Remove(type);
					continue;
				}
				enum_vals[type] = vals.Select(val => {
					var ret = instance ?? (IMatrixParameter)Activator.CreateInstance(type);
					instance = null;
					ret.Value = val;

					return ret;
				}

				).ToArray();


			}
			var maxPossible = enum_vals.Select(a => a.Value.Length).Aggregate(1, (a, b) => a * b);
			var posConfigurations = new List<BuildConfiguration>(maxPossible);
			foreach (var type in matrixTypes) {
				if (posConfigurations.Count == 0) {
					posConfigurations.AddRange(enum_vals[type].Select(a => new BuildConfiguration { parameterValues = new[] { a } }));
				} else {
					var cur = posConfigurations.ToArray();
					posConfigurations.Clear();
					foreach (var val in enum_vals[type])
						posConfigurations.AddRange(cur.Select(a => new BuildConfiguration { parameterValues = a.parameterValues.Union(new[] { val }).ToArray() }));
				}
			}
			var validConfigs = new List<BuildConfiguration>();
			foreach (var config in posConfigurations) {
				if (config.parameterValues.All(a => a.IsCompatConfig(config)))
					validConfigs.Add(config);
			}
			invalidConfigs = posConfigurations.Where(a => validConfigs.Contains(a) == false).ToList();
			/*Lets simplify invalidConfigs so if any cases cover all of a certain type that type can be removed from the cases and only one of those cases kept*/
			foreach (var config in invalidConfigs.ToArray()) {
				if (!invalidConfigs.Contains(config))//already removed
					continue;
				//var origValues = config.parameterValues;
				var ourValuesToRemove = new List<IMatrixParameter>();
				foreach (var value in config.parameterValues) {
					var otherValues = config.parameterValues.Except(new[] { value }).ToArray();
					var allOfSimilar = invalidConfigs.Where(a => a != config && a.parameterValues.Intersect(otherValues).Count() == otherValues.Length).ToArray();
					if (allOfSimilar.Count() + 1 == enum_vals[value.GetType()].Length) { //plus one for us
						foreach (var itm in allOfSimilar)
							invalidConfigs.Remove(itm);
						//config.parameterValues = otherValues;
						ourValuesToRemove.Add(value);
					}
				}
				config.parameterValues = config.parameterValues.Except(ourValuesToRemove).ToArray();

			}

			orderedEnumVals = enum_vals.OrderByDescending(a => a.Value.First().ValueType == typeof(GitHubActionsImage)).ToArray();


			BaseImageMaxtrixKey = orderedEnumVals.First(a => a.Value.First().ValueType == typeof(GitHubActionsImage)).Value.First().Name;

			var envNamesToWrite = orderedEnumVals.Select(a => a.Value.First().Name).ToArray();
			var ourMatrixWriter = this;
			MatrixKeys = new(envNamesToWrite);
			job.AddNewItem<GithubActionsEnvWriter>((envAction) => {
				foreach (var name in envNamesToWrite)
					envAction.AddVar($"NUKE_{name}", GAUtil.MatrixSub(name));
				envAction.after = ourMatrixWriter;
				envAction.level = WRITE_LEVEL.Job;
			});
		}
		private KeyValuePair<Type, IMatrixParameter[]>[] orderedEnumVals;
		private List<BuildConfiguration> invalidConfigs;

		public Enum[] InitialRange;
		public override void Write(CustomFileWriter writer) {

			writer.WriteLine($"runs-on: {GAUtil.MatrixSub(BaseImageMaxtrixKey)}");
			writer.WriteLine("strategy:");
			using (writer.Indent()) {
				writer.WriteLine("matrix:");
				using (writer.Indent()) {
					foreach (var kvp in orderedEnumVals)//base image first
						writer.WriteLine($"{kvp.Value.First().Name}: [{string.Join(", ", kvp.Value.Select(a => a.Value.TheirGetValue()))}]");
					if (invalidConfigs.Any()) {
						writer.WriteLine("exclude:");
						using (writer.Indent()) {
							foreach (var invalidConfig in invalidConfigs) {
								var isFirst = true;
								foreach (var val in invalidConfig.parameterValues) {
									var line = $"{val.Name}: {val.Value.TheirGetValue()}";

									if (isFirst)
										writer.WriteLine($"- {line}");
									else
										using (writer.Indent())
											writer.WriteLine(line);
									isFirst = false;
								}

							}
						}
					}

				}
			}


		}



		public List<string> MatrixKeys;
		public string BaseImageMaxtrixKey;

		public override WRITE_LEVEL level => WRITE_LEVEL.Job;
	}
}
