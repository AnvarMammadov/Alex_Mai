using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alex_Mai.Models
{
    public enum TransactionType
    {
        Income, // Gəlir
        Expense // Xərc
    }

    public class Transaction
    {
        public string Description { get; set; } // Məsələn, "Market alış-verişi", "Part-time iş qazancı"
        public int Amount { get; set; } // Məbləğ (həmişə müsbət saxlanılır, növü ilə ayrılır)
        public TransactionType Type { get; set; } // Gəlir yoxsa Xərc
        public DateTime Timestamp { get; set; } // Əməliyyat vaxtı
        public int BalanceAfter { get; set; } // Bu əməliyyatdan sonrakı balans (opsional)

        // Göstərmək üçün formatlanmış məbləğ (məsələn, "+$60" və ya "-$20")
        public string FormattedAmount => $"{(Type == TransactionType.Income ? "+" : "-")}{Amount}$";
    }
}
