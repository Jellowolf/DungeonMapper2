using DungeonMapper2.DataAccess;
using System;
using System.IO;
using System.Windows;

namespace DungeonMapper2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static void OnStartup()
        {
            DatabaseManager.AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DungeonMapper2");
        }
    }
}
