using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmasters.Core.Application
{
    public class ShellCommand
    {
        public async Task<string> Run(string program, string commandText)
        {
            Process p = new Process
            {
                StartInfo =
                         new ProcessStartInfo(program, commandText)
                         {
                             UseShellExecute = false,
                             RedirectStandardOutput = true,
                             RedirectStandardError = true
                         }
            };

            p.Start();

            var standardOutputReader = ConsumeReader(p.StandardOutput);
            var standardErrorReader = ConsumeReader(p.StandardError);

            p.WaitForExit();

            var outputString = await standardOutputReader;
            var errorString = await standardErrorReader;
            if (string.IsNullOrWhiteSpace(errorString))
            {
                return outputString;
            }
            throw new Exception(errorString);
        }

        async Task<string> ConsumeReader(TextReader reader)
        {
            var strings = new StringBuilder();
            var text = string.Empty;
            while ((text = await reader.ReadLineAsync()) != null)
            {
                strings.AppendLine(text);
            }
            return strings.ToString();
        }
    }

}
