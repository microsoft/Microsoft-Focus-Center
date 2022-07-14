using FocusCenterPRsChecker.Managers;

namespace FocusCenterPRsChecker.Helpers
{
    public class Cell
    {
        private string _value;

        public Cell(string value)
        {

            _value = value;
        }
        public Cell()
        {
            _value = string.Empty;
        }

        public string ToHtml()
        {
            return $"<td style='padding:{TableStyleSettings.PaddingStyle}'>{_value}</td>";
        }
    }
}
