namespace StrideTest.Ecs
{
	using JetBrains.Annotations;
	using System;

	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers | ImplicitUseTargetFlags.WithInheritors)]
	public interface IComponentInfo
	{
		public IComponent Create(Actor actor);
	}

	public interface ISingleComponentInfo : IComponentInfo
	{
	}

	public interface IMultipleComponentInfo : IComponentInfo
	{
	}

	public abstract class ComponentInfo<TComponent> : IComponentInfo
		where TComponent : IComponent
	{
		IComponent IComponentInfo.Create(Actor actor)
		{
			return this.Create(actor);
		}

		protected virtual TComponent Create(Actor actor)
		{
			return (TComponent)Activator.CreateInstance(typeof(TComponent), actor, this)!;
		}
	}

	public abstract class SingleComponentInfo<TComponent> : ComponentInfo<TComponent>, ISingleComponentInfo
		where TComponent : ISingleComponent
	{
	}

	public abstract class MultipleComponentInfo<TComponent> : ComponentInfo<TComponent>, IMultipleComponentInfo
		where TComponent : IMultipleComponent
	{
	}
}
