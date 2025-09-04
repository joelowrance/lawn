using System.Globalization;
using System.Text.RegularExpressions;

using LawnCare.CoreApi.Domain.Common;

namespace LawnCare.CoreApi.Domain.Entities
{
    public class EmailAddress : ValueObject
    {
        public string Value { get; private set; } = null!;

        public EmailAddress(string emailAddress)
        {
            Value = NormalizeAndValidate(emailAddress);
        }

        private static string NormalizeAndValidate(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
                throw new ArgumentException("Email address cannot be empty", nameof(emailAddress));

            var input = emailAddress.Trim();

            if (input.Any(char.IsWhiteSpace))
                throw new ArgumentException("Email address cannot contain spaces", nameof(emailAddress));

            var firstAt = input.IndexOf('@');
            var lastAt = input.LastIndexOf('@');
            if (firstAt <= 0 || firstAt != lastAt || firstAt == input.Length - 1)
                throw new ArgumentException("Email address must contain a single '@' separating local and domain parts.", nameof(emailAddress));

            var local = input[..firstAt];
            var domain = input[(firstAt + 1)..];

            if (local.Length > 64)
                throw new ArgumentException("Email local part is too long (max 64 characters).", nameof(emailAddress));

            if (domain.Length > 255)
                throw new ArgumentException("Email domain part is too long (max 255 characters).", nameof(emailAddress));

            // Normalize/validate domain using IDN (handles Unicode domains).
            string asciiDomain;
            try
            {
                var idn = new IdnMapping();
                asciiDomain = idn.GetAscii(domain);
            }
            catch
            {
                throw new ArgumentException("Email domain contains invalid characters.", nameof(emailAddress));
            }

            var labels = asciiDomain.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (labels.Length == 0)
                throw new ArgumentException("Email domain is missing.", nameof(emailAddress));

            if (labels.Any(l => l.Length == 0 || l.Length > 63))
                throw new ArgumentException("Email domain labels must be between 1 and 63 characters.", nameof(emailAddress));

            var labelRegex = new Regex("^[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?$", RegexOptions.Compiled);
            if (labels.Any(l => !labelRegex.IsMatch(l)))
                throw new ArgumentException("Email domain labels can only contain letters, digits, and hyphens (no leading/trailing hyphen).", nameof(emailAddress));

            var tld = labels[^1];
            if (tld.Length < 2)
                throw new ArgumentException("Email top-level domain is too short.", nameof(emailAddress));

            // Local part: allow dot-atom or quoted-string.
            var dotAtom = new Regex("^[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+)*$", RegexOptions.Compiled);
            var isQuoted = local.Length >= 2 && local.StartsWith("\"") && local.EndsWith("\"");

            if (!(dotAtom.IsMatch(local) || isQuoted))
                throw new ArgumentException("Email local part is invalid.", nameof(emailAddress));

            // Normalized value: domain lowercased; local preserved.
            return $"{local}@{asciiDomain.ToLowerInvariant()}";
        }

        public override string ToString() => Value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
	        yield return Value;
        }
    }
}