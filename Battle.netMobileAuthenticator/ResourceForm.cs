﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.ComponentModel;
using System.Security;
using System.Windows.Forms;

using MetroFramework.Components;
using MetroFramework.Drawing;
using MetroFramework.Interfaces;
using System.Resources;

namespace Battle.netMobileAuthenticator
{
    [Designer("MetroFramework.Design.Controls.MetroLabelDesigner, " + MetroFramework.AssemblyRef.MetroFrameworkDesignSN)]
    [ToolboxBitmap(typeof(Form))]
    public partial class ResourceForm : MetroFramework.Forms.MetroForm
    {
		class BasedOnComparer : IEqualityComparer<Type>
		{
			/// <summary>
			/// Check if two types are equal or subclassed
			/// </summary>
			/// <param name="t1"></param>
			/// <param name="t2"></param>
			/// <returns>true if equal or subclassed</returns>
			public bool Equals(Type t1, Type t2)
			{
				return (t1 == t2 || t2.IsSubclassOf(t1));
			}

			/// <summary>
			/// Get the hash code for a Type
			/// </summary>
			/// <param name="t"></param>
			/// <returns></returns>
			public int GetHashCode(Type t)
			{
				return t.GetHashCode();
			}
		}

		/// <summary>
		/// Create the new form
		/// </summary>
		public ResourceForm()
			: base()
		{
		}

		/// <summary>
		/// Search the resources for any _FORMNAME_CONTROLNAME_ strings and replace the text
		/// </summary>
		/// <param name="e"></param>
		/*protected override void OnLoad(EventArgs e)
		{
			// go through all controls and set any text from resources (including this form)
			//var controls = GetControls(this, new Type[] { typeof(MetroFramework.Controls.MetroLabel), typeof(MetroFramework.Controls.MetroCheckBox) });
			var controls = GetControls(this);

			string formname = "_" + this.Name + "_";
			string text = WinAuthMain.StringResources.GetString(formname, System.Threading.Thread.CurrentThread.CurrentCulture);
			if (text != null)
			{
				this.Text = text;
			}
			foreach (Control c in controls)
			{
				string controlname = formname + c.Name + "_";
				text = WinAuthMain.StringResources.GetString(controlname, System.Threading.Thread.CurrentThread.CurrentCulture);
				if (text != null)
				{
					if (c is MetroFramework.Controls.MetroTextBox)
					{
						((MetroFramework.Controls.MetroTextBox)c).PromptText = text;
					}
					else
					{
						c.Text = text;
					}
				}
			}

			base.OnLoad(e);
		}*/

		/// <summary>
		/// Get a recursive list of all controls that match the array of types
		/// </summary>
		/// <param name="control">parent control</param>
		/// <param name="controlTypes">array of types to match</param>
		/// <param name="controls">existing list of Controls to add to</param>
		/// <returns>list of Control objects</returns>
		private List<Control> GetControls(Control control, Type[] controlTypes = null, List<Control> controls = null)
		{
			if (controls == null)
			{
				controls = new List<Control>();
			}
			BasedOnComparer baseComparer = new BasedOnComparer();
			foreach (Control c in control.Controls)
			{
				if (controlTypes == null || controlTypes.Contains(c.GetType(), baseComparer) == true)
				{
					controls.Add(c);
				}
				if (c.Controls != null && c.Controls.Count != 0)
				{
					GetControls(c, controlTypes, controls);
				}
			}

			return controls;
		}
	}
}
