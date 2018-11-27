﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LibDmd.Common;
using NLog;

namespace LibDmd.Output.Virtual
{
	/// <summary>
	/// Interaction logic for AlphanumericControl.xaml
	/// </summary>
	public partial class AlphanumericControl : UserControl, IVirtualControl
	{
		protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		public bool IsAvailable => true;

		public bool IgnoreAspectRatio { get; set; }
		public VirtualWindow Host { get; set; }

		private WriteableBitmap _writeableBitmap;

		public AlphanumericControl()
		{
			DataContext = this;
			InitializeComponent();
			SizeChanged += SizeChanged_Event;

			CompositionTarget.Rendering += (o, e) => DrawImage(_writeableBitmap);
		}

		public void ClearDisplay()
		{
			throw new NotImplementedException();
		}

		public void RenderSegments(ushort[] data)
		{
			UpdateData(data);
		}

		private void SizeChanged_Event(object sender, SizeChangedEventArgs e)
		{
			if (!Host.Resizing) {
				CreateImage((int)e.NewSize.Width, (int)e.NewSize.Height);
			}
		}

		private void SetBitmap(WriteableBitmap bitmap)
		{
			AlphanumericDisplay.Source = _writeableBitmap = bitmap;
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}
}
