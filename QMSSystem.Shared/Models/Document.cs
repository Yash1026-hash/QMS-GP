namespace QMS.Models
{
    public class Document
    {
        public int Id { get; set; }

        public string DocumentNumber { get; set; }

        public string Title { get; set; }

        public string Department { get; set; }

        public int CurrentVersion { get; set; }

        public string Status { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreationTime { get; set; }
    }
}