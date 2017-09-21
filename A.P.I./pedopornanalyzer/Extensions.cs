using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace pedopornanalyzer
{
	public static class Extensions
	{
		private static Dictionary<string, string> ImageTypes;
		private static Dictionary<string, string> ZipTypes;

		static Extensions ()
		{
			ImageTypes = new Dictionary<string, string>();
			ImageTypes.Add("FFD8", "jpg");
			ImageTypes.Add("424D", "bmp");
			ImageTypes.Add("474946", "gif");
			ImageTypes.Add("89504E470D0A1A0A", "png");
			ZipTypes = new Dictionary<string, string>();
			ZipTypes.Add("504B", "zip");
			ZipTypes.Add("526172", "rar");
		}

		public static bool IsImage(this Stream stream, Stopwatch stopwatch = null)
		{
			if (stopwatch != null) { stopwatch.Start(); }
			string imageType;
			bool isImage = stream.IsImage(out imageType);
			if (stopwatch != null) { stopwatch.Stop(); }
			return isImage;
		}

		public static bool IsImage(this byte[] stream)
		{
			StringBuilder builder = new StringBuilder();
			int largestByteHeader = ImageTypes.Max(img => img.Value.Length);

			for (int i = 0; i < largestByteHeader; i++)
			{
				string bit = stream[i].ToString("X2");
				builder.Append(bit);

				string builtHex = builder.ToString();
				bool isImage = ImageTypes.Keys.Any(img => img == builtHex);
				if (isImage)
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsImage(this Stream stream, out string imageType)
		{
			stream.Seek(0, SeekOrigin.Begin);
			StringBuilder builder = new StringBuilder();
			int largestByteHeader = ImageTypes.Max(img => img.Value.Length);

			for (int i = 0; i < largestByteHeader; i++)
			{
				string bit = stream.ReadByte().ToString("X2");
				builder.Append(bit);

				string builtHex = builder.ToString();
				bool isImage = ImageTypes.Keys.Any(img => img == builtHex);
				if (isImage)
				{
					imageType = ImageTypes[builder.ToString()];
					return true;
				}
			}
			imageType = null;
			return false;
		}

		public static bool IsCompressed(this Stream stream, Stopwatch stopwatch = null)
		{
			if (stopwatch != null) { stopwatch.Start(); }
			bool result = false;
			stream.Seek(0, SeekOrigin.Begin);
			StringBuilder builder = new StringBuilder();
			int largestByteHeader = ZipTypes.Max(zip => zip.Value.Length);
			for (int i = 0; i < largestByteHeader; i++)
			{
				string bit = stream.ReadByte().ToString("X2");
				builder.Append(bit);

				string builtHex = builder.ToString();
				bool isCompressed = ZipTypes.Keys.Any(zip => zip == builtHex);
				if (isCompressed)
				{
					result = true;
					break;
				}
			}
			if (stopwatch != null) { stopwatch.Stop(); }
			return result;
		}

	}
}

