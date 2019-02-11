
namespace LogViewer.Lib
{
    public enum LogEntryLevel
    {
        //ULS trace levels value - http://msdn.microsoft.com/en-us/library/office/ff604025(v=office.14).aspx
        Unexpected = 10,
        Assert = 10,
        Monitorable = 15,
        High = 20,
        Critical = 30,
        Error = 40,
        Medium = 50,
        Warning = 55, //set from 50 to 55 for better searchability
        Information = 80,   
        Verbose = 100,
    }
}
