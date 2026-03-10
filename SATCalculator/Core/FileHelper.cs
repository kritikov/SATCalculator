using SATCalculator.Models;
using System;
using System.IO;
using System.Linq;

namespace SATCalculator.Core
{
    internal static class FileHelper
    {
        /// <summary>
        /// Save a formula as CNF in a file
        /// </summary>
        public static void SaveFormulaAsCNF(SATFormula formula, string filename)
        {
            if (formula == null)
                throw new ArgumentNullException(nameof(formula));
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("Filename cannot be null or empty.", nameof(filename));

            var lines = formula.GetCNFLines();
            using (var file = new StreamWriter(filename))
            {
                foreach (var line in lines)
                    file.WriteLine(line);
            }
        }

        /// <summary>
        /// Load a formula from a CNF file
        /// </summary>
        public static SATFormula GetFormulaFromCnfFile(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("Filename cannot be null or empty.", nameof(filename));

            if (!File.Exists(filename))
                throw new FileNotFoundException("CNF file not found.", filename);

            var lines = File.ReadAllLines(filename).ToList();
            return SATFormula.CreateFromCnfLines(lines);
        }
    }
}