# RANS0M

A fan-made recreation of the RANSOM (A-90) entity from the Roblox game
*Doors*, as a Windows desktop app. It randomly pops the entity's face up on
your screen, you need to stop moving your mouse and stay off the keyboard, or it
"infects" your PC: gold coin files (worth different amounts, `.gold1`-`.gold6`)
get scattered around your user folders, your cursor changes, and your
wallpaper turns dark red. You have to drag enough of them onto the ransom
window to cover the configured ransom amount before the timer runs out.
There's also a rare `.crucifix` file that, if found and dragged in, clears
the ransom instantly.

This was built for fun, it's kind of poorly coded.
Right now the only noticable bug is that the ransom window doesn't always stay on top of other windows, but it should be fine for the most part. It can't go on top of fullscreen apps.

## Read this before running it

By default, failing to pay in time just resets everything (no crash, no
command). But the app **can** shut down or crash your computer, or run an
arbitrary shell command, if you turn that on yourself in the config window
— it's opt-in, off by default. If you enable "crash on death" it calls
`shutdown /s /t 0`; if you enable "run command on death" it runs whatever
command you typed in. So:

- Only run it on a machine you own, and if you enable the crash/command
  options, save your work first and expect it to actually act on them.
- Not run it on anyone else's computer without them knowing exactly what
  it does and agreeing to it.

It is not malware in the sense of trying to steal anything, hide itself, or
spread, it doesn't touch your files besides dropping/deleting its own
harmless `.gold`/`.crucifix` marker files, and it's fully open source so you
can check that yourself. See [LICENSE.md](LICENSE.md) for the full terms and
disclaimer.

## Requirements

- Windows (uses Win32 hooks, `shutdown.exe`, the registry, etc. This
  won't run anywhere else)
- [.NET 10 SDK](https://dotnet.microsoft.com/) or newer
- Visual Studio 2022+ (optional, for the WinForms designer) or just the
  `dotnet` CLI

## Building & running

```
git clone https://github.com/Ixars/ransomdoors
cd ransomdoors
dotnet build
dotnet run
```

Or open `rans0m.slnx` in Visual Studio and hit F5.

The app runs from a system tray icon. Right-click it for:
- **Configuration**
- **Close**

## Configuration

All settings are managed through the **Configuration** window (tray icon →
Configuration), and saved to `config.json` next to the executable:

- **Spawn automatically** — whether the entity shows up on its own timer.
- **Min/max spawn delay** — how often (in seconds) it can randomly appear.
- **Infection duration** — how long (in seconds) you have to pay once
  infected.
- **Ransom amount** — how much gold you need to collect to pay it off.
- **Drawer mode** — scatter coins into a temp folder tree instead of your
  real Desktop/Documents/Pictures/Music/Videos/Downloads folders.
- **Crash on death** — run `shutdown /s /t 0` if you fail to pay in time.
  Off by default.
- **Run command on death** — run a custom shell command if you fail to pay
  in time. Off by default.

You can also trigger a ransom immediately from that window to test your
settings.

Images, sounds, and taunt window titles live in `Properties/Resources.resx`
and the `Resources/`/`Assets/` folders if you want to swap them out.

## Credits

- **Doors** is made by **LSPLASH**. The RANSOM/A-90 entity, its name, look,
  and concept are their original work — this project is an unofficial fan
  recreation, not affiliated with or endorsed by LSPLASH. Go play the real
  game.
- Built with [NAudio](https://github.com/naudio/NAudio) for audio playback.
- Sound effects and images are from the game, taken from the wikis.

## License

Source-available, free to use/modify/redistribute for educational and
non-commercial purposes, with credit required and reselling (original or
modified) forbidden. Full terms in [LICENSE.md](LICENSE.md) — read it, it's
short.
