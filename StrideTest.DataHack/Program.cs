using System;
using System.IO;

namespace StrideTest.DataHack
{
	public static class Program
	{
		public static void Main()
		{
			const string? target = "../../../../StrideTest.Desktop/bin/Debug/net6.0/data";
			Directory.Delete(target, true);
			Directory.Move("data", target);
			Console.WriteLine("Done!");
		}
	}
}
