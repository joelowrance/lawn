namespace LawnCare.Shared
{
	public static class GuidHelper
	{
		public static Guid NewId() => Guid.CreateVersion7();
	}
}