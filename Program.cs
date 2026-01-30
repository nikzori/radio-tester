using Terminal.Gui.App;
using Terminal.Gui.Input;


AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);
Device.Init();

using (IApplication app = Application.Create())
{
    app.Init();
    Application.QuitKey = Key.Q.WithCtrl;
    

    TerminalMain MainWindow = new();
    app.Run(MainWindow);
    MainWindow.Dispose();   
}
Device.port.Close();
Console.WriteLine("BaseDirectory: " + AppContext.BaseDirectory);
Console.WriteLine("Buh-bye!~");



static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
{
    Device.port.Close();
    Exception? ex = e.ExceptionObject as Exception;
    string LogFilePath = AppContext.BaseDirectory + "ErrorLog.txt";
    try { File.WriteAllText(LogFilePath, $"[{DateTime.Now}] Unhandled exception: {ex?.Message}\nStack trace: {ex?.StackTrace}\n"); }
    catch (Exception logEx) { Console.WriteLine($"Error logging unhandled exception: {logEx.Message}\n"); }
    finally { Console.WriteLine($"[{DateTime.Now}] Unhandled exception: {ex?.Message}\nStack trace: {ex?.StackTrace}"); }
}