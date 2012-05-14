using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ziyi
{
    public class RepeatTimer : System.Windows.Threading.DispatcherTimer
    {
        public bool IsFirstTick { get; set; }
        public TimeSpan InitialDelay { get; set; }
        public TimeSpan RepeatRate { get; set; }
        public RepeatTimer()
            : base()
        {
            int delay; 
            if (Properties.Settings.Default.RepeatInitialDelay <= 0)
                delay = 250 + System.Windows.SystemParameters.KeyboardDelay * 250; 
            else 
                delay = Properties.Settings.Default.RepeatInitialDelay; 

            this.InitialDelay = new TimeSpan(0, 0, 0, 0, delay); 

            int rate;
            if (Properties.Settings.Default.RepeatRate <= 0)
                rate = 1000 / (System.Windows.SystemParameters.KeyboardSpeed + 2); 
            else
                rate = 1000 / (Properties.Settings.Default.RepeatRate + 2); 
            this.RepeatRate = new TimeSpan(0, 0, 0, 0, rate);

            this.IsFirstTick = true;
            this.Interval = this.InitialDelay; 
            this.Tick += new EventHandler(RepeatTimer_Tick);
        }

        void RepeatTimer_Tick(object sender, EventArgs e)
        {
            if (!this.IsFirstTick)
            {
                //this.Interval = this.RepeatRate;
            }
            else
                this.IsFirstTick = false;

        }
    }
}
