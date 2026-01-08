using System;
using System.Collections.Generic;
using System.Linq;

namespace MechanicBuddy.Core.Application.Validation
{
    /// <summary>
    /// Validates image uploads for size, type, and format
    /// </summary>
    public static class ImageValidator
    {
        // Default max size: 5MB
        public const int DefaultMaxSizeBytes = 5 * 1024 * 1024;

        // Max size for logos: 2MB
        public const int LogoMaxSizeBytes = 2 * 1024 * 1024;

        // Max size for hero/gallery images: 10MB
        public const int LargeImageMaxSizeBytes = 10 * 1024 * 1024;

        // Allowed MIME types for images
        private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/gif",
            "image/webp",
            "image/svg+xml"
        };

        // Magic bytes for common image formats
        private static readonly Dictionary<string, byte[][]> MagicBytes = new()
        {
            ["image/jpeg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
            ["image/jpg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
            ["image/png"] = new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
            ["image/gif"] = new[] { new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 } },
            ["image/webp"] = new[] { new byte[] { 0x52, 0x49, 0x46, 0x46 } }, // RIFF header, WebP has additional check
        };

        public static ImageValidationResult Validate(string base64Data, string mimeType, int maxSizeBytes = DefaultMaxSizeBytes)
        {
            if (string.IsNullOrWhiteSpace(base64Data))
            {
                return ImageValidationResult.Success(); // Empty is allowed (no image)
            }

            // Validate MIME type
            if (string.IsNullOrWhiteSpace(mimeType))
            {
                return ImageValidationResult.Failure("MIME type is required when providing image data");
            }

            if (!AllowedMimeTypes.Contains(mimeType))
            {
                return ImageValidationResult.Failure($"Invalid image type '{mimeType}'. Allowed types: {string.Join(", ", AllowedMimeTypes)}");
            }

            // Try to decode base64
            byte[] imageBytes;
            try
            {
                // Remove data URI prefix if present (e.g., "data:image/png;base64,")
                var base64Content = base64Data;
                if (base64Data.Contains(","))
                {
                    base64Content = base64Data.Substring(base64Data.IndexOf(',') + 1);
                }

                imageBytes = Convert.FromBase64String(base64Content);
            }
            catch (FormatException)
            {
                return ImageValidationResult.Failure("Invalid base64 format");
            }

            // Validate size
            if (imageBytes.Length > maxSizeBytes)
            {
                var maxSizeMB = maxSizeBytes / (1024.0 * 1024.0);
                var actualSizeMB = imageBytes.Length / (1024.0 * 1024.0);
                return ImageValidationResult.Failure($"Image size ({actualSizeMB:F2}MB) exceeds maximum allowed size ({maxSizeMB:F2}MB)");
            }

            // Validate magic bytes (actual file content matches declared MIME type)
            // Skip SVG as it's text-based
            if (mimeType != "image/svg+xml" && !ValidateMagicBytes(imageBytes, mimeType))
            {
                return ImageValidationResult.Failure("Image content does not match declared MIME type");
            }

            return ImageValidationResult.Success();
        }

        public static ImageValidationResult ValidateLogo(string base64Data, string mimeType)
        {
            return Validate(base64Data, mimeType, LogoMaxSizeBytes);
        }

        public static ImageValidationResult ValidateLargeImage(string base64Data, string mimeType)
        {
            return Validate(base64Data, mimeType, LargeImageMaxSizeBytes);
        }

        /// <summary>
        /// Validates an image without requiring a declared MIME type.
        /// Checks file size and attempts to detect image type from magic bytes.
        /// </summary>
        public static ImageValidationResult ValidateWithoutMimeType(string base64Data, int maxSizeBytes = DefaultMaxSizeBytes)
        {
            if (string.IsNullOrWhiteSpace(base64Data))
            {
                return ImageValidationResult.Success(); // Empty is allowed
            }

            // Try to decode base64
            byte[] imageBytes;
            try
            {
                // Remove data URI prefix if present
                var base64Content = base64Data;
                if (base64Data.Contains(","))
                {
                    base64Content = base64Data.Substring(base64Data.IndexOf(',') + 1);
                }

                imageBytes = Convert.FromBase64String(base64Content);
            }
            catch (FormatException)
            {
                return ImageValidationResult.Failure("Invalid base64 format");
            }

            // Validate size
            if (imageBytes.Length > maxSizeBytes)
            {
                var maxSizeMB = maxSizeBytes / (1024.0 * 1024.0);
                var actualSizeMB = imageBytes.Length / (1024.0 * 1024.0);
                return ImageValidationResult.Failure($"Image size ({actualSizeMB:F2}MB) exceeds maximum allowed size ({maxSizeMB:F2}MB)");
            }

            // Validate it's actually an image by checking magic bytes for any supported format
            if (!IsValidImageFormat(imageBytes))
            {
                return ImageValidationResult.Failure("File does not appear to be a valid image");
            }

            return ImageValidationResult.Success();
        }

        /// <summary>
        /// Checks if the bytes represent any supported image format
        /// </summary>
        private static bool IsValidImageFormat(byte[] imageBytes)
        {
            if (imageBytes.Length < 8)
            {
                return false;
            }

            foreach (var entry in MagicBytes)
            {
                foreach (var expectedMagicBytes in entry.Value)
                {
                    if (imageBytes.Take(expectedMagicBytes.Length).SequenceEqual(expectedMagicBytes))
                    {
                        // Special check for WebP
                        if (entry.Key == "image/webp" && imageBytes.Length >= 12)
                        {
                            var webpMarker = new byte[] { 0x57, 0x45, 0x42, 0x50 };
                            if (!imageBytes.Skip(8).Take(4).SequenceEqual(webpMarker))
                            {
                                continue;
                            }
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool ValidateMagicBytes(byte[] imageBytes, string mimeType)
        {
            if (imageBytes.Length < 8)
            {
                return false;
            }

            // Normalize jpg to jpeg for lookup
            var lookupMime = mimeType.ToLowerInvariant();
            if (lookupMime == "image/jpg")
            {
                lookupMime = "image/jpeg";
            }

            if (!MagicBytes.TryGetValue(lookupMime, out var expectedMagicBytesArray))
            {
                // Unknown type, allow it (conservative approach for extensibility)
                return true;
            }

            foreach (var expectedMagicBytes in expectedMagicBytesArray)
            {
                if (imageBytes.Take(expectedMagicBytes.Length).SequenceEqual(expectedMagicBytes))
                {
                    // Special check for WebP - verify it has WebP marker after RIFF
                    if (lookupMime == "image/webp" && imageBytes.Length >= 12)
                    {
                        var webpMarker = new byte[] { 0x57, 0x45, 0x42, 0x50 }; // "WEBP"
                        if (!imageBytes.Skip(8).Take(4).SequenceEqual(webpMarker))
                        {
                            continue;
                        }
                    }
                    return true;
                }
            }

            return false;
        }
    }

    public class ImageValidationResult
    {
        public bool IsValid { get; private set; }
        public string ErrorMessage { get; private set; }

        private ImageValidationResult() { }

        public static ImageValidationResult Success()
        {
            return new ImageValidationResult { IsValid = true };
        }

        public static ImageValidationResult Failure(string errorMessage)
        {
            return new ImageValidationResult { IsValid = false, ErrorMessage = errorMessage };
        }
    }
}
