using LawnCare.CoreApi.Domain.Common;

namespace LawnCare.CoreApi.Domain.Entities
{
    public sealed class PhoneNumber : ValueObject
    {
        // Normalized E.164 value (e.g., +15551234567)
        public string Value { get; private set; } = null!;

        // 10-digit national significant number
        private readonly string _digits;

        // Optional extension (digits only), if provided in input
        public string? Extension { get; }

        // Convenience parts
        public string AreaCode => _digits.Substring(0, 3);
        public string Exchange => _digits.Substring(3, 3);
        public string LineNumber => _digits.Substring(6, 4);

        // Formatted national representation: (NPA) NXX-XXXX
        public string National => FormatNational(_digits, Extension);

        public PhoneNumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Phone number cannot be empty.", nameof(input));

            var (digits, ext) = ExtractDigitsAndExtension(input);

            // Handle country code 1
            if (digits.Length == 11 && digits[0] == '1')
                digits = digits.Substring(1);

            if (digits.Length != 10)
                throw new ArgumentException("US phone number must contain 10 digits (optionally prefixed with country code 1).", nameof(input));

            ValidateNanp(digits);

            _digits = digits;
            Value = "+1" + digits;
            Extension = ext;
        }

        private static (string digits, string? extension) ExtractDigitsAndExtension(string input)
        {
            string cleaned = input.Trim();

            // Look for common extension markers (case-insensitive): ext, ext., extension, x, #
            var lower = cleaned.ToLowerInvariant();
            var markers = new[] { "ext.", "extension", "ext", "x", "#" };
            string? ext = null;
            int markerIndex = -1;
            string? foundMarker = null;

            foreach (var m in markers)
            {
                markerIndex = lower.IndexOf(m, StringComparison.Ordinal);
                if (markerIndex >= 0)
                {
                    foundMarker = m;
                    break;
                }
            }

            string main = cleaned;
            if (markerIndex >= 0 && foundMarker != null)
            {
                main = cleaned[..markerIndex];
                var after = cleaned[(markerIndex + foundMarker.Length)..];
                var extDigits = new string(after.Where(char.IsDigit).ToArray());
                if (!string.IsNullOrEmpty(extDigits))
                    ext = extDigits;
            }

            var digits = new string(main.Where(char.IsDigit).ToArray());
            return (digits, ext);
        }

        private static void ValidateNanp(string digits)
        {
            // NANP basic constraints:
            // - 10 digits total (already enforced)
            // - Area code (NPA) and exchange (NXX) cannot start with 0 or 1
            // - Area code and exchange cannot be N11 (e.g., 211, 311, ..., 911)
            var area = digits.Substring(0, 3);
            var exch = digits.Substring(3, 3);

            if (area[0] < '2' || area[0] > '9')
                throw new ArgumentException("Area code must start with digits 2–9.", nameof(digits));
            if (exch[0] < '2' || exch[0] > '9')
                throw new ArgumentException("Exchange code must start with digits 2–9.", nameof(digits));

            if (area[1] == '1' && area[2] == '1')
                throw new ArgumentException("Area code cannot be an N11 code.", nameof(digits));
            if (exch[1] == '1' && exch[2] == '1')
                throw new ArgumentException("Exchange code cannot be an N11 code.", nameof(digits));
        }

        private static string FormatNational(string digits, string? ext)
        {
            var national = $"({digits.Substring(0, 3)}) {digits.Substring(3, 3)}-{digits.Substring(6, 4)}";
            return string.IsNullOrEmpty(ext) ? national : $"{national} x{ext}";
        }

        public override string ToString() => Value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            // Include extension in equality so two numbers with different extensions are distinct.
            yield return Value;       // E.164 normalized number
        }

        // Optional helper for safe creation without exceptions
        public static bool TryCreate(string input, out PhoneNumber? phoneNumber, out string? error)
        {
            try
            {
                phoneNumber = new PhoneNumber(input);
                error = null;
                return true;
            }
            catch (Exception ex)
            {
                phoneNumber = null;
                error = ex.Message;
                return false;
            }
        }
    }
}