using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LogViewer
{
    public static class ExtensionMethods
    {
        public static int GetSortDescriptionIndex(this SortDescriptionCollection sortDescriptions, string propertyName)
        {
            for(int i = 0; i < sortDescriptions.Count; i++)
            {
                var desc = sortDescriptions[i];
                if(desc.PropertyName == propertyName)
                {
                    return i;
                }
            }

            return -1;        
        }
    }
}
