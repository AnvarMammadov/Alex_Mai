using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using System.Windows;
using System.Windows.Media;

namespace Alex_Mai.Services
{
    public class AudioService
    {
        // Singleton Nümunəsi: Bütün proqramda yalnız bir AudioService olmasına zəmanət verir
        private static readonly Lazy<AudioService> lazy = new Lazy<AudioService>(() => new AudioService());
        public static AudioService Instance => lazy.Value;

        private MediaPlayer _bgmPlayer;
        private SoundPlayer _sfxPlayer;

        private AudioService()
        {
            _bgmPlayer = new MediaPlayer();
            _sfxPlayer = new SoundPlayer();
        }

        public void PlayBGM(string fileName, bool isLooping = true)
        {
            try
            {
                // Pack URI səs faylını proqramın resurslarından tapmaq üçün xüsusi bir ünvandır
                Uri uri = new Uri($"pack://application:,,,/Alex_Mai;component/Assets/Audio/BGM/{fileName}", UriKind.Absolute);
                _bgmPlayer.Open(uri);

                if (isLooping)
                {
                    _bgmPlayer.MediaEnded += (sender, e) =>
                    {
                        _bgmPlayer.Position = TimeSpan.Zero;
                        _bgmPlayer.Play();
                    };
                }

                _bgmPlayer.Play();
            }
            catch (Exception ex)
            {
                // Səs faylı tapılmadıqda və ya başqa xəta olduqda bura işləyəcək
                System.Diagnostics.Debug.WriteLine($"BGM xətası: {ex.Message}");
            }
        }

        public void StopBGM()
        {
            _bgmPlayer.Stop();
        }

        public void PlaySFX(string fileName)
        {
            try
            {
                Uri uri = new Uri($"pack://application:,,,/Alex_Mai;component/Assets/Audio/SFX/{fileName}", UriKind.Absolute);
                var streamInfo = Application.GetResourceStream(uri);
                _sfxPlayer.Stream = streamInfo.Stream;
                _sfxPlayer.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SFX xətası: {ex.Message}");
            }
        }
    }
}
