using System;

/// <summary>
/// Custom exception for path validation errors in FileTree scanning.
/// </summary>
/// <summary>
/// Entry namespace for path validation errors.
/// Contains PathErrorType enum and PathValidationException class.
/// </summary>
namespace FileTree.Core.Utilities
{
    /// <summary>
/// Represents different types of path validation errors.
    /// </summary>
    public enum PathErrorType
    {
        /// <summary>
        /// The path has invalid characters or format.
        /// </summary>
        InvalidFormat,
        
        /// <summary>
        /// The path does not exist.
        /// </summary>
        NotFound,
        
        /// <summary>
        /// The path exists but is a file, not a directory.
        /// </summary>
        NotDirectory,
        
        /// <summary>
        /// No permission to access the directory.
        /// </summary>
        AccessDenied
    }

    /// <summary>
    /// Exception thrown when directory path validation fails.
    /// Provides detailed error type and path for user-friendly messages.
    /// </summary>
    public class PathValidationException : Exception
    {
        /// <summary>
        /// Gets the error type.
        /// </summary>
        public PathErrorType ErrorType { get; }

        /// <summary>
        /// Gets the full normalized path that failed validation.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Initializes a new instance with error type and path.
        /// </summary>
        public PathValidationException(string message, PathErrorType errorType, string path) 
            : base(message)
        {
            ErrorType = errorType;
            Path = path;
        }

        /// <summary>
        /// Initializes with inner exception (e.g. ArgumentException from Path.GetFullPath).
        /// </summary>
        public PathValidationException(string message, PathErrorType errorType, string path, Exception innerException) 
            : base(message, innerException)
        {
            ErrorType = errorType;
            Path = path;
        }
    }
}
