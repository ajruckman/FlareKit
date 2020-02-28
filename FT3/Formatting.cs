#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FT3
{
    // ReSharper disable once ClassCanBeSealed.Global
    public partial class FlareTable<T>
    {
        public string AsCSV()
        {
            StringBuilder result = new StringBuilder();

            IEnumerable<Column> columns = Columns.Where(v => v.Shown).ToList();

            if (!columns.Any()) return "";

            List<string> headings = columns.Select(v => StringToCSVCell(v.DisplayName)).ToList();
            result.AppendLine(string.Join(',', headings));

            foreach (T row in AllRows())
            {
                List<string> line = columns.Select(column => StringToCSVCell(RowValue(row, column.ID) ?? "")).ToList();
                result.AppendLine(string.Join(',', line));
            }

            return result.ToString();
        }

        // https://stackoverflow.com/a/6377656/9911189
        private static string StringToCSVCell(string str)
        {
            bool mustQuote = str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n");
            if (mustQuote)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\"");
                foreach (char nextChar in str)
                {
                    sb.Append(nextChar);
                    if (nextChar == '"')
                        sb.Append("\"");
                }

                sb.Append("\"");
                return sb.ToString();
            }

            return str;
        }
    }
}