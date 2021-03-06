﻿using System;
using System.ComponentModel;
using System.Windows.Forms;
using ReClassNET.Util;

namespace ReClassNET.UI
{
	class HotSpotTextBox : TextBox
	{
		private HotSpot hotSpot;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public HotSpot HotSpot
		{
			get { return hotSpot; }
			set
			{
				if (hotSpot != value)
				{
					hotSpot = value;

					Left = hotSpot.Rect.Left + 2;
					Top = hotSpot.Rect.Top;
					Width = hotSpot.Rect.Width;
					Height = hotSpot.Rect.Height;

					MinimumWidth = Width;

					Text = hotSpot.Text.Trim();
				}
			}
		}

		public int MinimumWidth { get; set; }

		private FontEx font;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new FontEx Font
		{
			get { return font; }
			set
			{
				if (font != value)
				{
					font = value;

					base.Font = font.Font;
				}
			}
		}

		public event EventHandler Committed;

		public HotSpotTextBox()
		{
			BorderStyle = BorderStyle.None;
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (Visible)
			{
				if (HotSpot != null)
				{
					Focus();
					Select(0, TextLength);
				}
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				OnCommit();

				e.Handled = true;
				e.SuppressKeyPress = true;
			}

			base.OnKeyDown(e);
		}

		/*protected override void OnLeave(EventArgs e)
		{
			base.OnLeave(e);

			OnCommit();
		}*/

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);

			var w = (TextLength + 1) * font.Width;
			if (w > MinimumWidth)
			{
				Width = w;
			}
		}

		private void OnCommit()
		{
			hotSpot.Text = Text.Trim();

			try
			{
				hotSpot.Node.Update(hotSpot);
			}
			catch (Exception ex)
			{
				Program.Logger.Log(ex);
			}

			Committed?.Invoke(this, EventArgs.Empty);

			Visible = false;
		}
	}
}
