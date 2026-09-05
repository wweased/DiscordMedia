using System;
using DiscordRPC;

namespace DiscordMediaRPC.Сервисы
{
    public class ОтправительDiscord : IDisposable
    {
        public event Action<bool> СтатусИзменился;

        private readonly string идКлиента;
        private DiscordRpcClient клиент;

        public ОтправительDiscord(string идКлиента)
        {
            this.идКлиента = идКлиента;
        }

        public void Подключиться()
        {
            клиент = new DiscordRpcClient(идКлиента);
            клиент.OnReady += (s, e) => СтатусИзменился?.Invoke(true);
            клиент.OnClose += (s, e) => СтатусИзменился?.Invoke(false);
            клиент.Initialize();
        }

        public void ОбновитьСтатус(string название, string исполнитель, bool играет)
        {
            if (клиент == null || !клиент.IsInitialized)
            {
                return;
            }

            if (!играет || string.IsNullOrEmpty(название))
            {
                клиент.ClearPresence();
                return;
            }

            клиент.SetPresence(new RichPresence
            {
                Type = ActivityType.Listening,
                Details = название,
                State = исполнитель,
                Timestamps = Timestamps.Now
            });
        }

        public void Dispose()
        {
            клиент?.ClearPresence();
            клиент?.Dispose();
        }
    }
}
