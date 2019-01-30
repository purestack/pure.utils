using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Pure.Utils
{
    /// <summary>
    /// A static helper class that includes various parameter checking routines.
    /// </summary>
    public static partial class Guard
    {
        /// <summary>
        /// 如果提供的 <paramref name="argumentValue"/> 是 <see langword="null"/>，
        /// 则抛出 <see cref="ArgumentNullException"/>。
        /// </summary>
        /// <param name="argumentValue">参数值。</param>
        /// <param name="argumentName">参数名。</param>
        /// <exception cref="System.ArgumentNullException">
        /// 如果提供的 <paramref name="argumentValue"/> 是 <see langword="null"/>.
        /// </exception>
        public static void ArgumentNotNull(object argumentValue, string argumentName)
        {
            ArgumentNotNull(argumentValue, argumentName, "参数不能空");
        }

        /// <summary>
        /// 如果提供的 <paramref name="argumentValue"/> 是 <see langword="null"/>，
        /// 则抛出 <see cref="ArgumentNullException"/>。
        /// </summary>
        /// <param name="argumentValue">参数值。</param>
        /// <param name="argumentName">参数名。</param>
        /// <param name="message">提示信息。</param>
        /// <exception cref="System.ArgumentNullException">
        /// 如果提供的 <paramref name="argumentValue"/> 是 <see langword="null"/>.
        /// </exception>
        public static void ArgumentNotNull(object argumentValue, string argumentName, string message)
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException(
                    argumentName,
                    message.FormatWith(argumentName)
                );
            }
        }

        public static void ArgumentNotNullOrEmpty(string argumentValue, string argumentName)
        {
            ArgumentNotNullOrEmpty(argumentValue, argumentName, "参数不能空");
        }

        public static void ArgumentNotNullOrEmpty(string argumentValue, string argumentName, string message)
        {
            if (string.IsNullOrEmpty(argumentValue))
            {
                throw new ArgumentNullException(
                    argumentName,
                    message.FormatWith(argumentName)
                );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argumentName"></param>
        /// <param name="col"></param>
        public static void ArgumentHasLength(ICollection col, string argumentName)
        {
            if (col == null || col.Count <= 0)
            {
                throw new ArgumentException("参数不能空", argumentName);
            }
        }

        #region IsTrue

        /// <summary>
        /// 验证指定的条件是否为 true。如果该条件为 false，则断言失败。
        /// </summary>
        /// <param name="condition">要验证的条件为 true。</param>
        /// <param name="argumentName">参数的名称。</param>
        /// <exception cref="ArgumentException">condition 的计算结果为 false。</exception>
        public static void IsTrue(bool condition, string argumentName)
        {
            IsTrue(condition, argumentName, "", null);
        }

        /// <summary>
        /// 验证指定的条件是否为 true。如果该条件为 false，则断言失败。断言失败时将显示一则消息。
        /// </summary>
        /// <param name="condition">要验证的条件为 true。</param>
        /// <param name="argumentName">参数的名称。</param>
        /// <param name="message">断言失败时显示的消息。</param>
        /// <exception cref="ArgumentException">condition 的计算结果为 false。</exception>
        public static void IsTrue(bool condition, string argumentName, string message)
        {
            IsTrue(condition, argumentName, message, null);
        }

        /// <summary>
        /// 验证指定的条件是否为 true。如果该条件为 false，则断言失败。断言失败时将显示一则消息，并向该消息应用指定的格式。
        /// </summary>
        /// <param name="condition">要验证的条件为 true。</param>
        /// <param name="argumentName">参数的名称。</param>
        /// <param name="message">断言失败时显示的消息。</param>
        /// <param name="parameters">设置 message 格式时使用的参数的数组。</param>
        /// <exception cref="ArgumentException">condition 的计算结果为 false。</exception>
        public static void IsTrue(bool condition, string argumentName, string message, params object[] messageArgs)
        {
            if (!condition)
            {
                throw new ArgumentException(message.FormatWith(messageArgs), argumentName);
            }
        }

        #endregion

        #region IsFalse

        /// <summary>
        /// 验证指定的条件是否为 true。如果该条件为 false，则断言失败。
        /// </summary>
        /// <param name="condition">要验证的条件为 true。</param>
        /// <param name="argumentName">参数的名称。</param>
        /// <exception cref="ArgumentException">condition 的计算结果为 false。</exception>
        public static void IsFalse(bool condition, string argumentName)
        {
            IsFalse(condition, argumentName, "", null);
        }

        /// <summary>
        /// 验证指定的条件是否为 true。如果该条件为 false，则断言失败。断言失败时将显示一则消息。
        /// </summary>
        /// <param name="condition">要验证的条件为 true。</param>
        /// <param name="argumentName">参数的名称。</param>
        /// <param name="message">断言失败时显示的消息。</param>
        /// <exception cref="ArgumentException">condition 的计算结果为 false。</exception>
        public static void IsFalse(bool condition, string argumentName, string message)
        {
            IsFalse(condition, argumentName, message, null);
        }

        /// <summary>
        /// 验证指定的条件是否为 true。如果该条件为 false，则断言失败。断言失败时将显示一则消息，并向该消息应用指定的格式。
        /// </summary>
        /// <param name="condition">要验证的条件为 true。</param>
        /// <param name="argumentName">参数的名称。</param>
        /// <param name="message">断言失败时显示的消息。</param>
        /// <param name="parameters">设置 message 格式时使用的参数的数组。</param>
        /// <exception cref="ArgumentException">condition 的计算结果为 false。</exception>
        public static void IsFalse(bool condition, string argumentName, string message, params object[] messageArgs)
        {
            if (!condition)
            {
                throw new ArgumentException(message.FormatWith(messageArgs), argumentName);
            }
        }

        #endregion

        /// <summary>
        /// 将指定 <see cref="System.String"/> 中的格式项替换为指定数组中相应 <see cref="System.Object"/> 实例的值的文本等效项。
        /// 采用固定区域格式。
        /// </summary>
        /// <param name="format"><a href="http://msdn.microsoft.com/zh-cn/library/txafckwd.aspx">复合格式字符串</a></param>
        /// <param name="args">一个对象数组，其中包含零个或多个要设置格式的对象。</param>
        /// <returns>
        /// <paramref name="format"/> 的一个副本，
        /// 其中格式项已替换为 <paramref name="args"/> 中相应对象的字符串表示形式。
        /// </returns>
        /// <exception cref="System.FormatException">
        /// <paramref name="format"/> 无效。
        /// - 或 -
        /// 格式项的索引小于零或大于等于 <paramref name="args"/> 数组的长度。 
        /// </exception>
        /// <example>
        /// <code language="C#">
        /// string format = "The field:{0} is invlaid.";
        /// format.FormatWith("EmailAddress");
        /// </code>
        /// </example>
        public static string FormatWith(this string format, params object[] args)
        {
            string val = string.Empty;
            if (string.IsNullOrWhiteSpace(format))
            {
                val = format;
            }
            else
            {
                if (args == null || args.Length == 0)
                {
                    val = format;
                }
                else
                {
                    val = string.Format(CultureInfo.InvariantCulture, format, args);
                }
            }

            return val;
        }

        /// <summary>
        /// Check that the object supplied is not null and throw exception
        /// with message provided.
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <param name="message">Error to use when throwing an <see cref="ArgumentNullException"/>
        /// if the condition is false.</param>
        public static void IsNotNull(Object obj, string message)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(message);
            }
        }


        /// <summary>
        /// Check that the object provided is not null.
        /// </summary>
        /// <param name="obj">Object to check.</param>
        public static void IsNotNull(Object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("The argument provided cannot be null.");
            }
        }


        /// <summary>
        /// Check that the object supplied is null and throw exception
        /// with message provided.
        /// </summary>
        /// <param name="obj">Object to check.</param>
        /// <param name="message">Error to use when throwing an <see cref="ArgumentNullException"/>
        /// if the condition is false.</param>
        public static void IsNull(Object obj, string message)
        {
            if (obj != null)
            {
                throw new ArgumentNullException(message);
            }
        }


        /// <summary>
        /// Check that the object provided is null.
        /// </summary>
        /// <param name="obj">Object to check.</param>
        public static void IsNull(Object obj)
        {
            if (obj != null)
            {
                throw new ArgumentNullException("The argument provided cannot be null.");
            }
        }


        /// <summary>
        /// Check that the supplied object is one of a list of objects.
        /// </summary>
        /// <typeparam name="T">Type of object to check.</typeparam>
        /// <param name="obj">Object to look for.</param>
        /// <param name="possibles">List with possible values for object.</param>
        /// <returns>True if the object is equal to one in the supplied list.
        /// Otherwise, <see cref="ArgumentException"/> is thrown.</returns>
        public static bool IsOneOfSupplied<T>(T obj, List<T> possibles)
        {
            return IsOneOfSupplied<T>(obj, possibles, "The object does not have one of the supplied values.");
        }

        /// <summary>
        /// Check that the supplied object is one of a list of objects.
        /// </summary>
        /// <typeparam name="T">Type of object to check.</typeparam>
        /// <param name="obj">Object to look for.</param>
        /// <param name="possibles">List with possible values for object.</param>
        /// <param name="message">Message of exception to throw.</param>
        /// <returns>True if the object is equal to one in the supplied list.
        /// Otherwise, <see cref="ArgumentException"/> is thrown.</returns>
        public static bool IsOneOfSupplied<T>(T obj, List<T> possibles, string message)
        {
            foreach (T possible in possibles)
                if (possible.Equals(obj))
                    return true;
            throw new ArgumentException(message);
        }
    }
}
