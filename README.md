# OpenNefia

[![GitHub Actions](https://github.com/OpenNefia/OpenNefia/actions/workflows/build-test.yml/badge.svg)](https://github.com/OpenNefia/OpenNefia/actions/workflows/build-test.yml) [![Discord Chat](https://img.shields.io/discord/815674706559762442?style=plastic)](https://discord.gg/cFq452yFQa)

> *Welcome traveler!*

OpenNefia is an open-source, moddable engine reimplementation of the Japanese roguelike RPG [Elona](http://ylvania.org/en/elona).

Things are still pretty WIP at this point, but contributions are appreciated.

## Dependencies

The [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) is required to build the engine.

For development on Windows, [Visual Studio 2022](https://visualstudio.microsoft.com/vs) is recommended.

## Building and Running

1. Clone the repository.

```bash
git clone --recursive https://github.com/OpenNefia/OpenNefia.git
```

2. Open `OpenNefia.sln`.

3. Run the `OpenNefia.EntryPoint` project.

## Contributing

If you'd like to contribute, please base your changes on the `develop` branch. The `master` branch is reserved for the code of the latest tagged release.

## Credits

Parts of OpenNefia were directly ported from the original HSP source code of Elona, originally released by Noa in 2006.

OpenNefia uses code adapted from [RobustToolbox](https://github.com/space-wizards/RobustToolbox), licensed under MIT.
