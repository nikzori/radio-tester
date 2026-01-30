using System.Collections.ObjectModel;
using System.IO.Ports;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Terminal.Gui.Drivers;

public class TerminalMain : Window
{
    ObservableCollection<string> messages = new();
    // Controls
    FrameView ControlPanel = new() { Width = Dim.Percent(20), Height = Dim.Auto(),BorderStyle = LineStyle.Dotted};
    Label PortsLabel = new() { Width = Dim.Auto(), Height = 1, Text = "Port:"};
    ComboBox PortList = new() { Width = 10, Height = 3, ReadOnly = true};

    // Log
    ScrollBar scrollBar = new() {X = Pos.AnchorEnd(), AutoShow = true, ScrollableContentSize = 100, Height = Dim.Fill()};
    ListView Log = new() { Width = Dim.Percent(80), Height = Dim.Fill(), X = Pos.AnchorEnd() - 1, BorderStyle = LineStyle.Dotted, ViewportSettings = ViewportSettingsFlags.AllowNegativeY };
    public TerminalMain()
    {
        Device.ResponseReceived += OnResponseReceived;
        Device.TextMessageReceived += OnMessageReceived;
        this.Width = Dim.Fill();
        this.Height = Dim.Fill();
        this.BorderStyle = LineStyle.None;
        
        // Controls init
        ControlPanel.Padding?.Thickness = new(1);
        PortList.Y =  Pos.Bottom(PortsLabel);
        PortList.Source = new ListWrapper<string>(new ObservableCollection<string>(SerialPort.GetPortNames()));

        ControlPanel.Add(PortList, PortsLabel);

        // Log init
        Log.SetSource(messages);
        Log.Padding?.Thickness = new(1);
        scrollBar.ValueChanged += (_, e) => {Log.Viewport = Log.Viewport with {Y = e.NewValue};};
        Log.ViewportChanged += (_, e) => {scrollBar.Value = e.NewViewport.Y;};
        

        this.Add(ControlPanel, Log, scrollBar);
        Cursor = new Cursor {Style = CursorStyle.Hidden};

        // Debug and other temp objects
        Button test = new() {Width = Dim.Auto(), Text = "Test", Y = Pos.Bottom(PortList)};
        test.Accepting += (s, e) => {AddLog("Sending test value to serial port"); Device.Connect(PortList.Text); Device.SendMessage("test"); e.Handled = true;};
        ControlPanel.Add(test);
        AddLog("Hello, World!");
    }

    public void AddLog(string text)
    {
        messages.Add($"\n[{DateTime.Now}] {text}");
        scrollBar.ScrollableContentSize = messages.Count;
    }

    void OnResponseReceived(object sender, SerialResponseEventArgs e)
    {
        AddLog("Incoming message: " + BitConverter.ToString(e.Message));
    }
    void OnMessageReceived(object sender, TextLogEventArgs e)
    {
        AddLog("Incoming message: " + e.Message);
    }
}