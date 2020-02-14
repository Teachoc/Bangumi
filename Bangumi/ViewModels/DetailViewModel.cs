using Bangumi.Api.Models;
using System.Collections.Generic;
using System.Linq;

namespace Bangumi.ViewModels
{
    public class DetailViewModel
    {
        public string Name { get; set; }
        public string Info { get; set; }
        public string Summary { get; set; }
        // 角色资料
        public List<Character> Characters { get; set; }
        // 演职资料
        public List<Person> Staffs { get; set; }
        // 评论
        public List<Blog> Blogs { get; set; }
        // 讨论版
        public List<Topic> Topics { get; set; }

        /// <summary>
        /// 将演员列表转为string
        /// </summary>
        /// <param name="actors"></param>
        /// <returns></returns>
        public static string ActorListToString(List<Actor> actors)
        {
            if (actors != null && actors.Count != 0)
            {
                return "CV：" + string.Join('、', actors.Select(a => a.Name));
            }
            return string.Empty;
        }

        /// <summary>
        /// 将职责列表转为string
        /// </summary>
        /// <param name="jobs"></param>
        /// <returns></returns>
        public static string JobListToString(List<string> jobs)
        {
            if (jobs != null && jobs.Count != 0)
            {
                return string.Join('、', jobs);
            }
            return string.Empty;
        }

    }
}
