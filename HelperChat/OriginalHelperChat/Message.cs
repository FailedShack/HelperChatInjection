// Decompiled with JetBrains decompiler
// Type: HelperChat.Message
// Assembly: HelperChat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A0F4DAA2-3648-41D8-8BD0-F73040611929
// Assembly location: E:\Programme\Wii U USB Helper\z_HelperChat.dll

using Newtonsoft.Json;
using System;
using System.Text;

namespace HelperChat
{
  public class Message
  {
    public Message(string channel, string sender, string payload)
    {
      this.DestinationChannel = channel;
      this.Payload = payload;
      this.Sender = sender;
      this.TimeStamp = ((int) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString();
    }

    public string DestinationChannel { get; set; }

    public string Payload { get; set; }

    public string Sender { get; set; }

    public string TimeStamp { get; set; }

    public static Message Deserialize(byte[] data)
    {
      try
      {
        return JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(data));
      }
      catch
      {
        return (Message) null;
      }
    }

    public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
      return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimeStamp).ToLocalTime();
    }

    public byte[] Serialize()
    {
      try
      {
        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((object) this));
      }
      catch
      {
        return (byte[]) null;
      }
    }

    public string ToHtmlString()
    {
      try
      {
        return string.Format("<html><span style=\"color:#C1C6DC\">{0} ({1})</span>:<span><strong>{2}</strong></span></html>", (object) Message.UnixTimeStampToDateTime(double.Parse(this.TimeStamp)).ToShortTimeString(), (object) this.Sender, (object) this.Payload);
      }
      catch
      {
        return "";
      }
    }

    public override string ToString()
    {
      try
      {
        DateTime dateTime = Message.UnixTimeStampToDateTime(double.Parse(this.TimeStamp));
        return string.Format("{0}: ({1}) {2}", (object) (dateTime.ToString("MM-dd") + " " + dateTime.ToShortTimeString()), (object) this.Sender, (object) this.Payload);
      }
      catch
      {
        return "";
      }
    }
  }
}
