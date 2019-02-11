using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LogViewer.Lib
{
    public static class ExtensionMethods
    {
        const string _filename = "errors.log";

        public static void Sort<T>(this ObservableCollection<T> collection) where T : IComparable<T>
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null.");

            for (var startIndex = 0; startIndex < collection.Count - 1; startIndex += 1)
            {
                var indexOfSmallestItem = startIndex;
                for (var i = startIndex + 1; i < collection.Count; i += 1)
                    if (collection[i].CompareTo(collection[indexOfSmallestItem]) < 0)
                        indexOfSmallestItem = i;
                if (indexOfSmallestItem != startIndex)
                    collection.Move(indexOfSmallestItem, startIndex);
            }
        }

        public static void LogException(this Exception ex, string message = "")
        {
            using (var file = File.AppendText(_filename))
            {
                file.WriteLine("[{0}] Exception={1}, Message={2}", DateTime.Now, ex.Message, message);
                file.WriteLine("Stacktrace:");
                file.WriteLine(ex.StackTrace);
            }
        }

    }
}
