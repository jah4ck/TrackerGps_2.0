
using System;
using System.Threading;
using System.Threading.Tasks;
using TrackerGps;

namespace Core
{

    public static class SavePoint
    {

        public static async void SaveLatLong(string util, string lat, string longitude)
        {
            TimeSpan timer = DateTime.Now.ToUniversalTime() - MainActivity.DateSaveGeo;
            if (timer.TotalSeconds > 60)
            {
                MainActivity._erreur.Text += "Debut maj gps !! " + Environment.NewLine;
                MainActivity.DateSaveGeo = DateTime.Now.ToUniversalTime();
                try
                {
                    TrackerGps.WebService.WSCtrlPc ws = new TrackerGps.WebService.WSCtrlPc();
                    ws.Timeout = 1000;
                    ws.Registr_PositionAsync(util, longitude, lat);
                }
                catch (Exception err)
                {
                    MainActivity._erreur.Text = err.Message;
                }

                //CancellationTokenSource cts = new CancellationTokenSource();
                //Task task = GetNews(util, lat, longitude);
                //await Task.WhenAny(task, Task.Delay(1000, cts.Token)).ContinueWith(DisplayResults);
                //if (!task.IsCompleted)
                //{
                //    ReportTimeOut(task, cts);
                //}
                MainActivity._erreur.Text += "fin maj gps !! " + Environment.NewLine;
            }

        }
        public async static Task GetNews(string util, string lat, string longitude)
        {
            try
            {
                MainActivity._erreur.Text += "instanciation ws !! " + Environment.NewLine;
                TrackerGps.WebService.WSCtrlPc ws = new TrackerGps.WebService.WSCtrlPc();
                MainActivity._erreur.Text += "appel WS !! " + Environment.NewLine;
                ws.Registr_Position(util, longitude, lat);
                //ws.Registr_PositionAsync(util, longitude, lat);
            }
            catch (Exception err)
            {
                MainActivity._erreur.Text += err.Message + Environment.NewLine;
                throw;
            }
            
        }
        private static void DisplayResults(Task task)
        {
            MainActivity._erreur.Text += "Maj GPS OK !" + Environment.NewLine;
        }
        static void ReportTimeOut(Task task, CancellationTokenSource cts)
        {
            MainActivity._erreur.Text += "Time out, problème réseau !!" + Environment.NewLine;
            cts.Cancel();
            task.Dispose();
        }
    }
}