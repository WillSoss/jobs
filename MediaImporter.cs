using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Runly;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IODirectory = System.IO.Directory;

namespace WillSoss.Jobs
{
	/// <summary>
	/// Copies photos and videos from a source location, such as an SD card, to 
	/// destination photo and video directories, organizing the files by date.
	/// </summary>
	public class MediaImporter : Job<MediaImporterConfig, string>
	{
		public MediaImporter(MediaImporterConfig config)
			: base(config) { }
		
		/// <summary>
		/// Gets the files in the <see cref="MediaImporterConfig.Source"/> directory that match 
		/// one of the <see cref="MediaImporterConfig.PhotoFileTypes"/> or <see cref="MediaImporterConfig.VideoFileTypes"/>.
		/// </summary>
		/// <returns>A collection of file paths to be copied to their respective destinations.</returns>
		public override IAsyncEnumerable<string> GetItemsAsync() =>
			IODirectory.GetFiles(Config.Source, "*", Config.IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
			.Where(file => IsPhoto(file) || IsVideo(file))
			.ToAsyncEnumerable();

		/// <summary>
		/// Determines if the file is a photo.
		/// </summary>
		bool IsPhoto(string file) => Config.PhotoFileTypes.Contains(Path.GetExtension(file).TrimStart('.'), StringComparer.InvariantCultureIgnoreCase);

		/// <summary>
		/// Determines if the file is a video.
		/// </summary>
		bool IsVideo(string file) => Config.VideoFileTypes.Contains(Path.GetExtension(file).TrimStart('.'), StringComparer.InvariantCultureIgnoreCase);

		/// <summary>
		/// Gets the date the file was taken, using EXIF data if available and the file creation date if not, and copies the file
		/// to a year and date directory within the appropriate destination folder. E.g., if a photo was taken July 4, 2020 and the 
		/// destination photo directory is c:\photos, the file would be copied to c:\photos\2020\2020-07-04\.
		/// </summary>
		/// <param name="file">The file to copy.</param>
		public override async Task<Result> ProcessAsync(string file)
		{
			DateTime created;

			var exifDate = ImageMetadataReader.ReadMetadata(file).OfType<ExifSubIfdDirectory>().FirstOrDefault()?.GetDescription(ExifDirectoryBase.TagDateTime);

			if (exifDate == null || !DateTime.TryParseExact(exifDate, "yyyy:MM:dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out created))
				created = File.GetCreationTime(file);

			var destFile = Path.Combine(IsPhoto(file) ? Config.PhotoDestination : Config.VideoDestination, created.Year.ToString(), created.ToString("yyyy-MM-dd"), Path.GetFileName(file));

			if (File.Exists(destFile))
				return Result.Success("Already Exists");

			using (var source = File.Open(file, FileMode.Open))
			{
				var destDir = Path.GetDirectoryName(destFile);

				if (!Config.Preview)
				{
					if (!IODirectory.Exists(destDir))
						IODirectory.CreateDirectory(destDir);

					using var dest = File.Create(destFile);

					await source.CopyToAsync(dest, CancellationToken);
				}
				else
				{
					return Result.Success("To Be Copied", destFile);
				}
			}

			if (CancellationToken.IsCancellationRequested)
				return Result.SuccessOrCancelled(CancellationToken);
			else
				return Result.Success("Copied");
		}
	}

	public class MediaImporterConfig : Config
	{
		public bool Preview { get; set; }
		public string Source { get; set; }
		public bool IncludeSubfolders { get; set; } = true;
		public string PhotoDestination { get; set; }
		public string VideoDestination { get; set; }
		public string[] PhotoFileTypes { get; set; } = new[] { "arw", "bmp", "cr2", "cr3", "dng", "jpg", "jpeg", "nef", "png", "raf", "tif", "tiff" };
		public string[] VideoFileTypes { get; set; } = new[] { "avi", "flv", "mp4", "mkv", "mov", "mpg", "mpeg", "wmv" };
	}
}
