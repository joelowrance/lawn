using LawnCare.CoreApi.Domain.Common;

namespace LawnCare.CoreApi.Domain.ValueObjects;

public class Money : ValueObject
{
	public decimal Amount { get; }
	public string Currency { get; }

	private Money(decimal amount, string currency = "USD")
	{
		if (amount < 0)
			throw new ArgumentException("Amount cannot be negative", nameof(amount));

		Amount = amount;
		Currency = currency ?? throw new ArgumentNullException(nameof(currency));
	}

	public Money(decimal amount) : this(amount, "USD") { }

	public static Money Zero() => new(0);

	public Money Add(Money other)
	{
		if (Currency != other.Currency)
			throw new InvalidOperationException("Cannot add money with different currencies");

		return new Money(Amount + other.Amount, Currency);
	}

	protected override IEnumerable<object> GetEqualityComponents()
	{
		yield return Amount;
		yield return Currency;
	}

	public override string ToString() => $"{Amount:C}";
}