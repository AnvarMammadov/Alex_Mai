using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Alex_Mai.Models;

namespace Alex_Mai.Services
{
    public class RecipeService
    {
        private readonly List<Recipe> _recipes;

        public RecipeService(string filePath)
        {
            var jsonText = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _recipes = JsonSerializer.Deserialize<List<Recipe>>(jsonText, options);
        }

        public List<Recipe> GetAllRecipes()
        {
            return _recipes;
        }
    }
}