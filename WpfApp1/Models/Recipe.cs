using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alex_Mai.Models
{
    public class Recipe
    {
        public string Name { get; set; }
        public List<string> IngredientOrder { get; set; } // Düzgün ardıcıllıq (Id-lərə görə)
        public List<Ingredient> AllIngredients { get; set; } // Ekranda göstəriləcək bütün ərzaqlar
        public Dictionary<int, string> DishImages { get; set; } // Hər mərhələ üçün şəkil (1: bowl.png, 2: bowl_with_flour.png)
    };
}
