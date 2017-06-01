using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using ThomasClaudiusHuber.EventHub.Receiver.DataAccess;

namespace ThomasClaudiusHuber.EventHub.Receiver.ViewModel
{
  public class MainViewModel : ViewModelBase
  {
    private string _receivedEvents="";
    private bool _isStarting;
    private int _selectedHoursAgoToStartFrom;
    private string _connectionString = "";
    private readonly EventHubSubscriber _eventHubSubscriber;
    private static readonly object _lock = new object();
    public MainViewModel(EventHubSubscriber eventHubSubscriber)
    {
      _eventHubSubscriber = eventHubSubscriber;
      _eventHubSubscriber.EventReceived += EventHubAccess_SensorDataReceived;

      StartReceivingEventsCommand = new DelegateCommand(OnStartReceivingEventsExecute, OnStartReceivingEventsCanExecute);

      var hoursAgoToStartFrom = new List<int>();
      for (int i = 0; i >= -24; i--)
      {
        hoursAgoToStartFrom.Add(i);
      }
      HoursAgoToStartFromList = hoursAgoToStartFrom;
    }

    public string ReceivedEvents
    {
      get { return _receivedEvents; }
      set
      {
        _receivedEvents = value;
        OnPropertyChanged();
      }
    }

    private IEnumerable<int> _hoursAgoToStartFromList;

    public IEnumerable<int> HoursAgoToStartFromList
    {
      get { return _hoursAgoToStartFromList; }
      set { _hoursAgoToStartFromList = value; }
    }

    public ICommand StartReceivingEventsCommand { get; }

    public int SelectedHoursAgoToStartFrom
    {
      get { return _selectedHoursAgoToStartFrom; }
      set
      {
        _selectedHoursAgoToStartFrom = value;
        OnPropertyChanged();
      }
    }

    public string ConnectionString
    {
      get { return _connectionString; }
      set
      {
        _connectionString = value;
        OnPropertyChanged();
      }
    }

    private bool OnStartReceivingEventsCanExecute()
    {
      return !_isStarting;
    }

    private void OnStartReceivingEventsExecute()
    {
      _isStarting = true;
      ((DelegateCommand)StartReceivingEventsCommand).RaiseCanExecuteChanged();
      try
      {
        _eventHubSubscriber.SubscribeToEvents(ConnectionString, SelectedHoursAgoToStartFrom);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
      finally
      {
        _isStarting = false;
        ((DelegateCommand)StartReceivingEventsCommand).RaiseCanExecuteChanged();
      }
    }

    private void EventHubAccess_SensorDataReceived(object sender, string data)
    {
      lock (_lock)
      {
        ReceivedEvents = ReceivedEvents.Insert(0, data + Environment.NewLine);
      }
    }
  }
}
