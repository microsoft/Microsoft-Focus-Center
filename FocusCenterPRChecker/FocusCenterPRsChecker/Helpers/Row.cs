using System.Collections.Generic;
using System.Text;

namespace FocusCenterPRsChecker.Helpers
{
    public class Row
    {
        public List<Cell> Cells { get; } = new List<Cell>();

        public string ToHTML()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("<tr>");

            foreach(var cell in Cells)
            {
                stringBuilder.Append(cell.ToHtml());
            }

            stringBuilder.Append("</tr>");

            return stringBuilder.ToString();
        }
    }
}
