using System.IO;

namespace com.superneko.medlay.Core.Internal
{
    internal class Dump : System.IDisposable
    {
        StreamWriter writer;

        public Dump(string filePath)
        {
            var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            var streamWriter = new StreamWriter(fileStream);
            writer = streamWriter;
        }

        internal void Log(string message)
        {
            writer.WriteLine(message);
        }

        internal void Trace()
        {
            var stackTrace = new System.Diagnostics.StackTrace(1, true);
            writer.WriteLine("Date: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            writer.WriteLine("Stack Trace:");
            writer.WriteLine(stackTrace.ToString());
        }

        public void Dispose()
        {
            writer.Flush();
            writer.Close();
            writer = null;
        }
    }
}
