namespace StrideTest.Ecs
{
	using JetBrains.Annotations;
	using System.IO;

	public interface IComponent
	{
		public void Load(BinaryReader reader);
		public void Save(BinaryWriter writer);
	}

	public interface ISingleComponent : IComponent
	{
	}

	public interface IMultipleComponent : IComponent
	{
	}

	[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
	public abstract class Component<TComponentInfo> : IComponent
		where TComponentInfo : IComponentInfo
	{
		public Actor Actor { get; }
		public TComponentInfo Info { get; }

		protected Component(Actor actor, TComponentInfo info)
		{
			this.Actor = actor;
			this.Info = info;
		}

		public virtual void Load(BinaryReader reader)
		{
		}

		public virtual void Save(BinaryWriter writer)
		{
		}
	}

	public abstract class SingleComponent<TComponentInfo> : Component<TComponentInfo>, ISingleComponent
		where TComponentInfo : ISingleComponentInfo
	{
		protected SingleComponent(Actor actor, TComponentInfo info)
			: base(actor, info)
		{
		}
	}

	public abstract class MultipleComponent<TComponentInfo> : Component<TComponentInfo>, IMultipleComponent
		where TComponentInfo : IMultipleComponentInfo
	{
		protected MultipleComponent(Actor actor, TComponentInfo info)
			: base(actor, info)
		{
		}
	}
}
