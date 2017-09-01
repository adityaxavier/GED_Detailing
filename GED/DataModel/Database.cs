using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GED.DataModel
{
    public class TrackRecord
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string UniqueId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

    public class jsonData
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string jsonitem { get; set; }
    }
    class Database
    {
        String databaseName = "Track.db";
        
     
        public async void CreateTable()
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(databaseName);
            await conn.CreateTableAsync<TrackRecord>();
            await conn.CreateTableAsync<jsonData>();
        }
        public async void CreateJsonTable()
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(databaseName);
            await conn.CreateTableAsync<jsonData>();
        }

        public async void InsertJson(jsonData json)
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(databaseName);
            await conn.InsertAsync(json);
        }

        public async void UpdateJson(jsonData json)
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(databaseName);
            jsonData jData = new jsonData();
            jData.Id = 0;

            await conn.DeleteAsync(jData);
            
            await conn.InsertAsync(json);
        }

        public async Task<jsonData> GetJson()
        {
                SQLiteAsyncConnection conn = new SQLiteAsyncConnection(databaseName);
                var query = conn.Table<jsonData>().Where(x => x.Id == 0);
                var result = await query.CountAsync();
                var res = await query.ToListAsync();
                return res.FirstOrDefault();
        }

        public async void InsertRecord(TrackRecord record)
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(databaseName);
            await conn.InsertAsync(record);
        }

        public async Task<TrackRecord> GetRecord(int id)
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(databaseName);

            var query = conn.Table<TrackRecord>().Where(x => x.Id == id);
            var result = await query.ToListAsync();

            return result.FirstOrDefault();
        }

        public async Task<List<TrackRecord>> GetRecords()
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(databaseName);

            var query = conn.Table<TrackRecord>();
            var result = await query.ToListAsync();

            return result;
        }

        public async Task<List<jsonData>> GetJsonData()
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(databaseName);

            var query = conn.Table<jsonData>();
            var result = await query.ToListAsync();

            return result;
        }

        public async void UpdateRecord(TrackRecord record)
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(databaseName);
            await conn.UpdateAsync(record);
        }

        public async void DeleteRecord(TrackRecord record)
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(databaseName);
            await conn.DeleteAsync(record);
        }

        public async void DeleteTable()
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(databaseName);
            await conn.DropTableAsync<TrackRecord>();
        }
    }

}
