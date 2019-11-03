﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Core.IO.Database.Repositories
{
    internal class IdentityKeyValueRepository<TIdentifier, TKey, TValue>
    {
        private string _tableName;

        public IdentityKeyValueRepository (string tableName)
        {
            _tableName = tableName;
        }

        public void Init ()
        {
            CreateTableIfAbsent();
        }

        private IDatabaseConnector GetConnector() => new PostgreSQLDatabaseConnector();

        private void CreateTableIfAbsent ()
        {
            IDatabaseConnector db = GetConnector();
            var res = db.ReadQuery("SELECT COUNT(*) AS count FROM information_schema.tables WHERE table_name = @table", new Dictionary<string, object>() { { "@table", _tableName } });
            if ((long)res[0]["count"] == 0)
            {
                db.UpdateQuery($"CREATE TABLE {_tableName} (identifier text, key text, value text, CONSTRAINT identkey UNIQUE (identifier, key));", new Dictionary<string, object>());
            }
        }

        public void Create(TIdentifier identifier, TKey key, TValue value)
        {
            IDatabaseConnector db = GetConnector();
            db.UpdateQuery($"INSERT INTO {_tableName} VALUES (@identifier, @key, @value)", new Dictionary<string, object>() { { "@identifier", identifier }, { "@key", key }, { "@value", value } });
        }

        public TValue Read (TIdentifier identifier, TKey key)
        {
            IDatabaseConnector db = GetConnector();
            var res = db.ReadQuery($"SELECT value FROM {_tableName} WHERE identifier = @identifier AND key = @key", new Dictionary<string, object>() { { "@identifier", identifier }, { "@key", key } });
            return res.Length == 0 ? default : (TValue)res.Single ().FirstOrDefault ().Value;
        }

        public void Update (TIdentifier identifier, TKey key, TValue value)
        {
            IDatabaseConnector db = GetConnector();
            db.UpdateQuery($"UPDATE {_tableName} SET value = @value WHERE identifier = @identity AND key = @key", new Dictionary<string, object>() { { "@identifier", identifier }, { "@key", key }, { "@value", value } });
        }

        public void Delete (TIdentifier identifier, TKey key)
        {
            IDatabaseConnector db = GetConnector();
            db.UpdateQuery($"DELETE FROM {_tableName} WHERE identifier = @identity AND key = @key", new Dictionary<string, object>() { { "@identifier", identifier }, { "@key", key } });
        }

        public void InsertOrUpdate (TIdentifier identifier, TKey key, TValue value) // Totally did not steal this from Stack Overflow.
        {
            IDatabaseConnector db = GetConnector();
            string query = $"INSERT INTO {_tableName} VALUES (@identifier, @key, @value) ON CONFLICT ON CONSTRAINT identkey DO UPDATE SET value = @value WHERE {_tableName}.identifier = @identifier AND {_tableName}.key = @key";
            db.UpdateQuery(query, new Dictionary<string, object>() { { "@identifier", identifier }, { "@key", key }, { "@value", value } });
        }
    }
}
