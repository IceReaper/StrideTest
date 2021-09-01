namespace StrideTest.Ecs
{
	using System.Collections.Generic;
	using System.Linq;

	public sealed class EntityInfo
	{
		private readonly List<IComponentInfo> componentInfos;

		public readonly string Id;

		public IEnumerable<IComponentInfo> ComponentInfos => this.componentInfos;

		public EntityInfo(string id, IEnumerable<IComponentInfo> componentInfos)
		{
			this.Id = id;
			this.componentInfos = componentInfos.ToList();
		}

		public T? GetComponentInfo<T>()
			where T : ISingleComponentInfo
		{
			foreach (var componentInfo in this.componentInfos)
			{
				if (componentInfo is T explicitComponentInfo)
					return explicitComponentInfo;
			}

			return default;
		}

		public IEnumerable<T> GetComponentInfos<T>()
			where T : IMultipleComponentInfo
		{
			foreach (var componentInfo in this.componentInfos)
			{
				if (componentInfo is T explicitComponentInfo)
					yield return explicitComponentInfo;
			}
		}
	}
}
