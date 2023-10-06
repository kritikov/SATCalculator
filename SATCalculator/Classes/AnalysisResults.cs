using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes
{
    public class AnalysisResults
    {
        #region VARIABLES AND NESTED CLASSES

        public List<SelectionStep> SelectionSteps { get; set; } = new List<SelectionStep>();

        public Dictionary<Variable, EndVariableAppearances> EndVariablesAppearancesDict { get; set; } = new Dictionary<Variable, EndVariableAppearances>();

        public List<string> Problems { get; set; } = new List<string>();

        public DataTable EndVariableAppearancesTable { get; set; }

        public DataTable ConflictsTable { get; set; }


        #endregion


        #region Constructors

        public AnalysisResults()
        {

        }

        #endregion


        #region Methods

        /// <summary>
        /// Ananyze the formula based on algorithm 1
        /// </summary>
        public static AnalysisResults Analyze(SATFormula originalformula)
        {
            // create a copy from the formula to be unchanged the original
            SATFormula formula = originalformula.Copy();

            AnalysisResults analysisResults = new AnalysisResults();

            Dictionary<Variable, Indexes> variableIndexes = new Dictionary<Variable, Indexes>();
            //analysisResults.VariablesCount = editFormula.VariablesCount;

            // check the number of the variables per clause

            // get a list with the variables sorted by the contrasts
            //var variablesSequence = formula.Variables.Select(p => p).OrderByDescending(p => p.Contrasts).ToList();
            var variablesSequence = formula.Variables.Select(p => p).OrderBy(p => p.CnfIndex).ToList();

            #region Create the selection steps

            foreach (var variable in variablesSequence)
            {
                SelectionStep selectionStep = new SelectionStep();
                selectionStep.Variable = variable;

                // get the clauses with the positive appearances, remove the positive appearances of the variable
                // and add the reviewed clauses in the proper list
                foreach (var clause in variable.PositiveLiteral.ClausesContainingIt)
                {
                    Clause reducedClause = new Clause();

                    foreach (var literal in clause.Literals)
                    {
                        if (literal.Variable != variable)
                        {
                            literal.ClausesContainingIt.Remove(clause);
                            reducedClause.Literals.Add(literal);
                        }
                    }

                    selectionStep.ClausesWhenNegativeIsTrue.Add(reducedClause);
                }

                // get the clauses with the negative appearances, remove the negative appearances of the variable
                // and add the reviewed clauses in the proper list
                foreach (var clause in variable.NegativeLiteral.ClausesContainingIt)
                {
                    Clause reducedClause = new Clause();

                    foreach (var literal in clause.Literals)
                    {
                        if (literal.Variable != variable)
                        {
                            literal.ClausesContainingIt.Remove(clause);
                            reducedClause.Literals.Add(literal);
                        }
                    }

                    selectionStep.ClausesWhenPositiveIsTrue.Add(reducedClause);
                }

                Clause reducedClause1 = new Clause();
                reducedClause1.Literals.Add(variable.PositiveLiteral);
                selectionStep.ClausesWhenPositiveIsTrue.Add(reducedClause1);

                Clause reducedClause2 = new Clause();
                reducedClause2.Literals.Add(variable.NegativeLiteral);
                selectionStep.ClausesWhenNegativeIsTrue.Add(reducedClause2);

                //if (selectionStep.ClausesWhenPositiveIsTrue.Count == 0)
                //{
                //    Clause reducedClause = new Clause();
                //    reducedClause.Literals.Add(variable.PositiveLiteral);
                //    selectionStep.ClausesWhenPositiveIsTrue.Add(reducedClause);
                //}

                //if (selectionStep.ClausesWhenNegativeIsTrue.Count == 0)
                //{
                //    Clause reducedClause = new Clause();
                //    reducedClause.Literals.Add(variable.NegativeLiteral);
                //    selectionStep.ClausesWhenNegativeIsTrue.Add(reducedClause);
                //}

                analysisResults.SelectionSteps.Add(selectionStep);
            }

            #endregion

            #region Create the list with the end-variables appearances

            foreach (var selectionStep in analysisResults.SelectionSteps)
            {
                foreach (var clauseWhenPositiveIsTrue in selectionStep.ClausesWhenPositiveIsTrue)
                {
                    if (clauseWhenPositiveIsTrue.Literals.Count == 1)
                    {
                        Literal literal = clauseWhenPositiveIsTrue.Literals[0];

                        EndVariableAppearances endVariableAppearances;
                        if (analysisResults.EndVariablesAppearancesDict.ContainsKey(literal.Variable))
                        {
                            endVariableAppearances = analysisResults.EndVariablesAppearancesDict[literal.Variable];
                        }
                        else
                        {
                            endVariableAppearances = new EndVariableAppearances();
                            endVariableAppearances.Variable = literal.Variable;
                            analysisResults.EndVariablesAppearancesDict.Add(literal.Variable, endVariableAppearances);
                        }

                        if (literal.Sign == Sign.Positive)
                            endVariableAppearances.PositiveAppearances.Add(selectionStep.Variable.PositiveLiteral);
                        else
                            endVariableAppearances.NegativeAppearances.Add(selectionStep.Variable.PositiveLiteral);
                    }
                }

                foreach (var negativeClause in selectionStep.ClausesWhenNegativeIsTrue)
                {
                    if (negativeClause.Literals.Count == 1)
                    {
                        Literal literal = negativeClause.Literals[0];

                        EndVariableAppearances endVariableAppearances;
                        if (analysisResults.EndVariablesAppearancesDict.ContainsKey(literal.Variable))
                        {
                            endVariableAppearances = analysisResults.EndVariablesAppearancesDict[literal.Variable];
                        }
                        else
                        {
                            endVariableAppearances = new EndVariableAppearances();
                            endVariableAppearances.Variable = literal.Variable;
                            analysisResults.EndVariablesAppearancesDict.Add(literal.Variable, endVariableAppearances);
                        }
                        if (literal.Sign == Sign.Positive)
                            endVariableAppearances.PositiveAppearances.Add(selectionStep.Variable.NegativeLiteral);
                        else
                            endVariableAppearances.NegativeAppearances.Add(selectionStep.Variable.NegativeLiteral);
                    }
                }
            }

            #endregion

            #region Problems List

            // Create the problems list
            foreach (var endVariableDict in analysisResults.EndVariablesAppearancesDict)
            {
                foreach (var positiveAppearance in endVariableDict.Value.PositiveAppearances)
                {
                    foreach (var negativeAppearance in endVariableDict.Value.NegativeAppearances)
                    {
                        string problem = $"When {positiveAppearance.Name}=A and {negativeAppearance.Name}=A then contrast at {endVariableDict.Value.Variable.Name}";
                        analysisResults.Problems.Add(problem);
                    }
                }
            }

            #endregion

            #region Create the end variable appearances datatable to display the results

            analysisResults.EndVariableAppearancesTable = new DataTable();
            analysisResults.EndVariableAppearancesTable.Columns.Add("", typeof(string));

            // create the columns
            int columnIndex = 1;
            foreach (var variable in variablesSequence)
            {
                analysisResults.EndVariableAppearancesTable.Columns.Add(variable.Name, typeof(string), "");
                analysisResults.EndVariableAppearancesTable.Columns.Add("-" + variable.Name, typeof(string), "");

                Indexes indexes = new Indexes();
                indexes.ColumnIndex = columnIndex;
                variableIndexes.Add(variable, indexes);

                columnIndex += 2;
            }

            // fill the table
            foreach (var endVariableAppearancesDict in analysisResults.EndVariablesAppearancesDict)
            {
                DataRow row = analysisResults.EndVariableAppearancesTable.NewRow();
                row[0] = endVariableAppearancesDict.Value.Variable.Name;

                foreach (var endVariablePositiveAppearance in endVariableAppearancesDict.Value.PositiveAppearances)
                {
                    columnIndex = variableIndexes[endVariablePositiveAppearance.Variable].ColumnIndex;
                    if (endVariablePositiveAppearance.Sign == Sign.Negative)
                        columnIndex++;

                    row[columnIndex] = "+";
                }

                foreach (var endVariableNegativeAppearance in endVariableAppearancesDict.Value.NegativeAppearances)
                {
                    columnIndex = variableIndexes[endVariableNegativeAppearance.Variable].ColumnIndex;
                    if (endVariableNegativeAppearance.Sign == Sign.Negative)
                        columnIndex++;

                    row[columnIndex] = "-";
                }

                analysisResults.EndVariableAppearancesTable.Rows.Add(row);
            }

            #endregion


            #region Create the conflicts datatable to display the results

            analysisResults.ConflictsTable = new DataTable();
            analysisResults.ConflictsTable.Columns.Add("", typeof(string));

            // create the columns
            foreach (var variable in variablesSequence)
            {
                analysisResults.ConflictsTable.Columns.Add(variable.Name, typeof(string), "");
                analysisResults.ConflictsTable.Columns.Add("-" + variable.Name, typeof(string), "");
            }

            // create the rows
            foreach (var variable in variablesSequence)
            {
                DataRow rowPos = analysisResults.ConflictsTable.NewRow();
                rowPos[0] = variable.PositiveLiteral.Name;
                analysisResults.ConflictsTable.Rows.Add(rowPos);

                DataRow rowNeg = analysisResults.ConflictsTable.NewRow();
                rowNeg[0] = variable.NegativeLiteral.Name;
                analysisResults.ConflictsTable.Rows.Add(rowNeg);
            }

            // fill the table
            foreach (var endVariableAppearancesDict in analysisResults.EndVariablesAppearancesDict)
            {
                foreach (var positiveAppearance in endVariableAppearancesDict.Value.PositiveAppearances)
                {
                    columnIndex = variableIndexes[positiveAppearance.Variable].ColumnIndex;
                    if (positiveAppearance.Sign == Sign.Negative)
                        columnIndex++;

                    foreach (var negativeAppearance in endVariableAppearancesDict.Value.NegativeAppearances)
                    {
                        // forward
                        int rowIndex = variableIndexes[negativeAppearance.Variable].ColumnIndex-1;
                        //int rowIndex = negativeAppearance.Variable.SequenceIndex - 1;
                        if (negativeAppearance.Sign == Sign.Negative)
                            rowIndex++;

                        DataRow row = analysisResults.ConflictsTable.Rows[rowIndex];
                        row[columnIndex] = "x";
                        //if (positiveAppearance.Sign == Sign.Negative)
                        //    row[columnIndex - 1] = "v";
                        //else
                        //    row[columnIndex + 1] = "v";

                        // backward
                        int rowIndex2 = columnIndex - 1;
                        int columnIndex2 = rowIndex + 1;
                        row = analysisResults.ConflictsTable.Rows[rowIndex2];
                        row[columnIndex2] = "x";
                        //if (negativeAppearance.Sign == Sign.Negative)
                        //    row[columnIndex2 - 1] = "v";
                        //else
                        //    row[columnIndex2 + 1] = "v";
                    }
                }
            }

            #endregion

            //#region Create the conflicts list

            //ConflictsDict
            //foreach (var item in analysisResults.EndVariablesDict)
            //{
            //    foreach (var positiveAppearance in item.Value.PositiveAppearances)
            //    {
            //        List<VariableSign> variableSignList1;
            //        if (analysisResults.ConflictsDict.ContainsKey(positiveAppearance))
            //        {
            //            variableSignList1 = analysisResults.ConflictsDict[positiveAppearance];
            //        }
            //        else
            //        {
            //            variableSignList1 = new List<VariableSign>();
            //            analysisResults.ConflictsDict.Add(positiveAppearance, variableSignList1);
            //        }

            //        foreach (var negativeAppearance in item.Value.NegativeAppearances)
            //        {
            //            List<VariableSign> variableSignList2;
            //            if (analysisResults.ConflictsDict.ContainsKey(negativeAppearance))
            //            {
            //                variableSignList2 = analysisResults.ConflictsDict[negativeAppearance];
            //            }
            //            else
            //            {
            //                variableSignList2 = new List<VariableSign>();
            //                analysisResults.ConflictsDict.Add(negativeAppearance, variableSignList2);
            //            }

            //            variableSignList1.Add(negativeAppearance);
            //            variableSignList2.Add(positiveAppearance);
            //        }
            //    }
            //}

            //#endregion


            return analysisResults;
        }


        #endregion
    }

    public class SelectionStep
    {
        public Variable Variable { get; set; } = new Variable();
        public List<Clause> ClausesWhenPositiveIsTrue { get; set; } = new List<Clause>();
        public List<Clause> ClausesWhenNegativeIsTrue { get; set; } = new List<Clause>();
    }

    public class EndVariableAppearances
    {
        public Variable Variable { get; set; }
        public List<Literal> PositiveAppearances = new List<Literal>();
        public List<Literal> NegativeAppearances = new List<Literal>();
    }

   
    public class Indexes
    {
        public int SequenceIndex = 0;
        public int ColumnIndex = 0;
        public int RowIndex = 0;
    }

}
