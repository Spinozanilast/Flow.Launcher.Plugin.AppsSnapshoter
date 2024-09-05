using System;
using System.IO;
using System.Linq;

namespace Flow.Launcher.Plugin.SnapshotApps.Services
{
    /// <summary>
    /// Provides methods for managing files within a specified directory.
    /// </summary>
    public class FileService
    {
        private string _combinedPath;

        /// <summary>
        /// Gets the combined path of the base directory and the part to combine.
        /// </summary>
        public string CombinedPath => _combinedPath;

        /// <summary>
        /// Gets or sets the default file extension.
        /// </summary>
        public string DefaultFileExtension { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileService"/> class.
        /// </summary>
        /// <param name="baseDirectory">The base directory path.</param>
        /// <param name="partToCombine">The part to combine with the base directory.</param>
        /// <param name="defaultFileExtension">The default file extension.</param>
        public FileService(string baseDirectory, string partToCombine, string defaultFileExtension)
        {
            CombineBasePath(baseDirectory, partToCombine);
            CreateBaseFolderIfNotExists();
            DefaultFileExtension = defaultFileExtension;
        }

        /// <summary>
        /// Gets an array of file names within the combined directory path.
        /// </summary>
        /// <returns>An array of file names without the default file extension.</returns>
        public string[] GetFileNames()
        {
            var directoryFiles = Directory.GetFiles(_combinedPath);

            var fileNames = new string[directoryFiles.Length];

            for (int i = 0; i < fileNames.Length; i++)
            {
                fileNames[i] = Path.GetFileName(directoryFiles[i])[..^DefaultFileExtension.Length];
            }

            return fileNames;
        }

        /// <summary>
        /// Checks if there are any files inside the base directory.
        /// </summary>
        /// <returns>True if any files exist; otherwise, false.</returns>
        public bool IsAnyFileInsideDirectory()
        {
            return Directory.EnumerateFiles(_combinedPath).Any();
        }

        /// <summary>
        /// Opens an existing file for reading.
        /// </summary>
        /// <param name="fileName">The name of the file to open.</param>
        /// <returns>A <see cref="FileStream"/> object for reading the file.</returns>
        /// <exception cref="ArgumentException">Thrown if the file does not exist.</exception>
        public FileStream OpenReadExistingFile(string fileName)
        {
            var file = GetFileName(fileName);

            if (!File.Exists(file))
            {
                throw new ArgumentException(
                    "File doesn't exist. Something went wrong!",
                    nameof(file));
            }

            return File.OpenRead(file);
        }

        /// <summary>
        /// Creates a new file.
        /// </summary>
        /// <param name="newFileName">The name of the new file.</param>
        /// <returns>A <see cref="FileStream"/> object for writing to the new file.</returns>
        public FileStream CreateFile(string newFileName) =>
            File.Create(GetFileName(newFileName));

        /// <summary>
        /// Deletes a file.
        /// </summary>
        /// <param name="fileNameToDelete">The name of the file to delete.</param>
        public void DeleteFile(string fileNameToDelete) =>
            File.Delete(GetFileName(fileNameToDelete));

        /// <summary>
        /// Renames a file.
        /// </summary>
        /// <param name="oldFileName">The old name of the file.</param>
        /// <param name="newFileName">The new name of the file.</param>
        public void RenameFile(string oldFileName, string newFileName) =>
            File.Move(GetFileName(oldFileName), GetFileName(newFileName));

        /// <summary>
        /// Gets the full file name by combining the base path and the file name with the default extension.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The full file name including the default extension.</returns>
        public string GetFileName(string fileName) =>
            Path.Combine(_combinedPath, fileName) + DefaultFileExtension;

        /// <summary>
        /// Combines the base directory path with the part to combine.
        /// </summary>
        /// <param name="baseDirectory">The base directory to combine with <see cref="partToCombine"/>.</param>
        /// <param name="partToCombine">The part to combine with the base directory.</param>
        public void CombineBasePath(string baseDirectory, string partToCombine)
        {
            _combinedPath = Path.Combine(baseDirectory, partToCombine);
        }

        public string GetFileNameWithoutExtension(string fileName)
        {
            return Path.GetFileName(fileName)[..^DefaultFileExtension.Length];
        }

        public string[] GetFiles()
        {
            return Directory.GetFiles(_combinedPath);
        }

        /// <summary>
        /// Creates the base directory if it does not exist.
        /// </summary>
        private void CreateBaseFolderIfNotExists()
        {
            if (!Directory.Exists(_combinedPath))
            {
                Directory.CreateDirectory(_combinedPath);
            }
        }
    }
}