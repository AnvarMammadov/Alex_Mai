using Alex_Mai.ViewModels;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Alex_Mai.Views
{
    public partial class GameView : UserControl
    {
        // DƏYİŞİKLİK: Aktiv animasiyanı saxlamaq üçün yeni bir sahə (field) yaradırıq
        private Storyboard _activeFadeOutAnimation;

        public GameView()
        {
            InitializeComponent();
            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is GameViewModel vm)
            {
                vm.CharacterSpriteChanged += OnCharacterSpriteChanged;
            }
        }

        private void OnCharacterSpriteChanged(object sender, EventArgs e)
        {
            var fadeOutStoryboard = (Storyboard)this.Resources["FadeOutAnimation"];

            // DƏYİŞİKLİK: Klonu klass səviyyəsindəki dəyişənə mənimsədirik
            _activeFadeOutAnimation = fadeOutStoryboard.Clone();

            _activeFadeOutAnimation.Completed += FadeOutAnimation_Completed;
            _activeFadeOutAnimation.Begin(CharacterImage);
        }

        private void FadeOutAnimation_Completed(object sender, EventArgs e)
        {
            // DƏYİŞİKLİK: Artıq "sender"-dən istifadə etmirik, birbaşa saxladığımız klonla işləyirik
            if (_activeFadeOutAnimation != null)
            {
                _activeFadeOutAnimation.Completed -= FadeOutAnimation_Completed;
            }

            var fadeInStoryboard = (Storyboard)this.Resources["FadeInAnimation"];
            var fadeInAnimation = fadeInStoryboard.Clone();
            fadeInAnimation.Begin(CharacterImage);
        }

        // Dialoq atlatmaq üçün olan köhnə kodumuz
        private void OnGridMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext is GameViewModel vm)
            {
                vm.SkipDialogueAnimation();
            }
        }
    }
}