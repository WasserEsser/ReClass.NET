﻿using System;
using System.Diagnostics.Contracts;
using System.Drawing;
using ReClassNET.Memory;
using ReClassNET.UI;

namespace ReClassNET.Nodes
{
	public class BitFieldNode : BaseNode
	{
		private int size;
		private int bits;

		/// <summary>Gets or sets the bit count.</summary>
		/// <value>Possible values: 64, 32, 16, 8</value>
		public int Bits
		{
			get { return bits; }
			set
			{
				Contract.Ensures(bits > 0);
				Contract.Ensures(size > 0);

				if (value >= 64)
				{
					bits = 64;
				}
				else if (value >= 32)
				{
					bits = 32;
				}
				else if (value >= 16)
				{
					bits = 16;
				}
				else
				{
					bits = 8;
				}

				size = bits / 8;
			}
		}

		/// <summary>Size of the node in bytes.</summary>
		public override int MemorySize => size;

		/// <summary>Default constructor.</summary>
		public BitFieldNode()
		{
			Bits = IntPtr.Size * 8;

			levelsOpen.DefaultValue = true;
		}

		/// <summary>Initializes this object with a bit count which equals the other nodes memory size.</summary>
		/// <param name="node">The node to copy from.</param>
		public override void CopyFromNode(BaseNode node)
		{
			base.CopyFromNode(node);

			Bits = node.MemorySize * 8;
		}

		/// <summary>Converts the memory value to a bit string.</summary>
		/// <param name="memory">The process memory.</param>
		/// <returns>The value converted to a bit string.</returns>
		private string ConvertValueToBitString(MemoryBuffer memory)
		{
			Contract.Requires(memory != null);
			Contract.Ensures(Contract.Result<string>() != null);

			string str;
			switch(bits)
			{
				case 64:
					str = Convert.ToString(memory.ReadObject<long>(Offset), 2);
					break;
				case 32:
					str = Convert.ToString(memory.ReadObject<int>(Offset), 2);
					break;
				case 16:
					str = Convert.ToString(memory.ReadObject<short>(Offset), 2);
					break;
				default:
					str = Convert.ToString(memory.ReadObject<byte>(Offset), 2);
					break;
			}
			return str.PadLeft(bits, '0');
		}

		/// <summary>Draws this node.</summary>
		/// <param name="view">The view information.</param>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <returns>The height the node occupies.</returns>
		public override int Draw(ViewInfo view, int x, int y)
		{
			if (IsHidden)
			{
				return DrawHidden(view, x, y);
			}

			AddSelection(view, x, y, view.Font.Height);
			AddDelete(view, x, y);
			AddTypeDrop(view, x, y);

			x += TextPadding + 16;

			x = AddAddressOffset(view, x, y);

			x = AddText(view, x, y, Program.Settings.TypeColor, HotSpot.NoneId, "Bits") + view.Font.Width;
			x = AddText(view, x, y, Program.Settings.NameColor, HotSpot.NameId, Name) + view.Font.Width;

			x = AddOpenClose(view, x, y) + view.Font.Width;

			var tx = x - 3;

			for (var i = 0; i < bits; ++i)
			{
				var rect = new Rectangle(x + i * view.Font.Width, y, view.Font.Width, view.Font.Height);
				AddHotSpot(view, rect, string.Empty, i, HotSpotType.Edit);
			}
			x = AddText(view, x, y, Program.Settings.ValueColor, HotSpot.NoneId, ConvertValueToBitString(view.Memory)) + view.Font.Width;

			x += view.Font.Width;

			AddComment(view, x, y);

			if (levelsOpen[view.Level])
			{
				y += view.Font.Height;

				var format = new StringFormat(StringFormatFlags.DirectionVertical);

				using (var brush = new SolidBrush(Program.Settings.ValueColor))
				{
					view.Context.DrawString("1", view.Font.Font, brush, tx + (bits - 1) * view.Font.Width + 1, y, format);

					for (var i = 8; i <= bits; i += 8)
					{
						view.Context.DrawString(i.ToString(), view.Font.Font, brush, tx  + (bits - i) * view.Font.Width, y, format);
					}
				}

				y += 2;
			}

			return y + view.Font.Height;
		}

		public override int CalculateHeight(ViewInfo view)
		{
			if (IsHidden)
			{
				return HiddenHeight;
			}

			var h = view.Font.Height;
			if (levelsOpen[view.Level])
			{
				h += view.Font.Height + 2;
			}
			return h;
		}

		/// <summary>Updates the node from the given spot. Sets the value of the selected bit.</summary>
		/// <param name="spot">The spot.</param>
		public override void Update(HotSpot spot)
		{
			base.Update(spot);

			if (spot.Id >= 0 && spot.Id < bits)
			{
				if (spot.Text == "1" || spot.Text == "0")
				{
					var bit = (bits - 1) - spot.Id;
					var add = bit / 8;
					bit = bit % 8;

					var val = spot.Memory.ReadObject<sbyte>(Offset + add);
					if (spot.Text == "1")
					{
						val |= (sbyte)(1 << bit);
					}
					else
					{
						val &= (sbyte)~(1 << bit);
					}
					spot.Memory.Process.WriteRemoteMemory(spot.Address + add, val);
				}
			}
		}
	}
}
