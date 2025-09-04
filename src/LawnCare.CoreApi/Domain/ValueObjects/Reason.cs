using LawnCare.CoreApi.Domain.Common;

namespace LawnCare.CoreApi.Domain.ValueObjects;


public sealed record class Reason
{
    public string Value { get; }

    public Reason(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Reason value cannot be null or whitespace", nameof(value));
        }

        Value = value;
    }
}
