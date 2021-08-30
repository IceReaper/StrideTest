namespace StrideTest.Extensions
{
	using Stride.Core.Mathematics;
	using System.IO;

	public static class BinaryReaderExtensions
	{
		public static Vector3 ReadVector3(this BinaryReader reader)
		{
			return new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}

		public static Quaternion ReadQuaternion(this BinaryReader reader)
		{
			return new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}
	}
}
