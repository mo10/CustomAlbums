# Muse Dash Custom Albums Mod

This mod adds the ability to play custom-designed charts in Muse Dash!
Do note that out of fairness to the developers, it is still required to own the "Just as Planned" DLC in order to play charts.
Check out the Muse Dash Modding Community Discord server for more information on how to install, create, or compete on leaderboards:
https://discord.gg/mdmc

The current version of the mod is guaranteed to function on Muse Dash 2.2.0 (April Fools 2022).

## Branch
`master` Current stable release.
`2.0` Archived version from before the game transitioned to IL2CPP. No longer supported.
`3.0` Current working branch.
`4.0` Work-in-progress overhaul.

### How to build v3.0+
1. Clone repository and switch branch
2. Simply edit the `GameFolder` tag in `Directory.Build.props` to point towards your own installation
3. Click build

### ILRepack
A copy of [ILRepack.Lib.MSBuild.Task](https://github.com/ravibpatel/ILRepack.Lib.MSBuild.Task) and [ILRepack](https://github.com/gluck/il-repack) are included in the repo to build the solution
