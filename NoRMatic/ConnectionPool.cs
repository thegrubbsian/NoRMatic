using System;
using System.Collections.Generic;
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

        private const int FreeConnectionsStandingCount = 3;
        private const int MaxTimeToLive = 300;

        private readonly NormalConnectionProvider Provider;
        private readonly Stack<IConnection> FreeConnections;
        private readonly List<KeyValuePair<IConnection, long>> UsedConnections;

        private static string ConnectionString {
            get { 
                return NoRMaticModel<T>.ModelConfig.ConnectionStringProvider != null ?
                    NoRMaticModel<T>.ModelConfig.ConnectionStringProvider() : NoRMaticConfig.ConnectionString;
            }
        }

        public string DatabaseName {
            get { return Provider.ConnectionString.Database; }
        }

        public ConnectionPool() {

            var builder = ConnectionStringBuilder.Create(ConnectionString);
            Provider = new NormalConnectionProvider(builder);

            FreeConnections = new Stack<IConnection>();
            UsedConnections = new List<KeyValuePair<IConnection, long>>();

            CreateBatchOfConnections();
        }

        public IConnection GetConnection() {

            if (FreeConnections.Count == 0 || FreeConnections.Count > FreeConnectionsStandingCount) {
                Prune();
                if (FreeConnections.Count == 0) {
                    CreateBatchOfConnections();
                }
            }

            var connection = FreeConnections.Pop();
            UsedConnections.Add(new KeyValuePair<IConnection, long>(connection, DateTime.Now.Ticks));
            return connection;
        }

        private void Prune() {
            for (var i = 0; i < UsedConnections.Count; i++) {
                if ((DateTime.Now.Ticks - UsedConnections[i].Value) <= (MaxTimeToLive * 10000)) continue;
                ReclaimUsedConnectionToPool(UsedConnections[i]);
            }
            ReduceFreeConnectionsToBaseline();
        }

        private void ReduceFreeConnectionsToBaseline() {
            while (FreeConnections.Count > FreeConnectionsStandingCount) {
                var connection = FreeConnections.Pop();
                connection.Dispose();
            }
        }

        private void ReclaimUsedConnectionToPool(KeyValuePair<IConnection, long> usedItem) {
            UsedConnections.Remove(usedItem);
            FreeConnections.Push(usedItem.Key);
        }

        private void CreateBatchOfConnections() {
            for (var i = 0; i < FreeConnectionsStandingCount; i++) {
                FreeConnections.Push(Provider.Open(string.Empty));
            }
        }
    }
}