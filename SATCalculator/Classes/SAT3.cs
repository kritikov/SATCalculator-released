using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes {
    
    

    public class SAT3 {

        #region VARIABLES AND NESTED CLASSES

        public class Variable {
            internal string Identifier = "";
            internal int Index = 0;
            internal int UnsignedAppearances = 0;
            internal int NativeAppearances = 0;
            internal int ComplementaryAppearances = 0;
            internal bool IsComplementary = false;
            internal string DisplayName {
                get {
                    return !IsComplementary ? Identifier : Identifier + "'";
                }
            }

            internal bool HasBothAppearances {
                get {
                    return NativeAppearances > 0 && ComplementaryAppearances > 0;
                }
            }

            public Variable() {

            }

            public Variable(string identifier, int index = 0, bool isComplementary = false) {
                this.Identifier = identifier;
                this.Index = index;
                this.IsComplementary = isComplementary;
            }
        }


        public class Phrase {
            internal Variable[] Variables;

            public Phrase(string x1, string x2, string x3) {
                Variables = new Variable[3] { new Variable(x1), new Variable(x2), new Variable(x3) };
            }

            public override string ToString() {
                string value = $@"({Variables[0].DisplayName} ∨ {Variables[1].DisplayName} ∨ {Variables[2].DisplayName})";

                return value;
            }

        }

        private List<Phrase> phrases = new List<Phrase>();

        private List<string> analysisResults = new List<string>();
        public List<string> AnalysisResults {
            get => analysisResults;
        }

        #endregion


        #region CONSTRUCTORS

        public SAT3() {
            
        }

        #endregion


        #region METHODS

        /// <summary>
        /// add a phrase with 3 variables in the formula
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="x3"></param>
        public void Add(string x1, string x2, string x3) {

            phrases.Add(new Phrase(x1, x2, x3));
        }
        public void Add(Phrase phrase) {

            phrases.Add(phrase);
        }

        // remove a phrase from the list
        public void Remove(Phrase phrase) {

            phrases.Remove(phrase);
        }

        public override string ToString() {
            string value = "";

            foreach(Phrase phrase in phrases) {
                if (value != "") {
                    value += " ^ ";
                }
                value += phrase.ToString();
            }

            return value;
        }


        /// <summary>
        /// analyse the formula and tells if it can be true
        /// </summary>
        /// <returns></returns>
        public List<string> Analyze() {
            analysisResults.Clear();

            SAT3 alteredFormula = new SAT3();
            Dictionary<string, Variable> variablesByIdentifier = new Dictionary<string, Variable>();
            Dictionary<int, Variable> variablesByIndex = new Dictionary<int, Variable>();
            string results;
            int n = 0, m = 0;

            // DEFINE BASE SIZES
            int identifierIndex = 0;
            foreach (Phrase phrase in phrases) {

                int i;
                for (i = 0; i < 3; i++) {

                    Variable variable = new Variable(phrase.Variables[i].Identifier);

                    // check if the variable is simple or complementary
                    if (variable.Identifier.EndsWith("'")) {
                        variable.Identifier = variable.Identifier.Substring(0, variable.Identifier.Length - 1);
                        variable.IsComplementary = true;
                    }

                    // if we meet the variable for first time then add it in the list
                    if (!variablesByIdentifier.ContainsKey(variable.Identifier)) {

                        // set numeric identifier for the variable
                        variable.Index = identifierIndex;

                        // define the appearances
                        variable.UnsignedAppearances = 1;
                        if (variable.IsComplementary) {
                            variable.ComplementaryAppearances = 1;
                        }
                        else {
                            variable.NativeAppearances = 1;
                        }

                        // add the variable in the lists
                        variablesByIdentifier.Add(variable.Identifier, variable);
                        variablesByIndex.Add(identifierIndex, variable);

                        // increase index
                        identifierIndex++;
                    }
                    else {
                        variablesByIdentifier[variable.Identifier].UnsignedAppearances++;

                        if (variable.IsComplementary) {
                            variablesByIdentifier[variable.Identifier].ComplementaryAppearances++;
                        }
                        else {
                            variablesByIdentifier[variable.Identifier].NativeAppearances++;
                        }

                        variable = variablesByIdentifier[variable.Identifier];
                    }

                    // update the variable in the formula
                    phrase.Variables[i].IsComplementary = variable.IsComplementary;
                    phrase.Variables[i].Index = variable.Index;
                }
            }
            // add the list with the unique variables in the results
            results = $"STEP 1: define base sizes";
            analysisResults.Add(results);
            m = phrases.Count;
            results = $"number of phrases m = {m}";
            analysisResults.Add(results);
            n = variablesByIdentifier.Count;
            results = $"number of variables n = {n}";
            analysisResults.Add(results);
            results = "unique variables: " + string.Join(", ", variablesByIdentifier.Select(p => p.Key).ToArray());
            analysisResults.Add(results);
            results = $"step complexity = O(3m) = O(m)";
            analysisResults.Add(results);
            results = $"next formula: {ToString()}";
            analysisResults.Add(results);
            results = $"------------------------------------------------------------------------------------------------------------";
            analysisResults.Add(results);


            // REMOVE EQUAL PHRASES
            int[,,] variablesExistence = new int[n,n,n];
            SAT3 rejectedPhrases = new SAT3();

            foreach (Phrase phrase in this.phrases) {
                if (variablesExistence[phrase.Variables[0].Index, phrase.Variables[1].Index, phrase.Variables[2].Index] == 1 ||
                    variablesExistence[phrase.Variables[0].Index, phrase.Variables[2].Index, phrase.Variables[1].Index] == 1 ||
                    variablesExistence[phrase.Variables[1].Index, phrase.Variables[2].Index, phrase.Variables[0].Index] == 1 ||
                    variablesExistence[phrase.Variables[1].Index, phrase.Variables[0].Index, phrase.Variables[2].Index] == 1 ||
                    variablesExistence[phrase.Variables[2].Index, phrase.Variables[0].Index, phrase.Variables[1].Index] == 1 ||
                    variablesExistence[phrase.Variables[2].Index, phrase.Variables[1].Index, phrase.Variables[0].Index] == 1
                    ) {

                    // the phrase allready exists
                    rejectedPhrases.Add(phrase);
                }
                else {
                    variablesExistence[phrase.Variables[0].Index, phrase.Variables[1].Index, phrase.Variables[2].Index] = 1;
                    alteredFormula.Add(phrase);
                }
            }

            results = $"STEP 2: remove equal phrases";
            analysisResults.Add(results);
            results = $"removed duplicate phrases: {rejectedPhrases}";
            analysisResults.Add(results);
            results = $"step complexity = O(m)";
            analysisResults.Add(results);
            m = alteredFormula.phrases.Count;
            results = $"new m = {m}";
            analysisResults.Add(results);
            results = $"next formula: {alteredFormula}";
            analysisResults.Add(results);
            results = $"------------------------------------------------------------------------------------------------------------";
            analysisResults.Add(results);


            // REMOVE SATISFACTORY PHRASES
            SAT3 satisfactoryPhrases = new SAT3();
            SAT3 alteredFormula2 = new SAT3();

            // if a phrase contains a variable that appears in the whole
            // formula always with the same sign the exclude the phrase
            // because this variable satisfy it
            foreach (Phrase phrase in alteredFormula.phrases) {
                if (!variablesByIndex[phrase.Variables[0].Index].HasBothAppearances ||
                    !variablesByIndex[phrase.Variables[1].Index].HasBothAppearances ||
                    !variablesByIndex[phrase.Variables[2].Index].HasBothAppearances
                    ) {

                    // the phrase contains at least a variable that can satisfy it
                    satisfactoryPhrases.Add(phrase);
                }
                else {
                    alteredFormula2.Add(phrase);
                }
            }

            results = $"STEP 3: remove phrases that contains unique variables";
            analysisResults.Add(results);
            results = $"satisfactory phrases: {satisfactoryPhrases}";
            analysisResults.Add(results);
            results = $"step complexity = O(m)";
            analysisResults.Add(results);
            m = alteredFormula.phrases.Count;
            results = $"new m = {m}";
            analysisResults.Add(results);
            results = $"next formula: {alteredFormula2}";
            analysisResults.Add(results);
            results = $"------------------------------------------------------------------------------------------------------------";
            analysisResults.Add(results);



            return AnalysisResults;
        }

        #endregion

    }
}
