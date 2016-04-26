using System;

namespace ConsumingODataWebServices
{
    public class NewsItemModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime PublicationDate { get; set; }
    }
}