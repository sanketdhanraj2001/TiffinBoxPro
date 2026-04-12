using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Application.DTOs.Customer
{
    public class CreateSubscriptionDto
    {
        public Guid PlanId { get; set; }
        public DateTime StartDate { get; set; }
        public List<MealSelectionDto> MealSelections { get; set; } = new();
    }

    public class MealSelectionDto
    {
        public Guid MenuItemId { get; set; }
        public List<DayOfWeek> DaysOfWeek { get; set; } = new();
    }
}
