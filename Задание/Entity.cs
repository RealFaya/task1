using System.Dynamic;

namespace Задание
{
	class Entity
	{
		public string ConnectingString;
		public double DiskSize;

		public Entity(string ConnectingString, double DiskSize)
		{
			this.ConnectingString = ConnectingString;
			this.DiskSize = DiskSize;
		}
	}
}
