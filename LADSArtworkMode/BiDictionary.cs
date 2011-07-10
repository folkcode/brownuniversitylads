using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LADSArtworkMode
{
    class BiDictionary<TFirst, TSecond>
    {
        IDictionary<TFirst, IList<TSecond>> firstToSecond = new Dictionary<TFirst, IList<TSecond>>();
        IDictionary<TSecond, IList<TFirst>> secondToFirst = new Dictionary<TSecond, IList<TFirst>>();

        private static IList<TFirst> EmptyFirstList = new TFirst[0];
        private static IList<TSecond> EmptySecondList = new TSecond[0];

        public void Add(TFirst first, TSecond second)
        {
            IList<TFirst> firsts;
            IList<TSecond> seconds;
            if (!firstToSecond.TryGetValue(first, out seconds))
            {
                seconds = new List<TSecond>();
                firstToSecond[first] = seconds;
            }
            if (!secondToFirst.TryGetValue(second, out firsts))
            {
                firsts = new List<TFirst>();
                secondToFirst[second] = firsts;
            }
            seconds.Add(second);
            firsts.Add(first);
        }

        public BiDictionary<TFirst, TSecond> copy()
        {
            BiDictionary<TFirst, TSecond> toReturn = new BiDictionary<TFirst, TSecond>();
            foreach (TFirst t in firstToSecond.Keys)
            {
                IList<TSecond> list = firstToSecond[t];
                foreach (TSecond second in list)
                {
                    toReturn.Add(t, second);
                }
            }
            return toReturn;
        }

        public ICollection<TFirst> firstKeys
        {
            get
            {
                return firstToSecond.Keys;
            }
        }
        public ICollection<TSecond> secondKeys
        {
            get
            {
                return secondToFirst.Keys;
            }
        }
        public ICollection<TSecond> firstValues
        {
            get
            {
                return secondToFirst.Keys;
            }
        }
        public ICollection<TFirst> secondValues
        {
            get
            {
                return firstToSecond.Keys;
            }
        }

        public bool TryGetValue(TFirst first, out IList<TSecond> second)
        {
            if (!firstToSecond.TryGetValue(first, out second))
            {
                return false;
            }
            return true;
        }

        public bool TryGetValue(TSecond second, out IList<TFirst> first)
        {
            if (!secondToFirst.TryGetValue(second, out first))
            {
                return false;
            }
            return true;
        }

        // Note potential ambiguity using indexers (e.g. mapping from int to int)
        // Hence the methods as well...
        public IList<TSecond> this[TFirst first]
        {
            get { return GetByFirst(first); }
        }

        public IList<TFirst> this[TSecond second]
        {
            get { return GetBySecond(second); }
        }

        public IList<TSecond> GetByFirst(TFirst first)
        {
            IList<TSecond> list;
            if (!firstToSecond.TryGetValue(first, out list))
            {
                return EmptySecondList;
            }
            return new List<TSecond>(list); // Create a copy for sanity
        }

        public IList<TFirst> GetBySecond(TSecond second)
        {
            IList<TFirst> list;
            if (!secondToFirst.TryGetValue(second, out list))
            {
                return EmptyFirstList;
            }
            return new List<TFirst>(list); // Create a copy for sanity
        }

        public void RemoveByFirst(TFirst first)
        {
            IList<TSecond> seconds = GetByFirst(first);
            firstToSecond.Remove(first);
            foreach (TSecond t in seconds)
            {
                secondToFirst.Remove(t);
            }
        }

        public void RemoveBySecond(TSecond second)
        {
            IList<TFirst> firsts = GetBySecond(second);
            secondToFirst.Remove(second);
            foreach (TFirst t in firsts)
            {
                firstToSecond.Remove(t);
            }
        }

        public void Remove(TFirst first, TSecond second)
        {
            IList<TSecond> seconds = GetByFirst(first);
            IList<TFirst> firsts = GetBySecond(second);
            seconds.Remove(second);
            firsts.Remove(first);
            if (seconds.Count == 0) firstToSecond.Remove(first);
            if (firsts.Count == 0) secondToFirst.Remove(second);
        }
    }
}
