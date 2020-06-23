using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace StemSearch {
    public partial class Form1 : Form {
        Regex regexNewLines = new Regex(".*\n.*", RegexOptions.Compiled);
        Regex regexFormattingOne = new Regex("[ ]*long ([a-z0-9_]+)\\(long paramLong\\)\\s+\\{", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        Regex regexFormattingTwo = new Regex("(?:[\\r\\n]+\\s*){3,}", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        Regex regexSingleLineNumberReturn = new Regex("[ ]*long ([a-z0-9_]+)\\(long paramLong\\)\\s+\\{\\s+return (-?\\d+L);\\s+\\}", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        Regex regexReturnAssignment = new Regex("return paramLong = (.*?;)", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        Regex regexDirectFunctionCall = new Regex("[ ]*long ([a-z0-9_]+)\\(long paramLong\\)\\s+\\{\\s+return ([a-z0-9_]+)\\((?:paramLong|(\\d+L|0x[0-9a-f]+))\\);\\s+\\}", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        Regex regexAdditionSubtraction = new Regex("(-?\\d+L|0x[0-9a-f]+) ([+-]) (-?\\d+L|0x[0-9a-f]+)", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        Regex regexBinaryXor = new Regex("(?<![+-]) (-?\\d+L|0x[0-9a-f]+) \\^ (-?\\d+L|0x[0-9a-f]+)([ ;])(?![+-])", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        Regex regexFunctionCallWithAbsoluteValue = new Regex("([a-z][a-z0-9_]*)\\((-?\\d+L|0x[0-9a-f]+)\\)", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        Regex regexIfStyleValueReplacement = new Regex("if \\(-?\\w+ == 0L\\) {\\s+return (\\d+L|0x[0-9a-f]+);\\s+}\\s+return (\\d+L|0x[0-9a-f]+)(?: \\^ (-?\\d+L|0x[0-9a-f]+))?;", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

        public Form1() {
            InitializeComponent();
        }

        private void UpdateCounters() {
            lblLinesBefore.Text = "Lines of code before: " + (regexNewLines.Matches(txtBoxBefore.Text).Count + 1);
            lblLinesAfter.Text = "Lines of code after: " + (regexNewLines.Matches(txtBoxAfter.Text).Count + 1);
        }

        private void txtBoxBefore_TextChanged(object sender, EventArgs e) {
            UpdateCounters();
        }

        private void btnRun_Click(object sender, EventArgs e) {
            string previousCode = txtBoxBefore.Text;
            // formatting
            foreach (Match match in regexFormattingOne.Matches(previousCode)) {
                previousCode = previousCode.Replace(match.Groups[0].Value, "  long " + match.Groups[1].Value + "(long paramLong) {");
            }
            string nextCode = previousCode;

            do {
                previousCode = nextCode;
                Console.WriteLine("Iteration");
                // - * - = + ^^
                nextCode = nextCode.Replace("--", "");
                // functions that just return a single number
                foreach (Match match in regexSingleLineNumberReturn.Matches(nextCode)) {
                    Console.Write("#");
                    nextCode = Regex.Replace(nextCode, " " + match.Groups[1].Value + "\\((?!long).*?\\)", " " + match.Groups[2].Value);
                    nextCode = nextCode.Replace(match.Groups[0].Value, "");
                }
                // functions that have an assignment in their return statement
                foreach (Match match in regexReturnAssignment.Matches(nextCode)) {
                    Console.Write("#");
                    nextCode = nextCode.Replace(match.Groups[0].Value, "return " + match.Groups[1].Value);
                }
                // functions making a direct, unmodified call to another function
                Match matchDirectCall = regexDirectFunctionCall.Match(nextCode);
                while (matchDirectCall.Success) {
                    Console.Write("#");
                    if (!string.IsNullOrEmpty(matchDirectCall.Groups[3].Value)) {
                        nextCode = Regex.Replace(nextCode, " " + matchDirectCall.Groups[1].Value + "\\((?!long).*?\\)", " " + matchDirectCall.Groups[2].Value + "(" + matchDirectCall.Groups[3].Value + ")");
                    }
                    else {
                        nextCode = Regex.Replace(nextCode, " " + matchDirectCall.Groups[1].Value + "\\((?!long)", " " + matchDirectCall.Groups[2].Value + "(");
                    }
                    nextCode = nextCode.Replace(matchDirectCall.Groups[0].Value, "");
                    matchDirectCall = regexDirectFunctionCall.Match(nextCode);
                }
                // there is something we can actually calculate (+/-)
                foreach (Match match in regexAdditionSubtraction.Matches(nextCode)) {
                    Console.Write("#");
                    long valueA = parseLongDecOrHex(match.Groups[1].Value);
                    long valueB = parseLongDecOrHex(match.Groups[3].Value);
                    long result;
                    switch (match.Groups[2].Value) {
                        case "+":
                            result = valueA + valueB;
                            break;
                        case "-":
                            result = valueA - valueB;
                            break;
                        default:
                            throw new Exception("Encountered unknown mathmatical operator.");
                    }
                    nextCode = nextCode.Replace(match.Groups[0].Value, result + "L");
                }
                // XOR
                foreach (Match match in regexBinaryXor.Matches(nextCode)) {
                    Console.Write("#");
                    long valueA = parseLongDecOrHex(match.Groups[1].Value);
                    long valueB = parseLongDecOrHex(match.Groups[2].Value);
                    nextCode = nextCode.Replace(match.Groups[0].Value, " " + (valueA ^ valueB) + "L" + match.Groups[3].Value);
                }
                // function is called with an absolute value; propagate the value
                foreach (Match match in regexFunctionCallWithAbsoluteValue.Matches(nextCode)) {
                    string calledFunctionName = match.Groups[1].Value;
                    long absoluteValue = parseLongDecOrHex(match.Groups[2].Value);
                    // is it the only call to this function?
                    Regex regexCallsToSpecificFunction = new Regex(" " + calledFunctionName + "\\((?!long paramLong)[^)]+\\)", RegexOptions.Multiline | RegexOptions.Compiled);
                    if (regexCallsToSpecificFunction.Matches(nextCode).Count > 1) {
                        Console.WriteLine("-");
                        continue;
                    }
                    // find definition of function; only two kinds (return and if-else-return)
                    Regex regexSpecificFunctionDefinition = new Regex("[ ]*long " + calledFunctionName + "\\(long paramLong\\) {\\s+(if.+?{[^~]+?}\\s+return.+?;|return.+?;)\\s+}", RegexOptions.Multiline | RegexOptions.Compiled);
                    MatchCollection functionDefinitions = regexSpecificFunctionDefinition.Matches(nextCode);
                    // more than one definition?! no definition?
                    if (functionDefinitions.Count != 1) {
                        throw new Exception("Found more or less than exactly one function definition for '" + calledFunctionName + "'.");
                    }

                    foreach (Match definition in functionDefinitions) {
                        string functionBody = definition.Groups[1].Value;
                        string updatedFunctionBody = functionBody.Replace("paramLong", absoluteValue + "L");
                        // if-style function
                        if (functionBody.Contains("if (")) {
                            Match ifCondition = regexIfStyleValueReplacement.Match(updatedFunctionBody);
                            if (absoluteValue == 0) {
                                nextCode = nextCode.Replace(functionBody, "return " + ifCondition.Groups[1].Value + ";");
                            }
                            else {
                                if (!string.IsNullOrEmpty(ifCondition.Groups[3].Value)) {
                                    long valueA = parseLongDecOrHex(ifCondition.Groups[2].Value);
                                    long valueB = parseLongDecOrHex(ifCondition.Groups[3].Value);
                                    nextCode = nextCode.Replace(functionBody, "return " + (valueA ^ valueB) + "L;");
                                }
                                else {
                                    nextCode = nextCode.Replace(functionBody, "return " + ifCondition.Groups[2].Value + ";");
                                }
                            }
                        }
                        // return-style function
                        else {
                            nextCode = nextCode.Replace(definition.Groups[0].Value, "  long " + calledFunctionName + "(long paramLong) {\r\n    " + updatedFunctionBody + "\r\n  }");
                        }
                    }
                }
                Console.WriteLine();
                nextCode = nextCode.Replace("\r\n", " ");
                nextCode = nextCode.Replace("  ", " ");
            } while (previousCode.Length != nextCode.Length);

            // reduce newlines
            foreach (Match match in regexFormattingTwo.Matches(nextCode)) {
                nextCode = nextCode.Replace(match.Groups[0].Value, Environment.NewLine + "  " + Environment.NewLine + "  ");
            }
            txtBoxAfter.Text = nextCode;
            UpdateCounters();
        }

        private static long parseLongDecOrHex(string inputValue) {
            return inputValue.StartsWith("0x") ? long.Parse(inputValue.Substring(2, inputValue.Length - 2), System.Globalization.NumberStyles.HexNumber) : long.Parse(inputValue.Substring(0, inputValue.Length - 1));
        }

        private void Form1_Load(object sender, EventArgs e) {
            txtBoxBefore.Text = Properties.Resources.Branches;
            UpdateCounters();
            txtBoxAfter.AppendText("singleLineNumberReturns: " + regexSingleLineNumberReturn.Matches(txtBoxBefore.Text).Count + Environment.NewLine);
        }
    }
}
