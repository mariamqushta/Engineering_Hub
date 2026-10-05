using System.Collections.Generic;

namespace Engineering_Hub.models
{
    public class LessonType
    {
        public int Id { get; set; }

        public string Name { get; set; }


        // Relationships

        public ICollection<LessonContent> LessonContents { get; set; }
            = new List<LessonContent>();
    }
}