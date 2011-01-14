using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Norm;

namespace NoRMatic {

    internal sealed class ConnectionPool<T> where T : NoRMaticModel<T> {

        class Nested {
            static Nested() { } // Constructor so compiler doesn't mark beforefieldinit
            internal static readonly ConnectionPool<T> instance = new ConnectionPool<T>();
        }

        public static ConnectionPool<T> Instance {
            get { return Nested.instance; }
        }

        private const int MaxFreeConnections = 5;
        private const int MinTimeToLive = 100;

        private readonly NormalConnectionProvider _provider;
        private readonly Stack<IConnection> _freeConnections;
        private readonly List<KeyValuePair<IConnection, long>> _usedConnections;

        private static string ConnectionString {
            get { 
                return NoRMaticModel<T>.ModelConfig.ConnectionStringProvider != null ?
                    NoRMaticModel<T>.ModelConfig.ConnectionStringProvider() : NoRMaticConfig.ConnectionString;
            }
        }

        public string DatabaseName {
            get { return _provider.ConnectionString.Database; }
        }

        public ConnectionPool() {

            var builder = ConnectionStringBuilder.Create(ConnectionString);
            _provider = new NormalConnectionProvider(builder);

            _freeConnections = new Stack<IConnection>();
            _usedConnections = new List<KeyValuePair<IConnection, long>>();

            _freeConnections.Push(_provider.Open(string.Empty));
        }

        public IConnection GetConnection() {

            if (_freeConnections.Count == 0 || _freeConnections.Count > MaxFreeConnections) {
                Prune();
                if (_freeConnections.Count == 0) {
                    _freeConnections.Push(_provider.Open(string.Empty));
                }
            }

            var connection = _freeConnections.Pop();
            _usedConnections.Add(new KeyValuePair<IConnection, long>(connection, DateTime.Now.Ticks));
            return connection;
        }

        private void Prune() {
            for (var i = 0; i < _usedConnections.Count; i++) {
                var ticksToLive = MinTimeToLive * 10000;
                var lifetime = DateTime.Now.Ticks - _usedConnections[i].Value;
                if (_usedConnections[i].Key.GetStream().DataAvailable || lifetime <= ticksToLive) continue;
                ReclaimUsedConnectionToPool(_usedConnections[i]);
            }
            ReduceFreeConnectionsToBaseline();
        }

        private void ReduceFreeConnectionsToBaseline() {
            while (_freeConnections.Count > MaxFreeConnections) {
                var connection = _freeConnections.Pop();
                DestroyConnection(connection);
            }
        }

        private void ReclaimUsedConnectionToPool(KeyValuePair<IConnection, long> connectionItem) {
            _usedConnections.Remove(connectionItem);

            if (connectionItem.Key.IsInvalid || !connectionItem.Key.IsConnected) {
                DestroyConnection(connectionItem.Key);
                return;
            }

            ResetStream(connectionItem.Key.GetStream());
            _freeConnections.Push(connectionItem.Key);
        }

        private static void ResetStream(NetworkStream stream) {
            stream.Flush();
            if (!stream.DataAvailable) return;
            var toRead = Convert.ToInt32(stream.Length - stream.Position);
            var offset = Convert.ToInt32(stream.Position);
            stream.Read(new byte[toRead], offset, toRead);
        }

        private static void DestroyConnection(IConnection connectionn) {
            connectionn.GetStream().Close();
            connectionn.Dispose();
        }
    }
}