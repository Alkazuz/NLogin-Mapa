using AdvancedBot;
using AdvancedBot.client;
using AdvancedBot.Plugins;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace NLogin_Mapa
{
    public class Main : IPlugin
    {
        private static List<MapaBypass> all = new List<MapaBypass>();
        public static List<String> has = new List<String>();
        public static Dictionary<MinecraftClient, long> connected = new Dictionary<MinecraftClient, long>();
        public static Dictionary<MinecraftClient, List<JsonMessage>> jsons = new Dictionary<MinecraftClient, List<JsonMessage>>();
        public void onClientConnect(MinecraftClient client)
        {
            if (!connected.ContainsKey(client))
            {
                connected.Add(client, Utils.GetTimestamp());
            }
            else
            {
                connected[client] = Utils.GetTimestamp();
            }
        }

        public void onReceiveChat(string chat, byte pos, MinecraftClient client)
        {
        }

        public void OnReceivePacket(ReadBuffer pkt, MinecraftClient client)
        {
            try
            {
                switch (pkt.ID)
                {

                    case 0x02:
                        { //chat
                            try
                            {
                                string str = pkt.ReadString();
                                string chat = ChatParser.ParseJson(str);
                                if (!connected.ContainsKey(client))
                                {
                                    connected[client] = Utils.GetTimestamp();
                                }
                                if (str.Contains("clickEvent") && Utils.GetTimestamp() - Main.connected[client] <= 15000)
                                {
                                    JObject obj = JObject.Parse(str);
                                    List<JsonMessage> list = new List<JsonMessage>();
                                    String clickEvent = GetClickEvent(obj, Utils.StripColorCodes(chat));
                                    if (clickEvent == null)
                                    {
                                        client.PrintToChat("§cNão foi possivel encontrar o click event");
                                        return;
                                    }
                                    if (!jsons.ContainsKey(client))
                                    {
                                        list.Add(new JsonMessage(client, Utils.StripColorCodes(chat), clickEvent));
                                        jsons.Add(client, list);
                                    }
                                    else
                                    {
                                        list = jsons[client];
                                        list.Add(new JsonMessage(client, Utils.StripColorCodes(chat), clickEvent));
                                        jsons[client] = list;
                                    }
                                }

                            }
                            catch (Exception ex) { }
                            break;
                        }

                    case 0x34:
                        {
                            MapaBypass mp = new MapaBypass(client);
                            mp.jsonMessages = jsons[client];
                            all.Add(mp);
                            break;
                        }
                }
                lock (all)
                {
                    foreach (MapaBypass bp in all)
                    {
                        if (bp.IsFinished) continue;
                        bp.HandlePacket(pkt);
                    }
                }
            }catch(Exception ex) { }
        }

        public void onSendChat(string chat, MinecraftClient client)
        {
        }

        public void OnSendPacket(IPacket packet, MinecraftClient client)
        {
        }

        private string GetClickEvent(JToken obj, string text)
        {
            string result;
            if (obj == null)
            {
                result = null;
            }
            else
            {
                if (obj.Type == JTokenType.Object && obj["clickEvent"] != null)
                {
                    result = Utils.StripColorCodes(Utils.AsStr(obj["clickEvent"]["value"]));
                }
                else
                {
                    if (obj.Type == JTokenType.Object)
                    {
                        result = this.GetClickEvent(obj["extra"], text);
                    }
                    else
                    {
                        if (obj.Type == JTokenType.Array)
                        {
                            foreach (JToken obj2 in obj)
                            {
                                string clickEvent = this.GetClickEvent(obj2, text);
                                if (clickEvent != null)
                                {
                                    return clickEvent;
                                }
                            }
                        }
                        result = null;
                    }
                }
            }
            return result;
        }

        public void Tick()
        {
        }

        public void Unload()
        {
        }

        public static MapaBypass GetFromClient(MinecraftClient client)
        {
            foreach (MapaBypass bp in all)
            {
                if (bp.IsFinished) continue;
                if (bp.Client.Username.Equals(client.Username))
                {
                    return bp;
                }
            }
            return null;
        }
    }
}
