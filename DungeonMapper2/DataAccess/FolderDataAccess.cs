using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonMapper2.DataAccess
{
    public class FolderDataAccess
    {
        public static int SaveFolder(Folder folder)
        {
            int? folderId = folder.Id;
            using var database = DatabaseManager.GetDatabaseConnection();
            database.Open();
            var sql = @$"INSERT INTO Folder (Id, Name, ParentFolderId) VALUES ({(folder.Id.HasValue ? folder.Id.ToString() : "NULL")}, '{folder.Name}', {(folder.Parent?.Id != null ? folder.Parent.Id.ToString() : "NULL")})
                ON CONFLICT(Id) DO UPDATE SET Name = excluded.Name, ParentFolderId = excluded.ParentFolderId;
                SELECT LAST_INSERT_ROWID()";
            var command = new SqliteCommand(sql, database);
            using var reader = command.ExecuteReader();
            if (!folderId.HasValue)
                while (reader.Read()) { folderId = reader.GetInt32(0); }
            if (!folderId.HasValue)
                throw new Exception("Failed to create Folder and return an Id.");
            return folderId.Value;
        }

        public static List<Folder> GetFolders(List<Map> mapData = null)
        {
            using var database = DatabaseManager.GetDatabaseConnection();
            database.Open();
            var sql = $"SELECT Id, Name, ParentFolderId FROM Folder";
            var command = new SqliteCommand(sql, database);
            using var reader = command.ExecuteReader();
            var folderData = new List<(int? parentId, Folder folder)>();
            while (reader.Read())
            {
                folderData.Add((
                    !reader.IsDBNull(reader.GetOrdinal("ParentFolderId")) ? reader.GetInt32(reader.GetOrdinal("ParentFolderId")) : (int?)null,
                    new Folder
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Name = reader.GetString(reader.GetOrdinal("Name"))
                    }));
            }
            var folders = folderData.Where(data => !data.parentId.HasValue).Select(data => data.folder).ToList();
            folders.ForEach(folder => PopulateChildItems(folder));

            void PopulateChildItems(Folder folder, Folder parentFolder = null)
            {
                folder.Parent = parentFolder;
                folder.ChildItems = folderData.Where(data => data.parentId == folder.Id).Select(data => data.folder).ToList<IPathItem>();
                if (folder.ChildItems.Any())
                    foreach (var innerChildItem in folder.ChildItems)
                        PopulateChildItems((Folder)innerChildItem, (Folder)folder);
                if (mapData != null)
                    folder.ChildItems.AddRange(mapData.Where(map => map.FolderId == folder.Id));
            }

            return folders;
        }
    }
}
