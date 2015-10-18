﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Xml;
using School.Core;


namespace School.Droid
{
	public class LichHocHKFragment : Fragment
	{
		ListView listView_HK;
		TextView lbl_HK;
		TextView lbl_NH;
		ProgressBar progress;
		bool check,autoupdate;
		Bundle bundle;
		List<chiTietLH> listCT;

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			// Create your fragment here
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);

			//	  Lich hoc theo HK
			var rootView = inflater.Inflate (Resource.Layout.LichHoc_HK, container, false);
			listView_HK = rootView.FindViewById<ListView> (Resource.Id.listLH);
			lbl_HK = rootView.FindViewById<TextView> (Resource.Id.lbl_HK_LH);
			lbl_NH = rootView.FindViewById<TextView> (Resource.Id.lbl_NH_LH);
			progress = rootView.FindViewById<ProgressBar> (Resource.Id.progressLH);
		    bundle=this.Arguments;
			check = bundle.GetBoolean ("Remind");
			autoupdate = bundle.GetBoolean ("AutoUpdateData");

			//load data
			LichHoc lh = BLichHoc.GetLast (SQLite_Android.GetConnection ());
			if (lh != null) {
				LoadData_HK ();
			}
			else {
				progress.Visibility = ViewStates.Gone;
			}
			//radio button
			RadioButton rb_tuan = rootView.FindViewById<RadioButton> (Resource.Id.rb_dangTuan);
			RadioButton rb_hocKy = rootView.FindViewById<RadioButton> (Resource.Id.rb_dangHK);
			rb_hocKy.Checked = true;

			rb_tuan.Click += new EventHandler (rd_OnCheckedChangeListener);

			//button 
//			Button btnHKTruoc = rootView.FindViewById<Button> (Resource.Id.btnHK_Truoc_LH);
//			btnHKTruoc.Click += new EventHandler (btnHK_Truoc_Click);
//			Button btnHKKe = rootView.FindViewById<Button> (Resource.Id.btnHK_Ke_LH);
//			btnHKKe.Click += new EventHandler (btnHK_Ke_Click);	
							
			// row click
			listView_HK.ItemClick += listView_ItemClick;
						

			return rootView;
		}

		void listView_ItemClick(object sender, AdapterView.ItemClickEventArgs e){
			// content to pass 
			var bundle1 = new Bundle();
			LichHoc lh = BLichHoc.GetLH(SQLite_Android.GetConnection (),listCT[e.Position].Id);

			bundle1.PutString ("MH", lh.Id);
			bundle1.PutBoolean ("isLHT", false);
			bundle1.PutBoolean ("check", false);

			var fragment = new ReminderDialogFragment ();
			fragment.Arguments = bundle1;
			FragmentManager.BeginTransaction ()
				.Replace (Resource.Id.content_frame, fragment).AddToBackStack("1")
				.Commit ();
		}


		void rd_OnCheckedChangeListener (object sender, EventArgs e)
		{
			LichHocTuanFragment fragment = new LichHocTuanFragment ();
			fragment.Arguments = bundle;
			FragmentManager.BeginTransaction ()
				.Replace (Resource.Id.content_frame, fragment).AddToBackStack ("11")
				.Commit ();
		}
			
		async void LoadData_HK ()
		{
			listView_HK.Visibility = ViewStates.Invisible;
			progress.Visibility = ViewStates.Visible;
			progress.Indeterminate = true;
			List<LichHoc> listLH = new List<LichHoc> ();
			if (Common.checkNWConnection (Activity) == true&&autoupdate==true) {
			
				var newlistlh = BLichHoc.MakeDataFromXml (SQLite_Android.GetConnection ());
				List<LichHoc> newListLH= await newlistlh;
				if (check) {
					ScheduleReminder reminder = new ScheduleReminder (Activity);

					reminder.RemindAllLH (newListLH);
				}
			}
			listLH = BLichHoc.GetNewestLH (SQLite_Android.GetConnection ());

			listCT = new List<chiTietLH> ();
			foreach (var item in listLH) {
				listCT.AddRange (BLichHoc.GetCTLH (SQLite_Android.GetConnection (), item.Id));

			}

			lbl_HK.Text = listLH [0].HocKy;
			lbl_NH.Text = listLH [0].NamHoc;
			LichHocHKAdapter adapter = new LichHocHKAdapter (Activity, listCT);
			listView_HK.Adapter = adapter;  
			progress.Indeterminate = false;
			progress.Visibility = ViewStates.Gone;
			listView_HK.Visibility = ViewStates.Visible;

		
		}
		//
		//		void btnHK_Truoc_Click (object sender, EventArgs e)
		//		{
		//			string hK;
		//			string nH;
		//			Common.calSemester (lbl_HK.Text, lbl_NH.Text, -1, out hK, out nH);
		//			LoadData_HK (hK, nH);
		//		}
		//
		//		void btnHK_Ke_Click (object sender, EventArgs e)
		//		{
		//
		//			string hK;
		//			string nH;
		//			Common.calSemester (lbl_HK.Text, lbl_NH.Text, 1, out hK, out nH);
		//			LoadData_HK (hK, nH);
		//
		//		}
	}
}

