// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using FocusCenterPRChecker.Managers;

namespace FocusCenterPRChecker.Helpers
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
