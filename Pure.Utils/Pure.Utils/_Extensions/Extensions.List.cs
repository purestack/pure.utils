﻿using System.Collections.Generic;


namespace Pure.Utils
{
    /// <summary>
    /// 版 本 PureZero V8.2 敏捷开发框架 Copyright (c) 2012-2018
    /// 日 期：2017.03.06
    /// 描 述：List扩展
    /// </summary>
    internal static partial class Extensions
    {
		/// <summary>
		/// 获取list的分页数据
		/// </summary>
		/// <param name="obj">list对象</param>
		/// <param name="pagination">分页参数</param>
		/// <returns></returns>
        public static List<T> FindPage<T>(this List<T> obj, Pagination pagination) where T : class
        {
            pagination.records = obj.Count;
            int index = (pagination.page - 1) * pagination.rows;
            if (index >= obj.Count) {
                return new List<T>();
            }
            int end = index + pagination.rows;
            int count = end > obj.Count ? obj.Count - index : pagination.rows;
            List<T> list = obj.GetRange(index, count);
            return list;
        }
    }
}
