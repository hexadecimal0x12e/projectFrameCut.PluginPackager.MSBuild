using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace projectFrameCut.PluginPackager.MSBuild
{
    public class PluginBuilder : Microsoft.Build.Utilities.Task
    {
        public const int CurrentPluginAPIVersion = 4;
        public const int CurrentAppLevelPluginVersion = 4;
        public const int CurrentPluginAPIMinorVersion = 0;

        private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        private static readonly StringComparer PathComparer = IsWindows ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

        public override bool Execute()
        {
            LogImmediateInfo($"PluginBuilder started. Stage={(IsGenerateSourceStage ? "GenerateSource" : "Pack")}, PluginID={PluginID}, Version={PluginVersion}");

            if (IsGenerateSourceStage)
            {
                try
                {
                    var path = PartialClassGenerator.GeneratePartialClassFile(
                        PluginID, PluginVersion, PluginPublishUrl, NeutralLanguageDisplayName, Authors, NeutralLanguageDescription, PluginProjectUrl, ProjectRootPath, GenerateSource, IsAppLevelPlugin, PluginMajorVersion, PluginMinorVersion, AppLevelPluginMajorVersion, GenerateLoader, GenerateSourcePath);
                    Log.LogMessage(MessageImportance.High, $"Generated plugin source: {path}");
                    return true;
                }
                catch (Exception ex)
                {
                    Log.LogErrorFromException(ex, true);
                    return false;
                }
            }
#if NET5_0_OR_GREATER
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("PluginBuilder arguments:");
                var props = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var p in props)
                {
                    if (p.GetMethod == null)
                        continue;
                    object value;
                    try
                    {
                        value = p.GetValue(this);
                    }
                    catch (Exception ex)
                    {
                        value = $"<Failed to get value: {ex.Message}>";
                    }

                    string display;
                    if (value == null)
                    {
                        display = "<null>";
                    }
                    else if (value is string str)
                    {
                        display = str;
                    }
                    else if (value is IEnumerable enumerable && !(value is string))
                    {
                        var items = new List<string>();
                        foreach (var item in enumerable)
                        {
                            items.Add(item?.ToString() ?? "<null>");
                        }
                        display = string.Join(", ", items);
                    }
                    else
                    {
                        display = value.ToString();
                    }

                    sb.AppendLine($"{p.Name} = {display}");
                }

                Log.LogMessage(MessageImportance.High, sb.ToString());
                Log.LogMessage($"TFM:{TargetFrameworkID}");

                if (string.IsNullOrWhiteSpace(SignFilePath) || !File.Exists(SignFilePath))
                {
                    Log.LogError($"SignFilePath invalid or not found: {SignFilePath}");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(PluginOutputPath))
                {
                    Log.LogError("PluginOutputPath is required.");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(OutputDirectory) || !Directory.Exists(OutputDirectory))
                {
                    Log.LogError($"OutputDirectory invalid or not found: {OutputDirectory}");
                    return false;
                }

                // Try to find the plugin assembly (prefer the project's assembly name)
                var assemblyName = Path.GetFileNameWithoutExtension(PluginOutputPath);
                var targetDll = Path.Combine(OutputDirectory, assemblyName + ".dll");

                if (!Version.TryParse(PluginVersion, out var ver))
                {
                    Log.LogError($"PackageVersion '{PluginVersion}' is not a valid version string.");
                    return false;
                }

                PluginMetadata mtd = new PluginMetadata
                {
                    Author = Authors,
                    AuthorUrl = PluginProjectUrl,
                    Description = NeutralLanguageDescription,
                    PluginID = PluginID,
                    Version = ver,
                    Name = NeutralLanguageDisplayName,
                    PluginAPIVersion = PluginMajorVersion,
                    PublishingUrl = PluginPublishUrl

                };

                LogImmediateInfo($"Metadata: {JsonConvert.SerializeObject(mtd, Formatting.Indented)}.");

                Build(PluginID, ProjectRootPath, targetDll, AssetPath ?? string.Empty, OutputDirectory, SignFilePath, mtd);

                var expectedName = mtd.PluginID + "_" + mtd.Version + ".pjfcPlugin";
                var pkgPath = Path.Combine(OutputDirectory, expectedName);
                if (File.Exists(pkgPath))
                {
                    PluginPackagePath = pkgPath;
                    LogImmediateInfo($"Plugin packaged to {pkgPath}.");
                    return true;
                }
                else
                {
                    Log.LogError($"Packaging completed but expected package not found: {pkgPath}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex, true);
                return false;
            }


#endif
#pragma warning disable CS0162
            return true;
#pragma warning restore CS0162 
        }
#if NET5_0_OR_GREATER
        public void Build(string pluginID, string projectRoot, string DllPath, string assetPath, string tempPath, string kpPath, PluginMetadata mtd)
        {

            var mainDllPath = Path.Combine(projectRoot, DllPath);
            Log.LogMessage($"pluginID:{pluginID}, mainDLLPath: {mainDllPath}, assetPath:{assetPath}, tempPath:{tempPath}, kpPath:{kpPath}");
            if (mainDllPath is null || !File.Exists(mainDllPath)) throw new FileNotFoundException($"Plugin source not found.");
            Log.LogMessage($"PluginID: {pluginID}, workingDir: {tempPath}");
            string pubKey = "", priKey = "";
            GetKeypairFromFile(kpPath, out pubKey, out priKey);

            Log.LogMessage("Encrypting assembly...");
            var pluginDir = Path.Combine(tempPath, "plugin");
            if (Directory.Exists(pluginDir))
            {
                Directory.Delete(pluginDir, true);
            }
            Directory.CreateDirectory(pluginDir);
            var sig = projectFrameCut.Shared.FileSignerService.SignFile(priKey, mainDllPath);
            var sigPath = Path.Combine(tempPath, "plugin", pluginID + ".dll.sig");
            File.WriteAllText(sigPath, sig);
            var encFilePath = Path.Combine(tempPath, "plugin", pluginID + ".dll.enc");
            var sigKey = ComputeStringHash(pubKey, SHA512.Create());
            projectFrameCut.Shared.FileCryptoService.EncryptToFileWithPassword(sigKey, mainDllPath, encFilePath);
            Log.LogMessage("Making metadata...");
            mtd.PluginHash = ComputeFileHashAsync(mainDllPath);
            mtd.PluginKey = sigKey;
            var mtdJson = JsonConvert.SerializeObject(mtd, Formatting.Indented);
            Log.LogMessage("Packaging plugin...");
            File.WriteAllText(Path.Combine(tempPath, "plugin", "metadata.json"), mtdJson);
            File.WriteAllText(Path.Combine(tempPath, "plugin", "publickey.pem"), pubKey);

            if (BundlePublishOutputs)
            {
                try
                {
                    CopyPublishOutputsToPluginFolder(
                        publishDir: tempPath,
                        pluginDir: pluginDir,
                        mainAssemblyPath: mainDllPath,
                        includeMainAssemblyPlain: IncludeMainAssemblyPlain,
                        extraExcludePatterns: BundleExcludePatterns);
                }
                catch (Exception ex)
                {
                    Log.LogErrorFromException(ex, true, true, null);
                    throw;
                }
            }

            if (!string.IsNullOrWhiteSpace(assetPath) && Directory.Exists(assetPath))
            {
                //Copy assets
                var destAssetPath = Path.Combine(tempPath, "plugin");
                Directory.CreateDirectory(destAssetPath);
                foreach (var file in Directory.GetFiles(assetPath, "*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(assetPath, file);
                    var destFilePath = Path.Combine(destAssetPath, relativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));
                    Log.LogMessage($"Copying asset {file} to {destFilePath}...");
                    File.Copy(file, destFilePath, true);
                }
            }
            else
            {
                Log.LogMessage("No asset provided, skip.");
            }
            Log.LogMessage("Creating hashtable...");
            Dictionary<string, string> hashTable = new Dictionary<string, string>();
            foreach (var file in Directory.GetFiles(Path.Combine(tempPath, "plugin"), "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(Path.Combine(tempPath, "plugin"), file);
                var fileHash = ComputeFileHashAsync(file);
                hashTable[relativePath.Replace('\\', '/')] = fileHash;
                Log.LogMessage($"File: {relativePath}, Hash: {fileHash}");
            }
            var hashJson = JsonConvert.SerializeObject(hashTable, Formatting.Indented);
            File.WriteAllText(Path.Combine(tempPath, "plugin", "hashtable.json"), hashJson);

            var zipPath = Path.Combine(tempPath, $"{pluginID}_{mtd.Version}.pjfcPlugin");
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            Log.LogMessage("Creating plugin package...");
            ZipFile.CreateFromDirectory(Path.Combine(tempPath, "plugin"), zipPath, CompressionLevel.Optimal, false);

            try
            {
                File.Delete(Path.Combine(tempPath, "plugin", "hashtable.json"));
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to delete hashtable.json from staging folder: {ex.Message}");
            }


            Log.LogMessage($"Plugin packaged to {zipPath}");

        }

        private void CopyPublishOutputsToPluginFolder(
            string publishDir,
            string pluginDir,
            string mainAssemblyPath,
            bool includeMainAssemblyPlain,
            string extraExcludePatterns)
        {
            if (string.IsNullOrWhiteSpace(publishDir) || !Directory.Exists(publishDir))
            {
                Log.LogError($"Publish output directory not found: {publishDir}");
                return;
            }

            var publishRoot = Path.GetFullPath(AppendDirectorySeparatorChar(publishDir));
            var pluginRoot = Path.GetFullPath(AppendDirectorySeparatorChar(pluginDir));

            var mainAssemblyFullPath = string.IsNullOrWhiteSpace(mainAssemblyPath) ? string.Empty : Path.GetFullPath(mainAssemblyPath);
            var mainAssemblyFileName = string.IsNullOrWhiteSpace(mainAssemblyFullPath) ? string.Empty : Path.GetFileName(mainAssemblyFullPath);

            // Default exclude patterns for MSBuild packager's own dependencies
            var defaultExcludes = new List<string>
            {
                "Microsoft.Build*",
                "Newtonsoft.Json*"
            };

            var extraExcludes = ParseExcludePatterns(extraExcludePatterns);
            var allExcludes = defaultExcludes.Concat(extraExcludes).ToList();
            
            Log.LogMessage($"Bundling publish outputs to plugin folder (excluding: {string.Join(", ", defaultExcludes)})...");

            foreach (var srcFile in Directory.GetFiles(publishRoot, "*", SearchOption.AllDirectories))
            {
                var srcFullPath = Path.GetFullPath(srcFile);

                // Don't copy files that are already under the plugin staging folder (prevents recursion).
                if (IsUnderDirectory(srcFullPath, pluginRoot))
                    continue;

                // Don't copy existing plugin packages.
                if (string.Equals(Path.GetExtension(srcFullPath), ".pjfcPlugin", StringComparison.OrdinalIgnoreCase))
                    continue;

                // By default, do not include the plain main assembly (the package contains the encrypted one).
                if (!includeMainAssemblyPlain && !string.IsNullOrWhiteSpace(mainAssemblyFileName))
                {
                    var fileName = Path.GetFileName(srcFullPath);
                    if (string.Equals(fileName, mainAssemblyFileName, StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                var rel = Path.GetRelativePath(publishRoot, srcFullPath);

                // Skip top-level "plugin\\" folder even if path comparison above fails for any reason.
                if (rel.StartsWith("plugin" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                    rel.StartsWith("plugin/", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (IsExcludedByPatterns(rel, allExcludes))
                    continue;

                var destPath = Path.Combine(pluginRoot, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                File.Copy(srcFullPath, destPath, true);
            }
        }

        private static bool IsUnderDirectory(string fullPath, string directoryFullPathWithSeparator)
        {
            if (string.IsNullOrWhiteSpace(fullPath) || string.IsNullOrWhiteSpace(directoryFullPathWithSeparator))
                return false;

            var comparison = IsWindows ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return fullPath.StartsWith(directoryFullPathWithSeparator, comparison);
        }

        private static List<string> ParseExcludePatterns(string patterns)
        {
            if (string.IsNullOrWhiteSpace(patterns))
                return new List<string>();

            return patterns
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => p.Length > 0)
                .ToList();
        }

        private static bool IsExcludedByPatterns(string relativePath, List<string> patterns)
        {
            if (patterns == null || patterns.Count == 0)
                return false;

            // Normalize to forward slashes for matching.
            var path = (relativePath ?? string.Empty).Replace('\\', '/');
            foreach (var pattern in patterns)
            {
                if (WildcardMatch(path, pattern.Replace('\\', '/')))
                    return true;
            }

            return false;
        }

        private static bool WildcardMatch(string text, string pattern)
        {
            // Very small wildcard matcher: supports '*' and '?', case-insensitive on Windows.
            if (text == null) text = string.Empty;
            if (pattern == null) pattern = string.Empty;

            var comparison = IsWindows ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            int t = 0, p = 0, star = -1, match = 0;
            while (t < text.Length)
            {
                if (p < pattern.Length && (pattern[p] == '?' || string.Compare(text, t, pattern, p, 1, comparison) == 0))
                {
                    t++;
                    p++;
                    continue;
                }
                if (p < pattern.Length && pattern[p] == '*')
                {
                    star = p++;
                    match = t;
                    continue;
                }
                if (star != -1)
                {
                    p = star + 1;
                    t = ++match;
                    continue;
                }
                return false;
            }

            while (p < pattern.Length && pattern[p] == '*')
                p++;

            return p == pattern.Length;
        }
#endif

        public static void GetKeypairFromFile(string path, out string pubKey, out string priKey)
        {
            KeyValuePair<string, string> kp = JsonConvert.DeserializeObject<KeyValuePair<string, string>>(File.ReadAllText(path));
            pubKey = kp.Key;
            priKey = kp.Value;
        }

        [DebuggerNonUserCode()]
        public static string ComputeFileHashAsync(string fileName, HashAlgorithm algorithm = null)
        {
            algorithm = SHA256.Create();
            if (System.IO.File.Exists(fileName))
            {
                byte[] buffer;
                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, FileShare.Read))
                {
                    buffer = algorithm.ComputeHash(fs);
                }
                algorithm.Clear();
                return buffer.Select(c => c.ToString("x2")).Aggregate((a, b) => a + b);
            }
            throw new FileNotFoundException("File not found", fileName);
        }

        [DebuggerNonUserCode()]
        public static string ComputeStringHash(string input, HashAlgorithm algorithm = null)
            => (algorithm ?? SHA512.Create())
                .ComputeHash(Encoding.UTF8.GetBytes(input))
                .Select(c => c.ToString("x2"))
                .Aggregate((a, b) => a + b);


        private static string AppendDirectorySeparatorChar(string path)
        {
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                return path + Path.DirectorySeparatorChar;
            return path;
        }

        private void LogImmediateInfo(string text)
        {
            Console.WriteLine(text);
            Log.LogMessage(MessageImportance.High, text);
        }

        [Required]
        public string PluginOutputPath { get; set; }
        [Required]
        public string ProjectRootPath { get; set; }

        public string TargetFrameworkID { get; set; }


        [Required]
        public string OutputDirectory { get; set; }
        [Required]
        public string SignFilePath { get; set; }
        [Required]
        public string PluginID { get; set; }
        [Required]
        public string NeutralLanguageDisplayName { get; set; }
        [Required]
        public string PluginVersion { get; set; }
        [Required]
        public bool IsAppLevelPlugin { get; set; }
        [Required]
        public bool IsGenerateSourceStage { get; set; }


        public string AssetPath { get; set; }
        public string Authors { get; set; }
        public string PluginProjectUrl { get; set; }
        public string PluginPublishUrl { get; set; }
        public string NeutralLanguageDescription { get; set; }


        public bool GenerateSource { get; set; } = true;
        public bool GenerateLoader { get; set; } = true;
        public int PluginMajorVersion { get; set; } = CurrentPluginAPIVersion;
        public int PluginMinorVersion { get; set; } = 0;
        public int AppLevelPluginMajorVersion { get; set; } = CurrentAppLevelPluginVersion;
        public string GenerateSourcePath { get; set; }

        /// <summary>
        /// When true, bundle all files produced by publish output into the plugin package (dependencies/resources/native files).
        /// </summary>
        public bool BundlePublishOutputs { get; set; } = true;

        /// <summary>
        /// When true, also include the unencrypted main plugin assembly from publish output.
        /// Keep false by default to avoid leaking the plugin code because the package already contains the encrypted main assembly.
        /// </summary>
        public bool IncludeMainAssemblyPlain { get; set; } = false;

        /// <summary>
        /// Optional semicolon-separated wildcard patterns to exclude publish output files.
        /// Match is applied on relative path (with '/' separators). Example: "*.pdb;*.xml;runtimes/*/native/*".
        /// </summary>
        public string BundleExcludePatterns { get; set; }


        [Output]
        public string PluginPackagePath { get; set; }
    }
}
