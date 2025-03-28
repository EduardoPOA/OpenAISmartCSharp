﻿using Eduardo.OpenAISmartTest.Options;
using Microsoft.VisualStudio.Shell;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Eduardo.OpenAISmartTest.Utils
{
    /// <summary>
    /// Internal static utility class containing methods for text formatting. 
    /// </summary>
    internal static class TextFormat
    {
        public static string GetLanguage { get; set; }
        public static string GetCommentChars(string filePath)
        {
            string extension = System.IO.Path.GetExtension(filePath).TrimStart('.');

            if (extension.Equals("cs", StringComparison.InvariantCultureIgnoreCase) || extension.Equals("js", StringComparison.InvariantCultureIgnoreCase))
            {
                return "//";
            }

            if (extension.Equals("vb", StringComparison.InvariantCultureIgnoreCase))
            {
                return "'";
            }

            if (extension.Equals("sql", StringComparison.InvariantCultureIgnoreCase))
            {
                return "--";
            }

            return "<!--";
        }

        /// <summary>
        /// Formats a given command for a given language 
        /// for example for c#, for visual basic, for sql server, for java script
        /// or by default.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="selectedText">The selected text.</param>
        /// <param name="filePath">The file path.</param>
        /// <returns>A Formatted string</returns>
        public static string FormatForCompleteCommand(string command, string selectedText, string filePath)
        {
            string extension = System.IO.Path.GetExtension(filePath).TrimStart('.');

            string language = string.Empty;

            if (extension.Equals("cs", StringComparison.InvariantCultureIgnoreCase))
            {
                language = "for C#";
            }
            else if (extension.Equals("vb", StringComparison.InvariantCultureIgnoreCase))
            {
                language = "for Visual Basic";
            }
            else if (extension.Equals("sql", StringComparison.InvariantCultureIgnoreCase))
            {
                language = "for SQL Server";
            }
            else if (extension.Equals("js", StringComparison.InvariantCultureIgnoreCase))
            {
                language = "for Java Script";
            }

            return $"{command} {language}: {selectedText}";
        }

        /// <summary>
        /// Formats a command for a summary.
        /// </summary>
        /// <param name="command">The command to format.</param>
        /// <param name="selectedText">The selected text.</param>
        /// <returns>The formatted command.</returns>
        public static string FormatCommandForSummary(string command, string selectedText)
        {
            string summaryFormat;

            //Is not a function
            if (!(selectedText.Contains("(") && selectedText.Contains(")") && selectedText.Contains("{") && selectedText.Contains("}")))
            {
                summaryFormat = "/// <summary>\r\n/// \r\n/// </summary>";
            }
            else if (selectedText.Contains(" void "))
            {
                if (selectedText.Contains("()"))
                {
                    summaryFormat = "/// <summary>\r\n/// \r\n/// </summary>";
                }
                else
                {
                    summaryFormat = "/// <summary>\r\n/// \r\n/// </summary>\r\n/// <param name=\"\"></param>";
                }
            }
            else
            {
                if (selectedText.Contains("()"))
                {
                    summaryFormat = "/// <summary>\r\n/// \r\n/// </summary>\r\n/// <returns>\r\n/// \r\n/// </returns>";
                }
                else
                {
                    summaryFormat = "/// <summary>\r\n/// \r\n/// </summary>\r\n/// <param name=\"\"></param>\r\n/// <returns>\r\n/// \r\n/// </returns>";
                }
            }

            return string.Format(command, summaryFormat) + Environment.NewLine + "for" + Environment.NewLine + selectedText;
        }

         /// <summary>
        /// Removes lines from a given text that start with code tags
        /// </summary>
        /// <param name="text">Original text</param>
        /// <returns>Text without the code tags</returns>
        public static string RemoveLinesStartingWithCodeTags(string text)
        {
            string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            IEnumerable<string> filteredLines = lines.Where(line => !line.StartsWith("```"));

            string result = string.Join(Environment.NewLine, filteredLines);

            return RemoveBlankLinesFromResult(result);
        }

        /// <summary>
        /// Removes blank lines from the given string result.
        /// </summary>
        /// <param name="result">The string from which blank lines should be removed.</param>
        /// <returns>A string with blank lines removed.</returns>
        public static string RemoveBlankLinesFromResult(string result)
        {
            return result.TrimPrefix("\r\n").TrimPrefix("\n\n").TrimPrefix("\n").TrimPrefix("\r").TrimSuffix("\r\n").TrimSuffix("\n\n").TrimSuffix("\n").TrimSuffix("\r");
        }

        /// <summary>
        /// Detects the language of the given code.
        /// </summary>
        /// <param name="code">The code to detect the language of.</param>
        /// <returns>The language of the given code, or an empty string if the language could not be determined.</returns>
        public static string DetectCodeLanguage(string code)
        {
            Regex regex = new(@"(<\?xml.+?\?>)|(<.+?>.*?<\/.+?>)");

            if (regex.IsMatch(code))
            {
                return "XML";
            }

            regex = new(@"(<.+?>.*?<\/.+?>)");

            if (regex.IsMatch(code))
            {
                return "HTML";
            }

            regex = new(@"(public|private|protected|internal|static|class|void|string|double|float|in)");

            if (regex.IsMatch(code))
            {
                return "C#";
            }

            regex = new(@"(Public|Private|Protected|Friend|Static|Class|Sub|Function|End Sub|End Function|Dim|As|Integer|Boolean|String|Double|Single|If|Else|End If|While|End While|For|To|Step|Next|Each|In|Return)");

            if (regex.IsMatch(code))
            {
                return "VB";
            }

            regex = new(@"(function|do|switch|case|break|continue|let|instanceof|undefined|super|\bconsole\.)");

            if (regex.IsMatch(code))
            {
                return "JavaScript";
            }

            regex = new(@"([^{]*\{[^}]*\})");

            if (regex.IsMatch(code))
            {
                return "CSS";
            }

            regex = new(@"(CREATE|UPDATE|DELETE|INSERT|DROP|SELECT|FROM|WHERE|JOIN|LEFT\s+JOIN|RIGHT\s+JOIN|INNER\s+JOIN|OUTER\s+JOIN|ON|GROUP\s+BY|HAVING|ORDER\s+BY|LIMIT|\bAND\b|\bOR\b|\bNOT\b|\bIN\b|\bBETWEEN\b|\bLIKE\b|\bIS\s+NULL\b|\bIS\s+NOT\s+NULL\b|\bEXISTS\b|\bCOUNT\b|\bSUM\b|\bAVG\b|\bMIN\b|\bMAX\b|\bCAST\b|\bCONVERT\b|\bDATEADD\b|\bDATEDIFF\b|\bDATENAME\b|\bDATEPART\b|\bGETDATE\b|\bYEAR\b|\bMONTH\b|\bDAY\b|\bHOUR\b|\bMINUTE\b|\bSECOND\b|\bTOP\b|\bDISTINCT\b|\bAS\b)");
            Regex regex2 = new(@"(create|update|delete|insert|drop|select|from|where|join|left\s+join|right\s+join|inner\s+join|outer\s+join|on|group\s+by|having|order\s+by|limit|\band\b|\bor\b|\bnot\b|\bin\b|\bbetween\b|\blike\b|\bis\s+null\b|\bis\s+not\s+null\b|\bexists\b|\bcount\b|\bsum\b|\bavg\b|\bmin\b|\bmax\b|\bcast\b|\bconvert\b|\bdateadd\b|\bdatediff\b|\bdatename\b|\bdatepart\b|\bgetdate\b|\byear\b|\bmonth\b|\bday\b|\bhour\b|\bminute\b|\bsecond\b|\btop\b|\bdistinct\b|\bas\b)");

            if (regex.IsMatch(code) || regex2.IsMatch(code))
            {
                return "TSQL";
            }

            return string.Empty;
        }
    }
}
