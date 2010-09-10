using System;

namespace NoRMatic {

    internal sealed class ConfigContainer {

        class Nested {
            static Nested() { } // Constructor so compiler doesn't mark beforefieldinit
            internal static readonly ConfigContainer instance = new ConfigContainer();
        }

        public static ConfigContainer Instance {
            get { return Nested.instance; }
        }

        public Func<string> CurrentUserProvider { get; set; }
    }
}