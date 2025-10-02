using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alex_Mai.Models;
using Alex_Mai.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Alex_Mai.ViewModels
{
    public partial class CookingGameViewModel : ObservableObject
    {
        private readonly GameViewModel _parentViewModel;
        private readonly RecipeService _recipeService;
        private Recipe _currentRecipe;
        private int _currentStep = 0;

        [ObservableProperty] private string _dishImagePath;
        [ObservableProperty] private bool _isCookingScreenVisible = false;
        public ObservableCollection<Recipe> AvailableRecipes { get; set; }
        public ObservableCollection<Ingredient> CurrentIngredients { get; set; }


        public CookingGameViewModel(GameViewModel parent)
        {
            _parentViewModel = parent;
            _recipeService = new RecipeService("Data/recipes.json");
            AvailableRecipes = new ObservableCollection<Recipe>(_recipeService.GetAllRecipes());
            CurrentIngredients = new ObservableCollection<Ingredient>();
        }


        [RelayCommand]
        private void SelectRecipe(Recipe recipe)
        {
            _currentRecipe = recipe;
            _currentStep = 0;

            CurrentIngredients.Clear();
            foreach (var ingredient in _currentRecipe.AllIngredients)
            {
                CurrentIngredients.Add(ingredient);
            }
            DishImagePath = _currentRecipe.DishImages[0];
            IsCookingScreenVisible = true;
        }


        [RelayCommand]
        private async Task AddIngredient(Ingredient ingredient) // DİQQƏT: Metod "async Task" oldu
        {
            if (ingredient.Id == _currentRecipe.IngredientOrder[_currentStep])
            {
                // DÜZGÜN ADDIM
                _currentStep++;
                DishImagePath = _currentRecipe.DishImages[_currentStep];

                if (_currentStep >= _currentRecipe.IngredientOrder.Count)
                {
                    // UĞURLU SON
                    await Task.Delay(1000); // 1 saniyə son şəkli göstərmək üçün gözləyirik
                    _parentViewModel.EndCookingMinigame(true);
                    _currentStep = 0; // Oyunu sıfırla
                }
            }
            else
            {
                // SƏHV ADDIM
                DishImagePath = "/Assets/Sprites/dish_fail.png";
                await Task.Delay(1500); // 1.5 saniyə yanan yemək şəklini göstəririk
                _parentViewModel.EndCookingMinigame(false);
                _currentStep = 0; // Oyunu sıfırla
            }
        }

        public void Reset()
        {
            IsCookingScreenVisible = false;
            _currentStep = 0;
            _currentRecipe = null;
        }

    }
}
