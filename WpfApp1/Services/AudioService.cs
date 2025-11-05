using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Alex_Mai.Models;
using Alex_Mai.Services;

namespace Alex_Mai.Services
{
    public class AudioService
    {
        private static readonly Lazy<AudioService> lazy = new Lazy<AudioService>(() => new AudioService());
        public static AudioService Instance => lazy.Value;

        private MediaPlayer _bgmPlayer;
        private GameSettings _settings;
        private readonly List<MediaPlayer> _liveSfxPlayers;

        // YENİ: Hazırda çalınan musiqinin adını yadda saxlayırıq
        private string _currentBgmFile = "";

        private AudioService()
        {
            _bgmPlayer = new MediaPlayer();
            _liveSfxPlayers = new List<MediaPlayer>();

            _settings = SettingsService.Instance.GetSettings();
            _settings.PropertyChanged += OnSettingsChanged;

            UpdateBgmVolume();
        }

        private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GameSettings.MasterVolume) ||
                e.PropertyName == nameof(GameSettings.MusicVolume))
            {
                UpdateBgmVolume();
            }
        }

        private void UpdateBgmVolume()
        {
            if (_settings == null || _bgmPlayer == null) return;

            double masterVol = _settings.MasterVolume / 100.0;
            double musicVol = _settings.MusicVolume / 100.0;
            _bgmPlayer.Volume = masterVol * musicVol;
        }

        public void PlayBGM(string fileName, bool isLooping = true)
        {
            try
            {
                // DƏYİŞİKLİK: Əgər eyni musiqi *və* artıq çalınırsa, heç nə etmə.
                if (_currentBgmFile == fileName && _bgmPlayer.Position > TimeSpan.Zero)
                {
                    return;
                }

                _bgmPlayer.Stop();
                _currentBgmFile = fileName; // YENİ: Musiqinin adını yadda saxla

                Uri uri = new Uri($"pack://siteoforigin:,,,/Assets/Audio/BGM/{fileName}", UriKind.Absolute);

                UpdateBgmVolume();

                _bgmPlayer.MediaOpened -= BgmPlayer_MediaOpened;
                _bgmPlayer.MediaOpened += BgmPlayer_MediaOpened;

                _bgmPlayer.MediaEnded -= OnBgmLoop;
                if (isLooping)
                {
                    _bgmPlayer.MediaEnded += OnBgmLoop;
                }

                _bgmPlayer.MediaFailed += (s, e) => { System.Diagnostics.Debug.WriteLine($"BGM XƏTASI (MediaFailed): {e.ErrorException.Message}"); };

                _bgmPlayer.Open(uri);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BGM XƏTASI (Open): {ex.Message}");
            }
        }

        private void BgmPlayer_MediaOpened(object sender, EventArgs e)
        {
            try
            {
                _bgmPlayer.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BGM XƏTASI (Play): {ex.Message}");
            }
        }

        private void OnBgmLoop(object sender, EventArgs e)
        {
            _bgmPlayer.Position = TimeSpan.Zero;
            _bgmPlayer.Play();
        }

        public void StopBGM()
        {
            _bgmPlayer.Stop();
            _currentBgmFile = ""; // DƏYİŞİKLİK: Musiqi dayandıqda yaddaşı təmizlə
        }

        public void PlaySFX(string fileName)
        {
            try
            {
                MediaPlayer sfxPlayer = new MediaPlayer();

                Uri uri = new Uri($"pack://siteoforigin:,,,/Assets/Audio/SFX/{fileName}", UriKind.Absolute);

                double masterVol = _settings.MasterVolume / 100.0;
                double sfxVol = _settings.SfxVolume / 100.0;
                sfxPlayer.Volume = masterVol * sfxVol;

                sfxPlayer.MediaEnded += (sender, e) =>
                {
                    MediaPlayer mp = sender as MediaPlayer;
                    if (mp != null)
                    {
                        mp.Close();
                        _liveSfxPlayers.Remove(mp);
                    }
                };

                sfxPlayer.MediaFailed += (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine($"SFX XƏTASI (MediaFailed): {e.ErrorException.Message} - Fayl: {fileName}");
                    MediaPlayer mp = s as MediaPlayer;
                    if (mp != null)
                    {
                        mp.Close();
                        _liveSfxPlayers.Remove(mp);
                    }
                };

                _liveSfxPlayers.Add(sfxPlayer);

                sfxPlayer.Open(uri);
                sfxPlayer.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SFX XƏTASI (Open): {ex.Message} - Fayl: {fileName}");
            }
        }
    }
}