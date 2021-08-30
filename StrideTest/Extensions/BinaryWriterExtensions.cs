namespace StrideTest.Extensions
{
	using Stride.Core.Mathematics;
	using System.IO;

	public static class BinaryWriterExtensions
	{
		public static void Write(this BinaryWriter writer, Vector3 value)
		{
			writer.Write(value.X);
			writer.Write(value.Y);
			writer.Write(value.Z);
		}

		public static void Write(this BinaryWriter writer, Quaternion value)
		{
			writer.Write(value.X);
			writer.Write(value.Y);
			writer.Write(value.Z);
			writer.Write(value.W);
		}
	}
}
