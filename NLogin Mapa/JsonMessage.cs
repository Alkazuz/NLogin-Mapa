using AdvancedBot;
using AdvancedBot.client;
using System;
using System.Windows.Forms;

namespace NLogin_Mapa
{
    public class JsonMessage
    {

        public MinecraftClient client;
        public String message;
        public String command;

        public JsonMessage(MinecraftClient client, String message, String command)
        {
            this.client = client;
            this.message = message;
            this.command = command;
        }

        public void eventClick()
        {
            Program.getBot(client.Username).SendMessage(command);
        }
    }
}
