﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jtc.CsQuery
{
    /// <summary>
    /// Interface for objects that can contain other objects. Note that to allow some consistency with how DOM
    /// objects are used in the browser DOM, many methods are part of the base IDomObject interface so that they
    /// can be used (and return null/missing data) on elements to which they don't apply. So in actuality the only 
    /// unique methods are nonstandard ones.
    /// </summary>
    public interface IDomContainer : IDomObject
    {
        string GetNextChildID();
        IEnumerable<IDomObject> CloneChildren();

        void AddToIndex();
        void RemoveFromIndex();
    }

    /// <summary>
    /// Base class for Dom object that contain other elements
    /// </summary>
    public abstract class DomContainer<T> : DomObject<T>, IDomContainer where T : IDomObject, IDomContainer, new()
    {
        public DomContainer()
        {

        }

        public DomContainer(IEnumerable<IDomObject> elements)
        {
            ChildNodes.AddRange(elements);
        }

        
        /// <summary>
        /// Erase stored path information. This must be done whenever a node is added to a new DOM.
        /// </summary>
        protected void ResetPath()
        {
            //_Path = null;
            _PathID = null;
            //// Also must clear values of child nodes
            // REMOVED - it should be possible to just not store full path info, and leave child

            //if (HasChildren)
            //{
            //    foreach (DomObject node in ChildNodes)
            //    {
            //        node.ResetPath();
            //    }
            //}
        }
        public virtual void AddToIndex()
        {
            throw new Exception("This type of element, " + this.GetType().ToString() + ", cannot be indexed.");
        }
        public virtual void RemoveFromIndex()
        {
            throw new Exception("This type of element, " + this.GetType().ToString() + ", cannot be indexed.");
        }

 
        //protected IEnumerable<string> IndexKeys()
        //{
        //    DomElement e = this as DomElement;
        //    if (e == null)
        //    {
        //        yield break;
        //    }
        //    if (!Complete)
        //    {
        //        throw new Exception("This element is incomplete and cannot be added to a DOM.");
        //    }
        //    // Add just the element to the index no matter what so we have an ordered representation of the dom traversal
        //    yield return IndexKey(String.Empty);
        //    yield return IndexKey(e.nodeNameID.ToString());
        //    if (!String.IsNullOrEmpty(e.ID))
        //    {
        //        yield return IndexKey("#" + e.ID);
        //    }
        //    foreach (string cls in e.Classes)
        //    {
        //        yield return IndexKey("." + cls);
        //    }
        //    //todo -add attributes?
        //}

        public abstract IEnumerable<IDomObject> CloneChildren();
        /// <summary>
        /// Returns all children (including inner HTML as objects);
        /// </summary>
        public override NodeList ChildNodes
        {
            get
            {
                if (_ChildNodes == null)
                {
                    _ChildNodes = new NodeList(this);
                }
                return _ChildNodes;
            }
        }
        protected NodeList _ChildNodes = null;
        // Avoids creating children object when testing
        public override bool HasChildren
        {
            get
            {
                return _ChildNodes != null && ChildNodes.Count > 0;
            }
        }

        public override IDomObject FirstChild
        {
            get
            {
                if (HasChildren)
                {
                    return ChildNodes[0];
                }
                else
                {
                    return null;
                }
            }
        }
        public override IDomObject LastChild
        {
            get
            {
                if (HasChildren)
                {
                    return ChildNodes[ChildNodes.Count - 1];
                }
                else
                {
                    return null;
                }
            }
        }
        public override void AppendChild(IDomObject item)
        {
            ChildNodes.Add(item);
        }
        public override void RemoveChild(IDomObject item)
        {
            ChildNodes.Remove(item);
        }
        /// <summary>
        /// Returns all elements
        /// </summary>
        public override IEnumerable<IDomElement> ChildElements
        {
            get
            {
                if (HasChildren)
                {
                    foreach (IDomObject obj in ChildNodes)
                    {
                        var elm = obj as IDomElement;
                        if (elm != null)
                        {
                            yield return elm;
                        }
                    }
                }
                yield break;
            }
        }
        public IDomObject this[int index]
        {
            get
            {
                return ChildNodes[index];
            }
        }

        public override string Render()
        {
            StringBuilder sb = new StringBuilder();
            if (HasChildren)
            {
                foreach (IDomObject e in ChildNodes)
                {
                    sb.Append(e.Render());
                }
            }
            return (sb.ToString());
        }

        /// <summary>
        /// This is used to assign sequential IDs to children. Since they are requested by the children the method needs to be maintained in the parent.
        /// </summary>
        public string GetNextChildID()
        {
            return Base62Code(++IDCount);
        }
        // Just didn't use the / and the +. A three character ID will permit over 250,000 possible children at each level
        // so that should be plenty
        private static char[] chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToArray();
        protected string Base62Code(int number)
        {
            int ks_len = chars.Length;
            string sc_result = "";
            int num_to_encode = number;
            int i = 0;
            do
            {
                i++;
                sc_result = chars[(num_to_encode % ks_len)] + sc_result;
                num_to_encode = ((num_to_encode - (num_to_encode % ks_len)) / ks_len);
            }
            while (num_to_encode != 0);
            return sc_result.PadLeft(3, '0');
        }

        public override int DescendantCount()
        {
            int count = 0;
            if (HasChildren)
            {
                foreach (IDomObject obj in ChildNodes)
                {
                    count += 1 + obj.DescendantCount();
                }
            }
            return count;
        }
    }

}