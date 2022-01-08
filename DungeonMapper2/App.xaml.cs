using DungeonMapper2.Utilities;
using DungeonMapperStandard.DataAccess;
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
        public App() => Start();

        public static void Start()
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DungeonMapper2");
            DatabaseManager.Initialize(appDataPath, new DatabaseConnectionHandler());
        }
    }
}
