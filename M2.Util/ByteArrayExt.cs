using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace M2.Util
{
	public static class ByteArrayExt
	{
		public static byte[] SetValueRange(this byte[] ary, byte value, int startIndex, int count=-1)
		{
			if (count <= 0 || count > ary.Count() - startIndex)
				count = ary.Count() - startIndex;

			for (int ix = startIndex; ix < startIndex+count; ix++)
			{
				ary[ix] = value;
			}

			return ary;
		}

		public static byte[] AddValueRange(this byte[] ary, byte value, int startIndex, int count=-1)
		{
			if (count <= 0 || count > ary.Count()-startIndex)
				count = ary.Count() - startIndex;

			for (int ix = startIndex; ix < startIndex + count; ix++)
			{
				ary[ix] += value;
			}

			return ary;
		}

		public static byte[] Reset(this byte[] ary, byte value=0)
		{
			ary.SetValueRange(0, 0, ary.Count());
			return ary;
		}

		public static int FindFirst(this byte[] ary, byte value, int startIndex)
		{
			int ix = startIndex;
			while(ix < ary.Count() && ary[ix] != value)
				ix++;

			if (ix >= ary.Count())
				return -1;
			else
				return ix;
		}

		public static int FindFirstNot(this byte[] ary, byte value, int startIndex)
		{
			int ix = startIndex;
			while (ix < ary.Count() && ary[ix] == value)
				ix++;

			if (ix >= ary.Count())
				return -1;
			else
				return ix;
		}

		public static Pair<int,int> FindBlock(this byte[] ary, byte value, int startIndex, int blockSize)
		{
			int ix = ary.FindFirst(value, startIndex);
			if (ix < 0)
				return null;

			int iy = ary.FindFirstNot(value, ix + 1);
			if (iy < 1)
				return null;
			else
				iy--;

			if (iy - ix + 1 >= blockSize)
				return new Pair<int,int>(ix, ix + blockSize);
			else
				return null;
		}
	}
}
