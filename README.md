[![Latest Release](https://img.shields.io/github/v/release/TahirAhmad88/AudioPlayer?label=download&color=orange&style=for-the-badge)](https://github.com/TahirAhmad88/AudioPlayer/releases/latest)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](https://opensource.org/licenses/MIT)
[![Platform](https://img.shields.io/badge/platform-Windows-blue?style=for-the-badge)]()

# 🎵 AudioPlayerApp

A lightweight, keyboard-first audio player for Windows built with **C# WinForms** and **Windows Media Player (WMP) COM**. Designed for power users who want full control without touching the mouse.

> ⚡ **No external media framework required** – uses the built-in WMP engine for playback, plus **TagLib#** for metadata and album art.

---

## ✨ Features

### 🎹 Full Keyboard Control

| Key | Action |
|-----|--------|
| `Space` / `K` | Play / Pause |
| `Esc` | Stop |
| `←` / `→` | Seek −5s / +10s |
| `J` / `L` | Seek −10s / +10s |
| `Ctrl+←` / `Ctrl+→` | Previous / Next track |
| `↑` / `↓` | Volume +5 / −5 |
| `F6` | Mute / Unmute |
| `Delete` | Remove selected track |
| `Ctrl+O` | Open files |
| `Ctrl+S` / `Ctrl+L` | Save / Load playlist (.m3u) |
| `Ctrl+0…9` | Jump to 0%–90% of track |
| `A` / `B` / `C` | Set A-B repeat / Clear |
| `F3` | Toggle Repeat One |
| `F4` | Clear playlist |
| `F5` | Refresh track list |
| `F7` | Now Playing info |
| `F8` | Toggle Shuffle |
| `F9` | Toggle Mini Mode |
| `0–9` | Speed preset (0.25x – 2.5x) |
| `F1` | Show shortcut help |
| `F2` | Settings (forward key & seconds) |

### 🌐 Global Media Keys

Works **even when the app is minimized or in the background**:

- **Media Next / Previous / Stop / Play-Pause** – fully mapped.
- **Custom Forward Hotkey** – assign any key (A–Z, arrows) with `Ctrl+Alt` modifier, and set forward duration in seconds.

### 🎨 UI & Experience

- **Album art** and artist metadata via **TagLib#**.
- **Custom seek bar** with draggable fill.
- **Mini Mode** – compact layout for small screens.
- **Search / filter** tracks by name.
- **Drag & drop** files or folders to load.
- **Session restore** – remembers last playlist, track, and position.
- **Dark theme** with orange accents.

### 🧠 Advanced Playback

- **A-B Repeat** – set loop points on the fly.
- **Playback speed control** – 0.25x to 2.5x (WMP rate).
- **Shuffle** and **Repeat One** modes.
- **Volume persistence** and mute toggle.

---

## 📦 Requirements

- **Windows 7 / 8 / 10 / 11**
- **.NET Framework 4.7.2**
- **Windows Media Player** (installed by default on most Windows systems)
- **TagLib# 2.3.0** (via NuGet – included in `packages.config`)

---

## 🚀 Getting Started

1. **Clone the repository**

   ```bash
   git clone https://github.com/TahirAhmad88/AudioPlayerApp.git

2. Open in Visual Studio (2017 or later).

3. Restore NuGet packages (TagLib# is already referenced).

4. Build and run (F5).

5. Load audio files via Ctrl+O, drag & drop, or double-click a track.

💡 The app uses the Windows Media Player COM control (AxWMPLib). If it's missing, enable it via Turn Windows features on or off → Media Features → Windows Media Player.

🗂️ Project Structure

AudioPlayerApp/

├── Form1.cs              # Main logic (playback, hotkeys, UI events)

├── Form1.Designer.cs     # UI layout (WinForms)

├── Program.cs            # Entry point/

├── Properties/

│   ├── Settings.settings # User settings (forward key, seconds)

│   └── Resources.resx    # Default album art (audiopic.png)

├── packages.config       # NuGet dependencies (TagLib#)

└── AudioPlayerApp.csproj # Project file

🛠️ Configuration
Settings are stored in:

%APPDATA%\AudioPlayerApp\settings.ini

* ForwardKey – global forward hotkey (default: Right)

* ForwardSeconds – forward duration (default: 10)

* LastPlaylist, LastTrack, LastPosition – session restore data

🧩 Future Roadmap

This project is actively evolving. Planned updates:

* Built-in Equalizer (10-band, presets, bass boost)
* Video Player Mode (MP4, MKV, AVI support via WMP)
* Playlist Manager (drag-to-reorder, save/load multiple lists)
* Lyrics Support (.lrc sync)
* Custom Themes (light / dark / neon)
* Cross-platform exploration (.NET MAUI / Avalonia)

The roadmap is flexible – future updates will be driven by user feedback and technical feasibility.

🤝 Contributing

Contributions are welcome!
If you find a bug or have a feature request, please open an issue or submit a pull request.

1. Fork the repo

2. Create a feature branch (git checkout -b feature/equalizer)

3. Commit your changes (git commit -m 'Add equalizer')

4. Push to the branch (git push origin feature/equalizer)

5. Open a Pull Request

📄 License

This project is licensed under the MIT License – see the LICENSE file for details.

* 👤 Author
 Tahir Ahmad

* GitHub: @TahirAhmad88

LinkedIn: www.linkedin.com/in/tahir-ahmad88

⭐ Show Your Support
If you like this project, please star ⭐ the repository and share it with others who might find it useful.

📸 Screenshots

<img width="638" height="393" alt="Screenshot 2026-09-19 204746" src="https://github.com/user-attachments/assets/3df1bf80-de66-4c46-8660-277ad07889bb" />

<img width="409" height="127" alt="image" src="https://github.com/user-attachments/assets/1e2578a7-1d7d-4826-b356-89c93f32eff1" />

<img width="606" height="655" alt="image" src="https://github.com/user-attachments/assets/3d2b4920-c342-434d-b3f5-ea74671752ce" />

<img width="326" height="203" alt="image" src="https://github.com/user-attachments/assets/772519ef-7db1-453c-9d8f-9cfebb3ad0ba" />

<img width="326" height="439" alt="image" src="https://github.com/user-attachments/assets/407a8985-7fb6-426d-92ed-f4fca5cef384" />

🙏 Acknowledgements
* TagLib# – for audio metadata and album art.

* Windows Media Player COM – for reliable playback on Windows.

* .NET Framework – the foundation of this app.

Built with ❤️ for keyboard-first music lovers.
