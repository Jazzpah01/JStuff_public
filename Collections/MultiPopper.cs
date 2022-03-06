using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace JStuff.Collections
{
    /// <summary>
    /// A Dictionary where elements can only be accessed in bulk.
    /// Some function will combine each element of the dictionary,
    /// which will then be returned.
    /// </summary>
    /// <typeparam name="Key">Key</typeparam>
    /// <typeparam name="Element">Element</typeparam>
    public class MultiPopper<Key, Element>
    {
        private List<Element> list;
        private List<Key> key;

        public delegate bool Compare(Key key1, Key key2);
        public delegate Element Combine(Element elm1, Element elm2);

        private Compare compareFunction;
        private Combine combineFunction;

        private Key stackableKey;
        private Element stackableElement;

        private Element nullElement;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compareFunction">A method to compare the </param>
        /// <param name="combineFunction"></param>
        /// <param name="stackable"></param>
        public MultiPopper(Compare compareFunction, Combine combineFunction, Key stackable)
        {
            this.list = new List<Element>();
            this.key = new List<Key>();

            this.compareFunction = compareFunction;
            this.combineFunction = combineFunction;

            this.stackableKey = stackable;
        }

        /// <summary>
        /// Add an element to the MultiPopper.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="newKey"></param>
        public void Add(Key newKey, Element v)
        {
            if (compareFunction(newKey, stackableKey))
            {
                if (stackableElement == null)
                {
                    stackableElement = v;
                }
                else
                {
                    stackableElement = combineFunction(stackableElement, v);
                }
                return;
            }

            bool isNew = true;

            for (int i = 0; i < key.Count; i++)
            {
                if (compareFunction(key[i], newKey))
                {
                    isNew = false;
                    list[i] = v;
                }
            }

            if (isNew)
            {
                list.Add(v);
                key.Add(newKey);
            }
        }

        public Element MultiPop(Element initialValue)
        {
            Element retval = initialValue;

            retval = combineFunction(retval, stackableElement);

            foreach (Element v in list)
            {
                retval = combineFunction(retval, v);
            }

            list = new List<Element>();
            key = new List<Key>();
            stackableElement = nullElement;

            return retval;
        }
    }
}