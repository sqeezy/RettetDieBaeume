using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RettetDenWald.IO
{
    static class Utils
    {
        public static IEnumerable<string> RemoveComents(IEnumerable<string> lines)
        {
            var result = new List<string>();
            foreach (string s in lines)
            {
                result.Add(RemoveCommentFromLine(s));
            }
            return result.Select(s => s.Trim());
        }

        private static string RemoveCommentFromLine(string line)
        {
                if (line.Contains("%"))
                {
                    int indexOfComment = line.IndexOf("%", StringComparison.Ordinal);
                    return line.Remove(indexOfComment);
                }
            return line;
        }
    }
}
