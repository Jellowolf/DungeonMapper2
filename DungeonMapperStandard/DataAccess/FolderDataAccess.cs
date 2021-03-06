using DungeonMapperStandard.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DungeonMapperStandard.DataAccess
{
    public class FolderDataAccess
    {
        public static int SaveFolder(Folder folder)
        {
            int? folderId = folder.Id;
            using (var database = DatabaseManager.CreateDatabaseConnection())
            {
                database.Open();
                var sql = $@"INSERT INTO Folder (Id, Name, ParentFolderId) VALUES ({(folder.Id.HasValue ? folder.Id.ToString() : "NULL")}, '{folder.Name}', {(folder.Parent?.Id != null ? folder.Parent.Id.ToString() : "NULL")})
                ON CONFLICT(Id) DO UPDATE SET Name = excluded.Name, ParentFolderId = excluded.ParentFolderId;
                SELECT LAST_INSERT_ROWID()";
                var command = DatabaseManager.CreateSqlCommand(sql, database);

                using (var reader = command.ExecuteReader())
                {
                    if (!folderId.HasValue)
                        while (reader.Read()) { folderId = reader.GetInt32(0); }
                    if (!folderId.HasValue)
                        throw new Exception("Failed to create Folder and return an Id.");
                }
            }
            return folderId.Value;
        }

        public static List<Folder> GetFolders(List<Map> mapData = null)
        {
            using (var database = DatabaseManager.CreateDatabaseConnection())
            {
                database.Open();
                var sql = $"SELECT Id, Name, ParentFolderId FROM Folder";
                var command = DatabaseManager.CreateSqlCommand(sql, database);

                using (var reader = command.ExecuteReader())
                {
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
                        var childItems = folderData.Where(data => data.parentId == folder.Id).Select(data => data.folder).ToList<IPathItem>();
                        if (childItems.Any())
                            foreach (var innerChildItem in childItems)
                                PopulateChildItems((Folder)innerChildItem, (Folder)folder);
                        if (mapData != null)
                            childItems.AddRange(mapData.Where(map => map.FolderId == folder.Id));
                        folder.ChildItems = new ObservableCollection<IPathItem>(childItems);
                    }

                    return folders;
                }
            }
        }

        public static void DeleteFolderAndChildren(Folder folder) => DeleteSelfAndChildItems(folder);

        private static void DeleteSelfAndChildItems(IPathItem pathItem)
        {
            if (pathItem.ChildItems != null && pathItem.ChildItems.Any())
            {
                foreach (var child in pathItem.ChildItems)
                    DeleteSelfAndChildItems(child);
            }
            if (pathItem is Folder)
                DeleteFolder(pathItem as Folder);
            else if (pathItem is Map)
                MapDataAccess.DeleteMap(pathItem as Map);
        }

        private static void DeleteFolder(Folder folder)
        {
            using (var database = DatabaseManager.CreateDatabaseConnection())
            {
                database.Open();
                var sql = $"DELETE FROM Folder WHERE Id = {folder.Id}";
                var command = DatabaseManager.CreateSqlCommand(sql, database);
                command.ExecuteNonQuery();
            }
        }
    }
}
