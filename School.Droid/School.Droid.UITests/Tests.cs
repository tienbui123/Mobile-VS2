﻿using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.Queries;
using System.Reflection;

namespace School.Droid.UITests
{
	[TestFixture]
	public class Tests
	{
		AndroidApp app;

		[SetUp]
		public void BeforeEachTest ()
		{
			
			app = ConfigureApp.Android.StartApp ();
		}



		[Test]
		public void ClickingButtonTwiceShouldChangeItsLabel ()
		{
			Func<AppQuery, AppQuery> MyButton = c => c.Button ("btDangNhap");

			app.Tap (MyButton);
			app.Tap (MyButton);
			AppResult[] results = app.Query (MyButton);
			app.Screenshot ("Button clicked twice.");

			Assert.AreEqual ("2 clicks!", results [0].Text);
		}
	}
}

