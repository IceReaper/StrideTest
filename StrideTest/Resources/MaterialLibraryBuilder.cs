namespace StrideTest.Resources
{
	public class MaterialLibraryBuilder : LibraryBuilder
	{
		public virtual MaterialLibrary Build()
		{
			return new(this.Yaml.Nodes);
		}
	}
}
