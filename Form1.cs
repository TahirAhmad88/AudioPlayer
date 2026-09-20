using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMPLib;

namespace AudioPlayerApp
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_DLGMODALFRAME = 0x0001;

        private const int WM_HOTKEY = 0x0312;
        private const uint MOD_NONE = 0x0000;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;

        private const int HOTKEY_ID_NEXT = 1;
        private const int HOTKEY_ID_PREV = 2;
        private const int HOTKEY_ID_STOP = 3;
        private const int HOTKEY_ID_PLAYPAUSE = 4;
        private const int HOTKEY_ID_FORWARD = 5;

        private const uint VK_MEDIA_NEXT_TRACK = 0xB0;
        private const uint VK_MEDIA_PREV_TRACK = 0xB1;
        private const uint VK_MEDIA_STOP = 0xB2;
        private const uint VK_MEDIA_PLAY_PAUSE = 0xB3;

        private readonly List<string> _allTracks = new List<string>();
        private List<string> _filteredTracks = new List<string>();
        private int _currentIndex = -1;
        private bool _isPlaying;
        private bool _isDraggingSeek;
        private bool _isMuted;
        private double _lastVolumeBeforeMute = 50;
        private double? _pointA;
        private double? _pointB;
        private bool _repeatOne;
        private bool _hotkeysRegistered;
        private bool _shuffle;
        private readonly Random _rng = new Random();
        private bool _isMiniMode;
        private Size _originalClientSize = Size.Empty;
        private Point _originalBtnPreviewPos, _originalBtnNextPos, _originalBtnPlayPos,
                      _originalBtnPausePos, _originalBtnStopPos, _originalBtnOpenPos;
        private Point _originalPBarPos;
        private Size _originalPBarSize;
        private bool _miniStateSaved;
        private AppSettings _settings;

        public Form1()
        {
            InitializeComponent();
            _settings = AppSettings.Load();
            WireEvents();
        }

        private void WireEvents()
        {
            this.Load += Form1_Load;
            this.FormClosing += Form1_FormClosing;
            this.KeyDown += Form1_KeyDown;
            this.DragEnter += Form1_DragEnter;
            this.DragDrop += Form1_DragDrop;

            btnPreview.Click += (s, e) => PlayPrevious();
            btnNext.Click += (s, e) => PlayNext();
            btnPlay.Click += (s, e) => PlayCurrent();
            btnPause.Click += (s, e) => PausePlayback();
            btnStop.Click += (s, e) => StopPlayback();
            btnOpen.Click += (s, e) => OpenFiles();

            trackVolume.Scroll += (s, e) => ApplyVolume();
            trackVolume.MouseUp += (s, e) => this.Focus();
            txtSearch.TextChanged += (s, e) => FilterTracks();
            trackList.DoubleClick += (s, e) => PlaySelectedFromList();

            pBar.MouseDown += PBar_MouseDown;
            pBar.MouseMove += PBar_MouseMove;
            pBar.MouseUp += PBar_MouseUp;
            pBar.Resize += (s, e) => UpdateSeekFillFromPlayer();

            timer1.Tick += Timer1_Tick;
            player.PlayStateChange += Player_PlayStateChange;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                player.uiMode = "none";
                player.settings.autoStart = false;
                player.settings.volume = trackVolume.Value;
                lblVolume.Text = trackVolume.Value + "%";
                // Load default album art from resources
                picArt.Image = Properties.Resources.audiopic;
            }
            catch
            {
                // WMP COM control may not be fully initialized yet; ignore and continue.
            }

            // CRITICAL FIX: Force focus to the form so KeyDown events fire
            this.Focus();
            this.Select();
            this.KeyPreview = true;

            RegisterGlobalHotkeys();
            RestoreLastSession();
        }

        private void RestoreLastSession()
        {
            if (string.IsNullOrEmpty(_settings.LastPlaylist)) return;

            try
            {
                string[] restored = _settings.LastPlaylist.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string path in restored)
                {
                    if (File.Exists(path)) _allTracks.Add(path);
                }
                FilterTracks();

                if (!string.IsNullOrEmpty(_settings.LastTrack))
                {
                    int idx = _allTracks.IndexOf(_settings.LastTrack);
                    if (idx >= 0)
                    {
                        _currentIndex = idx;
                        LoadAndPlay(_allTracks[idx]);
                        SyncSelection();

                        // Restore position after WMP loads the media
                        Task.Delay(1500).ContinueWith(_ =>
                        {
                            try
                            {
                                if (player.currentMedia != null && this.IsHandleCreated && !this.IsDisposed)
                                {
                                    this.Invoke((Action)(() =>
                                    {
                                        player.Ctlcontrols.currentPosition = _settings.LastPosition;
                                    }));
                                }
                            }
                            catch { }
                        });
                    }
                }
            }
            catch { }
        }

        private void RegisterGlobalHotkeys()
        {
            try
            {
                bool ok = true;
                ok &= RegisterHotKey(this.Handle, HOTKEY_ID_NEXT, MOD_NONE, VK_MEDIA_NEXT_TRACK);
                ok &= RegisterHotKey(this.Handle, HOTKEY_ID_PREV, MOD_NONE, VK_MEDIA_PREV_TRACK);
                ok &= RegisterHotKey(this.Handle, HOTKEY_ID_STOP, MOD_NONE, VK_MEDIA_STOP);
                ok &= RegisterHotKey(this.Handle, HOTKEY_ID_PLAYPAUSE, MOD_NONE, VK_MEDIA_PLAY_PAUSE);
                ok &= RegisterHotKey(this.Handle, HOTKEY_ID_FORWARD, MOD_CONTROL | MOD_ALT, (uint)_settings.ForwardKey);
                _hotkeysRegistered = ok;
            }
            catch
            {
                _hotkeysRegistered = false;
            }
        }

        private void UnregisterGlobalHotkeys()
        {
            if (!_hotkeysRegistered) return;
            try
            {
                UnregisterHotKey(this.Handle, HOTKEY_ID_NEXT);
                UnregisterHotKey(this.Handle, HOTKEY_ID_PREV);
                UnregisterHotKey(this.Handle, HOTKEY_ID_STOP);
                UnregisterHotKey(this.Handle, HOTKEY_ID_PLAYPAUSE);
                UnregisterHotKey(this.Handle, HOTKEY_ID_FORWARD);
            }
            catch
            {
                // Best-effort cleanup; nothing else to do if this fails on shutdown.
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                switch (id)
                {
                    case HOTKEY_ID_NEXT: PlayNext(); break;
                    case HOTKEY_ID_PREV: PlayPrevious(); break;
                    case HOTKEY_ID_STOP: StopPlayback(); break;
                    case HOTKEY_ID_PLAYPAUSE: TogglePlayPause(); break;
                    case HOTKEY_ID_FORWARD: SeekRelative(_settings.ForwardSeconds); break;
                }
            }
            base.WndProc(ref m);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterGlobalHotkeys();

            // Save last session state
            try
            {
                _settings.LastPlaylist = string.Join("|", _allTracks);
                _settings.LastTrack = (_currentIndex >= 0 && _currentIndex < _allTracks.Count)
                    ? _allTracks[_currentIndex] : "";
                _settings.LastPosition = player?.Ctlcontrols?.currentPosition ?? 0;
                _settings.Save();
            }
            catch { }

            try
            {
                if (player != null) player.Ctlcontrols.stop();
            }
            catch { }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (txtSearch.Focused && e.KeyCode != Keys.Escape)
            {
                return;
            }

            if (e.Control && e.KeyCode == Keys.Left) { PlayPrevious(); e.Handled = true; return; }
            if (e.Control && e.KeyCode == Keys.Right) { PlayNext(); e.Handled = true; return; }
            if (e.Control && e.KeyCode == Keys.O) { OpenFiles(); e.Handled = true; return; }
            if (e.Control && e.KeyCode == Keys.S) { SavePlaylist(); e.Handled = true; return; }
            if (e.Control && e.KeyCode == Keys.L) { LoadPlaylist(); e.Handled = true; return; }
            if (e.Control && e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
            {
                JumpToPercent((e.KeyCode - Keys.D0) * 10);
                e.Handled = true;
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.Space: TogglePlayPause(); e.Handled = true; break;
                case Keys.Escape: StopPlayback(); e.Handled = true; break;
                case Keys.Left: SeekRelative(-5); e.Handled = true; break;
                case Keys.Right: SeekRelative(10); e.Handled = true; break;
                case Keys.Up: AdjustVolume(5); e.Handled = true; break;
                case Keys.Down: AdjustVolume(-5); e.Handled = true; break;
                case Keys.F6: ToggleMute(); e.Handled = true; break;
                case Keys.F1: ShowShortcutHelp(); e.Handled = true; break;
                case Keys.F2: OpenSettingsDialog(); e.Handled = true; break;
                case Keys.F3: ToggleRepeatOne(); e.Handled = true; break;
                case Keys.F4: ClearPlaylist(); e.Handled = true; break;
                case Keys.F5: RefreshTrackList(); e.Handled = true; break;
                case Keys.F7: ShowNowPlayingInfo(); e.Handled = true; break;
                case Keys.F8: ToggleShuffle(); e.Handled = true; break;
                case Keys.F9: ToggleMiniMode(); e.Handled = true; break;
                case Keys.Delete: RemoveSelectedTrack(); e.Handled = true; break;
                case Keys.J: SeekRelative(-10); e.Handled = true; break;
                case Keys.K: TogglePlayPause(); e.Handled = true; break;
                case Keys.L: SeekRelative(10); e.Handled = true; break;
                case Keys.A: SetPointA(); e.Handled = true; break;
                case Keys.B: SetPointB(); e.Handled = true; break;
                case Keys.C: ClearAB(); e.Handled = true; break;
                case Keys.D0: SetSpeedPreset(0); e.Handled = true; break;
                case Keys.D1: SetSpeedPreset(1); e.Handled = true; break;
                case Keys.D2: SetSpeedPreset(2); e.Handled = true; break;
                case Keys.D3: SetSpeedPreset(3); e.Handled = true; break;
                case Keys.D4: SetSpeedPreset(4); e.Handled = true; break;
                case Keys.D5: SetSpeedPreset(5); e.Handled = true; break;
                case Keys.D6: SetSpeedPreset(6); e.Handled = true; break;
                case Keys.D7: SetSpeedPreset(7); e.Handled = true; break;
                case Keys.D8: SetSpeedPreset(8); e.Handled = true; break;
                case Keys.D9: SetSpeedPreset(9); e.Handled = true; break;
            }
        }

        private void SavePlaylist()
        {
            if (_allTracks.Count == 0)
            {
                MessageBox.Show(this, "Playlist is empty.", "Save Playlist", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "M3U Playlist|*.m3u";
                dlg.FileName = "MyPlaylist.m3u";
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllLines(dlg.FileName, _allTracks);
                        MessageBox.Show(this, "Playlist saved!", "Save Playlist", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "Failed to save: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void LoadPlaylist()
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "M3U Playlist|*.m3u|All Files|*.*";
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        string[] lines = File.ReadAllLines(dlg.FileName);
                        List<string> validTracks = new List<string>();
                        foreach (string line in lines)
                        {
                            string trimmed = line.Trim();
                            if (!string.IsNullOrEmpty(trimmed) && !trimmed.StartsWith("#") && File.Exists(trimmed))
                            {
                                validTracks.Add(trimmed);
                            }
                        }

                        if (validTracks.Count > 0)
                        {
                            StopPlayback();
                            _allTracks.Clear();
                            _allTracks.AddRange(validTracks);
                            _currentIndex = -1;
                            FilterTracks();
                            MessageBox.Show(this, "Loaded " + validTracks.Count + " tracks.", "Load Playlist", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show(this, "No valid tracks found in playlist.", "Load Playlist", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "Failed to load: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ToggleShuffle()
        {
            _shuffle = !_shuffle;
            this.Text = "AudioPlayer - Shuffle " + (_shuffle ? "ON" : "OFF") + " - Press F1 for shortcuts";
        }

        private int GetRandomIndex()
        {
            if (_allTracks.Count <= 1) return 0;
            int newIdx;
            do { newIdx = _rng.Next(_allTracks.Count); } while (newIdx == _currentIndex);
            return newIdx;
        }

        private void ToggleMiniMode()
        {
            _isMiniMode = !_isMiniMode;

            if (_isMiniMode)
            {
                // Save original layout ONCE
                if (!_miniStateSaved)
                {
                    _originalClientSize = this.ClientSize;
                    _originalBtnPreviewPos = btnPreview.Location;
                    _originalBtnNextPos = btnNext.Location;
                    _originalBtnPlayPos = btnPlay.Location;
                    _originalBtnPausePos = btnPause.Location;
                    _originalBtnStopPos = btnStop.Location;
                    _originalBtnOpenPos = btnOpen.Location;
                    _originalPBarPos = pBar.Location;
                    _originalPBarSize = pBar.Size;
                    _miniStateSaved = true;
                }

                // Hide non-essential controls
                panel1.Visible = false;
                lblTrackStart.Visible = false;
                lblTrackEnd.Visible = false;

                // Remove any default margins so button sizes are exact
                btnPreview.Margin = new Padding(0);
                btnNext.Margin = new Padding(0);
                btnPlay.Margin = new Padding(0);
                btnPause.Margin = new Padding(0);
                btnStop.Margin = new Padding(0);
                btnOpen.Margin = new Padding(0);

                int sidePad = 3;
                int btnW = 65;
                int btnH = 25;
                int gap = 4;
                int safetyPad = 3;                                  // extra right-side safety

                int buttonRowWidth = (btnW * 6) + (gap * 5);        // 440px
                int miniWidth = sidePad + buttonRowWidth + sidePad + safetyPad;  // 458px
                int miniHeight = 130;
                int innerW = buttonRowWidth;                        // seek bar + WMP width = same as buttons

                // 1. SEEK BAR — same width as button row
                int seekY = 6;
                int seekH = 10;
                pBar.Location = new Point(sidePad, seekY);
                pBar.Size = new Size(innerW, seekH);

                // 2. BUTTONS — bottom row, same left/right margins
                int btnY = 65;
                int x = sidePad;

                btnPreview.Location = new Point(x, btnY); btnPreview.Size = new Size(btnW, btnH); x += btnW + gap;
                btnNext.Location = new Point(x, btnY); btnNext.Size = new Size(btnW, btnH); x += btnW + gap;
                btnPlay.Location = new Point(x, btnY); btnPlay.Size = new Size(btnW, btnH); x += btnW + gap;
                btnPause.Location = new Point(x, btnY); btnPause.Size = new Size(btnW, btnH); x += btnW + gap;
                btnStop.Location = new Point(x, btnY); btnStop.Size = new Size(btnW, btnH); x += btnW + gap;
                btnOpen.Location = new Point(x, btnY); btnOpen.Size = new Size(btnW, btnH);

                // 3. WMP VISUALIZER — auto-centered between seek bar and buttons
                int seekBottom = seekY + seekH;                    // 16
                int btnTop = btnY;                                 // 65
                int wmpHeight = 35;
                int gapAboveAndBelow = (btnTop - seekBottom - wmpHeight) / 2;   // (65-16-35)/2 = 7

                player.Visible = true;
                player.Location = new Point(sidePad, seekBottom + gapAboveAndBelow);
                player.Size = new Size(innerW, wmpHeight);

                // Resize the form AFTER positioning controls
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.MinimumSize = new Size(miniWidth, miniHeight);
                this.MaximumSize = new Size(miniWidth, miniHeight);
                this.ClientSize = new Size(miniWidth, miniHeight);

                // Force icon display
                this.Icon = this.Icon;
                SetWindowLong(this.Handle, GWL_EXSTYLE,
                    GetWindowLong(this.Handle, GWL_EXSTYLE) & ~WS_EX_DLGMODALFRAME);

                this.PerformLayout();
                this.Refresh();
            }
            else
            {
                // RESTORE FULL WINDOW
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.MaximizeBox = true;
                this.MinimizeBox = true;
                this.MinimumSize = new Size(0, 0);
                this.MaximumSize = new Size(0, 0);
                this.ClientSize = _originalClientSize;

                // Show hidden controls
                panel1.Visible = true;
                lblTrackStart.Visible = true;
                lblTrackEnd.Visible = true;

                // Restore WMP control to its original hidden state
                player.Location = new Point(2, 2);
                player.Size = new Size(1, 1);

                // Restore original positions
                btnPreview.Location = _originalBtnPreviewPos; btnPreview.Size = new Size(90, 32);
                btnNext.Location = _originalBtnNextPos; btnNext.Size = new Size(90, 32);
                btnPlay.Location = _originalBtnPlayPos; btnPlay.Size = new Size(90, 32);
                btnPause.Location = _originalBtnPausePos; btnPause.Size = new Size(90, 32);
                btnStop.Location = _originalBtnStopPos; btnStop.Size = new Size(90, 32);
                btnOpen.Location = _originalBtnOpenPos; btnOpen.Size = new Size(90, 32);

                pBar.Location = _originalPBarPos;
                pBar.Size = _originalPBarSize;

                // Restore Designer-defined positions for labels
                lblTrackStart.Location = new Point(20, 12);
                lblTrackStart.Size = new Size(140, 34);
                lblTrackEnd.Location = new Point(474, 12);
                lblTrackEnd.Size = new Size(140, 34);
                panel1.Location = new Point(20, 55);
                panel1.Size = new Size(594, 215);

                this.PerformLayout();
                this.Refresh();
            }
        }

        private void SetSpeedPreset(int digit)
        {
            try
            {
                double rate;
                switch (digit)
                {
                    case 1: rate = 0.25; break;
                    case 2: rate = 0.5; break;
                    case 3: rate = 0.75; break;
                    case 4: rate = 1.0; break;
                    case 5: rate = 1.25; break;
                    case 6: rate = 1.5; break;
                    case 7: rate = 1.75; break;
                    case 8: rate = 2.0; break;
                    case 9: rate = 2.5; break;
                    default: rate = 1.0; break;
                }
                player.settings.rate = rate;
                this.Text = "AudioPlayer - Speed " + rate.ToString("0.00") + "x - Press F1 for shortcuts";
            }
            catch { }
        }

        private void JumpToPercent(int percent)
        {
            try
            {
                if (player.currentMedia == null) return;
                double duration = player.currentMedia.duration;
                player.Ctlcontrols.currentPosition = duration * (percent / 100.0);
            }
            catch { }
        }

        private void OpenFiles()
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Audio Files|*.mp3;*.wav;*.wma;*.m4a;*.flac;*.aac|All Files|*.*";
                dlg.Multiselect = true;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    AddTracks(dlg.FileNames);
                }
            }
        }

        private void AddTracks(IEnumerable<string> paths)
        {
            StopPlayback();
            _allTracks.Clear();
            _currentIndex = -1;

            foreach (string path in paths)
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    _allTracks.Add(path);
                }
            }

            FilterTracks();

            if (_allTracks.Count > 0)
            {
                _currentIndex = 0;
                SyncSelection();
                LoadAndPlay(_allTracks[0]);
            }
        }

        private void FilterTracks()
        {
            string query = txtSearch.Text?.Trim() ?? string.Empty;
            string currentPath = (_currentIndex >= 0 && _currentIndex < _allTracks.Count) ? _allTracks[_currentIndex] : null;

            _filteredTracks = string.IsNullOrEmpty(query)
                ? new List<string>(_allTracks)
                : _allTracks.Where(p => Path.GetFileNameWithoutExtension(p).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            trackList.BeginUpdate();
            trackList.Items.Clear();
            foreach (string path in _filteredTracks)
            {
                trackList.Items.Add(Path.GetFileNameWithoutExtension(path));
            }
            trackList.EndUpdate();

            if (currentPath != null)
            {
                int idx = _filteredTracks.IndexOf(currentPath);
                if (idx >= 0) trackList.SelectedIndex = idx;
            }
        }

        private void PlaySelectedFromList()
        {
            if (trackList.SelectedIndex < 0 || trackList.SelectedIndex >= _filteredTracks.Count) return;
            string path = _filteredTracks[trackList.SelectedIndex];
            int idx = _allTracks.IndexOf(path);
            if (idx < 0) return;
            _currentIndex = idx;
            LoadAndPlay(path);
        }

        private void LoadAndPlay(string path)
        {
            try
            {
                player.URL = path;
                player.Ctlcontrols.play();
                _isPlaying = true;
                timer1.Start();
                Task.Run(() => LoadMetadata(path));
            }
            catch { }
        }

        private void LoadMetadata(string path)
        {
            string artist = "Unknown Artist";
            Image art = null;

            try
            {
                using (TagLib.File tagFile = TagLib.File.Create(path))
                {
                    if (tagFile.Tag.Performers != null && tagFile.Tag.Performers.Length > 0)
                    {
                        artist = tagFile.Tag.Performers[0];
                    }
                    else if (tagFile.Tag.AlbumArtists != null && tagFile.Tag.AlbumArtists.Length > 0)
                    {
                        artist = tagFile.Tag.AlbumArtists[0];
                    }

                    if (tagFile.Tag.Pictures != null && tagFile.Tag.Pictures.Length > 0)
                    {
                        byte[] data = tagFile.Tag.Pictures[0].Data.ToArray();
                        using (MemoryStream ms = new MemoryStream(data))
                        {
                            art = new Bitmap(Image.FromStream(ms));
                        }
                    }
                }
            }
            catch { }

            try
            {
                if (this.IsHandleCreated && !this.IsDisposed)
                {
                    this.Invoke((Action)(() =>
                    {
                        label1.Text = "By " + artist;
                        Image old = picArt.Image;

                        // If the track has no artwork, use the default audiopic
                        if (art != null)
                        {
                            picArt.Image = art;
                        }
                        else
                        {
                            picArt.Image = Properties.Resources.audiopic;
                        }

                        // Dispose old image only if it's not the resource image
                        if (old != null && old != Properties.Resources.audiopic)
                        {
                            old.Dispose();
                        }
                    }));
                }
            }
            catch { }
        }

        private void PlayCurrent()
        {
            try
            {
                if (_currentIndex == -1 && _allTracks.Count > 0) _currentIndex = 0;
                if (_currentIndex >= 0 && _currentIndex < _allTracks.Count)
                {
                    if (player.playState == WMPPlayState.wmppsPaused && player.currentMedia != null)
                    {
                        player.Ctlcontrols.play();
                        timer1.Start();
                    }
                    else
                    {
                        LoadAndPlay(_allTracks[_currentIndex]);
                        SyncSelection();
                    }
                    _isPlaying = true;
                }
            }
            catch { }
        }

        private void PausePlayback()
        {
            try
            {
                player.Ctlcontrols.pause();
                _isPlaying = false;
            }
            catch { }
        }

        private void StopPlayback()
        {
            try { player.Ctlcontrols.stop(); } catch { }
            _isPlaying = false;
            timer1.Stop();
            lblTrackStart.Text = "00:00";
            lblTrackEnd.Text = "00:00";
            UpdateSeekFill(0);
        }

        private void TogglePlayPause()
        {
            try
            {
                if (player.playState == WMPPlayState.wmppsPlaying) PausePlayback();
                else PlayCurrent();
            }
            catch { PlayCurrent(); }
        }

        private void PlayNext()
        {
            if (_allTracks.Count == 0) return;
            _currentIndex = _shuffle ? GetRandomIndex() : (_currentIndex + 1) % _allTracks.Count;
            LoadAndPlay(_allTracks[_currentIndex]);
            SyncSelection();
        }

        private void PlayPrevious()
        {
            if (_allTracks.Count == 0) return;
            _currentIndex = (_currentIndex - 1 + _allTracks.Count) % _allTracks.Count;
            LoadAndPlay(_allTracks[_currentIndex]);
            SyncSelection();
        }

        private void SyncSelection()
        {
            if (_currentIndex < 0 || _currentIndex >= _allTracks.Count) return;
            string path = _allTracks[_currentIndex];
            int idx = _filteredTracks.IndexOf(path);
            if (idx >= 0) trackList.SelectedIndex = idx;
        }

        private void RemoveSelectedTrack()
        {
            if (trackList.SelectedIndex < 0 || trackList.SelectedIndex >= _filteredTracks.Count) return;
            string path = _filteredTracks[trackList.SelectedIndex];
            int idx = _allTracks.IndexOf(path);
            if (idx < 0) return;

            bool wasCurrent = idx == _currentIndex;
            _allTracks.RemoveAt(idx);
            if (idx < _currentIndex) _currentIndex--;
            else if (wasCurrent) { StopPlayback(); _currentIndex = -1; }
            FilterTracks();
        }

        private void ClearPlaylist()
        {
            StopPlayback();
            _allTracks.Clear();
            _currentIndex = -1;
            FilterTracks();
        }

        private void RefreshTrackList() { FilterTracks(); }

        private void ApplyVolume()
        {
            try
            {
                player.settings.volume = trackVolume.Value;
                lblVolume.Text = trackVolume.Value + "%";
                _isMuted = trackVolume.Value == 0;
            }
            catch { }
        }

        private void AdjustVolume(int delta)
        {
            int newVal = trackVolume.Value + delta;
            newVal = Math.Max(trackVolume.Minimum, Math.Min(trackVolume.Maximum, newVal));
            trackVolume.Value = newVal;
            ApplyVolume();
        }

        private void ToggleMute()
        {
            if (_isMuted)
            {
                trackVolume.Value = (int)_lastVolumeBeforeMute;
                _isMuted = false;
            }
            else
            {
                _lastVolumeBeforeMute = trackVolume.Value;
                trackVolume.Value = 0;
                _isMuted = true;
            }
            ApplyVolume();
        }

        private void SeekRelative(double seconds)
        {
            try
            {
                if (player.currentMedia == null) return;
                double duration = player.currentMedia.duration;
                double newPos = player.Ctlcontrols.currentPosition + seconds;
                if (newPos < 0) newPos = 0;
                if (newPos > duration) newPos = duration;
                player.Ctlcontrols.currentPosition = newPos;
            }
            catch { }
        }

        private void SetPointA()
        {
            try
            {
                if (player.currentMedia == null) return;
                _pointA = player.Ctlcontrols.currentPosition;
                _pointB = null;
                this.Text = "AudioPlayer - A point set - Press F1 for shortcuts";
            }
            catch { }
        }

        private void SetPointB()
        {
            try
            {
                if (player.currentMedia == null || _pointA == null) return;
                double pos = player.Ctlcontrols.currentPosition;
                if (pos > _pointA.Value)
                {
                    _pointB = pos;
                    this.Text = "AudioPlayer - A-B repeat active - Press F1 for shortcuts";
                }
            }
            catch { }
        }

        private void ClearAB()
        {
            _pointA = null;
            _pointB = null;
            this.Text = "AudioPlayer - Ready - Press F1 for shortcuts";
        }

        private void ToggleRepeatOne()
        {
            _repeatOne = !_repeatOne;
            this.Text = "AudioPlayer - Repeat " + (_repeatOne ? "ON" : "OFF") + " - Press F1 for shortcuts";
        }

        private void ShowNowPlayingInfo()
        {
            string name = (_currentIndex >= 0 && _currentIndex < _allTracks.Count)
                ? Path.GetFileNameWithoutExtension(_allTracks[_currentIndex])
                : "No track loaded";
            MessageBox.Show(this, "Now playing: " + name, "Now Playing", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Player_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            if (e.newState == (int)WMPPlayState.wmppsMediaEnded)
            {
                try
                {
                    if (this.IsHandleCreated && !this.IsDisposed)
                    {
                        this.BeginInvoke((Action)OnTrackEnded);
                    }
                }
                catch { }
            }
        }

        private void OnTrackEnded()
        {
            if (_allTracks.Count == 0) return;
            if (_repeatOne) LoadAndPlay(_allTracks[_currentIndex]);
            else PlayNext();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (player.currentMedia == null) return;

                double duration = player.currentMedia.duration;
                double position = player.Ctlcontrols.currentPosition;

                if (_pointA.HasValue && _pointB.HasValue && position >= _pointB.Value)
                {
                    player.Ctlcontrols.currentPosition = _pointA.Value;
                    position = _pointA.Value;
                }

                lblTrackStart.Text = FormatTime(position);
                lblTrackEnd.Text = FormatTime(Math.Max(0, duration - position));

                if (!_isDraggingSeek)
                {
                    UpdateSeekFill(duration > 0 ? position / duration : 0);
                }

                // Fallback detection for auto-advance
                if (_isPlaying && player.playState == WMPPlayState.wmppsStopped && duration > 1.0 && position >= duration - 0.5)
                {
                    _isPlaying = false;
                    OnTrackEnded();
                }
            }
            catch { }
        }

        private static string FormatTime(double totalSeconds)
        {
            if (double.IsNaN(totalSeconds) || totalSeconds < 0) totalSeconds = 0;
            TimeSpan ts = TimeSpan.FromSeconds(totalSeconds);
            return string.Format("{0:00}:{1:00}", (int)ts.TotalMinutes, ts.Seconds);
        }

        private void PBar_MouseDown(object sender, MouseEventArgs e) { _isDraggingSeek = true; SeekFromMouse(e.X); }
        private void PBar_MouseMove(object sender, MouseEventArgs e) { if (_isDraggingSeek) SeekFromMouse(e.X); }
        private void PBar_MouseUp(object sender, MouseEventArgs e)
        {
            if (_isDraggingSeek)
            {
                SeekFromMouse(e.X);
                _isDraggingSeek = false;
            }
        }

        private void SeekFromMouse(int x)
        {
            try
            {
                if (player.currentMedia == null || pBar.Width <= 0) return;
                double fraction = Math.Max(0, Math.Min(1, (double)x / pBar.Width));
                UpdateSeekFill(fraction);
                double duration = player.currentMedia.duration;
                player.Ctlcontrols.currentPosition = duration * fraction;
            }
            catch { }
        }

        private void UpdateSeekFill(double fraction)
        {
            if (pBar.IsDisposed || pBarFill.IsDisposed) return;
            fraction = Math.Max(0, Math.Min(1, fraction));
            int width = (int)(pBar.Width * fraction);

            if (pBar.InvokeRequired)
            {
                try { pBar.Invoke((Action)(() => { pBarFill.Width = width; })); } catch { }
            }
            else pBarFill.Width = width;
        }

        private void UpdateSeekFillFromPlayer()
        {
            try
            {
                if (player.currentMedia == null) return;
                double duration = player.currentMedia.duration;
                double position = player.Ctlcontrols.currentPosition;
                UpdateSeekFill(duration > 0 ? position / duration : 0);
            }
            catch { }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            string[] droppedItems = (string[])e.Data.GetData(DataFormats.FileDrop);
            string[] audioExt = { ".mp3", ".wav", ".wma", ".m4a", ".flac", ".aac" };
            List<string> validFiles = new List<string>();

            foreach (string item in droppedItems)
            {
                if (Directory.Exists(item))
                {
                    validFiles.AddRange(Directory.GetFiles(item, "*.*", SearchOption.AllDirectories)
                        .Where(f => audioExt.Contains(Path.GetExtension(f).ToLowerInvariant())));
                }
                else if (File.Exists(item) && audioExt.Contains(Path.GetExtension(item).ToLowerInvariant()))
                {
                    validFiles.Add(item);
                }
            }

            if (validFiles.Count > 0) AddTracks(validFiles);
        }

        private void ShowShortcutHelp()
        {
            string help =
                "HOW TO START\r\n" +
                "------------------------------------------------------\r\n" +
                "1. Press Ctrl+O  or  drag audio files/folders onto the window\r\n" +
                "2. Double-click any track in the list to play it\r\n" +
                "3. Press F1 anytime to see this help\r\n\r\n" +

                "PLAYBACK CONTROLS\r\n" +
                "------------------------------------------------------\r\n" +
                "Space\tPlay / Pause\t\tK\tPlay / Pause\r\n" +
                "Esc\tStop\r\n" +
                "Left\tSeek -5s\t\tRight\tSeek +10s\r\n" +
                "J\tSeek -10s\t\tL\tSeek +10s\r\n\r\n" +

                "TRACK & VOLUME\r\n" +
                "------------------------------------------------------\r\n" +
                "Ctrl+Left\tPrevious Track\tCtrl+Right\tNext Track\r\n" +
                "Up\tVolume +5\t\tDown\tVolume -5\r\n" +
                "F6\tMute / Unmute\t\tDelete\tRemove Selected\r\n\r\n" +

                "PLAYLIST & FILES\r\n" +
                "------------------------------------------------------\r\n" +
                "Ctrl+O\tOpen Files\t\tCtrl+S\tSave Playlist\r\n" +
                "Ctrl+L\tLoad Playlist\t\tF4\tClear Playlist\r\n" +
                "F5\tRefresh List\t\tF7\tNow Playing Info\r\n" +
                "Drag\tDrop files or folders to load\r\n\r\n" +

                "A-B REPEAT & SPEED\r\n" +
                "------------------------------------------------------\r\n" +
                "A\tSet Point A\t\tB\tSet Point B\r\n" +
                "C\tClear A-B\t\tF3\tToggle Repeat One\r\n" +
                "0-9\tSpeed Preset (0.25x - 2.5x)\r\n" +
                "Ctrl+0..9\tJump to 0% - 90%\r\n\r\n" +

                "VIEW & MODES\r\n" +
                "------------------------------------------------------\r\n" +
                "F8\tToggle Shuffle\t\tF9\tToggle Mini Mode\r\n\r\n" +

                "GLOBAL HOTKEYS (Work even when app is not focused)\r\n" +
                "------------------------------------------------------\r\n" +
                "Media Keys\tNext / Prev / Stop / Play-Pause\r\n" +
                "Ctrl+Alt+<key>\tForward Seek (configure in F2)\r\n\r\n" +

                "OTHER\r\n" +
                "------------------------------------------------------\r\n" +
                "F1\tShow this Help\t\tF2\tSettings";

            using (Form helpForm = new Form())
            {
                helpForm.Text = "Keyboard Shortcuts";
                helpForm.BackColor = Color.Black;
                helpForm.ForeColor = Color.Orange;
                helpForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                helpForm.StartPosition = FormStartPosition.CenterParent;
                helpForm.MaximizeBox = false;
                helpForm.MinimizeBox = false;
                helpForm.ClientSize = new Size(600, 620);

                TextBox txtHelp = new TextBox
                {
                    Multiline = true,
                    ReadOnly = true,
                    Dock = DockStyle.Fill,
                    BackColor = Color.Black,
                    ForeColor = Color.Orange,
                    BorderStyle = BorderStyle.None,
                    Font = new Font("Consolas", 10F, FontStyle.Regular),
                    Text = help,
                    ScrollBars = ScrollBars.Vertical,
                    WordWrap = false,
                    TabStop = false
                };

                Button btnOk = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Dock = DockStyle.Bottom,
                    Height = 35,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Black,
                    ForeColor = Color.Orange
                };

                helpForm.Controls.Add(txtHelp);
                helpForm.Controls.Add(btnOk);
                helpForm.AcceptButton = btnOk;
                helpForm.ShowDialog(this);
            }
        }

        private void OpenSettingsDialog()
        {
            using (Form settingsForm = new Form())
            {
                settingsForm.Text = "Settings";
                settingsForm.BackColor = Color.Black;
                settingsForm.ForeColor = Color.Orange;
                settingsForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                settingsForm.StartPosition = FormStartPosition.CenterParent;
                settingsForm.MaximizeBox = false;
                settingsForm.MinimizeBox = false;
                settingsForm.ClientSize = new Size(320, 170);

                TableLayoutPanel layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 3,
                    Padding = new Padding(12),
                    BackColor = Color.Black
                };

                Label lblKey = new Label { Text = "Global Forward Key:", ForeColor = Color.Orange, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 3, 3) };
                ComboBox cmbKey = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
                foreach (Keys k in Enum.GetValues(typeof(Keys)))
                {
                    if (k >= Keys.A && k <= Keys.Z) cmbKey.Items.Add(k);
                }
                cmbKey.Items.Add(Keys.Right);
                cmbKey.Items.Add(Keys.Left);
                cmbKey.SelectedItem = _settings.ForwardKey;
                if (cmbKey.SelectedIndex < 0) cmbKey.SelectedIndex = 0;

                Label lblSeconds = new Label { Text = "Forward Seconds:", ForeColor = Color.Orange, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 3, 3) };
                NumericUpDown numSeconds = new NumericUpDown
                {
                    Minimum = 1,
                    Maximum = 120,
                    Value = Math.Max(1, Math.Min(120, _settings.ForwardSeconds)),
                    Width = 150
                };

                Button btnSave = new Button
                {
                    Text = "Save",
                    ForeColor = Color.Orange,
                    BackColor = Color.Black,
                    FlatStyle = FlatStyle.Flat
                };
                btnSave.Click += (s, e) =>
                {
                    _settings.ForwardKey = (Keys)cmbKey.SelectedItem;
                    _settings.ForwardSeconds = (int)numSeconds.Value;
                    try { _settings.Save(); } catch { }
                    try
                    {
                        UnregisterHotKey(this.Handle, HOTKEY_ID_FORWARD);
                        RegisterHotKey(this.Handle, HOTKEY_ID_FORWARD, MOD_CONTROL | MOD_ALT, (uint)_settings.ForwardKey);
                    }
                    catch { }
                    settingsForm.Close();
                };

                layout.Controls.Add(lblKey, 0, 0);
                layout.Controls.Add(cmbKey, 1, 0);
                layout.Controls.Add(lblSeconds, 0, 1);
                layout.Controls.Add(numSeconds, 1, 1);
                layout.Controls.Add(btnSave, 1, 2);

                settingsForm.Controls.Add(layout);
                settingsForm.ShowDialog(this);
            }
        }

        private class AppSettings
        {
            public Keys ForwardKey { get; set; } = Keys.Right;
            public int ForwardSeconds { get; set; } = 10;
            public string LastPlaylist { get; set; } = "";
            public string LastTrack { get; set; } = "";
            public double LastPosition { get; set; } = 0;

            private static string SettingsFilePath
            {
                get
                {
                    string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AudioPlayerApp");
                    Directory.CreateDirectory(dir);
                    return Path.Combine(dir, "settings.ini");
                }
            }

            public static AppSettings Load()
            {
                AppSettings settings = new AppSettings();
                try
                {
                    if (File.Exists(SettingsFilePath))
                    {
                        foreach (string line in File.ReadAllLines(SettingsFilePath))
                        {
                            string[] parts = line.Split(new[] { '=' }, 2);
                            if (parts.Length != 2) continue;

                            string key = parts[0].Trim();
                            string value = parts[1].Trim();

                            if (key == "ForwardKey" && Enum.TryParse(value, out Keys parsedKey)) settings.ForwardKey = parsedKey;
                            else if (key == "ForwardSeconds" && int.TryParse(value, out int parsedSeconds)) settings.ForwardSeconds = parsedSeconds;
                            else if (key == "LastPlaylist") settings.LastPlaylist = value;
                            else if (key == "LastTrack") settings.LastTrack = value;
                            else if (key == "LastPosition" && double.TryParse(value, out double pos)) settings.LastPosition = pos;
                        }
                    }
                }
                catch { }
                return settings;
            }

            public void Save()
            {
                string[] lines = {
                    "ForwardKey=" + ForwardKey,
                    "ForwardSeconds=" + ForwardSeconds,
                    "LastPlaylist=" + LastPlaylist,
                    "LastTrack=" + LastTrack,
                    "LastPosition=" + LastPosition
                };
                File.WriteAllLines(SettingsFilePath, lines);
            }
        }
    }
}