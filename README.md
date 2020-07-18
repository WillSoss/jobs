# WillSoss.Jobs
A collection of Runly jobs I use at home. Learn how to [get started](https://www.runly.io/docs/getting-started/) with [Runly](https://www.runly.io/).

## MediaImporter
Copies photos and videos from a source location, such as an SD card, to destination photo and video directories, organizing the files by date. Gets the date the photo/video was taken, uing EXIF data if available and the file creation date if not, and copies the file to a year and date directory within the appropriate destination folder. E.g., if a photo was taken July 4, 2020 and the destination photo directory is c:\photos, the file would be copied to c:\photos\2020\2020-07-04\.

Files that are photos or videos are determined based on the file extension matching an entry in `PhotoFileTypes` or `VideoFileTypes` of the config. The default value for each of these can be modified to add or remove file types.

### Config

```json
{
  "job": "WillSoss.Jobs.MediaImporter",
  "source": "I:\\DCIM\\100GOPRO",
  "includeSubfolders": true,
  "photoDestination": "d:\\photos",
  "videoDestination": "d:\\videos"
}
```

**source** - Path to copy from.

**includeSubfolders** - Determines whether to include files in subfolders of the source directory.

**photoDestination** - Path to copy photos to.

**videoDestination** - Path to copy videos to.

## SyncDirectory
Copies a source directory and all its subdirectories and files contained within to a destination path. You can optionally ignore `UnauthorizedAccessExceptions`.

### Config

```json
{
  "job": "WillSoss.Jobs.SyncDirectory",
  "source": "d:\\photos",
  "destination": "e:\\backup\\photos",
  "preview": false,
  "ignoreUnauthorizedAccessException": true,
  "execution": {
    "parallelTaskCount": 10
  }
}
```

**source** - Path to copy from.

**destination** - Path to copy to.

**preview** - Makes the job tell you what it'll do without copying anything.

**ignoreUnauthorizedAccessException** - Prevents the job from stopping when this exception occurs.

**parallelTaskCount** - Number of files to copy simultaneously.
