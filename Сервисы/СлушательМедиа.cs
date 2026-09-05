using System;
using System.Threading.Tasks;
using Windows.Media.Control;
using WindowsMediaController;

namespace DiscordMediaRPC.Сервисы
{
    public class СлушательМедиа
    {
        public event Action<string, string, TimeSpan, TimeSpan, bool> ТрекИзменился;

        private readonly MediaManager менеджер = new MediaManager();

        public async Task ЗапуститьAsync()
        {
            менеджер.OnFocusedSessionChanged += (сессия) => _ = ОбновитьДанные(сессия);
            менеджер.OnAnyMediaPropertyChanged += (сессия, свойства) => _ = ОбновитьДанные(сессия);
            менеджер.OnAnyPlaybackStateChanged += (сессия, состояние) => _ = ОбновитьДанные(сессия);
            менеджер.OnAnyTimelinePropertyChanged += (сессия, таймлайн) => _ = ОбновитьДанные(сессия);
            менеджер.OnAnySessionClosed += (сессия) => ТрекИзменился?.Invoke(null, null, TimeSpan.Zero, TimeSpan.Zero, false);

            await менеджер.StartAsync();
        }

        private async Task ОбновитьДанные(MediaManager.MediaSession сессия)
        {
            if (сессия == null)
            {
                ТрекИзменился?.Invoke(null, null, TimeSpan.Zero, TimeSpan.Zero, false);
                return;
            }

            var свойства = await сессия.ControlSession.TryGetMediaPropertiesAsync();
            var таймлайн = сессия.ControlSession.GetTimelineProperties();
            var состояниеВоспроизведения = сессия.ControlSession.GetPlaybackInfo();

            bool играет = состояниеВоспроизведения.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;

            ТрекИзменился?.Invoke(свойства.Title, свойства.Artist, таймлайн.Position, таймлайн.EndTime, играет);
        }
    }
}
