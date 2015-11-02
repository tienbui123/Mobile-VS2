﻿
using System;

using Foundation;
using UIKit;

namespace School.iOS
{
	public partial class LogOut : UIViewController
	{
		public LogOut () : base ("LogOut", null)
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();

			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			
			base.ViewDidLoad ();
			ApiHelper.LogOut ();
			// Perform any additional setup after loading the view, typically from a nib.
		}
	}
}
