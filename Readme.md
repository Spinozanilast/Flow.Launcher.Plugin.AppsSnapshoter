Flow.Launcher.Plugin.SnapshotApps
==================

## About

A plugin for the [Flow launcher](https://github.com/Flow-Launcher/Flow.Launcher).
This plugin allows you to save currently open applications for later launch. Open applications are windowed
applications. Applications that go into suspended state are not launched (
see [UWP](https://learn.microsoft.com/en-us/windows/uwp/get-started/universal-application-platform-guide) applications),
i.e. many standard Microsoft applications.

## Usage

### The plugin is called on "sa" keyword by default.

### List existing Snapshots

* Select list result

| Command     | Description                                           |
|-------------|-------------------------------------------------------|
| sa **list** | List currently existing snapshots, if they are exists |

----------------------

### Snapshots Creating

| Command                | Description                                                     |
|------------------------|-----------------------------------------------------------------|
| sa **{Snapshot Name}** | Create Snapshot with currently opened apps with _snapshot name_ |

----------------------

### Open Snapshot

* Select Open result with written _Snapshot name_
* Select Open result on Snapshot from List Result

----------------------

### Remove Snapshot

* Select Remove result with written _Snapshot name_
* Select Remove result on Snapshot from List Result

----------------------

### Rename Snapshot

* Select Rename result with written Snapshot name and also _New name_
* Select Rename result on Snapshot from List Result and write new _Snapshot name_