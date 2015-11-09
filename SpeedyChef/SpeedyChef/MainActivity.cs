﻿using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using v7Widget = Android.Support.V7.Widget;
using System.Collections.Generic;

namespace SpeedyChef
{
	[Activity (Theme="@style/MyTheme", Label = "SpeedyChef", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : CustomActivity, SearchView.IOnQueryTextListener, SearchView.IOnSuggestionListener
	{
		v7Widget.RecyclerView mRecyclerView;
		v7Widget.RecyclerView.LayoutManager mLayoutManager;
		PlannedMealAdapter mAdapter;
		PlannedMealObject mObject;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			//RECYCLER VIEW
			mObject = new PlannedMealObject ();
			mAdapter = new PlannedMealAdapter (mObject);
			SetContentView (Resource.Layout.Main);
			mRecyclerView = FindViewById<v7Widget.RecyclerView> (Resource.Id.recyclerView);
			mRecyclerView.SetAdapter (mAdapter);
			mLayoutManager = new v7Widget.LinearLayoutManager (this);
			mRecyclerView.SetLayoutManager (mLayoutManager);

			//SEARCH VIEW
			SearchView searchView = FindViewById<SearchView> (Resource.Id.main_search);
			searchView.SetBackgroundColor (Android.Graphics.Color.White);
			searchView.SetOnQueryTextListener ((SearchView.IOnQueryTextListener) this);
			int id = Resources.GetIdentifier("android:id/search_src_text", null, null);
			TextView textView = (TextView) searchView.FindViewById(id);
			textView.SetTextColor(Android.Graphics.Color.Black);
			textView.SetHintTextColor (Android.Graphics.Color.Black);
			searchView.SetQueryHint ("Search Recipes...");
			LinearLayout search_container = FindViewById<LinearLayout> (Resource.Id.search_container);
			search_container.Click += (sender, e) => {
				if (searchView.Iconified != false){
					searchView.Iconified = false;
				}
			};



			//MENU VIEW
			Button menu_button = FindViewById<Button> (Resource.Id.menu_button);
			menu_button.Click += (s, arg) => {
				menu_button.SetBackgroundResource(Resource.Drawable.pressed_lines);
				PopupMenu menu = new PopupMenu (this, menu_button);
				menu.Inflate (Resource.Menu.Main_Menu);
				menu.MenuItemClick += this.MenuButtonClick;
				menu.DismissEvent += (s2, arg2) => {
					menu_button.SetBackgroundResource(Resource.Drawable.menu_lines);
					Console.WriteLine ("menu dismissed");
				};
				menu.Show ();
			};
				
		}

		protected override void OnResume(){
			base.OnResume ();
			CachedData.Instance.CurrHighLevelType = this.GetType ();
		}

		public override void OnBackPressed(){
			base.OnPause ();
			CachedData.Instance.PreviousActivity = this;
			Finish ();
		}

		public bool OnQueryTextChange(string input)
		{
			System.Console.WriteLine (input);
			return true;
		}

		public bool OnQueryTextSubmit(string input)
		{
			System.Console.WriteLine (input);
			return true;
		}

		public bool OnSuggestionSelect (int position)
		{
			return false;
		}

		public bool OnSuggestionClick (int position)
		{
			return false;
		}
	}

	public class PlannedMealViewHolder : v7Widget.RecyclerView.ViewHolder
	{
		public TextView mealDescription { get; private set; }
		// Locate and cache view references
		public PlannedMealViewHolder (View itemView) : base (itemView)
		{
			mealDescription = itemView.FindViewById<TextView> (Resource.Id.textView);
		}
	}

	public class PlannedMealAdapter : v7Widget.RecyclerView.Adapter
	{
		public PlannedMealObject mPMObject;

		public PlannedMealAdapter (PlannedMealObject inPMObject)
		{
			mPMObject = inPMObject;
		}

		public override v7Widget.RecyclerView.ViewHolder
		OnCreateViewHolder (ViewGroup parent, int viewType)
		{
			View itemView = LayoutInflater.From (parent.Context).
				Inflate (Resource.Layout.LinearCardView, parent, false);
			PlannedMealViewHolder vh = new PlannedMealViewHolder (itemView);
			return vh;
		}
		public override void
		OnBindViewHolder (v7Widget.RecyclerView.ViewHolder holder, int position)
		{
			PlannedMealViewHolder vh = holder as PlannedMealViewHolder;
			vh.mealDescription.Text = mPMObject.getObjectInPosition(position);
		}

		public override int ItemCount
		{
			get { return mPMObject.NumElements; }
		}
	}

	public class PlannedMealObject
	{
		public int NumElements;
		public string[] mealArray;

		public PlannedMealObject ()
		{
			mealArray = new string[5];
			mealArray [0] = "10/28 Mom's Spaghetti";
			mealArray [1] = "10/30 Halloween Cake w/ Candy Corn";
			mealArray [2] = "10/31 All Saints Day Omelette";
			mealArray [3] = "11/2 Wedding Present (Brownies)";
			mealArray [4] = "11/3 Uncle Chuck's Chicken Noodle Soup";
			NumElements = mealArray.Length;
		}

		public string getObjectInPosition(int position)
		{
			return this.mealArray [position];
		}
	}
}