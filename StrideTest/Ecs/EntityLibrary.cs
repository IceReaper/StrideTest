namespace StrideTest.Ecs
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public sealed class EntityLibrary
	{
		private readonly Dictionary<string, EntityInfo> entityInfos = new();

		public EntityLibrary(IEnumerable<EntityInfo> entityInfos)
		{
			foreach (var entityInfo in entityInfos)
				this.entityInfos.Add(entityInfo.Id, entityInfo);
		}

		public IEnumerable<EntityInfo> All()
		{
			return this.entityInfos.Values;
		}

		public EntityInfo ById(string id)
		{
			return this.entityInfos[id];
		}

		public IEnumerable<EntityInfo> ByComponentInfo<TComponentInfo>()
			where TComponentInfo : IComponentInfo
		{
			if (typeof(TComponentInfo).IsAssignableTo(typeof(ISingleComponentInfo)))
				return this.entityInfos.Values.Where(entityInfo => entityInfo.GetComponentInfo<ISingleComponentInfo>() != null);

			// ReSharper disable once ConvertIfStatementToReturnStatement
			if (typeof(TComponentInfo).IsAssignableTo(typeof(IMultipleComponentInfo)))
				return this.entityInfos.Values.Where(entityInfo => entityInfo.GetComponentInfos<IMultipleComponentInfo>().Any());

			return Array.Empty<EntityInfo>();
		}
	}
}
