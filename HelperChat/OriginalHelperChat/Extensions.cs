// Decompiled with JetBrains decompiler
// Type: HelperChat.Extensions
// Assembly: HelperChat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A0F4DAA2-3648-41D8-8BD0-F73040611929
// Assembly location: E:\Programme\Wii U USB Helper\z_HelperChat.dll

using System;
using System.IO;

namespace HelperChat
{
  public static class Extensions
  {
    public static void ReadStrictBytes(this Stream input, byte[] buf, int count)
    {
      int offset = 0;
      do
      {
        int num = input.Read(buf, offset, count - offset);
        if (num != 0)
          offset += num;
        else
          goto label_3;
      }
      while (offset < count);
      goto label_4;
label_3:
      return;
label_4:;
    }

    public static void SendToDebug(this string message)
    {
      Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + message);
    }
  }
}
