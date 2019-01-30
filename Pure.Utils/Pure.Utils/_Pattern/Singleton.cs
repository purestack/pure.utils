using System;

namespace Pure.Utils
{
    /// <summary>
    /// A base class for the singleton design pattern.
    /// </summary>
    /// <typeparam name="T">Class type of the singleton</typeparam>
    public abstract class Singleton<T> where T : class
    {
        #region Members

        /// <summary>
        /// Static instance. Needs to use lambda expression
        /// to construct an instance (since constructor is private).
        /// </summary>
        private static readonly Lazy<T> sInstance = new Lazy<T>(() => CreateInstanceOfT());

        #endregion

        #region Properties

        /// <summary>
        /// Gets the instance of this singleton.
        /// </summary>
        public static T Instance { get { return sInstance.Value; } }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an instance of T via reflection since T's constructor is expected to be private.
        /// </summary>
        /// <returns></returns>
        private static T CreateInstanceOfT()
        {
            return Activator.CreateInstance(typeof(T), true) as T;
        }

        #endregion
    }



    ///// <summary>
    ///// Singleton implementation using generics.
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    //public class Singleton<T>
    //{
    //    private static T _item;
    //    private static object _syncRoot = new object();
    //    private static Func<T> _creator;


    //    /// <summary>
    //    /// Prevent instantiation
    //    /// </summary>
    //    private Singleton() { }


    //    /// <summary>
    //    /// Initalize with the creator.
    //    /// </summary>
    //    /// <param name="creator"></param>
    //    public static void Init(Func<T> creator)
    //    {
    //        _creator = creator;
    //    }


    //    /// <summary>
    //    /// Initialize using instance.
    //    /// </summary>
    //    /// <param name="item"></param>
    //    public static void Init(T item)
    //    {
    //        _item = item;
    //        _creator = new Func<T>(() => item);
    //    }


    //    /// <summary>
    //    /// Get the instance of the singleton item T.
    //    /// </summary>
    //    public static T Instance
    //    {
    //        get
    //        {
    //            if (_creator == null)
    //                return default(T);

    //            if (_item == null)
    //            {
    //                lock (_syncRoot)
    //                {
    //                    _item = _creator();
    //                }
    //            }

    //            return _item;
    //        }
    //    }
    //}
}
