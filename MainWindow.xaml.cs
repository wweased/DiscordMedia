using System;
using System.Windows;
using System.Windows.Media;
using DiscordMediaRPC.Сервисы;

namespace DiscordMediaRPC
{
    public partial class MainWindow : Window
    {
        private readonly СлушательМедиа слушатель = new СлушательМедиа();
        private readonly ОтправительDiscord отправитель = new ОтправительDiscord("1545798793197523085");
        private System.Windows.Forms.NotifyIcon значокВТрее;
        private string последнееНазвание;
        private string последнийИсполнитель;
        private bool последнееИграет;

        public MainWindow()
        {
            InitializeComponent();
            НастроитьТрей();
            слушатель.ТрекИзменился += Слушатель_ТрекИзменился;
            отправитель.СтатусИзменился += Отправитель_СтатусИзменился;
            Loaded += async (s, e) =>
            {
                отправитель.Подключиться();
                await слушатель.ЗапуститьAsync();
            };
        }

        private void Слушатель_ТрекИзменился(string название, string исполнитель, TimeSpan позиция, TimeSpan длительность, bool играет)
        {
            последнееНазвание = название;
            последнийИсполнитель = исполнитель;
            последнееИграет = играет;

            Dispatcher.Invoke(() =>
            {
                ТекстНазваниеТрека.Text = string.IsNullOrEmpty(название) ? "Ничего не играет" : название;
                ТекстИсполнитель.Text = исполнитель ?? "";

                if (длительность.TotalSeconds > 0)
                {
                    ПрогрессТрека.Value = позиция.TotalSeconds / длительность.TotalSeconds * 100;
                }
                else
                {
                    ПрогрессТрека.Value = 0;
                }

                ТекстВремя.Text = $"{ФорматВремени(позиция)} / {ФорматВремени(длительность)}";
            });

            отправитель.ОбновитьСтатус(название, исполнитель, играет);
        }

        private void Отправитель_СтатусИзменился(bool подключено)
        {
            Dispatcher.Invoke(() =>
            {
                ИндикаторСтатуса.Fill = new SolidColorBrush(подключено
                    ? (Color)ColorConverter.ConvertFromString("#E8A33D")
                    : (Color)ColorConverter.ConvertFromString("#8A8177"));
            });

            if (подключено)
            {
                отправитель.ОбновитьСтатус(последнееНазвание, последнийИсполнитель, последнееИграет);
            }
        }

        private static string ФорматВремени(TimeSpan время)
        {
            return $"{(int)время.TotalMinutes}:{время.Seconds:D2}";
        }

        private void НастроитьТрей()
        {
            значокВТрее = new System.Windows.Forms.NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Application,
                Visible = false,
                Text = "Медиа для Discord"
            };

            значокВТрее.DoubleClick += (s, e) => ВосстановитьОкно();

            var меню = new System.Windows.Forms.ContextMenuStrip();
            меню.Items.Add("Открыть", null, (s, e) => ВосстановитьОкно());
            меню.Items.Add("Выход", null, (s, e) => Close());
            значокВТрее.ContextMenuStrip = меню;
        }

        private void ВосстановитьОкно()
        {
            Show();
            WindowState = WindowState.Normal;
            значокВТрее.Visible = false;
        }

        private void СвернутьВТрей_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            значокВТрее.Visible = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            значокВТрее?.Dispose();
            отправитель?.Dispose();
            base.OnClosed(e);
        }
    }
}
