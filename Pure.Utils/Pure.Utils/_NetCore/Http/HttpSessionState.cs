using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
namespace Pure.Utils
{


    public static class HttpSessionExtentions
    {


        public static string GetSessionId(this ISession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            return session.Id;
        }

    }

    public class HttpSessionState : NameObjectCollectionBase, ISession
    {
        private Microsoft.AspNetCore.Http.HttpContext context;

        public HttpSessionState(Microsoft.AspNetCore.Http.HttpContext context)
        {
            this.context = context;
        }

        public string this[string name]
        {
            get
            {
                return GetString(name);
            }
            set
            {
                Set(name, value);
            }
        }

        public string SessionID { get { return context.Session.Id; } }

        public bool IsAvailable => context.Session.IsAvailable;

        public string Id => context.Session.Id;

        public IEnumerable<string> Keys => context.Session.Keys;

        public void Clear()
        {
            context.Session.Clear();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return context.Session.CommitAsync(cancellationToken = default(CancellationToken));
        }

        public Task LoadAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return context.Session.LoadAsync(cancellationToken = default(CancellationToken));
        }

        public void Remove(string key)
        {
            context.Session.Remove(key);
        }
        public void TryRemove(string key)
        {
            bool hasExits = Exists( key);
            if (hasExits == true)
            {
                Remove( key);
            }
        }
        public bool Exists(string key)
        {
            return context.Session.Keys.Contains(key);
        }
        public void Set(string key, byte[] value)
        {
            if (value == null)
            {
                TryRemove(key);
                return;
            }
            context.Session.Set(key, value);
        }

        public void Set(string key, string value)
        {
            if (value == null)
            {
                TryRemove( key);
                return;
            }
            context.Session.SetString(key, value);
        }
        public void Set(string key, int value)
        { 
            context.Session.SetInt32(key, value);
        }
        public string GetString(string key)
        {
            try
            {
                return context.Session.GetString(key);

            }
            catch (Exception)
            {
                return null;
            }
        }
        public int? GetInt32(string key)
        {
            try
            {
                return context.Session.GetInt32(key);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public bool TryGetValue(string key, out byte[] value)
        {

            return context.Session.TryGetValue(key, out value);
        }
    }
}