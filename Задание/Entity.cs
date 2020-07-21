namespace Задание
{
	class Entity<T>
	{
		public string _String;
		public T Size;

		public Entity(string _String, T Size)
		{
			this._String = _String;
			this.Size = Size;
		}
	}
}
