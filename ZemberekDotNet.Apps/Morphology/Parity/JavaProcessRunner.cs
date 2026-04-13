using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ZemberekDotNet.Apps.Morphology.Parity
{
    /// <summary>
    /// Spawns a Java process to run Zemberek analysis and writes output in the
    /// TSV format expected by <see cref="JavaOutputParser"/>.
    ///
    /// The Java invocation command is fully configurable via <paramref name="argsTemplate"/>.
    /// Use {input} and {output} as placeholders for the temporary sentence file path and
    /// the output TSV path respectively.
    ///
    /// Default template:
    ///   zemberek.apps.ApplicationRunner MorphAnalyze --input {input} --output {output}
    ///
    /// If the Java Zemberek version you are using has a different batch analysis command,
    /// pass the appropriate template string via --java-args in the CLI.
    /// </summary>
    public static class JavaProcessRunner
    {
        private const string DefaultArgsTemplate =
            "zemberek.apps.ApplicationRunner MorphAnalyze --input {input} --output {output}";

        /// <summary>
        /// Runs the Java analysis process.
        /// </summary>
        /// <param name="jarPath">Path to the Zemberek fat JAR (e.g., zemberek-full-0.17.1.jar).</param>
        /// <param name="argsTemplate">
        /// Command arguments template. Supports {input} and {output} placeholders.
        /// Pass empty string to use the default template.
        /// </param>
        /// <param name="inputSentencesPath">Path to the input file with one sentence per line.</param>
        /// <param name="outputTsvPath">Path where the Java side should write the TSV output.</param>
        public static void Run(string jarPath, string argsTemplate, string inputSentencesPath, string outputTsvPath)
        {
            if (!File.Exists(jarPath))
                throw new FileNotFoundException($"Java JAR not found: {jarPath}");

            string template = string.IsNullOrWhiteSpace(argsTemplate) ? DefaultArgsTemplate : argsTemplate;
            string resolvedArgs = template
                .Replace("{input}", $"\"{inputSentencesPath}\"")
                .Replace("{output}", $"\"{outputTsvPath}\"");

            string javaArgs = $"-jar \"{jarPath}\" {resolvedArgs}";

            Console.WriteLine($"[JavaProcessRunner] Invoking: java {javaArgs}");

            var psi = new ProcessStartInfo
            {
                FileName = "java",
                Arguments = javaArgs,
                RedirectStandardOutput = false,
                RedirectStandardError = true,
                UseShellExecute = false,
                StandardErrorEncoding = Encoding.UTF8,
                CreateNoWindow = true
            };

            using Process process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start Java process.");

            string stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Java process exited with code {process.ExitCode}.\nStderr:\n{stderr}");
            }

            if (!string.IsNullOrWhiteSpace(stderr))
                Console.Error.WriteLine($"[JavaProcessRunner] stderr:\n{stderr}");

            if (!File.Exists(outputTsvPath))
                throw new InvalidOperationException(
                    $"Java process completed but output file was not created: {outputTsvPath}\n" +
                    "Check that the --java-args template is correct for your Zemberek JAR version.");

            Console.WriteLine($"[JavaProcessRunner] Java analysis complete: {outputTsvPath}");
        }
    }
}
