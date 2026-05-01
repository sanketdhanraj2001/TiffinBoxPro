using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Domain.ValueObjects
{
    public class MealSelection
    {
        public int MenuItemId { get; private set; }
        public List<DayOfWeek> DaysOfWeek { get; private set; }

        public MealSelection(int menuItemId, List<DayOfWeek> daysOfWeek)
        {
            MenuItemId = menuItemId;
            DaysOfWeek = daysOfWeek ?? new List<DayOfWeek>();
        }

        public bool IsSelectedForDay(DayOfWeek day)
        {
            return DaysOfWeek.Contains(day);
        }

        public void AddDay(DayOfWeek day)
        {
            if (!DaysOfWeek.Contains(day))
                DaysOfWeek.Add(day);
        }

        public void RemoveDay(DayOfWeek day)
        {
            DaysOfWeek.Remove(day);
        }
    }
}
