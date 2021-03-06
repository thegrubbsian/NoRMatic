﻿using System;
using System.Collections.Generic;
using System.Linq;
using Norm;

namespace NoRMatic {

    // A first-in-last-out container for connections
    internal class Pipe<T> {

        private readonly List<T> _list = new List<T>();

        public int Count {
            get { return _list.Count; }
        }

        public void Give(T item) {
            _list.Insert(0, item);
        }

        public T Take() {
            var item = _list.LastOrDefault();
            if (_list.Count > 0) _list.RemoveAt(_list.Count - 1);
            return item;
        }

        public T[] ToArray() {
            return _list.ToArray();
        }

        public void Clear() {
            _list.Clear();
        }
    }


    internal class ConnectionInfo : IDisposable {

        public IConnection Connection { get; private set; }
        public string DatabaseName { get; private set; }
        public string ConnectionString { get; private set; }
        public long CreatedTimestamp { get; private set; }
        public long LastUsedTimestamp { get; set; }
        public bool IsDisposed { get; private set; }

        public ConnectionInfo(IConnection connection, 
            string databaseName, string connectionString) {
            Connection = connection;
            DatabaseName = databaseName;
            ConnectionString = connectionString;
            CreatedTimestamp = DateTime.Now.Ticks;
        }

        public bool IsAlive() {
            return !Connection.IsInvalid &&
                !IsDisposed && Connection.IsConnected;
        }

        public void Dispose() {
            Connection.GetStream().Close();
            Connection.GetStream().Dispose();
            Connection.Dispose();
            IsDisposed = true;
        }
    }

    internal sealed class ConnectionPool {

        class Nested {
            static Nested() { } // Constructor so compiler doesn't mark beforefieldinit
            internal static readonly ConnectionPool instance = new ConnectionPool();
        }

        public static ConnectionPool Instance {
            get { return Nested.instance; }
        }

        // In miliseconds
        private const int MinCycleAge = 300;
        private const int PruningAge = 300000;

        private readonly object _lock = new object();
        private readonly Dictionary<string, Pipe<ConnectionInfo>> _freeConnections = new Dictionary<string, Pipe<ConnectionInfo>>();
        private readonly Dictionary<string, List<ConnectionInfo>> _usedConnections = new Dictionary<string, List<ConnectionInfo>>();

        public ConnectionInfo GetConnection(string connectionString) {

            ConnectionInfo connectionInfo;

            lock (_lock) {

                CheckContainersForInitialization(connectionString);

                Prune();

                if (_freeConnections[connectionString].Count == 0) {
                    Free(connectionString);
                    if (_freeConnections[connectionString].Count == 0) {
                        _freeConnections[connectionString].Give(CreateConnection(connectionString));
                    }
                }

                connectionInfo = _freeConnections[connectionString].Take();
                connectionInfo.LastUsedTimestamp = DateTime.Now.Ticks;
                _usedConnections[connectionString].Add(connectionInfo);
            }

            return connectionInfo;
        }

        private void CheckContainersForInitialization(string connectionString) {

            if (!_freeConnections.ContainsKey(connectionString))
                _freeConnections[connectionString] = new Pipe<ConnectionInfo>();

            if (!_usedConnections.ContainsKey(connectionString))
                _usedConnections[connectionString] = new List<ConnectionInfo>();
        }

        private static ConnectionInfo CreateConnection(string connectionString) {
            var builder = ConnectionStringBuilder.Create(connectionString);
            var provider = new NormalConnectionProvider(builder);
            return new ConnectionInfo(provider.Open(string.Empty), 
                builder.Database, connectionString);
        }

        private void Free(string connectionString) {
            for (var i = 0; i < _usedConnections[connectionString].Count; i++) {
                var connectionInfo = _usedConnections[connectionString][i];
                var cycleAge = DateTime.Now.Ticks - connectionInfo.LastUsedTimestamp;
                if (cycleAge <= (MinCycleAge * 10000D)) continue;
                _usedConnections[connectionString].Remove(connectionInfo);
                if (connectionInfo.IsAlive()) {
                    connectionInfo.LastUsedTimestamp = DateTime.Now.Ticks;
                    _freeConnections[connectionString].Give(connectionInfo);
                } else {
                    connectionInfo.Dispose();
                }
            }
        }

        private void Prune() {
            var connectionStrings = _freeConnections.Keys;
            foreach (var connectionString in connectionStrings) {
                var connectionInfos = _freeConnections[connectionString].ToArray();
                _freeConnections[connectionString].Clear();
                foreach (var connectionInfo in connectionInfos) {
                    var age = DateTime.Now.Ticks - connectionInfo.CreatedTimestamp;
                    if (!connectionInfo.IsAlive() || age >= (PruningAge * 10000D)) {
                        connectionInfo.Dispose();
                        continue;
                    }
                    _freeConnections[connectionString].Give(connectionInfo);
                }
            }
        }
    }
}