using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jlr_sample
{
    internal class Logger
    {
        private static Logger instance = new Logger();
        public Logger() { }
        static public Logger Instance { get { return instance; } }
        public void Info(string message) { this.WriteLine(DateTime.Now.ToString("f") + ": (I) " + message); }
        public void Warn(string message) { this.WriteLine(DateTime.Now.ToString("f") + ": (W) " + message); }
        public void Error(string message) { this.WriteLine(DateTime.Now.ToString("f") + ": (E) " + message); }
        public void Fatal(string message) { this.WriteLine(DateTime.Now.ToString("f") + ": (F) " + message); }
        public void Debug(string message) { this.WriteLine(DateTime.Now.ToString("f") + ": (D) " + message); }
        private void WriteLine(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
            // Console.WriteLine(message);
        }

    }
}
