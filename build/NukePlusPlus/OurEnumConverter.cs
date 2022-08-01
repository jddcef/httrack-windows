using Nuke.Common.Tooling;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace NukePlusPlus {
	public static class EnumHelper {
		public static string TheirGetValue(this Enum value) { //we cant use GetValue as they use typeof(T) rather than value.GetType()
			string name = Nuke.Common.Assert.NotNull(Enum.GetName(value.GetType(), value), string.Format("Enum value {0} is not valid for {1}", value, value.GetType().Name), "Enum.GetName(typeof(T), value)");
			EnumValueAttribute attribute = value.GetType().GetMember(name).Single().GetCustomAttribute<EnumValueAttribute>();
			if (attribute == null) {
				return value.ToString();
			}
			return attribute.Value;
		}
	}
	public class OurEnumConverter : EnumConverter {
		static OurEnumConverter() {
			TypeDescriptor.AddAttributes(typeof(Enum), new TypeConverterAttribute(typeof(OurEnumConverter)));
		}
		public static void EnsureRegistered() { }//make sure class is initialized
		public OurEnumConverter([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicFields)] Type type) : base(type) {

		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
			try {
				return base.ConvertFrom(context, culture, value);
			} catch (Exception) {
				var realVals = Enum.GetValues(EnumType).Cast<Enum>().ToArray();
				var attribValues = realVals.Select(a => new { key = a, val = a.TheirGetValue() }).ToArray();
				var realVal = attribValues.FirstOrDefault(a => a.val.Equals(value));
				if (realVal != null)
					return realVal.key;
				throw;
			}
		}

	}
}
