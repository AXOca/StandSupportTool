# Stand Support Tool

> [!WARNING]  
> This repository is not intended for reporting issues related to Stand. For any Stand-related issues, please refer to the help resources available [here](https://stand.gg/help/).

Initial concept was made with Python on this project: [SST](https://github.com/AXOca/Stand-Tools/tree/main/SST)

Afterwards, [Alessandro](https://github.com/alessandromrc) made the initial polished version of my rough Python script in C#. They invited me to their repo, and I added a few features and optimized it. I also made the design (which is much easier in Visual Studio than what I did previously). Now, it's at least a tiny bit stand look-a-like like the website. That's it.

![Screenshot of Stand Support Tool](https://github.com/AXOca/StandSupportTool/assets/66976091/e8250050-e0ff-4d83-b5e3-a791dbd75ecb)

## To-Do List

- Add translation support for all languages already supported by Stand itself.

## Changelog

### Version 1.5 08-20-24
- Add 3rd Party AV Check for Launchpad download
- Implement fade in/out for Activation Key

### Version 1.4 07-02-24
- Update links to stand.sh
- Update Launchpad url
- Update Regex for Injection check and also improve log reader

### Version 1.3 07-10-24
- Ensure user has Windows Defender before trying to exclude directory
- Improve Diagnostics by getting OS name and adding Antivirus Info
  
### Version 1.2 07-06-24
- Fix profile names with spaces causing diagnostics to fail
 
### Version 1.1 07-06-24
- Added Compatibility Checks for Diagnostics
- Improved error handling for cases when the Stand DLL is missing in the Bin folder (When performing Diagnostics)
- Refactored the version display to show only the major and minor version numbers (Title Bar)
- Modified the window spawning behavior to position windows in more human-friendly locations
- Improved checks for the Activation Key to ensure better validation
- Bugfixes
  
### Version 1.0 07-05-24
- Rework UI
- Improve Copy Log to Clipboard
- Bugfixes
  
### Version 0.9 06-28-24
- Improve Versioning
- Improve Update Manager (now uses github releases)
- Bugfixes

### Version 0.8 06-28-24
- Improve Diagnostics
- Add "Unload" Focus Link when changing protocol
- Bugfixes

### Version 0.7 06-27-24
- Added Diagnostics

### Version 0.5 06-21-24
- Refactored AV Checker
- Some improvements and bugfixes

### Version 0.4 06-20-24
Thanks to [Alessandro](https://github.com/alessandromrc):
- Added Hotkey Manager
- Added Open Dashboard

### Version 0.3 06-16-24
- Fixed Auto Updater

### Version 0.2 06-16-24
- Added "AV Checker" to identify the user's antivirus software.
  - Thanks to stand.dll and bababoiiiii for contributing by writing the batch file for it.

### Version 0.1 06-16-24
- Initial release with the following features:
  - Full Reset of Stand
  - Clear Cache of Stand
  - Copy Log to Clipboard
  - Copy Profile to Clipboard
  - Clear Hotkeys
  - 60% Hotkeys
  - DL Launchpad
  - Add Exclusion
  - Activation Key
  - Switch Network Protocol for Stand

## Known Issues

- None
