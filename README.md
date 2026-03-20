# The Ignavior Launcher

The *Ignavior Launcher* is a standalone download manager designed for distributing binary patches for privately-distributed Windows games, primarily those created with the Unity game engine.

The program retrieves full builds and incremental updates from a remote host and applies them using locally using binary diff patches generated with [xdelta3](https://github.com/jmacd/xdelta).

## What?

Distributing homemade video games — especially those not intended for public release — can be challenging, particularly when frequent updates are involved. Requiring users to download the full build for every update wastes both bandwidth and time. 

This launcher provides a simple user interface for both retrieving the base builds and all incremental binary patches. The primary benefit of patching is that only the data required to update from one version to another is shipped.

## How does it work?

The launcher connects to a remote host containing a manifest describing:
- all available releases
- the identifier of the latest version of each release
- the base builds of each release
- the incremental update patches between versions

The system also finds hosted changelogs. When the user selects a game to launch, the launcher fetches the remote manifest, determines whether the game has been installed, determines the version of the installed release, and downloads, extracts and applies the archives for the required build(s) and patch(es) as needed. 

The program is built as a WPF graphical user interface built on .NET 8.0 with a C# backend. The system is designed for internal use, but can easily be adapted for the distribution of any self-hosted files.

## Features
- Secret management: Optional password protection for internal-sensitivity files, with robust secret preservation, encryption and retrieval
- Bundled executables: All helper standalone executables have been bundled with the main program, and are extracted to a /bin/ folder on first install
- Changelog: Hosted markdown files are displayed on a panel in the launcher
- Game icons: Hosted icons are retrieved and displayed on the top game bar
- Progress bar: Downloads and extraction update a progress bar to provide real-time user feedback

## What does the Ignavior Launcher NOT do?

This is not a particularly sophisticated system. Essentially, all it does is retrieve archive files from an external host, managing their extraction and application. The program may support game-specific data, such as settings or achievements. The system is not intended for identification or authentication of users, advanced package management or digital rights management.

## Future plans

- Manifest extensions: sizes, hashes and mirror hosts
- Self-Updater: The launcher itself is added as an entry in the manifest, containing (sequential or absolute) patches and self-applying updates automatically while running
- Changelog: Display hosted markdown files in the correct WPF panel
- UI updates: ship font styling, and allow for more robust customization, fix bugs, icon fallbacks etc
- Offline: manifest caching, and if connection fails, enable Play button with currently installed version, even without updates
- 

# Installation

The project is currently in its beta phase. Be aware that the program may contain many bugs and issues. Only the Windows 10 and 11 operating systems are officially supported.

To download, navigate to the [Releases](https://github.com/puuhapake/IgnaviorLauncher/releases) section of this repository and download the zip file, extract it and run the executable. A more straightforward installation experience has been planned.

# For developers

## Adding a new game
1. Build the base version of the game
2. Add the files to a `.rar` archive
   - optionally, password-protect the archive with the universal password
4. Upload the archive to the host and any relevant mirrors
5. Copy the direct download URL of the file(s)
6. Add an entry to `manifest.json`:
   - Choose a unique identifier string
   - Set `name` to the display name of the game
   - Set `latest` to an integer representing the current latest version (e.g. `1`)
   - Set the values for `base`, including `version` and the `url`.
7. Create a changelog markdown file for the version, and place it in `changelogs/{game_id}/{version_integer}.md` of the file host
8. Add an icon for the game, and place it in `/icons/` of the file host
9. Upload the updated manifest and changelog(s). Now, the launcher will show the new game.

## Adding an update to an existing game
1. Build the new version of the game
2. Generate patches using [the provided patcher](https://github.com/puuhapake/IgnaviorLauncher_patcher), or an external xdelta3 build
3. Archive the patch folder into a `.rar` archive
   - optionally, password-protect the archive with the universal password
5. Upload the patch to the host
6. Update `manifest.json`:
   - Add a new entry to the `patches` array with definitions for `from` (the old version), `to` (the updated version) and `url`
   - Increment the `latest` field to the new version
7. Add a changelog markdown for the new version, and place it in `changelogs/{game_id}/{latest_version}.md`
9. Upload the updated manifest and changelog(s). Now, the launcher will retrieve the update(s) and apply them sequentially.

# License

License terms have not yet been finalized. 
All rights reserved, except for external programs and libraries, such as `xdelta3`, `7-Zip`, `Inno` or `SharpCompress`.

## Liability

Neither the developers of the launcher nor the launcher itself holds any responsibility for the integrity, morality or legality of files distributed using the program.
