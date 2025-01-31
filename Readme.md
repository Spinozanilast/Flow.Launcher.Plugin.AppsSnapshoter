Flow.Launcher.Plugin.AppsSnasphoter
==================

## About

<div align="center">

[![Publish Release](https://github.com/Spinozanilast/Flow.Launcher.Plugin.AppsSnapshoter/actions/workflows/publish.yml/badge.svg)](https://github.com/Spinozanilast/Flow.Launcher.Plugin.AppsSnapshoter/actions/workflows/publish.yml)
[![build](https://github.com/Spinozanilast/Flow.Launcher.Plugin.AppsSnapshoter/actions/workflows/build.yml/badge.svg)](https://github.com/Spinozanilast/Flow.Launcher.Plugin.AppsSnapshoter/actions/workflows/build.yml)

  <img src="snapshot.png" alt="plugin icon"/>
</div>

A plugin for the [Flow launcher](https://github.com/Flow-Launcher/Flow.Launcher).
This plugin allows you to save currently open applications for later launch. Open applications are windowed
applications. Applications that go into suspended state are not launched (
see [UWP](https://learn.microsoft.com/en-us/windows/uwp/get-started/universal-application-platform-guide) applications),
i.e. many standard Microsoft applications.

<div align="center">

![Preview](https://github.com/Spinozanilast/Flow.Launcher.Plugin.AppsSnapshoter/raw/master/preview.gif)

</div>

## Usage

```The plugin is called on "sa" keyword by default. ```

### Snapshot Commands

| Command Meaning                                    | Command                                              | Description                                                                                                       |
|----------------------------------------------------|------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------|
| List Snapshots                                     | ```sa list```                                        | List currently existing snapshots, if they are exists                                                             |
| Create/Open/Remove snapshot or add app to snapshot | ```sa {Snapshot Name}```                             | Create/Open/Remove selected from list (or written) snapshot by selecting specific action from drop down-down list |
| Rename snapshot                                    | ```sa {Current Snapshot Name} {New Snapshot Name}``` | Rename Snapshot with _Current Snapshot Name_ to _New Snapshot Name_                                               |
| List snapshot apps                                 | ```sa {Snapshot Name} apps```                        | Listing apps included in snapshot with _Snapshot Name_                                                            |

### Apps Commands

Command is the same for all actions ```sa {Snapshot Name} {App Name}``` (you just need to select from app from app list
or write his name)

| Command Meaning        | Description                                                                |
|------------------------|----------------------------------------------------------------------------|
| Remove app             | Remove selected app from _Snapshot Name_ snapshot                          |
| Block and delete app   | Remove selected app from all snapshots and block future additions          |
| Replace app with other | Remove selected app and create new from executable selected in file dialog |

## TO DO

- [ ] Add blocked apps view on settings
- [ ] Add localization
 