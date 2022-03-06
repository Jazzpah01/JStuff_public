using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Collections
{
    [Serializable]

    public class DropDownList
    {
        [SerializeField] public List<string> values = new List<string>();
        [SerializeField] private string _value;
        [SerializeField] private int _index;

        public Action<string> OnValueChanged = null;

        public string value
        {
            get
            {
                return _value;
            }
            set
            {
                if (values.Contains(value))
                {
                    _value = value;
                    _index = values.IndexOf(value);
                    if (OnValueChanged != null)
                    {
                        OnValueChanged(_value);
                    }
                }
                else
                {
                    throw new Exception("Value does not exist in list: " + value);
                }

            }
        }
        public int index
        {
            get
            {
                return _index;
            }
            set
            {
                if (values.Count > value)
                {
                    _index = value;
                    _value = values[value];
                    if (OnValueChanged != null)
                    {
                        OnValueChanged(_value);
                    }
                }
                else
                {
                    throw new Exception("Index does not exist in list: " + value);
                }
            }
        }

        public void Add(string v)
        {
            if (values.Contains(v))
            {
                throw new Exception("Cannot add multiples of same value of: " + v);
            }

            values.Add(v);
        }

        public void Clear()
        {
            _value = "";
            _index = 0;
            values.Clear();
        }

        public bool Contains(string value)
        {
            return values.Contains(value);
        }
    }
}