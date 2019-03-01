#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.Gui.Tools;
using System.Windows.Forms;
using System.IO;
#endregion


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 	Addon			:	Chart Debugger Window																		//
//	Description		:	Helps debugging the indicators and strategies												//
//	Author			:	Puvox Software (contact@puvox.software)	: Tazo Todua										//
//	URL				:	https://puvox.software/blog/chart-debugger-window-printbox-for-ninjatrader/					//
//	History																											//
//		01.03.2019	: 	1.00	Initial version 																	//
//	License			: 	Apache																						//
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


 #region Extend the Strategy & Indicator classes
namespace NinjaTrader.NinjaScript.Strategies
{
	
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{ 
		Library_PuvoxSoftware.DebugPrint DP_; 
		 
		public virtual void PrintBox(object a1)
		{ 
			if (DP_ == null)
			{
				if ( ChartControl != null ) 
				{ 
					DP_ = new Library_PuvoxSoftware.DebugPrint();
					DP_.init_or_deinit(this, true);
				}
			}
			DP_.PRINT(a1); 
		}
		
		// overloads
		public virtual void PrintBox(object a1, object a2) 							{ PrintBox( (a1==null? "null": a1.ToString()) + " | " +  (a2==null? "null": a2.ToString()) ); }
		public virtual void PrintBox(object a1, object a2, object a3)				{ PrintBox( (a1==null? "null": a1.ToString()) + " | " +  (a2==null? "null": a2.ToString()) + " | " +  (a3==null? "null": a3.ToString()) );	}
		public virtual void PrintBox(object a1, object a2, object a3, object a4)	{ PrintBox( (a1==null? "null": a1.ToString()) + " | " +  (a2==null? "null": a2.ToString()) + " | " +  (a3==null? "null": a3.ToString())+ " | " +  (a4==null? "null": a4.ToString()) ); }
	
	}
}


namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		Library_PuvoxSoftware.DebugPrint DP_; 
		 
		public virtual void PrintBox(object a1)
		{ 
			if (DP_ == null)
			{
				if ( ChartControl != null ) 
				{ 
					DP_ = new Library_PuvoxSoftware.DebugPrint();
					DP_.init_or_deinit(this, true);
				}
			}
			DP_.PRINT(a1); 
		}
		
		// overloads
		public virtual void PrintBox(object a1, object a2) 							{ PrintBox( (a1==null? "null": a1.ToString()) + " | " +  (a2==null? "null": a2.ToString()) ); }
		public virtual void PrintBox(object a1, object a2, object a3)				{ PrintBox( (a1==null? "null": a1.ToString()) + " | " +  (a2==null? "null": a2.ToString()) + " | " +  (a3==null? "null": a3.ToString()) );	}
		public virtual void PrintBox(object a1, object a2, object a3, object a4)	{ PrintBox( (a1==null? "null": a1.ToString()) + " | " +  (a2==null? "null": a2.ToString()) + " | " +  (a3==null? "null": a3.ToString())+ " | " +  (a4==null? "null": a4.ToString()) ); }
	
	}
}
#endregion


#region DEBUGGER class
namespace Library_PuvoxSoftware
{ 
		
	//====debugger ====//
	// in OnStateChange():	d.init_or_deinit(this, true); 
	// OnBarUpdate:			d.PRINT(...);

	public class DebugPrint
	{
		public NinjaTrader.NinjaScript.Strategies.Strategy strat;
		public NinjaTrader.NinjaScript.Indicators.Indicator indi;
		public NinjaTrader.NinjaScript.NinjaScriptBase indi_or_strat;
		
		
		ChartControl chartControl;
		ChartPanel chartPnl;
		ChartBars chartBars;
		bool enabled;

		public void init_or_deinit(object indi_or_strat_, bool init)
		{
			if (init && indi_or_strat==null)
			{
				indi_or_strat = (NinjaTrader.NinjaScript.NinjaScriptBase)  indi_or_strat_;
				 
				if (indi_or_strat_ is NinjaTrader.NinjaScript.IndicatorBase)  {
					indi= (NinjaTrader.NinjaScript.Indicators.Indicator) indi_or_strat_;
					chartControl= indi.ChartControl;
					chartPnl	= indi.ChartControl.ChartPanels[0];
					chartBars	= indi.ChartBars;
				} 
				else if (indi_or_strat_ is NinjaTrader.NinjaScript.Strategies.Strategy)  {  
					strat=(NinjaTrader.NinjaScript.Strategies.Strategy)  indi_or_strat_;
					chartControl= strat.ChartControl;
					chartPnl	= strat.ChartControl.ChartPanels[0];
					chartBars	= strat.ChartBars;
				} 
				
				if (chartControl == null || chartPnl == null) { 
					enabled=false; return;
				}

				enabled = true;
				registerEventHandlers();
			}
			else
			{
				checkForDeregister();
			}  
		}
		
	
		//
		public void registerEventHandlers()
		{ 
			try
			{
				chartControl.Dispatcher.InvokeAsync(() =>
            	{
					chartPnl.MouseDown +=   this.OnMouseDownEvent ;//new MouseButtonEventHandler(midasMouseEvents);
					chartPnl.KeyDown += this.onKeyDown; 
					createForm();
				}); 
			}
			catch(Exception e)   { }
		}
		public void deRegisterEventHandlers()
		{
			chartPnl.MouseDown -= this.OnMouseDownEvent;
			chartPnl.KeyDown -= this.onKeyDown;  
			if (logForm != null) 
				logForm.Resize  -= resizeTextarea; 
			destroyForm(true); 
		}
		 
		public bool checkForDeregister()
		{
			if (enabled ) 
			{
				if (indi_or_strat == null || indi_or_strat.State == State.Finalized)
				{
					if (chartPnl!=null)
					{
						deRegisterEventHandlers(); 
						enabled=false;
						return true;
					}
				}
			}
			return false;
		}
		
		public Dictionary<string, string> dataDict = new Dictionary<string,string>();
		public static Form lastForm;
		public void createForm()
		{
			
			logForm = new Form();
			logForm.Width = (lastForm != null ? lastForm.Width : 310);
			logForm.Height = (lastForm != null ? lastForm.Height : 450);
			logForm.Resize  += resizeTextarea  ; 
			logForm.Text = @"Debug Print";
			logForm.TopMost = true; 
			logForm.Opacity = 0.95; 
			lastForm = logForm;
			//
			textarea= new System.Windows.Forms.TextBox(); 
			logForm.Controls.Add( textarea);
			textarea.Multiline = true;
			textarea.ScrollBars = ScrollBars.Both;
			resizeTextarea(null,null);
			//textarea.Font = new SimpleFont(textarea.Font.FontFamily, 11); 
		}
		public void destroyForm(bool dispose)
		{
			if (logForm != null) logForm.Close();
			if (dispose) logForm.Dispose();
			logForm = null; 
		}
		
		
		public void resizeTextarea(object sender, System.EventArgs e)
		{
			int scrollbar = 17;
			textarea.Width = textarea.Parent.Width-scrollbar;
			textarea.Height = textarea.Parent.Height-scrollbar*2; 
		}
 
		 
		System.Windows.Forms.Form logForm ; 
		System.Windows.Forms.TextBox textarea;

		DateTime lastPressTime = DateTime.MinValue; 
		bool isKeyPressed= false;
		
		private void onKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
			//isKeyPressed =false; if (e.KeyboardDevice.IsKeyDown(Key.LeftShift)) { isKeyPressed =true; lastPressTime=DateTime.Now; }  
		}
		//private void onKeyUp(object sender, KeyEventArgs e) { isKeyPressed=false;   Print("keyup");  }
		
		private void OnMouseDownEvent(object sender, MouseButtonEventArgs e)
		{
			if (checkForDeregister()) return;
			
			Point  cPos = chartControl.MouseDownPoint;
			int    posX = (int)ChartingExtensions.ConvertToHorizontalPixels(cPos.X-chartControl.BarWidthArray[0], chartControl.PresentationSource);
			int    slot = (int)chartControl.GetSlotIndexByX(posX);

			//if clicked on visible
			if(slot >= chartBars.FromIndex && slot <= chartBars.ToIndex) 
			{ 
				int    clicked_Bar = slot;

				if (System.Windows.Forms.Control.ModifierKeys == Keys.Shift)  //isKeyPressed
				{
					//isKeyPressed=false;
					destroyForm(false);  
					createForm();
					logForm.Show(); 
				}
				if( logForm.Visible )
				{
					if (indi_or_strat !=null )
					{
						if (indi_or_strat.BarsArray[0] != null)
						{ 
							Bars bars = indi_or_strat.BarsArray[0];
							
							int barIdx = indi_or_strat.CurrentBars[0]-clicked_Bar-1;  //indi.CurrentBars[0]
							string bn = Library_PuvoxSoftware.Methods.barIdentifier( indi_or_strat, barIdx );
							//IBar thebar = GetBarFromX(e.X);
							textarea.Text =  dataDict.ContainsKey(bn) ? dataDict[ bn ] : "";
							DrawVerticalLine( bars.GetTime(clicked_Bar+1) );
						}
					}
				} 
			}
		}
		 
		
		public void PRINT(object text1) { PRINT(text1, ""); }
		public void PRINT(object text1, object text2)
		{
			if (!enabled) return;
			if (checkForDeregister()) return; 
			if(indi_or_strat.CurrentBars[0] <1  || indi_or_strat.CurrentBar <1 ) return;
			string bn = Library_PuvoxSoftware.Methods.barIdentifier( indi_or_strat, 0 );   
			DateTime barTime = indi_or_strat.Times[0][0]; //BarsArray[0].GetTime(barNumb);
			dataDict[bn] = 
				(dataDict.ContainsKey(bn) ?  dataDict[bn] : barTime.ToString("HH:mm:ss   dd/MMM/yyyy") + " [ BarN: "+indi_or_strat.CurrentBars[0] + "]" + Environment.NewLine + "----------------------------"  )
				+ Environment.NewLine;
			if ( indi_or_strat.CurrentBars.Length > 1 && indi_or_strat.BarsInProgress != 0 )
				dataDict[bn] +="[ "+indi_or_strat.Time[0].ToString("dd MMM>HH:mm:ss ") + " n:"+ indi_or_strat.CurrentBar +" ] ";

			dataDict[bn] += text1 == null ? "null" : (text1 is double ? ((double)text1).ToString("N6") : text1.ToString() );
			dataDict[bn] += text2 == null ? "null" : (text2 is double ? ((double)text2).ToString("N6") : text2.ToString() );
			indi_or_strat.Print("a3");
		}
   
		
		
		int randLast; 
		
		private void DrawVerticalLine(DateTime time)
		{
			try{
				int rand = new Random().Next(10000);
				var IRB = (indi_or_strat as NinjaTrader.Gui.NinjaScript.IndicatorRenderBase);
				try { IRB.RemoveDrawObject("Debugger_CurrentBarVLine"+randLast); } catch{}
				
				NinjaTrader.NinjaScript.DrawingTools.Draw.VerticalLine(indi_or_strat, "Debugger_CurrentBarVLine"+rand, time , Brushes.Orange, DashStyleHelper.DashDot, 2);
				// in 5 seconds, remove it
				System.Threading.Timer timer = null;
				timer = new System.Threading.Timer(
				    (object state) => { 
						IRB.RemoveDrawObject("Debugger_CurrentBarVLine"+rand);
						timer.Dispose();
					}
				    , null // no state required
				    ,TimeSpan.FromSeconds(5) // Do it in x seconds
				    ,TimeSpan.FromMilliseconds(-1)); // don't repeat
				randLast = rand; 
				
			}
			catch(Exception ex)
			{
				indi_or_strat.Print(ex.ToString());
			}
		}
		
		
	}  
	
}
	
#endregion



#region helpers
namespace Library_PuvoxSoftware 
{
	public partial class Methods   
	{
		public static string barIdentifier( object NinjaScript_, int idx){
			NinjaTrader.NinjaScript.NinjaScriptBase obj = (NinjaTrader.NinjaScript.NinjaScriptBase) NinjaScript_;
			if (obj.CurrentBars[0] <1) return "";
			return sanitize_filename( getNinjaScriptIdentifier(obj, true) + "--" + obj.BarsArray[0].GetTime(obj.CurrentBars[0]-idx).ToString("yyyy-MM-dd_HH-mm-ss") , "");
		}  
		
		public static string getNinjaScriptIdentifier( object NinjaScript_ , bool include_script_name){
			NinjaTrader.NinjaScript.NinjaScriptBase obj = (NinjaTrader.NinjaScript.NinjaScriptBase) NinjaScript_;
			return sanitize_filename((include_script_name ? obj.Name + "_" : "" ) + obj.Instrument.FullName + "--" + obj.BarsArray[0].BarsPeriod.BarsPeriodType.ToString() +"-"+obj.BarsArray[0].BarsPeriod.Value.ToString(), "") ; 
		}
	
	    public static string sanitize_filename(string dirtyString, string sanitizeType)
        {
			string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
			foreach (char c in invalid)
			{
			    dirtyString = dirtyString.Replace(c.ToString(), ""); 
			}
            return dirtyString;
        }
	}
}
#endregion



#region ShortHands 
namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{ 
		public virtual void P(params object[] obj){   PrintBox(obj); }
	}
}


namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		public virtual void P(params object[] obj){   PrintBox(obj); }
	}
}
#endregion