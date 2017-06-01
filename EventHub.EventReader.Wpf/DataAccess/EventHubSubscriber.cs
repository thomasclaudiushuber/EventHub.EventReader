using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using System.IO;

namespace ThomasClaudiusHuber.EventHub.Receiver.DataAccess
{
  public class EventHubSubscriber
  {
    private List<Task> _tasks;
    private CancellationTokenSource _cts;
    private CancellationToken _token;

    public void SubscribeToEvents(string connectionString, int hoursAgoToStartFrom)
    {
      if (hoursAgoToStartFrom > 0)
      {
        throw new ArgumentException("Should contain zero or a negative value", nameof(hoursAgoToStartFrom));
      }
      if (_cts != null)
      {
        _cts.Cancel();
      }

      _cts = new CancellationTokenSource();
      _token = _cts.Token;

      List<EventHubReceiver> receivers = CreateEventHubReceivers(connectionString, hoursAgoToStartFrom);

      StartTaskForEachReceiver(receivers);
    }

    private static List<EventHubReceiver> CreateEventHubReceivers(string connectionString, int hoursAgoToStartFrom)
    {
      var client = EventHubClient.CreateFromConnectionString(connectionString);

      EventHubConsumerGroup consumerGroup = client.GetDefaultConsumerGroup();
      string[] partitionIds = client.GetRuntimeInformation().PartitionIds;

      List<EventHubReceiver> receivers =
      partitionIds.Select(
        partitionId => consumerGroup.CreateReceiver(partitionId, DateTime.UtcNow.AddHours(hoursAgoToStartFrom))).ToList();
      return receivers;
    }

    private void StartTaskForEachReceiver(List<EventHubReceiver> receivers)
    {
      _tasks = new List<Task>();
      foreach (var receiver in receivers)
      {
        var task = Task.Run(() =>
        {
          while (true)
          {
            try
            {
              if (_token.IsCancellationRequested)
              {
                break;
              }

              ReceiveEventFromReceiver(receiver);
            }
            catch (Exception ex)
            {
              Debug.WriteLine(ex.Message);
            }
          }

        }, _token);
        _tasks.Add(task);
      }
    }

    private void ReceiveEventFromReceiver(EventHubReceiver receiver)
    {
      var message = receiver.Receive();

      if (message != null)
      {
        string body = Encoding.UTF8.GetString(message.GetBytes());
        OnEventReceived(body);
      }
    }

    public event EventHandler<string> EventReceived;

    protected virtual void OnEventReceived(string data)
    {
      EventReceived?.Invoke(this, data);
    }
  }
}
