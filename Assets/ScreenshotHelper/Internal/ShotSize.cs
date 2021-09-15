using System.Xml.Serialization;
using System.Collections.Generic;

namespace NG
{
	[System.Serializable]
	public class ShotSize
	{
		[XmlAttribute("width")]
		public int width;
		[XmlAttribute("height")]
		public int height;
		[XmlAttribute("label")]
		public string label = "";

		public ShotSize()
		{ }

		public ShotSize(int x, int y)
		{
			width = x;
			height = y;
		}

		public override string ToString()
		{
			return "(" + width.ToString() + "," + height.ToString() + ")";
		}

		public string GetFileNameBase()
		{
			return width.ToString() + "x" + height.ToString();
		}
	}

    public class ShotSizeComparer: IEqualityComparer<ShotSize>
    {
        public bool Equals(ShotSize a, ShotSize b)
        {
            return (a.width == b.width) && (a.height == b.height);
        }

        public int GetHashCode(ShotSize obj)
        {
            return obj.height.GetHashCode() ^ obj.width.GetHashCode();
        }
    }
}