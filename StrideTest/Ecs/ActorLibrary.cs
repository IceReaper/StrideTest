namespace StrideTest.Ecs
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public sealed class ActorLibrary
	{
		private readonly Dictionary<string, ActorInfo> actorInfos = new();

		public ActorLibrary(IEnumerable<ActorInfo> actorInfos)
		{
			foreach (var actorInfo in actorInfos)
				this.actorInfos.Add(actorInfo.Id, actorInfo);
		}

		public ActorInfo ById(string id)
		{
			return this.actorInfos[id];
		}

		public IEnumerable<ActorInfo> ByComponentInfo<TComponentInfo>()
			where TComponentInfo : IComponentInfo
		{
			if (typeof(TComponentInfo).IsAssignableTo(typeof(ISingleComponentInfo)))
				return this.actorInfos.Values.Where(actorInfo => actorInfo.GetComponentInfo<ISingleComponentInfo>() != null);

			// ReSharper disable once ConvertIfStatementToReturnStatement
			if (typeof(TComponentInfo).IsAssignableTo(typeof(IMultipleComponentInfo)))
				return this.actorInfos.Values.Where(actorInfo => actorInfo.GetComponentInfos<IMultipleComponentInfo>().Any());

			return Array.Empty<ActorInfo>();
		}
	}
}
