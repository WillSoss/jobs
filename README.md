# WillSoss.Jobs
A collection of Runly jobs I use at home. Learn how to [get started](https://www.runly.io/docs/getting-started/) with [Runly](https://www.runly.io/).

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

**preview** - Makes the job run and tell you what it'll do without copying anything.

**ignoreUnauthorizedAccessException** - Prevents the job from stopping when this exception occurs.

**parallelTaskCount** - How many files to copy simultaneously.
