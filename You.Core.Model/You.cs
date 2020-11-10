using System;
using System.ComponentModel.DataAnnotations;

namespace You.Core.Model
{
    /// <summary>
    /// 油
    /// </summary>
    public class You
    {
        /// <summary>
        /// id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 油魂
        /// </summary>
        public string YouHun { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
    }
}
