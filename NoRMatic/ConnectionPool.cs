using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Norm;

namespace NoRMatic {

    internal class ConnectionInfo {

        public IConnection Connection { get; private set; }
        public string DatabaseName { get; private set; }
        public string ConnectionString { get; private set; }
        public long CreatedTimestamp { get; private set; }

        public ConnectionInfo(IConnection connection, string databaseName, 
            string connectionString, long createdTimestamp) {
            Connection = connection;
            DatabaseName = databaseName;
            ConnectionString = connectionString;
            CreatedTimestamp = createdTimestamp;
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

        private const int MaxFreeConnections = 10;
        private const int MinTimeToLive = 500;

        private readonly object _lock = new object();
        private readonly Dictionary<string, Stack<ConnectionInfo>> _freeConnections;
        private readonly Dictionary<string, List<ConnectionInfo>> _usedConnections;

        public ConnectionPool() {
            _freeConnections = new Dictionary<string, Stack<ConnectionInfo>>();
            _usedConnections = new Dictionary<string, List<ConnectionInfo>>();
        }

        public ConnectionInfo GetConnection(string connectionString) {

            CheckContainersForInitialization(connectionString);

            lock (_lock) {
                if (_freeConnections[connectionString].Count == 0 ||
                    _freeConnections[connectionString].Count > MaxFreeConnections) {
                    Prune(connectionString);
                    if (_freeConnections[connectionString].Count == 0) {
                        _freeConnections[connectionString].Push(CreateConnection(connectionString));
                    }
                }
            }

            ConnectionInfo connectionInfo;
            lock (_lock) {
                connectionInfo = _freeConnections[connectionString].Pop();
                _usedConnections[connectionString].Add(connectionInfo);
            }
            return connectionInfo;
        }

        private void CheckContainersForInitialization(string connectionString) {

            lock (_lock) {
                if (!_freeConnections.ContainsKey(connectionString))
                    _freeConnections[connectionString] = new Stack<ConnectionInfo>();

                if (!_usedConnections.ContainsKey(connectionString))
                    _usedConnections[connectionString] = new List<ConnectionInfo>();
            }
        }

        private static ConnectionInfo CreateConnection(string connectionString) {
            var builder = ConnectionStringBuilder.Create(connectionString);
            var provider = new NormalConnectionProvider(builder);
            return new ConnectionInfo(provider.Open(string.Empty), 
                builder.Database, connectionString, DateTime.Now.Ticks);
        }

        private void Prune(string connectionString) {
            lock (_lock) {
                for (var i = 0; i < _usedConnections[connectionString].Count; i++) {
                    var connectionInfo = _usedConnections[connectionString][i];
                    var lifetime = DateTime.Now.Ticks - connectionInfo.CreatedTimestamp;
                    if (connectionInfo.Connection.GetStream().DataAvailable || lifetime <= (MinTimeToLive*10000))
                        continue;
                    ReclaimUsedConnectionToPool(connectionString, connectionInfo);
                }
                ReduceFreeConnectionsToBaseline(connectionString);
            }
        }

        private void ReduceFreeConnectionsToBaseline(string connectionString) {
            lock (_lock) {
                while (_freeConnections[connectionString].Count > MaxFreeConnections) {
                    var connectionInfo = _freeConnections[connectionString].Pop();
                    DestroyConnection(connectionInfo.Connection);
                }
                Monitor.Pulse(_lock);
            }
        }

        private void ReclaimUsedConnectionToPool(string connectionString, ConnectionInfo connectionInfo) {
            lock (_lock) {
                _usedConnections[connectionString].Remove(connectionInfo);
                if (connectionInfo.Connection.IsInvalid || !connectionInfo.Connection.IsConnected) {
                    DestroyConnection(connectionInfo.Connection);
                    return;
                }
                _freeConnections[connectionString].Push(connectionInfo);
                Monitor.Pulse(_lock);
            }
        }

        private static void DestroyConnection(IConnection connectionn) {
            connectionn.GetStream().Close();
            connectionn.GetStream().Dispose();
            connectionn.Dispose();
        }
    }
}