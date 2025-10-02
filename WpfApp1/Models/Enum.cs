using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Alex_Mai.Models
{
    // Bu atribut, JSON-a enum-u rəqəmlərlə (0,1,2) yox, adları ilə ("Səhər", "Günorta") yazmağı deyir.
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TimeOfDay
    {
        Morning,
        Afternoon,
        Evening,
        Night
    }

}
