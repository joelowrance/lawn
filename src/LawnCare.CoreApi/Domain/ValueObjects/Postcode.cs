using System.Text.RegularExpressions;

namespace LawnCare.CoreApi.Domain.Entities
{
	public sealed class Postcode : IEquatable<Postcode>
	{
		// US ZIP or ZIP+4 (e.g., "12345" or "12345-6789")
		private static readonly Regex ZipPattern = new(@"^\d{5}(?:-\d{4})?$", RegexOptions.Compiled);

		public string Value { get; private set; }

		// EF Core parameterless constructor
		private Postcode()
		{
			Value = string.Empty;
		}

		public Postcode(string value)
		{
			Value = value;
		}

		public static Postcode Empty => Create("00000");

		public static Postcode Create(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				throw new ArgumentException("Postcode cannot be empty.", nameof(input));

			var normalized = Normalize(input);

			if (!IsValid(normalized))
				throw new ArgumentException($"Invalid postcode format: '{input}'. Expected 12345 or 12345-6789.", nameof(input));

			return new Postcode(normalized);
		}

		public static bool TryCreate(string? input, out Postcode? postcode)
		{
			postcode = null;

			if (string.IsNullOrWhiteSpace(input))
				return false;

			var normalized = Normalize(input);

			if (!IsValid(normalized))
				return false;

			postcode = new Postcode(normalized);
			return true;
		}

		public static bool IsValid(string input) => ZipPattern.IsMatch(input);

		private static string Normalize(string input)
		{
			// For US ZIPs, just trim and keep as-is except whitespace
			return input.Trim();
		}

		public override string ToString() => Value;

		// Value-based equality
		public bool Equals(Postcode? other) =>
			other is not null &&
			string.Equals(Value, other.Value, StringComparison.Ordinal);

		public override bool Equals(object? obj) => obj is Postcode other && Equals(other);

		public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

		public static bool operator ==(Postcode? left, Postcode? right) => Equals(left, right);
		public static bool operator !=(Postcode? left, Postcode? right) => !Equals(left, right);

		// Convenience
		public static Postcode Parse(string input) => Create(input);
		public static bool TryParse(string? input, out Postcode? postcode) => TryCreate(input, out postcode);

		public static explicit operator string(Postcode postcode) => postcode.Value;
	}
}