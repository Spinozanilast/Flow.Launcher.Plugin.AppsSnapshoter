Flow.Launcher.Plugin.SnapshotApps
==================

## About

<div align="center"><img src="snapshot.png" alt="plugin icon"/></div>

A plugin for the [Flow launcher](https://github.com/Flow-Launcher/Flow.Launcher).
This plugin allows you to save currently open applications for later launch. Open applications are windowed
applications. Applications that go into suspended state are not launched (
see [UWP](https://learn.microsoft.com/en-us/windows/uwp/get-started/universal-application-platform-guide) applications),
i.e. many standard Microsoft applications.

https://github.com/user-attachments/assets/df49c246-2a44-427d-82b5-9516d0b108c0

## Usage

### The plugin is called on "sa" keyword by default.

### List existing Snapshots

* Select list result

| Command       | Description                                           |
|---------------|-------------------------------------------------------|
| ```sa list``` | List currently existing snapshots, if they are exists |

* Or select **List Snapshots** option to do the same

----------------------

### Snapshots Creating

| Command                  | Description                                                                                            |
|--------------------------|--------------------------------------------------------------------------------------------------------|
| ```sa {Snapshot Name}``` | Create Snapshot with currently opened apps with _snapshot name_ with choosing **Create Result** option |

----------------------

### Open Snapshot

| Command                  | Description                                                           |
|--------------------------|-----------------------------------------------------------------------|
| ```sa {Snapshot Name}``` | Open Snapshot apps with _snapshot name_ with choosing **Open** option |

* Select Open result with written _Snapshot name_
* Select Open result on Snapshot from List Result

----------------------

### Remove Snapshot

| Command                  | Description                                                          |
|--------------------------|----------------------------------------------------------------------|
| ```sa {Snapshot Name}``` | Remove Snapshot with _snapshot name_ with choosing **Remove** option |

* Select Remove result with written _Snapshot name_
* Select Remove result on Snapshot from List Result

----------------------

### Rename Snapshot

| Command                                              | Description                                                         |
|------------------------------------------------------|---------------------------------------------------------------------|
| ```sa {Current Snapshot Name} {New Snapshot Name}``` | Rename Snapshot with _Current Snapshot Name_ to _New Snapshot Name_ |

* Select Rename result with written Snapshot name and also _New name_
* Select Rename result on Snapshot from List Result and write new _Snapshot name_

----------------------

### Listing Snapshot Apps

| Command                       | Description                                            |
|-------------------------------|--------------------------------------------------------|
| ```sa {Snapshot Name} apps``` | Listing apps included in snapshot with _Snapshot Name_ |

## TO DO

* Individual app settings flow _(same operations as for snapshots(renaming, adding, deleting, changing executable path)_
