using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alex_Mai.Models;

namespace Alex_Mai.Models
{
    public class SaveData
    {
        public GameState GameState { get; set; }
        public CharacterStats CharacterStats { get; set; }
        // Gələcəkdə bura inventar, kilidi açılmış hadisələr və s. əlavə edə bilərik
    }
}
