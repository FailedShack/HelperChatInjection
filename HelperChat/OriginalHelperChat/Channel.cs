// Decompiled with JetBrains decompiler
// Type: HelperChat.Channel
// Assembly: HelperChat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A0F4DAA2-3648-41D8-8BD0-F73040611929
// Assembly location: E:\Programme\Wii U USB Helper\z_HelperChat.dll

using System.Collections.Generic;

namespace HelperChat
{
  public class Channel
  {
    public static Dictionary<string, Channel> Channels = new Dictionary<string, Channel>();
    private static object _staticThreadLocker = new object();
    private object _instanceThreadLocker = new object();

    public Channel(string name)
    {
      this.Clients = new List<Client>();
      this.Name = name;
    }

    public int ClientCount
    {
      get
      {
        return this.Clients.Count;
      }
    }

    public string Name { get; }

    public static Channel GetChannel(string name)
    {
      lock (Channel._staticThreadLocker)
      {
        name = name.ToLowerInvariant();
        if (!Channel.Channels.ContainsKey(name))
          Channel.Channels.Add(name, new Channel(name));
        return Channel.Channels[name];
      }
    }

    public void AddClient(Client client)
    {
      lock (this._instanceThreadLocker)
      {
        if (this.Clients.Contains(client))
          return;
        this.Clients.Add(client);
      }
      foreach (Message message in this.MessageHistory.ToArray())
        client.SendMessage(message);
    }

    public void PublishMessage(Message message)
    {
      lock (this._instanceThreadLocker)
      {
        this.MessageHistory.Add(message);
        if (this.MessageHistory.Count > 50)
          this.MessageHistory.RemoveAt(0);
      }
      foreach (Client client in this.Clients.ToArray())
        client.SendMessage(message);
    }

    public void RemoveClient(Client client)
    {
      lock (this._instanceThreadLocker)
      {
        if (!this.Clients.Contains(client))
          return;
        this.Clients.Remove(client);
      }
    }

    private List<Client> Clients { get; }

    private List<Message> MessageHistory { get; } = new List<Message>();
  }
}
