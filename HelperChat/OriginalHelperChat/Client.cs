// Decompiled with JetBrains decompiler
// Type: HelperChat.Client
// Assembly: HelperChat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A0F4DAA2-3648-41D8-8BD0-F73040611929
// Assembly location: E:\Programme\Wii U USB Helper\z_HelperChat.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace HelperChat
{
  public class Client
  {
    private NetworkStream _baseStream;
    private DateTime _lastContact;
    private SslStream _protectedStream;
    private TcpClient _socket;

    public Client(TcpClient socket, bool server, string certificate_file = null, string certficatePassword = null)
    {
      Client client = this;
      this._lastContact = DateTime.Now;
      Task.Run((Action) (() =>
      {
        try
        {
          client._socket = socket;
          client._baseStream = client._socket.GetStream();
          client._protectedStream = new SslStream((Stream) client._baseStream);
          if (server)
            client._protectedStream.AuthenticateAsServer((X509Certificate) new X509Certificate2(certificate_file, certficatePassword));
          else
            client._protectedStream.AuthenticateAsClient("chat.wiiuusbhelper.com");
          if (!client._protectedStream.IsAuthenticated || !client._protectedStream.IsEncrypted)
            client.DestroyClient();
          else
            client.ClientReady = true;
        }
        catch (Exception ex)
        {
          client.DestroyClient();
          Console.WriteLine("CLIENT EXCEPTION+" + ex.ToString());
        }
      }));
    }

    public event EventHandler ClientDestroyed;

    public event EventHandler<Message> MessageReceived;

    public bool ClientReady { get; set; }

    public bool Destroyed { get; private set; }

    public int InfringementCount { get; set; }

    public DateTime LastMessageTime { get; set; } = DateTime.MinValue;

    public IPAddress RemoteIp
    {
      get
      {
        return ((IPEndPoint) this._socket.Client.RemoteEndPoint).Address;
      }
    }

    public void CheckDataStream()
    {
      if ((DateTime.Now - this._lastContact).TotalMinutes >= 30.0)
      {
        this.DestroyClient();
      }
      else
      {
        this._baseStream.ReadTimeout = 1000;
        if (this.Destroyed || !this.ClientReady || !this._baseStream.DataAvailable)
          return;
        byte[] buf = new byte[4];
        this._protectedStream.ReadStrictBytes(buf, 4);
        int int32 = BitConverter.ToInt32(buf, 0);
        if (int32 > 32000)
        {
          this.DestroyClient();
        }
        else
        {
          byte[] numArray = new byte[int32];
          this._protectedStream.ReadStrictBytes(numArray, int32);
          Message e = Message.Deserialize(numArray);
          if (e == null || string.IsNullOrEmpty(e.Payload) || (string.IsNullOrEmpty(e.Sender) || string.IsNullOrEmpty(e.TimeStamp)))
            return;
          this._lastContact = DateTime.Now;
          Channel channel = Channel.GetChannel(e.DestinationChannel);
          if (e.Payload[0] == '!')
          {
            string payload = e.Payload;
            if (!(payload == "!HELO"))
            {
              if (!(payload == "!GBYE"))
              {
                if (!(payload == "!FRWL"))
                {
                  if (!(payload == "!CONT"))
                    return;
                  this.SendMessage(new Message("INFO_CLIENT_COUNT", "SERVER", channel.ClientCount.ToString()));
                }
                else
                  this.DestroyClient();
              }
              else
                this.UnsubscribeToChannel(channel);
            }
            else
              this.SubscribeToChannel(channel);
          }
          else
          {
            this.SubscribeToChannel(channel);
            // ISSUE: reference to a compiler-generated field
            EventHandler<Message> messageReceived = this.MessageReceived;
            if (messageReceived == null)
              return;
            messageReceived((object) this, e);
          }
        }
      }
    }

    public void DestroyClient()
    {
      try
      {
        if (this.Destroyed)
          return;
        this.Destroyed = true;
        // ISSUE: reference to a compiler-generated field
        EventHandler clientDestroyed = this.ClientDestroyed;
        if (clientDestroyed != null)
          clientDestroyed((object) this, (EventArgs) null);
        this._baseStream.Dispose();
        this._protectedStream.Dispose();
        this._socket.Close();
        foreach (Channel channel in this.Channels.ToArray())
          this.UnsubscribeToChannel(channel);
      }
      catch
      {
      }
    }

    public void SendMessage(Message message)
    {
      try
      {
        if (this.Destroyed)
          return;
        byte[] buffer = message.Serialize();
        this._protectedStream.Write(BitConverter.GetBytes(buffer.Length), 0, 4);
        this._protectedStream.Write(buffer, 0, buffer.Length);
        this._protectedStream.Flush();
      }
      catch (Exception ex)
      {
        this.DestroyClient();
      }
    }

    public void StartAsyncListen()
    {
      Task.Run((Action) (() =>
      {
        try
        {
          while (!this.Destroyed)
          {
            this.CheckDataStream();
            Thread.Sleep(30);
          }
        }
        catch (Exception ex)
        {
          this.DestroyClient();
        }
      }));
    }

    private List<Channel> Channels { get; } = new List<Channel>();

    private void SubscribeToChannel(Channel channel)
    {
      if (this.Channels.Contains(channel))
        return;
      this.Channels.Add(channel);
      channel.AddClient(this);
    }

    private void UnsubscribeToChannel(Channel channel)
    {
      if (!this.Channels.Contains(channel))
        return;
      this.Channels.Remove(channel);
      channel.RemoveClient(this);
    }
  }
}
