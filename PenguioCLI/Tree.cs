using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PenguioCLI
{
    [DebuggerStepThrough]
    public class Tree<T>
    {
        public override string ToString()
        {
            return $"{Key}";
        }
        public Tree<T> Parent { get; set; }
        public T Key { get; set; }
        public List<Tree<T>> Children { get; set; }

        public Tree(T key)
        {
            Key = key;
            Children = new List<Tree<T>>();
        }

        public Tree<T> GetChild(T key)
        {
            return Children.FirstOrDefault(a => Equals(a.Key, key));
        }

        public bool IsLeaf()
        {
            return Children.Count == 0;
        }

        public Tree<T> AddChild(T child)
        {
            var item = new Tree<T>(child);
            item.Parent = this;
            Children.Add(item);
            return item;
        }

        public string GetPath(Func<T, string> func, string concat)
        {
            var m = func(this.Key);
            if (Parent != null)
                return Parent.GetPath(func, concat) + concat + m;
            else return m;
        }
    }
}