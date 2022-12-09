using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.TextFormatting;

namespace SATCalculator.Classes
{
    public class SAT3Formula :SATFormula
    {
        #region Fields


        #endregion


        #region Constructors

        public SAT3Formula()
        {

        }

        #endregion


        #region Methods

        /// <summary>
        /// Create an instance of SAT3Formula from a cnf file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static SAT3Formula GetFromFile(string filename) {

            SAT3Formula formula = new SAT3Formula();

            try
            {
                string[] lines = File.ReadAllLines(filename);

                foreach (string line in lines)
                {
                    var lineParts = line.Trim().Split(' ').ToList();

                    if (lineParts[0] == "c")
                        continue;

                    if (lineParts[0] == "p")
                        continue;

                    if (lineParts.Count() != 4)
                        continue;

                    formula.AddClause(lineParts);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


            return formula;
        }

        #endregion
    }

}
