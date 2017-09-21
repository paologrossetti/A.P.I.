using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SharpCompress;
using SharpCompress.Archive;
using Eto.Forms;
using Eto.Drawing;

namespace pedopornanalyzer
{
	public class DiskUtils
	{
		private static List<string> LinuxPathsToIgnore = new List<string>() { "/proc", "/sys", "/root" };
		private static List<string> ImageExtensions = new List<string>() { ".bmp", ".jpg", ".jpeg", ".png" };
		private static bool canPosix = false;
		static System.Collections.Specialized.StringCollection DeniedFilesAccess = new System.Collections.Specialized.StringCollection();
		public static HashSet<System.IO.FileInfo> ImagePaths = new HashSet<System.IO.FileInfo>();
		public static HashSet<System.IO.FileInfo> ZipPaths = new HashSet<System.IO.FileInfo>();

		public static List<string> fileimmagine;

		// STATS
		public static int smallFileSizeThresholdInBytes = 18000; // 18 kb roughly
		public static int bigFileSizeThresholdBytes = 30000000; // 30 MB roughly
		public static long sizeProcessed = 0;
		public static long filesProcessed = 0;
		public static TimeSpan HeaderProcessingTime = new TimeSpan(0);
		public static TimeSpan ZipProcessingTime = new TimeSpan(0);

		// CLASS ENTRY POINTS
		public static void GetImagesFromExtension(string drive = null)
		{
			fileimmagine = new List<string>();
			GetFileTypesFromDrives(CheckFileExtension, drive);
		}
		public static void GetImages(string drive = null)
		{
			//GetFileTypesFromDrives(CheckFileType, drive);
		}

		private static void GetFileTypesFromDrives(Action<FileInfo> action, string drive = null)
		{
			canPosix = CanPosix();
			string[] drives = drive == null ? System.Environment.GetLogicalDrives() : new string[] { drive };
			try
			{
			foreach (string dr in drives)
			  {
					try{
						System.IO.DriveInfo driveInfo = new System.IO.DriveInfo(dr);
						if (!driveInfo.IsReady)
						{
							Console.WriteLine("The drive {0} could not be read", driveInfo.Name);
							continue;
						};
						WalkDrive(driveInfo, action);
						}
					catch(Exception e){
						WalkDriveForNotSlashMedia(dr,action);
						}	
			  }	
			}
			catch(Exception e) {
				MessageBox.Show ("Il dispositivo selezionato non esiste o non è stato selezionato correttamente.");
			}
		}

		private static bool CanPosix()
		{
			try // https://stackoverflow.com/questions/19428170/detecting-symbolic-links-and-pipes-in-mono
			{
				Mono.Unix.UnixSymbolicLinkInfo info = new Mono.Unix.UnixSymbolicLinkInfo("/");
			}
			catch (TypeLoadException e)
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Checks if a file is an image by reading its extension.
		/// </summary>
		private static void CheckFileExtension(System.IO.FileInfo fileInfo)
		{
			try
			{
				long fileSize = fileInfo.Length;
				sizeProcessed += fileSize;
				if (fileSize < smallFileSizeThresholdInBytes) { return; }
				if (ImageExtensions.Contains(fileInfo.Extension, StringComparer.OrdinalIgnoreCase))
				{
					fileimmagine.Add(fileInfo.FullName);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return;
			}
		}

		/// <summary>
		/// Given a file info, checks if it's an image; if it's a compressed file, checks whether there are images in it
		/// </summary>
		private static void CheckFileType(System.IO.FileInfo fileInfo)
		{
			if (ImagePaths.Contains(fileInfo)) { ImagePaths.Remove(fileInfo); return; } // File already considered by preliminary extension analysis
			try
			{
				long fileSize = fileInfo.Length;
				sizeProcessed += fileSize;
				if (fileSize < smallFileSizeThresholdInBytes) { return; }
				using (Stream stream = File.Open(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read)) // new FileStream(fileInfo.FullName, FileMode.Open))
				{
					//Console.WriteLine(fileInfo.FullName);
					Stopwatch stopwatch = new Stopwatch();
					if (stream.IsCompressed(stopwatch))
					{
						HeaderProcessingTime += stopwatch.Elapsed;
						//ZipPaths.Add(fileInfo);
						try
						{
							//Console.WriteLine(fileInfo.FullName);
							stopwatch.Reset();
							CheckFilesInArchive(ArchiveFactory.Open(fileInfo.FullName), fileInfo.FullName, stopwatch);
							ZipProcessingTime += stopwatch.Elapsed;
						}
						catch (SharpCompress.Common.CryptographicException e)
						{
							// TODO: add list of files to analyze manually
							Console.WriteLine("ENCRYPTED ARCHIVE " + fileInfo.FullName);
						}
						catch (InvalidOperationException e)
						{
							Console.WriteLine("ARCHIVE NOT READABLE (maybe not a real archive?) " + fileInfo.FullName);
							return;
						}
						catch (Exception e)
						{
							Console.WriteLine(e);
							return;
						}
					}
					stopwatch.Reset();
					if (fileSize > bigFileSizeThresholdBytes) { return; }
					if (stream.IsImage(stopwatch)) { ImagePaths.Add(fileInfo); HeaderProcessingTime += stopwatch.Elapsed; }
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return;
			}
		}
		// https://stackoverflow.com/questions/5967864/how-to-read-data-from-a-zip-file-without-having-to-unzip-the-entire-file
		// TODO: Extract image files found in archive and add them to image paths
		private static void CheckFilesInArchive(IArchive archive, string archiveName = null, Stopwatch stopwatch = null)
		{
			if (stopwatch != null) { stopwatch.Start(); }
			foreach (var entry in archive.Entries)
			{
				using (Stream stream = entry.OpenEntryStream())
				{
					if (!entry.IsDirectory)
					{
						if (entry.Size < smallFileSizeThresholdInBytes) { continue; }
						using (var ms = new MemoryStream())
						{
							stream.CopyTo(ms); // https://stackoverflow.com/questions/13035925/stream-wrapper-to-make-stream-seekable
							ms.Position = 0;
							if (ms.IsCompressed())
							{
								ms.Position = 0;
								CheckFilesInArchive(ArchiveFactory.Open(ms));
							}
							else if (entry.Size > bigFileSizeThresholdBytes) { continue; }
							if (ms.IsImage())
							{
								ZipPaths.Add(new FileInfo(Path.Combine(archiveName, entry.Key)));
							}
						}
					}
				}
			}
			if (stopwatch != null) { stopwatch.Stop(); }
		}
		private static void WalkDriveForNotSlashMedia(string pathDevice,Action<System.IO.FileInfo> action)
		{
			System.IO.DirectoryInfo rootDir = new DirectoryInfo (pathDevice);
			WalkDirectoryTree(rootDir, action);
			// Write out all the files that could not be processed.
			foreach (string s in DeniedFilesAccess)
			{
				Console.WriteLine("ACCESS DENIED: " + s);
			}
		}
		private static void WalkDrive(System.IO.DriveInfo driveInfo, Action<System.IO.FileInfo> action)
		{
			// Here we skip the drive if it is not ready to be read. This
			// is not necessarily the appropriate action in all scenarios.
			System.IO.DirectoryInfo rootDir = driveInfo.RootDirectory;
			WalkDirectoryTree(rootDir, action);

			// Write out all the files that could not be processed.
			foreach (string s in DeniedFilesAccess)
			{
				Console.WriteLine("ACCESS DENIED: " + s);
			}
		}

		static void WalkDirectoryTree(System.IO.DirectoryInfo root, Action<System.IO.FileInfo> action)
		{
			System.IO.FileInfo[] files = null;
			System.IO.DirectoryInfo[] subDirs = null;

			// First, process all the files directly under this folder
			try
			{
				files = root.GetFiles();
			}
			// This is thrown if even one of the files requires permissions greater
			// than the application provides.
			catch (UnauthorizedAccessException e)
			{
				DeniedFilesAccess.Add(e.Message);
			}

			catch (System.IO.DirectoryNotFoundException e)
			{
				Console.WriteLine(e.Message);
			}

			if (files != null)
			{
				foreach (System.IO.FileInfo fi in files)
				{
					if (canPosix)
					{
						try // https://stackoverflow.com/questions/19428170/detecting-symbolic-links-and-pipes-in-mono
						{
							Mono.Unix.UnixSymbolicLinkInfo info = new Mono.Unix.UnixSymbolicLinkInfo(fi.FullName);
							if (info.FileType != Mono.Unix.FileTypes.RegularFile && info.FileType != Mono.Unix.FileTypes.Directory) { continue; }
						}
						catch (Exception e)
						{
							Console.WriteLine(e);
							continue;
						}
					}
					if (LinuxPathsToIgnore.Any(x => fi.FullName.StartsWith(x)))
					{
						continue;
					}

					try
					{
						action(fi);
					}
					catch (UnauthorizedAccessException e)
					{
						DeniedFilesAccess.Add(fi.FullName);
						continue;
					}
					catch (IOException e)
					{
						Console.WriteLine(e);
						continue;
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
						continue;
					}
				}

				// Now find all the subdirectories under this directory.
				subDirs = root.GetDirectories();

				foreach (System.IO.DirectoryInfo dirInfo in subDirs)
				{
					if (canPosix)
					{
						try // https://stackoverflow.com/questions/19428170/detecting-symbolic-links-and-pipes-in-mono
						{
							Mono.Unix.UnixSymbolicLinkInfo info = new Mono.Unix.UnixSymbolicLinkInfo(dirInfo.FullName);
							if (info.FileType != Mono.Unix.FileTypes.RegularFile && info.FileType != Mono.Unix.FileTypes.Directory) { continue; }
						}
						catch (Exception e)
						{
							Console.WriteLine(e);
							continue;
						}
					}
					// Recursive call for each subdirectory.
					WalkDirectoryTree(dirInfo, action);
				}
			}
		}
	}
}

