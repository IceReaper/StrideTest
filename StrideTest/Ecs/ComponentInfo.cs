namespace StrideTest.Ecs
{
	using JetBrains.Annotations;
	using System;

	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers | ImplicitUseTargetFlags.WithInheritors)]
	public interface IComponentInfo
	{
		public IComponent Create(Entity entity);
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
		IComponent IComponentInfo.Create(Entity entity)
		{
			return this.Create(entity);
		}

		protected virtual TComponent Create(Entity entity)
		{
			return (TComponent)Activator.CreateInstance(typeof(TComponent), entity, this)!;
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
