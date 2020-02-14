using Bangumi.Api.Models;
using System.Collections.Generic;

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
    }
}
