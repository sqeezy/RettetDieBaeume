using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RettetDenWald.IO
{
    internal static class FileValidator
    {
        public static ValidationResult Validate(string path)
        {
            if (!File.Exists(path))
            {
                return ValidationResult.NoFileFound;
            }

            var lines = new List<string>(File.ReadAllLines(path));

            lines =  new List<string>(Utils.RemoveComents(lines));

            var result = ValidationResult.Ok;

            if (lines.Count() < 3)
            {
                result = ValidationResult.WrongFormat;
            }

            try
            {
                string[] lineOneSplit = lines[1].Split(' ');
                double dummy;
                foreach (string s in lineOneSplit)
                {
                    double.TryParse(s, out dummy);
                    if (dummy <= 0)
                    {
                        result = ValidationResult.WrongFormat;
                    }
                }

                for (int i = 2; i < lines.Count; i++)
                {
                    double.TryParse(lines[i], out dummy);
                    if (dummy <= 0)
                    {
                        result = ValidationResult.WrongFormat;
                    }
                }
            }
            catch (Exception)
            {
                result = ValidationResult.WrongFormat;
            }

            return result;
        }

    }

    internal enum ValidationResult
    {
        Ok,
        WrongFormat,
        NoFileFound
    }
}