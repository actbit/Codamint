using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;

namespace Codamint.Plugins
{
    /// <summary>
    /// ファイル操作機能を提供するプラグイン
    /// </summary>
    public class FileOperationPlugin
    {
        [KernelFunction, Description("Read file content")]
        public async Task<string> ReadFile(
            [Description("The file path to read")] string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return $"Error: File not found - {filePath}";
                }

                var content = await File.ReadAllTextAsync(filePath);
                return content;
            }
            catch (Exception ex)
            {
                return $"Error reading file: {ex.Message}";
            }
        }

        [KernelFunction, Description("Write content to file")]
        public async Task<string> WriteFile(
            [Description("The file path to write")] string filePath,
            [Description("The content to write")] string content)
        {
            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllTextAsync(filePath, content);
                return $"File written successfully: {filePath}";
            }
            catch (Exception ex)
            {
                return $"Error writing file: {ex.Message}";
            }
        }

        [KernelFunction, Description("Append content to file")]
        public async Task<string> AppendFile(
            [Description("The file path to append")] string filePath,
            [Description("The content to append")] string content)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return await WriteFile(filePath, content);
                }

                await File.AppendAllTextAsync(filePath, content);
                return $"Content appended successfully: {filePath}";
            }
            catch (Exception ex)
            {
                return $"Error appending to file: {ex.Message}";
            }
        }

        [KernelFunction, Description("Delete file")]
        public async Task<string> DeleteFile(
            [Description("The file path to delete")] string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return $"Error: File not found - {filePath}";
                }

                File.Delete(filePath);
                return $"File deleted successfully: {filePath}";
            }
            catch (Exception ex)
            {
                return $"Error deleting file: {ex.Message}";
            }
        }

        [KernelFunction, Description("List files in directory")]
        public async Task<string> ListFiles(
            [Description("The directory path")] string directoryPath,
            [Description("Search pattern (e.g. '*.cs')")] string searchPattern = "*")
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    return $"Error: Directory not found - {directoryPath}";
                }

                var files = Directory.GetFiles(directoryPath, searchPattern);
                var result = new StringBuilder();
                result.AppendLine($"Files in {directoryPath}:");
                foreach (var file in files)
                {
                    var info = new FileInfo(file);
                    result.AppendLine($"  {Path.GetFileName(file)} ({info.Length} bytes)");
                }

                return result.ToString();
            }
            catch (Exception ex)
            {
                return $"Error listing files: {ex.Message}";
            }
        }

        [KernelFunction, Description("Get file info")]
        public async Task<string> GetFileInfo(
            [Description("The file path")] string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return $"Error: File not found - {filePath}";
                }

                var info = new FileInfo(filePath);
                var result = new StringBuilder();
                result.AppendLine($"File: {Path.GetFileName(filePath)}");
                result.AppendLine($"Full Path: {info.FullName}");
                result.AppendLine($"Size: {info.Length} bytes");
                result.AppendLine($"Created: {info.CreationTime:yyyy-MM-dd HH:mm:ss}");
                result.AppendLine($"Modified: {info.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
                result.AppendLine($"Extension: {info.Extension}");

                return result.ToString();
            }
            catch (Exception ex)
            {
                return $"Error getting file info: {ex.Message}";
            }
        }

        [KernelFunction, Description("Create directory")]
        public async Task<string> CreateDirectory(
            [Description("The directory path to create")] string directoryPath)
        {
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    return $"Directory already exists: {directoryPath}";
                }

                Directory.CreateDirectory(directoryPath);
                return $"Directory created successfully: {directoryPath}";
            }
            catch (Exception ex)
            {
                return $"Error creating directory: {ex.Message}";
            }
        }

        [KernelFunction, Description("Search for files by pattern")]
        public async Task<string> SearchFiles(
            [Description("The root directory to search")] string rootPath,
            [Description("Search pattern")] string pattern,
            [Description("Search recursively")] bool recursive = true)
        {
            try
            {
                if (!Directory.Exists(rootPath))
                {
                    return $"Error: Directory not found - {rootPath}";
                }

                var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var files = Directory.GetFiles(rootPath, pattern, searchOption);

                if (files.Length == 0)
                {
                    return $"No files found matching pattern: {pattern}";
                }

                var result = new StringBuilder();
                result.AppendLine($"Found {files.Length} file(s) matching '{pattern}':");
                foreach (var file in files)
                {
                    result.AppendLine($"  {file}");
                }

                return result.ToString();
            }
            catch (Exception ex)
            {
                return $"Error searching files: {ex.Message}";
            }
        }

        [KernelFunction, Description("Get directory structure as a tree")]
        public async Task<string> GetDirectoryTree(
            [Description("The root directory path")] string rootPath,
            [Description("Maximum depth to display (-1 for unlimited)")] int maxDepth = -1,
            [Description("File extensions to include (comma-separated, empty for all)")] string includeExtensions = "")
        {
            try
            {
                if (!Directory.Exists(rootPath))
                {
                    return $"Error: Directory not found - {rootPath}";
                }

                var extensions = string.IsNullOrEmpty(includeExtensions)
                    ? new List<string>()
                    : includeExtensions.Split(',').Select(e => e.Trim().ToLower()).ToList();

                var tree = new StringBuilder();
                tree.AppendLine($"📁 {Path.GetFileName(rootPath) ?? rootPath}");
                BuildTreeStructure(rootPath, "", tree, maxDepth, 0, extensions);

                return tree.ToString();
            }
            catch (Exception ex)
            {
                return $"Error building tree structure: {ex.Message}";
            }
        }

        [KernelFunction, Description("Get detailed directory information as tree")]
        public async Task<string> GetDetailedDirectoryTree(
            [Description("The root directory path")] string rootPath,
            [Description("Include file sizes")] bool includeSize = true,
            [Description("Include modification dates")] bool includeDates = true,
            [Description("Maximum depth to display (-1 for unlimited)")] int maxDepth = -1)
        {
            try
            {
                if (!Directory.Exists(rootPath))
                {
                    return $"Error: Directory not found - {rootPath}";
                }

                var tree = new StringBuilder();
                tree.AppendLine($"📁 {Path.GetFileName(rootPath) ?? rootPath}/");
                BuildDetailedTreeStructure(rootPath, "", tree, maxDepth, 0, includeSize, includeDates);

                return tree.ToString();
            }
            catch (Exception ex)
            {
                return $"Error building detailed tree structure: {ex.Message}";
            }
        }

        [KernelFunction, Description("Count files and directories")]
        public async Task<string> CountDirectoryContents(
            [Description("The root directory path")] string rootPath,
            [Description("Include subdirectories recursively")] bool recursive = true)
        {
            try
            {
                if (!Directory.Exists(rootPath))
                {
                    return $"Error: Directory not found - {rootPath}";
                }

                var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var files = Directory.GetFiles(rootPath, "*", searchOption);
                var directories = Directory.GetDirectories(rootPath, "*", searchOption);

                var totalSize = files.Sum(f => new FileInfo(f).Length);

                var result = new StringBuilder();
                result.AppendLine($"Directory Statistics for: {rootPath}");
                result.AppendLine($"Total Files: {files.Length}");
                result.AppendLine($"Total Directories: {directories.Length}");
                result.AppendLine($"Total Size: {FormatFileSize(totalSize)}");

                return result.ToString();
            }
            catch (Exception ex)
            {
                return $"Error counting contents: {ex.Message}";
            }
        }

        [KernelFunction, Description("Get file tree with file information")]
        public async Task<string> GetFileTree(
            [Description("The root directory path")] string rootPath,
            [Description("File extension to filter (e.g., '.cs', '.json')")] string extension = "",
            [Description("Maximum depth")] int maxDepth = -1)
        {
            try
            {
                if (!Directory.Exists(rootPath))
                {
                    return $"Error: Directory not found - {rootPath}";
                }

                var tree = new StringBuilder();
                var searchPattern = string.IsNullOrEmpty(extension) ? "*" : $"*{(extension.StartsWith(".") ? extension : "." + extension)}";

                tree.AppendLine($"📄 Files in {rootPath}");
                tree.AppendLine(string.IsNullOrEmpty(extension) ? "(all files)" : $"(filtered by {searchPattern})");
                tree.AppendLine();

                BuildFileTree(rootPath, "", tree, maxDepth, 0, searchPattern);

                return tree.ToString();
            }
            catch (Exception ex)
            {
                return $"Error building file tree: {ex.Message}";
            }
        }

        [KernelFunction, Description("Get folder structure only (no files)")]
        public async Task<string> GetFolderStructure(
            [Description("The root directory path")] string rootPath,
            [Description("Maximum depth (-1 for unlimited)")] int maxDepth = -1)
        {
            try
            {
                if (!Directory.Exists(rootPath))
                {
                    return $"Error: Directory not found - {rootPath}";
                }

                var tree = new StringBuilder();
                tree.AppendLine($"📁 {Path.GetFileName(rootPath) ?? rootPath}");
                BuildFolderStructure(rootPath, "", tree, maxDepth, 0);

                return tree.ToString();
            }
            catch (Exception ex)
            {
                return $"Error building folder structure: {ex.Message}";
            }
        }

        private void BuildTreeStructure(
            string currentPath,
            string prefix,
            StringBuilder sb,
            int maxDepth,
            int currentDepth,
            List<string> includeExtensions)
        {
            if (maxDepth != -1 && currentDepth >= maxDepth)
            {
                return;
            }

            try
            {
                var directories = Directory.GetDirectories(currentPath);
                var files = Directory.GetFiles(currentPath);

                if (includeExtensions.Count > 0)
                {
                    files = files.Where(f =>
                    {
                        var ext = Path.GetExtension(f).ToLower();
                        return includeExtensions.Contains(ext);
                    }).ToArray();
                }

                var allItems = new List<(string path, bool isDirectory)>();
                foreach (var dir in directories.OrderBy(d => d))
                {
                    allItems.Add((dir, true));
                }
                foreach (var file in files.OrderBy(f => f))
                {
                    allItems.Add((file, false));
                }

                for (int i = 0; i < allItems.Count; i++)
                {
                    var (itemPath, isDirectory) = allItems[i];
                    var isLast = i == allItems.Count - 1;
                    var itemName = Path.GetFileName(itemPath);

                    var icon = isDirectory ? "📁" : "📄";
                    var connector = isLast ? "└── " : "├── ";

                    sb.AppendLine($"{prefix}{connector}{icon} {itemName}");

                    if (isDirectory)
                    {
                        var newPrefix = prefix + (isLast ? "    " : "│   ");
                        BuildTreeStructure(itemPath, newPrefix, sb, maxDepth, currentDepth + 1, includeExtensions);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                sb.AppendLine($"{prefix}└── [Access Denied]");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"{prefix}└── [Error: {ex.Message}]");
            }
        }

        private void BuildDetailedTreeStructure(
            string currentPath,
            string prefix,
            StringBuilder sb,
            int maxDepth,
            int currentDepth,
            bool includeSize,
            bool includeDates)
        {
            if (maxDepth != -1 && currentDepth >= maxDepth)
            {
                return;
            }

            try
            {
                var directories = Directory.GetDirectories(currentPath);
                var files = Directory.GetFiles(currentPath);
                var allItems = new List<(string path, bool isDirectory)>();

                foreach (var dir in directories.OrderBy(d => d))
                {
                    allItems.Add((dir, true));
                }
                foreach (var file in files.OrderBy(f => f))
                {
                    allItems.Add((file, false));
                }

                for (int i = 0; i < allItems.Count; i++)
                {
                    var (itemPath, isDirectory) = allItems[i];
                    var isLast = i == allItems.Count - 1;
                    var itemName = Path.GetFileName(itemPath);
                    var connector = isLast ? "└── " : "├── ";

                    var info = isDirectory ? new DirectoryInfo(itemPath) : (FileSystemInfo)new FileInfo(itemPath);

                    var details = new StringBuilder();
                    if (includeSize && !isDirectory)
                    {
                        var fileInfo = (FileInfo)info;
                        details.Append($" [{FormatFileSize(fileInfo.Length)}]");
                    }
                    if (includeDates)
                    {
                        details.Append($" [{info.LastWriteTime:yyyy-MM-dd HH:mm}]");
                    }

                    var icon = isDirectory ? "📁" : "📄";
                    sb.AppendLine($"{prefix}{connector}{icon} {itemName}{details}");

                    if (isDirectory)
                    {
                        var newPrefix = prefix + (isLast ? "    " : "│   ");
                        BuildDetailedTreeStructure(itemPath, newPrefix, sb, maxDepth, currentDepth + 1, includeSize, includeDates);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                sb.AppendLine($"{prefix}└── [Access Denied]");
            }
        }

        private void BuildFileTree(
            string currentPath,
            string prefix,
            StringBuilder sb,
            int maxDepth,
            int currentDepth,
            string searchPattern)
        {
            if (maxDepth != -1 && currentDepth >= maxDepth)
            {
                return;
            }

            try
            {
                var files = Directory.GetFiles(currentPath, searchPattern);
                var directories = Directory.GetDirectories(currentPath);

                foreach (var file in files.OrderBy(f => f))
                {
                    var fileName = Path.GetFileName(file);
                    var fileInfo = new FileInfo(file);
                    var isLast = file == files.LastOrDefault();

                    var connector = isLast && directories.Length == 0 ? "└── " : "├── ";
                    sb.AppendLine($"{prefix}{connector}📄 {fileName} ({FormatFileSize(fileInfo.Length)})");
                }

                for (int i = 0; i < directories.Length; i++)
                {
                    var dir = directories.OrderBy(d => d).ElementAt(i);
                    var dirName = Path.GetFileName(dir);
                    var isLast = i == directories.Length - 1;

                    var connector = isLast ? "└── " : "├── ";
                    sb.AppendLine($"{prefix}{connector}📁 {dirName}/");

                    var newPrefix = prefix + (isLast ? "    " : "│   ");
                    BuildFileTree(dir, newPrefix, sb, maxDepth, currentDepth + 1, searchPattern);
                }
            }
            catch (UnauthorizedAccessException)
            {
                sb.AppendLine($"{prefix}└── [Access Denied]");
            }
        }

        private void BuildFolderStructure(
            string currentPath,
            string prefix,
            StringBuilder sb,
            int maxDepth,
            int currentDepth)
        {
            if (maxDepth != -1 && currentDepth >= maxDepth)
            {
                return;
            }

            try
            {
                var directories = Directory.GetDirectories(currentPath).OrderBy(d => d).ToArray();

                for (int i = 0; i < directories.Length; i++)
                {
                    var dir = directories[i];
                    var dirName = Path.GetFileName(dir);
                    var isLast = i == directories.Length - 1;

                    var connector = isLast ? "└── " : "├── ";
                    sb.AppendLine($"{prefix}{connector}📁 {dirName}");

                    var newPrefix = prefix + (isLast ? "    " : "│   ");
                    BuildFolderStructure(dir, newPrefix, sb, maxDepth, currentDepth + 1);
                }
            }
            catch (UnauthorizedAccessException)
            {
                sb.AppendLine($"{prefix}└── [Access Denied]");
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}
