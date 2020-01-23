using System.Collections.Generic;

namespace Hop
{
    public class Query
    {
        public string Search { get; }
        public bool Execute { get; }
        public Stack<Item> Stack { get; }

        public Query(string search, bool execute, Stack<Item> stack)
        {
            Search = search;
            Execute = execute;
            Stack = stack;
        }
    }
}
