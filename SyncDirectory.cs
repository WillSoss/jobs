﻿using Runly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WillSoss.Jobs
{
	/// <summary>
	/// Copies a directory tree from its source path to the destination path.
	/// </summary>
	public class SyncDirectory : Job<SyncDirectoryConfig, string>
	{
		public SyncDirectory(SyncDirectoryConfig config)
			: base(config) { }

		public override Task InitializeAsync()
		{
			if (!Directory.Exists(Config.Source))
				throw new ConfigException($"Directory not found: {Config.Source}", nameof(Config.Source));

			if (!Directory.Exists(Config.Destination))
				Directory.CreateDirectory(Config.Destination);

			return base.InitializeAsync();
		}

		public override IAsyncEnumerable<string> GetItemsAsync() => GetFilesIn(Config.Source).Select(file => Path.GetRelativePath(Config.Source, file)).ToAsyncEnumerable();

		private IEnumerable<string> GetFilesIn(string dir)
		{
			foreach (var file in Directory.GetFiles(dir))
				yield return file;

			foreach (var subdir in Directory.GetDirectories(dir))
				foreach (var file in GetFilesIn(subdir))
					yield return file;
		}

		public override async Task<Result> ProcessAsync(string file)
		{
			var sourceFile = Path.Combine(Config.Source, file);
			var destFile = Path.Combine(Config.Destination, file);

			if (File.Exists(destFile))
				return Result.Success("Already Exists");

			try
			{
				using (var source = File.Open(sourceFile, FileMode.Open))
				{
					var destDir = Path.GetDirectoryName(destFile);

					if (!Config.Preview)
					{
						if (!Directory.Exists(destDir))
							Directory.CreateDirectory(destDir);

						using var dest = File.Create(destFile);

						await source.CopyToAsync(dest, CancellationToken);
					}
					else
					{
						return Result.Success("To Be Copied");
					}
				}
			}
			catch (UnauthorizedAccessException) when (Config.IgnoreUnauthorizedAccessException)
			{
				return Result.Success("Skipped - Unauthorized", "Skipping file copy due to an UnauthorizedAccessException being thrown. Set IgnoreWhenAccessDenied = false to treat as an error.");
			}

			if (CancellationToken.IsCancellationRequested)
				return Result.SuccessOrCancelled(CancellationToken);
			else
				return Result.Success("Copied");
		}
	}

	public class SyncDirectoryConfig : Config
	{
		public string Source { get; set; }
		public string Destination { get; set; }
		public bool Preview { get; set; }

		/// <summary>
		/// Causes an <see cref="UnauthorizedAccessException"/> to be ignored by skipping the file and returning a successful result.
		/// </summary>
		public bool IgnoreUnauthorizedAccessException { get; set; }
	}
}