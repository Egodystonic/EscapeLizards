using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophidian.Losgap.AssetManagement {
	public enum LoadedModelAttributeMapping {
		Position,
		Normal,
		TexCoord
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple=false, Inherited=false)]
	public sealed class MappedModelComponent : Attribute {
		public readonly LoadedModelAttributeMapping Mapping;

		public MappedModelComponent(LoadedModelAttributeMapping mapping) {
			Mapping = mapping;
		}
	}
}
